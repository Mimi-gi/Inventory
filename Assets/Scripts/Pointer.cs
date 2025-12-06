using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class Pointer : MonoBehaviour
{
    [SerializeField] InputActionAsset actionMap;
    public static Pointer Instance { get; private set; }
    public Vector3 Position => Input.mousePosition;
    public Item HoveredItem { get; private set; }
    public Frame CurrentFrame { get; private set; }
    RectTransform rectTransform;
    InputProcessor inputProcessor;


    CancellationTokenSource cts;

    public (bool, bool) UnderPointer
    {
        get
        {
            return (HoveredItem != null, CurrentFrame != null);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        rectTransform = GetComponent<RectTransform>();

        inputProcessor = new InputProcessor(actionMap);
    }

    void Start()
    {
        inputProcessor.CurrentInputButton.Subscribe(inputButton =>
        {
            switch (inputButton)
            {
                case InputButton.LeftShortPress:
                    if (!UnderPointer.Item1 && UnderPointer.Item2 && CurrentFrame.SlotType == SlotType.Normal && CurrentFrame.Item != null && CurrentFrame.Item.Amount.Value > 0)
                    {
                        Debug.Log($"LeftShortPress: (ナシ, アリ, アリ)");
                        if (inputProcessor.IsShiftPressed)
                        {
                            // Shift+クリックで全移動
                            return;
                        }

                        // 通常のクリックで取得
                        SetItem(CurrentFrame.Item);
                        CurrentFrame.ClearItem();
                        return;
                    }
                    else if (CurrentFrame != null && CurrentFrame.SlotType == SlotType.Creative && CurrentFrame.Item != null)
                    {
                        Debug.Log($"LeftShortPress: (ナシ, アリ[クリエイティブ], アリ)");
                        // クリエイティブモードのスロットからは無限に取得可能
                        var newItem = Instantiate(CurrentFrame.Item);
                        newItem.Amount.Value = 64;
                        newItem.Type = CurrentFrame.Item.Type;
                        SetItem(newItem);
                        newItem.RectTransform.localScale = Vector3.one;
                        return;
                    }

                    if (UnderPointer.Item1)
                    {
                        if (UnderPointer.Item2 && CurrentFrame.SlotType == SlotType.Normal)
                        {
                            // スロットと交換
                            if (CurrentFrame.Item == null)
                            {
                                Debug.Log($"LeftShortPress: (アリ, アリ, ナシ)");
                                CurrentFrame.SetItem(HoveredItem);
                                ClearItem();
                                return;
                            }
                            if (HoveredItem.Type != CurrentFrame.Item.Type || HoveredItem.Amount.Value + CurrentFrame.Item.Amount.Value > 64)
                            {
                                Debug.Log($"LeftShortPress: (アリ, アリ, アリ) - 交換");
                                var temp = CurrentFrame.Item;
                                CurrentFrame.SetItem(HoveredItem);
                                SetItem(temp);
                                return;
                            }
                            else if (HoveredItem.Type == CurrentFrame.Item.Type)
                            {
                                Debug.Log($"LeftShortPress: (アリ, アリ, アリ) - 合成");
                                // 同種アイテムであれば合成
                                CurrentFrame.Item.Amount.Value += HoveredItem.Amount.Value;
                                Destroy(HoveredItem.gameObject);
                                ClearItem();
                            }
                            return;
                        }
                        else
                        {
                            Debug.Log($"LeftShortPress: (アリ, ナシ)");
                            Debug.Log($"{UnderPointer.Item1}, {UnderPointer.Item2}");
                            Destroy(HoveredItem.gameObject);
                            ClearItem();
                        }
                    }
                    break;
                case InputButton.RightShortPress:
                    if (!UnderPointer.Item1 && UnderPointer.Item2 && CurrentFrame.SlotType == SlotType.Normal && CurrentFrame.Item != null)
                    {
                        Debug.Log($"RightShortPress: (ナシ, アリ, アリ)");
                        var item = CurrentFrame.Item;
                        var num = Mathf.CeilToInt(item.Amount.Value / 2.0f);
                        var remain = item.Amount.Value - num;
                        var newItem = Instantiate(item);
                        newItem.Amount.Value = num;
                        SetItem(newItem);
                        newItem.RectTransform.localScale = Vector3.one;
                        if (remain <= 0)
                        {
                            CurrentFrame.ClearItem();
                        }
                        else
                        {
                            item.Amount.Value = remain;
                        }
                        return;
                    }

                    if (UnderPointer.Item1)
                    {
                        if (UnderPointer.Item2 && CurrentFrame.SlotType == SlotType.Normal)
                        {
                            // フレームにアイテムがない場合は1個だけ置く
                            if (CurrentFrame.Item == null)
                            {
                                Debug.Log($"RightShortPress: (アリ, アリ, ナシ)");
                                var newItem = Instantiate(HoveredItem);
                                newItem.Amount.Value = 1;
                                CurrentFrame.SetItem(newItem);
                                HoveredItem.Amount.Value -= 1;
                                if (HoveredItem.Amount.Value <= 0)
                                {
                                    ClearItem();
                                }
                                return;
                            }

                            var item = CurrentFrame.Item;
                            // 64じゃない&同じアイテム→1個置く、そうじゃない→交換
                            if (item.Type == HoveredItem.Type && item.Amount.Value < 64)
                            {
                                Debug.Log($"RightShortPress: (アリ, アリ, アリ) - 1個置く");
                                item.Amount.Value += 1;
                                HoveredItem.Amount.Value -= 1;
                                if (HoveredItem.Amount.Value <= 0)
                                {
                                    ClearItem();
                                }
                            }
                            else
                            {
                                Debug.Log($"RightShortPress: (アリ, アリ, アリ) - 交換");
                                // 交換
                                var temp = CurrentFrame.Item;
                                CurrentFrame.SetItem(HoveredItem);
                                SetItem(temp);
                            }
                            return;
                        }
                        else
                        {
                            Debug.Log($"RightShortPress: (アリ, ナシ)");
                            // アイテムを1つ減らす
                            HoveredItem.Amount.Value -= 1;
                            if (HoveredItem.Amount.Value <= 0)
                            {
                                ClearItem();
                            }
                        }
                    }
                    break;
                case InputButton.LeftLongPress:
                    //軌跡を取得し、64を超えない範囲で分配する
                    DistributeByTrail(cts.Token).Forget();
                    break;
                case InputButton.RightLongPress:
                    //軌跡に対して一個ずつ分配する
                    DistributeByTrailOneByOne(cts.Token).Forget();
                    break;
                case InputButton.LeftMultiPress:
                    break;
                case InputButton.RightMultiPress:
                    break;
                case InputButton.None:
                    cts?.Cancel();
                    cts = new CancellationTokenSource();
                    break;
            }
        });
    }

    async UniTaskVoid DistributeByTrailOneByOne(CancellationToken token)
    {
        if (HoveredItem == null)
        {
            return;
        }

        try
        {
            HashSet<Frame> trailFrames = new HashSet<Frame>();
            Frame lastFrame = null;

            while (!token.IsCancellationRequested && HoveredItem != null && HoveredItem.Amount.Value > 0)
            {
                if (CurrentFrame != null && CurrentFrame != lastFrame &&
                    CurrentFrame.SlotType == SlotType.Normal &&
                    (CurrentFrame.Item == null || CurrentFrame.Item.Type == HoveredItem.Type) &&
                    !trailFrames.Contains(CurrentFrame))
                {
                    lastFrame = CurrentFrame;
                    trailFrames.Add(CurrentFrame);

                    // 1個ずつ配置
                    if (CurrentFrame.Item == null)
                    {
                        var newItem = Instantiate(HoveredItem);
                        newItem.Amount.Value = 1;
                        CurrentFrame.SetItem(newItem);
                    }
                    else if (CurrentFrame.Item.Type == HoveredItem.Type && CurrentFrame.Item.Amount.Value < 64)
                    {
                        CurrentFrame.Item.Amount.Value += 1;
                    }

                    HoveredItem.Amount.Value -= 1;
                    if (HoveredItem.Amount.Value <= 0)
                    {
                        break;
                    }
                }

                await UniTask.Yield(token);
            }
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("Right distribution cancelled");
        }
        finally
        {
            if (HoveredItem != null && HoveredItem.Amount.Value <= 0)
            {
                Destroy(HoveredItem.gameObject);
                ClearItem();
            }
        }
    }

    public void SetItem(Item item)
    {
        Debug.Log("SetItem");
        HoveredItem = item;
        item.RectTransform.SetParent(this.rectTransform);
        item.RectTransform.anchoredPosition = Vector2.zero;
    }
    public void ClearItem()
    {
        //HoveredItem.RectTransform.SetParent(null);
        HoveredItem = null;
    }
    public void SetCurrentFrame(Frame frame)
    {
        CurrentFrame = frame;
    }
    public void ClearCurrentFrame(Frame frame)
    {
        if (CurrentFrame == frame)
            CurrentFrame = null;
    }


    async UniTaskVoid DistributeByTrail(CancellationToken token)
    {
        try
        {
            if (HoveredItem == null) return;
            int oridinalAmount = HoveredItem.Amount.Value;
            HashSet<Frame> trailFrames = new HashSet<Frame>();
            Dictionary<Frame, Item> originalFrameStates = new Dictionary<Frame, Item>();
            Frame lastFrame = null;
            
            while (!token.IsCancellationRequested)
            {
                if (CurrentFrame != null && CurrentFrame != lastFrame && CurrentFrame.SlotType == SlotType.Normal && (CurrentFrame.Item == null || CurrentFrame.Item.Type == HoveredItem.Type) && !trailFrames.Contains(CurrentFrame))
                {
                    lastFrame = CurrentFrame;
                    trailFrames.Add(CurrentFrame);
                    
                    // 元の状態を保存（新しいフレームのみ）
                    if (!originalFrameStates.ContainsKey(CurrentFrame))
                    {
                        if (CurrentFrame.Item != null)
                        {
                            var originalItem = Instantiate(CurrentFrame.Item);
                            originalItem.Amount.Value = CurrentFrame.Item.Amount.Value;
                            originalFrameStates[CurrentFrame] = originalItem;
                        }
                        else
                        {
                            originalFrameStates[CurrentFrame] = null;
                        }
                    }
                    
                    // 全てのフレームを元の状態に戻す
                    foreach (var frame in trailFrames)
                    {
                        if (frame.Item != null)
                        {
                            Destroy(frame.Item.gameObject);
                            frame.Item = null;
                        }
                        
                        if (originalFrameStates[frame] != null)
                        {
                            var restoredItem = Instantiate(originalFrameStates[frame]);
                            restoredItem.Amount.Value = originalFrameStates[frame].Amount.Value;
                            frame.SetItem(restoredItem);
                        }
                    }
                    
                    // 新しく分配
                    foreach (var frame in trailFrames)
                    {
                        if (frame.Item == null)
                        {
                            var newItem = Instantiate(HoveredItem);
                            newItem.Amount.Value += Mathf.FloorToInt((float)oridinalAmount / trailFrames.Count);
                            frame.SetItem(newItem);
                        }
                        else
                        {
                            var num = Mathf.FloorToInt((float)oridinalAmount / trailFrames.Count);
                            if (num + frame.Item.Amount.Value > 64)
                            {
                                num = 64 - frame.Item.Amount.Value;
                            }
                            frame.Item.Amount.Value += num;
                        }
                    }
                    
                    // HoveredItemの量を更新
                    HoveredItem.Amount.Value = oridinalAmount - CalculateDistributedAmount(trailFrames, originalFrameStates);
                }
                
                await UniTask.Yield(token);
            }
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("Distribution cancelled");
        }
        finally
        {
            if(HoveredItem != null && HoveredItem.Amount.Value <= 0)
            {
                Debug.Log("Distribution complete - clearing hovered item");
                Destroy(HoveredItem.gameObject);
                ClearItem();
            }
        }
    }
    
    private int CalculateDistributedAmount(HashSet<Frame> frames, Dictionary<Frame, Item> originalStates)
    {
        int distributed = 0;
        foreach (var frame in frames)
        {
            if (frame.Item != null)
            {
                int currentAmount = frame.Item.Amount.Value;
                int originalAmount = originalStates[frame] != null ? originalStates[frame].Amount.Value : 0;
                distributed += currentAmount - originalAmount;
            }
        }
        return distributed;
    }

    void Update()
    {
        rectTransform.position = Position;

    }
}
