using UnityEngine;

public class RoomTriggerZone : MonoBehaviour
{
    [SerializeField] private RoomType roomType;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        switch (roomType)
        {
            case RoomType.Classroom:
                break;
            case RoomType.Faculty:
                if (player != null)
                    player.inFaculty = true;
                break;
            case RoomType.Office:
                if (player != null)
                    player.inOffice = true;
                break;
            case RoomType.Cafeteria:
                break;
            case RoomType.Library:
                break;
            case RoomType.Playground:
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        switch (roomType)
        {
            case RoomType.Classroom:
                break;
            case RoomType.Faculty:
                if (player != null)
                {
                    player.inFaculty = false;
                    player.guiltTime = 0.5f;
                    player.guiltType = GuiltType.Faculty;
                }
                break;
            case RoomType.Office:
                if (player != null)
                    player.inOffice = false;
                break;
            case RoomType.Cafeteria:
                break;
            case RoomType.Library:
                break;
            case RoomType.Playground:
                break;
        }
    }
}
