namespace CroctoberMod.Content.Items;

internal abstract class Vial : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 3;

    public override void SetDefaults()
    {
        Item.buffTime = 60 * 30;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.buyPrice(0, 0, 2, 0);
        Item.useAnimation = Item.useTime = 30;
        Item.placeStyle = ItemUseStyleID.DrinkLiquid;
    }
}

internal class JustifiedVial : Vial
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.buffType = ModContent.BuffType<JustifiedBuff>();
    }
}

internal class JustifiedBuff : ModBuff
{
    public override void Update(Player player, ref int buffIndex) => player.statLifeMax2 += 50;
}

internal class SturdyVial : Vial
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.buffType = ModContent.BuffType<SturdyBuff>();
    }
}

internal class SturdyBuff : ModBuff
{
    public override void Update(Player player, ref int buffIndex) => player.statDefense += 4;
}

internal class FuriousVial : Vial
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.buffType = ModContent.BuffType<FuriousBuff>();
    }
}

internal class FuriousBuff : ModBuff
{
    public override void Update(Player player, ref int buffIndex) => player.GetDamage(DamageClass.Generic) += 0.05f;
}

internal class GlimmeringVial : Vial
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.buffType = ModContent.BuffType<GlimmeringBuff>();
        Item.rare = ItemRarityID.Green;
    }
}

internal class GlimmeringBuff : ModBuff
{
    public override void Update(Player player, ref int buffIndex)
    {
        player.GetDamage(DamageClass.Generic).Flat++;
        player.GetDamage(DamageClass.Generic) += 0.04f;
        player.endurance += 0.02f;
        player.statDefense += 2;
        player.statLifeMax2 += 25;
    }
}