using UnityEngine;
using UnityEngine.AI;

public class SwingDoor : MonoBehaviour
{
    public bool isOpen = false;
    public float openDuration = 3f;
    [SerializeField] MeshRenderer inDoor, outDoor;
    [SerializeField] Material openMat, closedMat, lockedMat;
    float openTime = 0f;
    bool isLocked = false;
    float lockTime = 0f;

    private void Update()
    {
        if (isLocked)
        {
            lockTime -= Time.deltaTime;
            if (lockTime <= 0f)
            {
                isLocked = false;
                GetComponent<NavMeshObstacle>().enabled = false;
                inDoor.material = closedMat;
                outDoor.material = closedMat;
            }
        }

        if (isOpen)
        {
            openTime -= Time.deltaTime;
            if (openTime <= 0f)
            {
                CloseDoor();
            }
        }
    }

    void OpenDoor()
    {
        isOpen = true;
        inDoor.material = openMat;
        outDoor.material = openMat;
        openTime = openDuration;
    }

    void CloseDoor()
    {
        isOpen = false;
        inDoor.material = closedMat;
        outDoor.material = closedMat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC") || other.CompareTag("Player") && !isOpen && !isLocked)
        {
            OpenDoor();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("NPC") && isOpen && !isLocked)
        {
            openTime = openDuration;
        }
    }

    public void LockDoor(float duration)
    {
        isLocked = true;
        lockTime = duration;
        CloseDoor();
        inDoor.material = lockedMat;
        outDoor.material = lockedMat;
        GetComponent<NavMeshObstacle>().enabled = true;
    }
}