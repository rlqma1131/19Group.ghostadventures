using UnityEngine;

namespace _01.Scripts.Object.MemoryObject
{
    public class Ch4_MemoryObject_CassetteTape : MemoryFragment {
        [Header("References")]
        [SerializeField] Ch4_MemoryObject_ShatteredGlass shatteredGlass;
        
        public override void SetScannable(bool value) {
            base.SetScannable(value);
            gameObject.SetActive(value);
        }

        public override void AfterScan() => shatteredGlass.UpdateProgress();
    }
}
