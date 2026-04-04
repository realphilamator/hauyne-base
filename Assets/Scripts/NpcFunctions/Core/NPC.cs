using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    protected float coolDown = 0.5f;
    protected NavMeshAgent agent;
    protected LocationSelector locationSelector;
    protected PlayerController player;
    public Locations locations = Locations.Hallway;
    public bool spottedPlayer = false;

    protected virtual void Awake() // Setting up references
    {
        locationSelector = FindObjectOfType<LocationSelector>();
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<PlayerController>();
    }

    protected virtual void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, player.transform.position - transform.position); // Raycast from the NPC's head to the player's position
        if (Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                spottedPlayer = true;
            }
            else
            {
                spottedPlayer = false;
            }
        }
    }
}
