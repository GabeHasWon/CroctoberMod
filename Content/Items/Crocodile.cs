
namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class Crocodile : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(50, 32);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<CrocodilePlayer>().active = SportsMode;
    }
}

internal class CrocodilePlayer : ModPlayer
{
    public bool? active = null;

    public override void ResetEffects() => active = null;

    public override void PostUpdateEquips()
    {
        if (active is null)
        {
            return;
        }

        Player.breathEffectiveness += 2f;

        if (!Player.wet)
        {
            return;
        }

        if (active.Value)
        {
            Player.accFlipper = true;
            Player.GetDamage(DamageClass.Melee) += 0.4f;
        }
        else
        {
            Player.ignoreWater = true;
            Player.moveSpeed += 0.2f;
            Player.GetDamage(DamageClass.Generic) += 0.1f;
        }
    }
}
