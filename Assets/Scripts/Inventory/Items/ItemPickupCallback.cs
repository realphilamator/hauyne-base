using UnityEngine;

/// <summary>
/// Attach to a pickup GameObject to run custom logic when the item is collected,
/// without modifying PickupScript or BaseItem.
///
/// Example uses:
///   - Playing a pickup sound at the pickup's position
///   - Spawning a particle effect
///   - Triggering a world event
///
/// Example:
///
///     public class CoinPickupCallback : ItemPickupCallback
///     {
///         public AudioClip coinSound;
///
///         public override void OnPickup()
///         {
///             AudioSource.PlayClipAtPoint(coinSound, transform.position);
///         }
///     }
/// </summary>
public class ItemPickupCallback : MonoBehaviour
{
    /// <summary>
    /// Called by PickupScript immediately after the item is collected.
    /// Override this in a subclass to add custom pickup behaviour.
    /// </summary>
    public virtual void OnPickup() { }
}
