using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ch03_Memory01_Savedata : MonoBehaviour
{

    [SerializeField] private MemoryData memoryData;
    // Start is called before the first frame update
    void Start()
    {
        MemoryManager.Instance.TryCollect(memoryData); // 기억 조각 수집
        Inventory_Player _inventory = GameManager.Instance.Player.GetComponent<Inventory_Player>(); 


    }

}
 