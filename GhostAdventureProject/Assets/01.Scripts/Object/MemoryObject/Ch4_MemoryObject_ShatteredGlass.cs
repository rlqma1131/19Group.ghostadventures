using UnityEngine;

namespace _01.Scripts.Object.MemoryObject
{
    public class Ch4_MemoryObject_ShatteredGlass : MemoryFragment
    {
        readonly static int Color1 = Shader.PropertyToID("_Color");

        [Header("References")]
        [SerializeField] SpriteRenderer body;
        
        [Header("Target Count to activate")]
        [SerializeField] int targetCount = 3;

        MaterialPropertyBlock block;
        int currentCount;

        override protected void Start() {
            base.Start();
            isScannable = false;
            
            // Set body color to transparent
            block = new MaterialPropertyBlock();
            body.GetPropertyBlock(block);
            block.SetColor(Color1, Color.clear);
            body.SetPropertyBlock(block);
        }

        public void UpdateProgress() {
            currentCount++;
            if (currentCount >= targetCount) {
                if (!alreadyScanned) isScannable = true;
                
                block.Clear();
                body.GetPropertyBlock(block);
                block.SetColor(Color1, Color.white);
                body.SetPropertyBlock(block);
            }
        }
    }
}