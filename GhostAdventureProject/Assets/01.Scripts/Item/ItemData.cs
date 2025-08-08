using UnityEngine;

public enum ItemType {Consumable, Clue}

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/ItemData")]

public class ItemData : ScriptableObject
{
    [Header("Default")]
    public string Item_Name; // 이름
    public Sprite Item_Icon; // 아이콘
    public ItemType Item_Type; // 타입
    public string Item_Description; // 설명
    public ClueData clue;

    // [Header("Stacking")]
    // public bool canStack;
    // public int maxStackAmount;
}