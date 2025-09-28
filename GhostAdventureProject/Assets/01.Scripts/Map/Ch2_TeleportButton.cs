using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_TeleportButton : MonoBehaviour
{
    GameObject player;
    [SerializeField] Transform UnderGround;
    [SerializeField] Transform BackStreet;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    public void Teleport1()
    {
        player.transform.position = UnderGround.position;
    }
    public void Teleport_BackStreet()
    {
        player.transform.position = BackStreet.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
