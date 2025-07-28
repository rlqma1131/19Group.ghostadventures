using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_DollPicture : BaseInteractable
{
    private bool interactAble = false;
    CluePickup cluePickup;
    void Start()
    {
        cluePickup = GetComponent<CluePickup>();
    }
    void Update()
    {
        if(interactAble == true)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                cluePickup.PickupClue();
            }
        }
        
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        interactAble = true;
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        interactAble = false;   
    }
}
