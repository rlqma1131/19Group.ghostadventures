using UnityEngine;

public class Ch1_Closet : BaseUnlockObject
{
    [SerializeField] private GameObject baseball;

    public override void Unlock()
    {
        anim.SetTrigger("Unlock");
        Ch1_MemoryFake_03_Baseball _baseball = baseball.GetComponent<Ch1_MemoryFake_03_Baseball>();

        baseball.SetActive(true);
        _baseball.ActivateBaseball();
    }
}
