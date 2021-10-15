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
    [Space]
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _runSpeed = 10f;
    [Space]
    [SerializeField] private float _sensitivity = 5f;
    [Space]
    [SerializeField] private float _gravity = -9.8f;
    [SerializeField] private float _depth = -1f;
    [SerializeField] private float _angle = 90f;
    [SerializeField] private float _rayDistance = 10f;

    private PlayerInput _input;
    
    private int _layerGround;
    private float _xRotation = 0f;

    private void Awake()
    {
        _input = new PlayerInput(_walkSpeed, _sensitivity, _depth, _runSpeed);
    }

    private void Start()
    {
        _layerGround = LayerMask.NameToLayer("Ground");
        _input.isEnable.Value = true;
    }

    private void OnEnable()
    {
        _input.move
            .Subscribe(_ =>
            {
                Vector3 movement = new Vector3(_input.horizontal.Value, 0f, _input.vertical.Value);
                movement = Vector3.ClampMagnitude(movement, _input.speed.Value);
                movement.y = _gravity;
                movement = transform.TransformDirection(movement);

                Vector3 next = transform.position.Add(movement.normalized);
                
                //Debug.DrawRay(next, Vector3.down, Color.red);

                Ray ray = new Ray { origin = next, direction = Vector3.down };

                if (Physics.Raycast(ray, out RaycastHit hit, _rayDistance, 1 << _layerGround))
                {
                    if (transform.position.y + hit.point.y < _input.depth.Value)
                    {
                        return;
                    }
                }

                _characterController.Move(movement * Time.deltaTime);
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
            .Subscribe(value => _depthText.text = $"Depth: {value}")
            .AddTo(this)
            .AddTo(_input.lifetimeDisposable);
    }

    private void OnDisable()
    {
        _input.lifetimeDisposable.Clear();
    }
}
