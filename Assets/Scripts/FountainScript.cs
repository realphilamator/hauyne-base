using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FountainScript : MonoBehaviour, IInteractable
{
    public PlayerController player;

    [SerializeField] AudioClip sip;
    private ManagedAudioSource managedAudio;
    private AudioSource audioSource;

    public float interactionDistance = 10f;

    float IInteractable.InteractionDistance => interactionDistance;

    public bool CanInteract(GameObject interactor)
    {
        return true;
    }

    public void Interact(GameObject interactor)
    {
        player.stamina = player.maxStamina;
        managedAudio.Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        managedAudio = gameObject.GetComponentInChildren<ManagedAudioSource>();
        audioSource = gameObject.GetComponentInChildren<AudioSource>();

        audioSource.clip = sip;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
