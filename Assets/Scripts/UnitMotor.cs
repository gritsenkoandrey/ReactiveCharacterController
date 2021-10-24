using UniRx;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public sealed class UnitMotor : MonoBehaviour
{
    private const float Depth = 1.2f;
    
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _cameraTransform;
    [Space]
    [SerializeField] private Text _depthText;
    [Space]
    [SerializeField] private float _speedWalk = 5f;
    [SerializeField] private float _speedRun = 10f;
    [SerializeField] private float _sensitivity = 5f;
    [SerializeField] private float _gravity = 9.8f;
    [SerializeField] private float _angle = 90f;
    [SerializeField] private float _rayDistance = 10f;
    [SerializeField] private float _changeDepth = 0.5f;

    private int _layerGround;
    
    private PlayerInput _input;

    private void Awake()
    {
        _layerGround = LayerMask.NameToLayer("Ground");
        
        _input = new PlayerInput(_speedWalk, _speedRun, _sensitivity, _changeDepth);
    }

    private void Start()
    {
        _input.isEnable.Value = true;
    }

    private void OnEnable()
    {
        float gravityForce = default;
        float xRotation = default;
        float depth = default;
        float speed = default;

        Transform t = transform;

        _input.onInputMove
            .Where(vector => vector != Vector2.zero)
            .Subscribe(value =>
            {
                Vector3 movement = t.forward * value.y * speed + t.right * value.x * speed;
                
                if (!_characterController.isGrounded)
                {
                    gravityForce -= _gravity * Time.deltaTime;
                }
                else
                {
                    gravityForce = -_gravity;
                }

                movement.y = gravityForce;

                Vector3 next = t.position.Add(movement * Time.deltaTime);
                
                //Debug.DrawRay(next, Vector3.down, Color.red);

                Ray ray = new Ray { origin = next, direction = Vector3.down };

                if (Physics.Raycast(ray, out RaycastHit hit, _rayDistance, 1 << _layerGround))
                {
                    if (t.position.y + hit.point.y < depth)
                    {
                        return;
                    }
                }

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

                if (Mathf.Approximately(value, 0f))
                {
                    depth = Depth;
                }
                else
                {
                    if (value > 0f)
                    {
                        depth = Depth + value;
                    }
                    else
                    {
                        depth = value;
                    }
                }
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
