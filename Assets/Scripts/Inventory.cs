using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [SerializeField] Vector2Int size = new Vector2Int(5, 5);

    [SerializeField] GameObject inventorySlotPrefab;

    [SerializeField] Vector2 offset;
    [SerializeField] SlotType slotType;
    [SerializeField] SerializeDictionary<ItemType, Item> itemDic;
    public InventorySlot Slot;

    private void Awake()
    {

        this.GetComponent<RectTransform>().sizeDelta = new Vector2(offset.y * (size.y + 1f), offset.x * (size.x + 1f));

        Slot = new InventorySlot(size.x, size.y, slotType, itemDic);

        for (int i = 0; i < Slot.Items.Length; i++)
        {
            for (int j = 0; j < Slot.Items[i].Length; j++)
            {
                var slotObj = Instantiate(inventorySlotPrefab, this.transform);
                var rectTransform = slotObj.GetComponent<RectTransform>();
                float xPos = offset.x * (j - Slot.Items[i].Length / 2.0f + 0.5f);
                float yPos = -offset.y * (i - Slot.Items.Length / 2.0f + 0.5f);
                rectTransform.anchoredPosition = new Vector2(xPos, yPos);
                var frame = slotObj.GetComponent<Frame>();
                frame.SlotType = slotType;
                Slot.Items[i][j] = frame;
                
                // Creative の場合、itemDic から Item を設定
                if (slotType == SlotType.Creative && itemDic != null)
                {
                    int index = i * Slot.Items[i].Length + j;
                    if (index < Slot.ItemList.Count)
                    {
                        frame.SetCreativeItem(Slot.ItemList[index]);
                    }
                }
            }
        }
    }
}

public struct InventorySlot
{
    public Frame[][] Items;
    public System.Collections.Generic.List<Item> ItemList;
    
    public InventorySlot(int height, int wide, SlotType slotType, SerializeDictionary<ItemType, Item> itemDic = null)
    {
        Items = null;
        ItemList = null;
        
        switch (slotType)
        {
            case SlotType.Normal:
                Items = new Frame[height][];
                for (int i = 0; i < height; i++)
                {
                    Items[i] = new Frame[wide];
                }
                break;
            case SlotType.Equipment:
            case SlotType.Creative:
                if (itemDic != null)
                {
                    // Keyの順にソートしてリスト化
                    var sortedKeys = new List<ItemType>(itemDic.GetDictionary.Keys);
                    sortedKeys.Sort();
                    
                    ItemList = new List<Item>();
                    foreach (var key in sortedKeys)
                    {
                        ItemList.Add(itemDic.GetDictionary[key]);
                    }
                    
                    var h = Mathf.CeilToInt(ItemList.Count / 2.0f);
                    Items = new Frame[h][];
                    for (int i = 0; i < h; i++)
                    {
                        Items[i] = new Frame[2];
                    }
                }
                break;
            default:
                break;
        }
    }
}