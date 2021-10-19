using UniRx;
using UnityEngine;

public sealed class PlayerInput
{
    public readonly ReactiveProperty<bool> isEnable = new ReactiveProperty<bool>();
    
    public readonly ReactiveCommand move = new ReactiveCommand();
    public readonly ReactiveCommand rotate = new ReactiveCommand();
    
    public readonly ReactiveProperty<float> horizontal = new ReactiveProperty<float>();
    public readonly ReactiveProperty<float> vertical = new ReactiveProperty<float>();
    public readonly ReactiveProperty<float> mouseX = new ReactiveProperty<float>();
    public readonly ReactiveProperty<float> mouseY = new ReactiveProperty<float>();
    
    public readonly ReactiveProperty<float> depth = new ReactiveProperty<float>();
    public readonly ReactiveProperty<float> speed = new ReactiveProperty<float>();

    public readonly CompositeDisposable lifetimeDisposable = new CompositeDisposable();
    private readonly CompositeDisposable _updateInputDisposable = new CompositeDisposable();

    private const KeyCode PlusDepth = KeyCode.E;
    private const KeyCode MinusDepth = KeyCode.Q;
    private const KeyCode Run = KeyCode.LeftShift;

    public PlayerInput(float speedWalk, float speedRun, float sensitivity, float depth)
    {
        this.speed.Value = speedWalk;
        this.depth.Value = depth;

        isEnable
            .Where(value => value)
            .Subscribe(_ =>
            {
                Observable
                    .EveryUpdate()
                    .Subscribe(_ => UpdateInput(speedWalk, speedRun, sensitivity))
                    .AddTo(_updateInputDisposable);
            })
            .AddTo(lifetimeDisposable);

        isEnable
            .Where(value => !value)
            .Subscribe(_ => _updateInputDisposable.Clear())
            .AddTo(lifetimeDisposable);
    }
    
    private void UpdateInput(float speedWalk, float speedRun, float sensitivity)
    {
        vertical.SetValueAndForceNotify(Input.GetAxis("Vertical") * speed.Value);
        horizontal.SetValueAndForceNotify(Input.GetAxis("Horizontal") * speed.Value);
        mouseX.SetValueAndForceNotify(Input.GetAxis("Mouse X") * sensitivity);
        mouseY.SetValueAndForceNotify(Input.GetAxis("Mouse Y") * sensitivity);

        if (Input.GetKeyDown(PlusDepth)) depth.Value += 0.5f;
        if (Input.GetKeyDown(MinusDepth)) depth.Value -= 0.5f;
        if (Input.GetKeyDown(Run)) speed.SetValueAndForceNotify(speedRun);
        if (Input.GetKeyUp(Run)) speed.SetValueAndForceNotify(speedWalk);

        move.Execute();
        rotate.Execute();
    }
}