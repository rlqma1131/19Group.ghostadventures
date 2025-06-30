using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/ItemData")]

public class ItemData : ScriptableObject
{
    [Header("Default")]
    public string Item_Name; // 아이템 이름
    public Sprite Item_Icon; // 아이템 아이콘
    public string Item_Description; // 아이템 설명

    [Header("Stacking")]
    public bool canStack;
    public int maxStackAmount;
}