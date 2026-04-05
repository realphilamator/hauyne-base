using UnityEngine;

public class StandardDoor : MonoBehaviour
{
    public bool isOpen = false;
    public bool locked = false;
    public float openDuration = 3f;
    public float interactionDistance = 10f;

    [SerializeField] MeshRenderer inDoor, outDoor;
    [SerializeField] Material openMat, closedMat;

    [System.Serializable]
    public struct DoorSound
    {
        public AudioClip clip;
        public string subtitleKey;
    }

    [SerializeField] DoorSound openSound = new DoorSound { subtitleKey = "Sfx_Doors_StandardOpen" };
    [SerializeField] DoorSound closeSound = new DoorSound { subtitleKey = "Sfx_Doors_StandardShut" };
    [SerializeField] DoorSound lockedSound = new DoorSound { subtitleKey = "Sfx_Doors_StandardLocked" };
    [SerializeField] DoorSound lockSound = new DoorSound { subtitleKey = "Sfx_Doors_StandardLock" };
    [SerializeField] DoorSound unlockSound = new DoorSound { subtitleKey = "Sfx_Doors_StandardUnlock" };

    private ManagedAudioSource managedAudioSource;
    private AudioSource audioSource;
    float openTime = 0f;
    bool isLocked = false;
    float lockTime = 0f;

    private void Start()
    {
        managedAudioSource = gameObject.GetComponentInChildren<ManagedAudioSource>();
        audioSource = gameObject.GetComponentInChildren<AudioSource>();
    }

    private void Update()
    {
        if (isLocked)
        {
            lockTime -= Time.deltaTime;
            if (lockTime <= 0f)
            {
                isLocked = false;
            }
        }

        if (isOpen)
        {
            openTime -= Time.deltaTime;
            if (openTime <= 0f)
                CloseDoor();
        }
        Interact();
    }

    void Interact()
    {
        if (isLocked)
            return;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)); // RAYCAST IS PLACEHOLDER TOO BLAH BLAH BLAH
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance) && Input.GetMouseButtonDown(0) && Time.timeScale != 0f)
        {
            if (hit.collider.gameObject == gameObject && Vector3.Distance(player.position, transform.position) <= interactionDistance)
                OpenDoor();
        }
    }

    void OpenDoor()
    {
        if (locked)
        {
            PlaySound(lockedSound);
            return;
        }
        if (!isOpen) PlaySound(openSound);
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
        PlaySound(closeSound);
        inDoor.material = closedMat;
        outDoor.material = closedMat;
    }

    [ContextMenu("Lock Door")]
    void LockDoorContext() => LockDoor(true);

    [ContextMenu("Unlock Door")]
    void UnlockDoorContext() => LockDoor(false);

    public void LockDoor(bool lockState)
    {
        locked = lockState;
        PlaySound(lockState ? lockSound : unlockSound);
    }

    void PlaySound(DoorSound sound)
    {
        if (sound.clip != null)
            audioSource.clip = sound.clip;
        managedAudioSource.Play(sound.subtitleKey);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC") && !isOpen && !isLocked)
        {
            OpenDoor();
    }

    private void OnTriggerStay(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("NPC")) && isOpen)
            openTime = openDuration;
    }

    public void LockDoor(float duration)
    {
        CloseDoor();
        isLocked = true;
        lockTime = duration;
    }
}