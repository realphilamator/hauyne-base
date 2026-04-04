using UnityEngine;

/// <summary>
/// Handles camera behaviour for a first-person player.
/// Attach this script directly to the Main Camera, which should be a child of the Player.
///
/// Hierarchy:
///   Player          (PlayerController)
///     Main Camera   (this script)
/// </summary>
public class CameraScript : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------------------------

    /// <summary>The player GameObject this camera belongs to. Assign in the Inspector.</summary>
    public GameObject player;

    /// <summary>
    /// Reference to the PlayerController. Not used directly by this script,
    /// but exposed for other scripts that extend camera behaviour (e.g. checking stamina).
    /// </summary>
    public PlayerController ps;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    /// <summary>
    /// Current look-behind yaw offset in degrees. 0 = forward, 180 = behind.
    /// Snaps instantly — lerp this value if you want a smooth turn animation.
    /// </summary>
    private int lookBehind;

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    private void Update()
    {
        // Snap to look behind while the action key is held, return forward when released
        lookBehind = Singleton<InputManager>.Instance.GetActionKey(InputAction.LookBehind) ? 180 : 0;
    }

    private void LateUpdate()
    {
        // Apply look-behind on top of the player's current rotation.
        // LateUpdate ensures this runs after PlayerController has updated the player's rotation.
        transform.rotation = player.transform.rotation * Quaternion.Euler(0f, (float)lookBehind, 0f);
    }
}
