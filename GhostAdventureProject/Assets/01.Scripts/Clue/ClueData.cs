using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public enum ClueStage
{
    Stage1,
    Stage2,
    Stage3,
    Stage4
}

public enum ClueType
{
    None,
    DrawingClue,
    BookShelf,
}

[CreateAssetMenu(fileName = "NewClue", menuName = "Clue/ClueData")]

public class ClueData : ScriptableObject
{
    [Header("Default")]
    public string clue_Name; // 단서 이름
    public Sprite clue_Icon; // 단서 아이콘
    public Sprite clue_Image; // 단서 이미지 
    public string clue_Description; // 단서 설명
    public ClueStage clue_Stage;
    public ClueType clueType; // 단서 타입 (스테이지 몇에서 획득가능한지)
    // public bool isDrawingClue;
    // public int defaultCount; 

    [Header("Stacking")]
    public bool canStack;
    public int maxStackAmount;

}
