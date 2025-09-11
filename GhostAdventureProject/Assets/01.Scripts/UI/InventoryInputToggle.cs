using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryInputToggle : MonoBehaviour
{
    public static bool FocusIsPlayer = true;  // true:플레이어인벤토리 / false:빙의인벤토리
    private Inventory_Player inventory_Player;
    private Inventory_PossessableObject inventory_PossessableObject;

    void Start()
    {
        inventory_Player = UIManager.Instance.Inventory_PlayerUI;
        inventory_PossessableObject = UIManager.Instance.Inventory_PossessableObjectUI;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {   
            // 빙의인벤토리가 있을때만 Toggle가능
            if(!inventory_PossessableObject.IsSpawnslots()) return;
            FocusIsPlayer = !FocusIsPlayer;

            // 플레이어인벤토리 - FocusIsPlayer가 true일때
            inventory_Player.SetPlayerKeyLabelsVisible(FocusIsPlayer);

            // 빙의인벤토리 - FocusIsPlayer가 false일때
            if (inventory_PossessableObject != null)
                inventory_PossessableObject.SetKeyLabelsVisible(!FocusIsPlayer);
        }
    }
}

