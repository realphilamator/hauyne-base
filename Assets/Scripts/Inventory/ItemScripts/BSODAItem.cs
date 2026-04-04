// ─────────────────────────────────────────────────────────────────────────────
// Item 04 — BSODA
// ─────────────────────────────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(fileName = "Item_04_BSODA", menuName = "Items/Definitions/BSODA")]
public class BSODAItem : BaseItem
{
    [Tooltip("The BSODA spray projectile prefab.")]
    public GameObject bsodaSpray;

    [Tooltip("Audio clip played when the can is sprayed.")]
    public AudioClip aud_Soda;

    public override bool Use(ItemUseContext ctx, System.Action onConsumed)
    {
        //Object.Instantiate(bsodaSpray, ctx.playerTransform.position, ctx.cameraTransform.rotation);
        ctx.audioSource.PlayOneShot(aud_Soda);
        onConsumed();
        return true;
    }
}
