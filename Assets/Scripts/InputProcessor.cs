using System;
using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using R3.Triggers;
using Cysharp.Threading.Tasks;

public class InputProcessor : IDisposable
{
    readonly InputActionMap _actionMap;

    Observable<Unit> MonoLeft;
    Observable<Unit> MonoRight;
    Observable<Unit> HoldLeft;
    Observable<Unit> HoldRight;
    Observable<Unit> DualLeft;
    Observable<Unit> DualRight;
    Observable<Unit> Release;
    Observable<Unit> Shift;

    public ReactiveProperty<InputButton> CurrentInputButton { get; private set; } = new ReactiveProperty<InputButton>(InputButton.None);
    public bool IsShiftPressed=false;
    public InputProcessor(InputActionAsset asset)
    {
        _actionMap = asset.FindActionMap("Mouse");
        _actionMap.Enable();

        MonoLeft = ConvertToObservable(_actionMap.FindAction("Left"));
        MonoRight = ConvertToObservable(_actionMap.FindAction("Right"));
        HoldLeft = ConvertToObservable(_actionMap.FindAction("LeftHold"));
        HoldRight = ConvertToObservable(_actionMap.FindAction("RightHold"));
        DualLeft = ConvertToObservable(_actionMap.FindAction("LeftMulti"));
        DualRight = ConvertToObservable(_actionMap.FindAction("RightMulti"));
        Release = ConvertToObservable(_actionMap.FindAction("Release"));
        Shift = ConvertToObservable(_actionMap.FindAction("Shift"));

        MonoLeft.SubscribeAwait(async (_, ct) =>
        {
            Debug.Log("MonoLeft performed");
            var wait=  UniTask.Delay(250, cancellationToken: ct);
            var dual = DualLeft.FirstAsync(ct).AsUniTask();

            var idx = await UniTask.WhenAny(wait, dual);

            if(idx == 0)
            {
                CurrentInputButton.Value = InputButton.LeftShortPress;
            }
            return;
        },AwaitOperation.Sequential);

        DualLeft.Subscribe(_ =>
        {
            Debug.Log("DualLeft performed");
            CurrentInputButton.Value = InputButton.LeftMultiPress;
        });

        HoldLeft.Subscribe(_ =>
        {
            Debug.Log("HoldLeft performed");
            CurrentInputButton.Value = InputButton.LeftLongPress;
        });

        MonoRight.SubscribeAwait(async (_, ct) =>
        {
            Debug.Log("MonoRight performed");
            var wait = UniTask.Delay(250, cancellationToken: ct);
            var dual = DualRight.FirstAsync(ct).AsUniTask();

            var idx = await UniTask.WhenAny(wait, dual);

            if (idx == 0)
            {
                CurrentInputButton.Value = InputButton.RightShortPress;
            }
            return;
        }, AwaitOperation.Sequential);

        DualRight.Subscribe(_ =>
        {
            Debug.Log("DualRight performed");
            CurrentInputButton.Value = InputButton.RightMultiPress;
        });

        HoldRight.Subscribe(_ =>
        {
            Debug.Log("HoldRight performed");
            CurrentInputButton.Value = InputButton.RightLongPress;
        });

        Release.Subscribe(_ =>
        {
            Debug.Log("Release performed");
            CurrentInputButton.Value = InputButton.None;
        });

        var shiftAction = _actionMap.FindAction("Shift");
        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                IsShiftPressed = shiftAction.IsPressed();
            });

        Debug.Log("InputProcessor initialized");
    }

    private Observable<Unit> ConvertToObservable(InputAction action)
    {
        return Observable.FromEvent<InputAction.CallbackContext>(
            h => action.performed += h,
            h => action.performed -= h
        )
        .Select(_ => Unit.Default);
    }

    public void Dispose()
    {
        _actionMap.Disable();
        _actionMap.Dispose();
        
    }
}

public enum InputButton
{
    LeftLongPress,
    RightLongPress,
    LeftShortPress,
    RightShortPress,
    LeftMultiPress,
    RightMultiPress,
    None
}
