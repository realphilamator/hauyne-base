using UnityEngine;

public class StandardDoor : MonoBehaviour, IInteractable
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

    float openTime = 0f;
    bool isLocked = false;
    float lockTime = 0f;

    // IInteractable
    public float InteractionDistance => interactionDistance;

    public bool CanInteract(GameObject interactor)
    {
        return !isLocked;
    }

    public void Interact(GameObject interactor)
    {
        OpenDoor();
    }

    private void Start()
    {
        managedAudioSource = gameObject.GetComponentInChildren<ManagedAudioSource>();
    }

    private void Update()
    {
        if (isLocked)
        {
            lockTime -= Time.deltaTime;
            if (lockTime <= 0f)
                isLocked = false;
        }

        if (isOpen)
        {
            openTime -= Time.deltaTime;
            if (openTime <= 0f)
                CloseDoor();
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
        inDoor.GetComponent<BoxCollider>().enabled = false;
        outDoor.GetComponent<BoxCollider>().enabled = false;
        inDoor.material = openMat;
        outDoor.material = openMat;
        openTime = openDuration;
    }

    void CloseDoor()
    {
        isOpen = false;
        inDoor.GetComponent<BoxCollider>().enabled = true;
        outDoor.GetComponent<BoxCollider>().enabled = true;
        PlaySound(closeSound);
        inDoor.material = closedMat;
        outDoor.material = closedMat;
    }

    [ContextMenu("Lock Door")]
    void LockDoorContext() => LockDoor(true);
    [ContextMenu("Unlock Door")]
    void UnlockDoorContext() => LockDoor(false);

    public void LockDoor(bool lockState, float duration = 15f)
    {
        locked = lockState;
        PlaySound(lockState ? lockSound : unlockSound);
        if (lockState)
        {
            isLocked = true;
            lockTime = duration;
        }
        else
        {
            isLocked = false;
        }
    }

    void PlaySound(DoorSound sound)
    {
        managedAudioSource.SetClipAndPlay(sound.clip, sound.subtitleKey);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC") && !isOpen && !isLocked)
            OpenDoor();
    }

    private void OnTriggerStay(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("NPC")) && isOpen)
            openTime = openDuration;
    }
}