using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationSelector : MonoBehaviour
{
    [SerializeField] Transform[] hallPoints, classPoints, facultyPoints;

    bool isValidLocation(Locations location) // Checks if the location is valid by checking if it has at least one of the locations selected
    {
        return (location & Locations.Hallway) != 0 ||
               (location & Locations.Classrooms) != 0 ||
               (location & Locations.FacultyRooms) != 0;
    }

    public Vector3 GetLocation(Locations location)
    {
        if (!isValidLocation(location)) // If the location is not valid, it logs an error and returns a zero vector
        {
            Debug.LogError("Invalid location requested: " + location);
            return Vector3.zero;
        }

        List<Vector3> possibleLocations = new List<Vector3>(); // List to store all possible locations based on the selected flags
        if ((location & Locations.Hallway) != 0) // Checks if the location has the hallway flag, if it does it adds all the hallway points to the possible locations list
        {
            foreach (Transform point in hallPoints)
            {
                possibleLocations.Add(point.position);
            }
        }

        if ((location & Locations.Classrooms) != 0) // Ditto for classrooms
        {
            foreach (Transform point in classPoints)
            {
                possibleLocations.Add(point.position);
            }
        }

        if ((location & Locations.FacultyRooms) != 0) // Ditto for faculty rooms
        {
            foreach (Transform point in facultyPoints)
            {
                possibleLocations.Add(point.position);
            }
        }

        if (possibleLocations.Count == 0) // If there are no possible locations, it logs an error and returns a zero vector
        {
            Debug.LogError("No valid locations found for: " + location);
            return Vector3.zero;
        }

        int randomLocation = Random.Range(0, possibleLocations.Count); // Selects a random location from the possible locations list
        return possibleLocations[randomLocation];
    }
}
