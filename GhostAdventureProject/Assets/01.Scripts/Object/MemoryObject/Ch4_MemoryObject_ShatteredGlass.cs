namespace _01.Scripts.Object.MemoryObject
{
    public class Ch4_MemoryObject_ShatteredGlass : MemoryFragment {
        public override void SetScannable(bool value) {
            base.SetScannable(value);
            gameObject.SetActive(value);
        }
    }
}