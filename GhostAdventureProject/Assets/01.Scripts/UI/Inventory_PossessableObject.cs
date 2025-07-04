using System.Collections.Generic;
using UnityEngine;

public class Inventory_PossessableObject : MonoBehaviour
{
    public Transform slotParent;
    public GameObject slotPrefab;

    private List<GameObject> spawnedSlots = new List<GameObject>();


    void Start()
    {
        UIManager.Instance.Inventory_PossessableObjectUI.gameObject.SetActive(false);
    }

    // 미사용(보류)
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

    public void ShowInventory(List<InventorySlot_PossessableObject> slots)
    {
        Clear();

        foreach (var slot in slots)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            obj.GetComponent<InventorySlot_PossessableObject>().SetSlot(slot);
            spawnedSlots.Add(obj);
        }

        gameObject.SetActive(true);
    }

    public void HideInventory()
    {
        Clear();
        gameObject.SetActive(false);
    }

    private void Clear()
    {
        foreach (var slot in spawnedSlots)
        {
            Destroy(slot);
        }
        spawnedSlots.Clear();
    }

    public void OpenInventory(BasePossessable target)
    {
        HaveItem haveItem = target.GetComponent<HaveItem>();
        if (haveItem != null)
        {
            ShowInventory(haveItem.inventorySlots);
            return;
        }
        HideInventory();

    }

    public void CloseInventory()
    {
        HideInventory();
    }
}
