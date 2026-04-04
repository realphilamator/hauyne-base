using UnityEngine;

/// <summary>
/// Contains the scene references passed into BaseItem.Use() every time an item is used.
/// This is intentionally kept minimal — only references that are universally useful
/// to any item in any game belong here.
///
/// If your item needs something not listed here (e.g. a specific NPC script, a prefab,
/// an audio clip), declare it as a field on your BaseItem subclass instead. That way
/// each item is fully self-contained and this class never needs to grow.
/// </summary>
public class ItemUseContext
{
    /// <summary>The player. Use this to read or modify player state (e.g. stamina, canMove).</summary>
    public PlayerController player;

    /// <summary>The player's Transform. Use this for position and direction calculations.</summary>
    public Transform playerTransform;

    /// <summary>The main camera's Transform. Use this for raycast direction or spawn orientation.</summary>
    public Transform cameraTransform;

    /// <summary>
    /// A general-purpose AudioSource on the player for playing item sounds.
    /// For items with specific sounds, store the AudioClip on the item itself and play it here.
    /// </summary>
    public AudioSource audioSource;
}
