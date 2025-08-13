using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_Shelf : MonoBehaviour
{
    [SerializeField] private GameObject open;
    [SerializeField] private GameObject memoryFragment;
    [SerializeField] private Ch3_MemoryNegative_03_Handbones bone;

    public void OpenShelf()
    {
        if (open != null && memoryFragment != null)
        {
            open.SetActive(true);
            memoryFragment.SetActive(true);
            bone.ActivateBone();
        }
    }
}
