using UniRx;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public sealed class UnitMotor : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _cameraTransform;
    [Space]
    [SerializeField] private Text _depthText;
    [SerializeField] private Transform _water;
    [Space]
    [SerializeField] private float _speedWalk = 5f;
    [SerializeField] private float _speedRun = 10f;
    [SerializeField] private float _sensitivity = 5f;
    [SerializeField] private float _angle = 90f;
    [SerializeField] private float _rayDistance = 10f;
    [SerializeField] private float _changeDepth = 0.5f;

    private float _gravity;
    private int _layerGround;
    
    private PlayerInput _input;

    private void Awake()
    {
        _gravity = Physics.gravity.y;
        _layerGround = LayerMask.GetMask("Ground");
        _input = new PlayerInput(_speedWalk, _speedRun, _sensitivity, _changeDepth);
    }

    private void Start()
    {
        _input.isEnable.Value = true;
    }

    private void OnEnable()
    {
        float gravity = default;
        float xRotation = default;
        float depth = default;
        float speed = default;

        Transform t = transform;

        _input.onInputMove
            .Subscribe(value =>
            {
                Vector3 movement = t.forward * value.y * speed + t.right * value.x * speed;
                Vector3 next = t.position + movement * Time.deltaTime;
                Ray ray = new Ray { origin = next, direction = Vector3.down };

                if (Physics.Raycast(ray, out RaycastHit hit, _rayDistance, _layerGround))
                {
                    float waterDepth = _water.position.y + hit.point.y;

                    if (waterDepth < depth)
                    {
                        return;
                    }
                }
                
                if (!_characterController.isGrounded)
                {
                    gravity += _gravity * Time.deltaTime;
                }
                else
                {
                    gravity = 0f;
                }

                movement.y = gravity;
                
                _characterController.Move(movement * Time.deltaTime);
            })
            .AddTo(_input.lifetimeDisposable)
            .AddTo(this);

        _input.onInputRotate
            .Where(vector => vector != Vector2.zero)
            .Subscribe(value =>
            {
                xRotation -= value.y;
                xRotation = Mathf.Clamp(xRotation, -_angle, _angle);
                _cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                t.Rotate(Vector3.up * value.x);
            })
            .AddTo(_input.lifetimeDisposable)
            .AddTo(this);

        _input.depth
            .Subscribe(value =>
            {
                _depthText.text = $"Depth: {value:F}";
                depth = value;
            })
            .AddTo(_input.lifetimeDisposable)
            .AddTo(this);

        _input.speed
            .Subscribe(value =>
            {
                if (value > _speedRun)
                {
                    value = _speedRun;
                }
                
                speed = value;
            })
            .AddTo(_input.lifetimeDisposable)
            .AddTo(this);
    }

    private void OnDisable()
    {
        _input.lifetimeDisposable.Clear();
    }
}
