using UnityEngine;
using UnityEngine.EventSystems;
using R3;


public enum ItemType
{
    None,
    Sample,
    Sample_2,
    Soil,
    Stick,
    Wood,
    Stone,
    Iron,
    Gold,
    Diamond,
    Apple,
}

[RequireComponent(typeof(RectTransform))]
public class Item : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI amountText;
    public ItemType Type;
    public ReactiveProperty<int> Amount = new ReactiveProperty<int>(0);
    public RectTransform RectTransform { get; private set; }
    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        //amountText.text = "";
        Amount.Subscribe(amount =>
        {
            if (amount <= 1)
            {
                amountText.text = "";
            }
            else
            {
                amountText.text = amount.ToString();
            }
        });
    }
}