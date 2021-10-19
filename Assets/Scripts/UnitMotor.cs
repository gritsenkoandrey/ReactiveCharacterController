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
    [Space]
    [SerializeField] private float _sensitivity = 5f;
    [Space]
    [SerializeField] private float _gravity = -9.8f;
    [SerializeField] private float _angle = 90f;
    [SerializeField] private float _rayDistance = 10f;

    private Vector3 _velocity;
    private float _xRotation;
    private int _layerGround;
    
    private PlayerInput _input;

    private void Awake()
    {
        _input = new PlayerInput(_speedWalk, _speedRun, _sensitivity, Depth);
        _layerGround = LayerMask.NameToLayer("Ground");
    }

    private void Start()
    {
        _input.isEnable.Value = true;
    }

    private void OnEnable()
    {
        _input.move
            .Subscribe(_ =>
            {
                Vector3 movement = new Vector3(_input.horizontal.Value, 0f, _input.vertical.Value);
                movement = transform.TransformDirection(movement);
                _velocity.y += _gravity * Time.deltaTime;
                Vector3 next = transform.position.Add(movement * Time.deltaTime);
                
                //Debug.DrawRay(next, Vector3.down, Color.red);

                Ray ray = new Ray { origin = next, direction = Vector3.down };

                if (Physics.Raycast(ray, out RaycastHit hit, _rayDistance, 1 << _layerGround))
                {
                    float nextPosY = transform.position.y + hit.point.y;

                    if (nextPosY < _input.depth.Value)
                    {
                        return;
                    }
                }

                _characterController.Move(movement * Time.deltaTime);
                _characterController.Move(_velocity * Time.deltaTime);
            })
            .AddTo(this)
            .AddTo(_input.lifetimeDisposable);

        _input.rotate
            .Subscribe(_ =>
            {
                _xRotation -= _input.mouseY.Value;
                _xRotation = Mathf.Clamp(_xRotation, -_angle, _angle);
                _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
                transform.Rotate(Vector3.up * _input.mouseX.Value);
            })
            .AddTo(this)
            .AddTo(_input.lifetimeDisposable);

        _input.depth
            .Subscribe(value => _depthText.text = $"Depth: {value - Depth}")
            .AddTo(this)
            .AddTo(_input.lifetimeDisposable);
    }

    private void OnDisable()
    {
        _input.lifetimeDisposable.Clear();
    }
}
