using UnityEngine;
using NaughtyAttributes;

public class PickupScript : MonoBehaviour
{
    [BoxGroup("References")]
    [Required]
    [Tooltip("The ItemRegistry asset containing all item definitions.")]
    public ItemRegistry itemRegistry;

    [BoxGroup("References")]
    [Required]
    [Tooltip("The player's Transform, used for distance checks.")]
    public Transform playerTransform;
}