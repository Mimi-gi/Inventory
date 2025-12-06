using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class Frame : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform rectTransform;
    [SerializeField] Image highlightedSprite;
    public SlotType SlotType;
    public ItemType SlotItemType;
    public Item Item;

    public RectTransform RectTransform
    {
        get
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            return rectTransform;
        }
    }
    void Awake()
    {
        highlightedSprite.color = Color.clear;
    }

    public void HighlightFrame()
    {
        highlightedSprite.color = Color.white;
        Pointer.Instance.SetCurrentFrame(this);
    }

    public void HighlightOffFrame()
    {
        highlightedSprite.color = Color.clear;
        Pointer.Instance.ClearCurrentFrame(this);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        HighlightFrame();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        HighlightOffFrame();
    }

    public void SetItem(Item item)
    {
        Item = item;
        item.RectTransform.SetParent(this.RectTransform);
        item.RectTransform.anchoredPosition = Vector2.zero;
        item.RectTransform.localScale = Vector3.one;
    }
    public void ClearItem()
    {
        if(Item == null) return;
        //Item.RectTransform.SetParent(null);
        Item = null;
        Debug.Log("くりあ");
    }
    
    public void SetCreativeItem(Item item)
    {
        if (item != null)
        {
            Item = Instantiate(item);
            Item.RectTransform.SetParent(this.RectTransform);
            Item.RectTransform.anchoredPosition = Vector2.zero;
            SlotItemType = Item.Type;
        }
    }
}

public enum SlotType
{
    Normal,
    Equipment,
    Creative
}