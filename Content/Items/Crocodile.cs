
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

        Player.breathEffectiveness += Player.JibbitModifier(2, 2.5f);

        if (!Player.wet)
        {
            return;
        }

        if (active.Value)
        {
            Player.accFlipper = true;
            Player.GetDamage(DamageClass.Melee) += Player.JibbitModifier(0.4f, 0.6f);
        }
        else
        {
            Player.ignoreWater = true;
            Player.moveSpeed += Player.JibbitModifier(0.2f, 0.25f);
            Player.GetDamage(DamageClass.Generic) += Player.JibbitModifier(0.1f, 0.2f);
        }
    }
}
