using UnityEngine;

public class Ch1_MemoryFake_01_BirthdayHat : MemoryFragment
{
    private Animator anim;

    void Start()
    {
        isScannable = false;
        anim = GetComponentInChildren<Animator>();
    }

    public void ActivateHat()
    {
        isScannable = true;
    }
}
