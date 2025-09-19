using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_MemoryPositive_01_TeddyBear : MemoryFragment
{
    public bool PlayerNearby = false;
    private Collider2D col;

    [SerializeField] Collider2D garageDoor;

    override protected void Start()
    {
        base.Start();
        col = GetComponent<Collider2D>();
        col.enabled = false;
        isScannable = false;
    }

    public void ActivateTeddyBear()
    {
        isScannable = true;
        col.enabled = true;
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);
    }

    public override void AfterScan() 
    {
        EnemyAI.PauseAllEnemies();
        //garageDoor.enabled = false;

        base.AfterScan();
    }
    
}
