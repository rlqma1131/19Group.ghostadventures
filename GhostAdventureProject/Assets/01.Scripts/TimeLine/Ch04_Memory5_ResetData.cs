using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch04_Memory5_ResetData : MonoBehaviour
{


    public void ResetSaveData()
    {
        SaveManager.DeleteSave();
        ChapterEndingManager.Instance?.ResetAllAndNotify();
    }
}
