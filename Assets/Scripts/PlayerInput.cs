using UniRx;
using UnityEngine;

public sealed class PlayerInput
{
    public readonly ReactiveProperty<bool> isEnable = new ReactiveProperty<bool>();
    
    public readonly ReactiveCommand<Vector2> onInputMove = new ReactiveCommand<Vector2>();
    public readonly ReactiveCommand<Vector2> onInputRotate = new ReactiveCommand<Vector2>();
    public readonly ReactiveProperty<float> depth = new ReactiveProperty<float>();
    public readonly ReactiveProperty<float> speed = new ReactiveProperty<float>();
    public readonly CompositeDisposable lifetimeDisposable = new CompositeDisposable();
    
    private readonly ReactiveProperty<Vector2> _inputMove = new ReactiveProperty<Vector2>();
    private readonly ReactiveProperty<Vector2> _inputRotate = new ReactiveProperty<Vector2>();
    private readonly CompositeDisposable _updateInputDisposable = new CompositeDisposable();

    private const KeyCode PlusDepth = KeyCode.E;
    private const KeyCode MinusDepth = KeyCode.Q;
    private const KeyCode Run = KeyCode.LeftShift;

    public PlayerInput(float walk, float run, float sensitivity, float changeDepth)
    {
        speed.Value = walk;
        depth.Value = 0f;

        isEnable
            .Where(value => value)
            .Subscribe(_ =>
            {
                Observable
                    .EveryUpdate()
                    .Subscribe(_ => UpdateInput(walk, run, sensitivity, changeDepth))
                    .AddTo(_updateInputDisposable)
                    .AddTo(lifetimeDisposable);
            })
            .AddTo(lifetimeDisposable);

        isEnable
            .Where(value => !value)
            .Subscribe(_ => _updateInputDisposable.Clear())
            .AddTo(lifetimeDisposable);
    }
    
    private void UpdateInput(float walk, float run, float sensitivity, float changeDepth)
    {
        _inputMove.SetValueAndForceNotify
            (new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        _inputRotate.SetValueAndForceNotify
            (new Vector2(Input.GetAxis("Mouse X") * sensitivity,Input.GetAxis("Mouse Y") * sensitivity));

        if (Input.GetKeyDown(PlusDepth))
        {
            depth.Value += changeDepth;
        }

        if (Input.GetKeyDown(MinusDepth))
        {
            depth.Value -= changeDepth;
        }

        if (Input.GetKeyDown(Run))
        {
            speed.SetValueAndForceNotify(run);
        }

        if (Input.GetKeyUp(Run))
        {
            speed.SetValueAndForceNotify(walk);
        }

        onInputMove.Execute(_inputMove.Value);
        onInputRotate.Execute(_inputRotate.Value);
    }
}