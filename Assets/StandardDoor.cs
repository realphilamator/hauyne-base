using UnityEngine;

public class StandardDoor : MonoBehaviour
{
    public bool isOpen = false;
    public float openDuration = 3f;
    public float interactionDistance = 10f;
    [SerializeField] MeshRenderer inDoor, outDoor;
    [SerializeField] Material openMat, closedMat;
    float openTime = 0f;

    private void Update()
    {
        if (isOpen)
        {
            openTime -= Time.deltaTime;
            if (openTime <= 0f)
            {
                CloseDoor();
            }
        }

        Interact();
    }

    void Interact() // PLACEHOLDER VOID, THERE MUST BE A PLAYER INTERACTION SCRIPT THAT CALLS INTERACTION FUNCTIONS LIKE THIS ONE
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)); // RAYCAST IS PLACEHOLDER TOO BLAH BLAH BLAH
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance) && Input.GetMouseButtonDown(0) && Time.timeScale != 0f)
        {
            if (hit.collider.gameObject == gameObject && Vector3.Distance(player.position, transform.position) <= interactionDistance)
            {
                OpenDoor();
            }
        }
    }

    void OpenDoor()
    {
        isOpen = true;
        inDoor.GetComponent<MeshCollider>().enabled = false;
        outDoor.GetComponent<MeshCollider>().enabled = false;
        inDoor.material = openMat;
        outDoor.material = openMat;
        openTime = openDuration;
    }

    void CloseDoor()
    {
        isOpen = false;
        inDoor.GetComponent<MeshCollider>().enabled = true;
        outDoor.GetComponent<MeshCollider>().enabled = true;
        inDoor.material = closedMat;
        outDoor.material = closedMat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC") && !isOpen)
        {
            OpenDoor();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("NPC") && isOpen)
        {
            openTime = openDuration;
        }
    }
}
}