using UnityEngine;

[CreateAssetMenu(fileName = "Item_12_Apple", menuName = "Items/Definitions/Apple For Baldi")]
public class AppleItem : BaseItem
{
    public override bool Use(ItemUseContext ctx, System.Action onConsumed)
    {
        return false;
    }
}