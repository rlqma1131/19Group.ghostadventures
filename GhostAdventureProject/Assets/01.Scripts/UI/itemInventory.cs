using System.Collections.Generic;
using UnityEngine;

public class itemInventory : MonoBehaviour
{
    public Transform slotParent;
    public GameObject slotPrefab;

    private List<GameObject> spawnedSlots = new List<GameObject>();


    void Update()
    {
        BasePossessable possessed = FindPossessed();
        if (possessed != null)
        {
            HaveItem haveItem = possessed.GetComponent<HaveItem>();

            if (haveItem != null)
            {
                ShowInventory(haveItem.inventorySlots);
                return;
            }
        }

        HideInventory();
    }


    BasePossessable FindPossessed()
    {
        BasePossessable[] all = FindObjectsOfType<BasePossessable>();
        foreach (var obj in all)
        {
            if (obj.IsPossessed)
                Debug.Log("obj:" + obj);
                return obj;
        }

        return null;
    }

    public void ShowInventory(List<itemInventorySlot> slots)
    {
        Clear();

        foreach (var slot in slots)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            obj.GetComponent<itemInventorySlot>().SetSlot(slot);
            spawnedSlots.Add(obj);
        }

        gameObject.SetActive(true);
    }

    public void HideInventory()
    {
        Clear();
        // gameObject.SetActive(false);
    }

    private void Clear()
    {
        foreach (var slot in spawnedSlots)
        {
            Destroy(slot);
        }
        spawnedSlots.Clear();
    }
}
