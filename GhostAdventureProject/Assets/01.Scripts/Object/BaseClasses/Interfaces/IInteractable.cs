namespace _01.Scripts.Object.BaseClasses.Interfaces
{
    public interface IInteractable
    {
        public bool IsScannable();
        public void ShowHighlight(bool pop);
        public void TriggerEvent();
    }
}