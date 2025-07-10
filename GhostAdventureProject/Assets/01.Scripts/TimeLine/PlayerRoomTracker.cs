using UnityEngine;

public class PlayerRoomTracker : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {

        if(other.tag == "Room")
        {

        RoomInfo room = other.GetComponent<RoomInfo>();
        if (room != null)
        {
            room.roomCount++;
            Debug.Log($"Entered {room.roomName}, count: {room.roomCount}");
        }

        }

    }
}
 