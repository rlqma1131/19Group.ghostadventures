using UnityEngine;

public class BaseUnlockObject : MonoBehaviour
{
    protected Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public virtual void Unlock() { }
}
