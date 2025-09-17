using UnityEngine;

public class Ch3_TeleportButton : MonoBehaviour
{
    [SerializeField] Transform lobby_1F;
    [SerializeField] Transform medicineRoom_1F;
    [SerializeField] Transform directorsoffice_2F;
    Transform player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject.transform;
    }
    public void Teleport_lobby_1F()
    {
        player.position = lobby_1F.position;
    }
    public void Teleport_medicineRoom_1F()
    {
        player.position = medicineRoom_1F.position;
    }
    public void Teleport_directorsoffice_2F()
    {
        player.position = directorsoffice_2F.position;
    }
}
