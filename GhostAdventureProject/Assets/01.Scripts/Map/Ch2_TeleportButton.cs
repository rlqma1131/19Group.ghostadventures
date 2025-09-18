using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_TeleportButton : MonoBehaviour
{
    GameObject player;
    [SerializeField] Transform UnderGround;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    public void Teleport1()
    {
        player.transform.position = UnderGround.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
