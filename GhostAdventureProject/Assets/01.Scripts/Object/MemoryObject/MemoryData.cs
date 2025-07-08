using UnityEngine;

[CreateAssetMenu(menuName = "Memory/MemoryData")]
public class MemoryData : ScriptableObject
{
    public enum MemoryType { Positive, Negative, Fake }

    public string memoryID;
    public MemoryType type;

    // 스캔오브젝트 스프라이트
    public Sprite MemoryObjectSprite;

    // 스캔 후 드랍하는 조각 스프라이트
    public Sprite PositiveFragmentSprite;
    public Sprite NegativeFragmentSprite;
    public Sprite FakeFragmentSprite;
    public string CutSceneName;
    // 갖고 있는 기억은 나중에
    //public Sprite memoryImage;       
    public string memoryTitle;

    [TextArea(3, 10)]
    public string memoryDescription;

    public int soulRecovery = 0;
}