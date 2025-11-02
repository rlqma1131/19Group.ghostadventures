using UnityEngine;

namespace _01.Scripts.Object.MemoryObject
{
    public class Ch4_MemoryObject_Mirror : MemoryFragment {
        override protected void Start() {
            base.Start();
            isScannable = true;
        }
    }
}