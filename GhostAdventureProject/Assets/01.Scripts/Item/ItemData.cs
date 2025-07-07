using UnityEngine;

// public enum ItemType {Consumable, Placeable}

public enum ItemType { Consumable, Placeable, Both }

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/ItemData")]

public class ItemData : ScriptableObject
{
    [Header("Default")]
    public string Item_Name; // 아이템 이름
    public Sprite Item_Icon; // 아이템 아이콘
    public ItemType Item_Type; // 아이템 타입
    public GameObject placeablePrefab; // 설치용
    public string Item_Description; // 아이템 설명

    [Header("Stacking")]
    public bool canStack;
    public int maxStackAmount;
}