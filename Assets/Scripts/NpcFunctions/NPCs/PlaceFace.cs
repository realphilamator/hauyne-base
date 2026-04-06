using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlaceFace : NPC
{
    float goneCrazyTimer = 0f;


    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        coolDown = Mathf.Max(0f, coolDown - Time.deltaTime);

        if (goneCrazyTimer > 0f)
        {
            goneCrazyTimer -= Time.deltaTime;
            if (goneCrazyTimer <= 0f)
            {
                agent.speed = 20f;
                goneCrazyTimer = 0f;
            }
        }

        if (spottedPlayer && goneCrazyTimer <= 0f)
        {
            agent.SetDestination(player.transform.position); // Move towards the player
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f && coolDown <= 0f) // If the NPC is not currently moving towards a destination and the cooldown has expired, it gets a new location to move towards
            {
                Vector3 newLocation = locationSelector.GetLocation(locations);
                int attempts = 0;

                while (Vector3.Distance(transform.position, newLocation) < 1f && attempts < 10) // If the new location is too close to the current position, it gets a new location
                {
                    newLocation = locationSelector.GetLocation(locations);
                    attempts++;
                }

                agent.SetDestination(newLocation);
                coolDown = 0.5f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && goneCrazyTimer <= 0f) // Replica of Plus's Crazy Action from the Test NPC
        {
            goneCrazyTimer = 15f;
            agent.speed = 99f;
            Debug.Log("OOOOOOOOOOOOOOOOOOOOOHHHHHHHHHH");
        }
    }
}
