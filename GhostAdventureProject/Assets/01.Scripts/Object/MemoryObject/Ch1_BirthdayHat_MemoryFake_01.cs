using UnityEngine;

public class Ch1_BirthdayHat_MemoryFake_01 : MemoryFragment
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
