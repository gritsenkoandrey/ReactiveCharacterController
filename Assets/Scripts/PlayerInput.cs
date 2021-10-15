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

    public PlayerInput(float speed, float sensitivity, float depth, float speedRun)
    {
        this.speed.Value = speed;
        this.depth.Value = depth;

        isEnable
            .Where(value => value)
            .Subscribe(_ =>
            {
                Observable
                    .EveryUpdate()
                    .Subscribe(_ => UpdateInput(sensitivity, speedRun))
                    .AddTo(_updateInputDisposable);
            })
            .AddTo(lifetimeDisposable);

        isEnable
            .Where(value => !value)
            .Subscribe(_ => _updateInputDisposable.Clear())
            .AddTo(lifetimeDisposable);
    }
    
    private void UpdateInput(float sensitivity, float speedRun)
    {
        vertical.SetValueAndForceNotify(Input.GetAxis("Vertical") * speed.Value);
        horizontal.SetValueAndForceNotify(Input.GetAxis("Horizontal") * speed.Value);
        mouseX.SetValueAndForceNotify(Input.GetAxis("Mouse X") * sensitivity);
        mouseY.SetValueAndForceNotify(Input.GetAxis("Mouse Y") * sensitivity);

        if (Input.GetKeyDown(PlusDepth)) depth.Value += 1f;
        if (Input.GetKeyDown(MinusDepth)) depth.Value -= 1f;
        if (Input.GetKeyDown(Run)) speed.Value += speedRun;
        if (Input.GetKeyUp(Run)) speed.Value -= speedRun;

        move.Execute();
        rotate.Execute();
    }
}