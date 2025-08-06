using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroMemoryGive : MonoBehaviour
{

   [SerializeField] List<MemoryData> memories = new List<MemoryData>();




    public void GiveMemories()
    {


        MemoryManager.Instance.ClearScannedDebug(); 
        MemoryManager.Instance.TryCollectAll(memories); 

    }
}
