
namespace CroctoberMod.Content.Items;

internal class Crocodile : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(50, 32);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => base.UpdateAccessory(player, hideVisual);
}
