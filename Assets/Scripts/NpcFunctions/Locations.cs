using System;

[Flags]
public enum Locations
{
    // Be careful when changing or adding more locations, you need to add the new location by the power of 2.
    Hallway = 1,
    Classrooms = 2,
    FacultyRooms = 4
}