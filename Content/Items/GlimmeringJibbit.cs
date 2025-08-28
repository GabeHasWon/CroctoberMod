using ReLogic.Content;
using System;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace CroctoberMod.Content.Items;

internal class GlimmeringJibbit : ModItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.Ambrosia);
        Item.Size = new Vector2(50, 32);
        Item.consumable = true;
    }

    public override bool? UseItem(Player player) => player.GetModPlayer<GlimmeringPlayer>().usedJibbit = true;

    // Copied from the same implementation in SpiritReforged.
    internal static Color GetShimmerGradient()
    {
        float factor = MathF.Sin(Main.GameUpdateCount * 0.03f) * 0.5f + 0.5f;
        var color = Color.Lerp(Color.Lerp(Color.White, new Color(150, 214, 245), factor), Color.Lerp(new Color(150, 214, 245), new Color(240, 146, 251), factor), factor);

        return color;
    }
}

internal class GlimmeringPlayer : ModPlayer
{
    public bool usedJibbit = false;

    public override void SaveData(TagCompound tag) => tag.Add("usedJibbit", usedJibbit);
    public override void LoadData(TagCompound tag) => usedJibbit = tag.GetBool("usedJibbit");
}

internal static class GlimmeringExtension
{
    public static bool GlimmeringJibbit(this Player player) => player.GetModPlayer<GlimmeringPlayer>().usedJibbit;
    public static float JibbitModifier(this Player player, float off, float on) => player.GetModPlayer<GlimmeringPlayer>().usedJibbit ? on : off;
}

internal class JibbitLayer : PlayerDrawLayer
{
    private static MethodInfo DrawSittingLegsInfo = null;
    private static MethodInfo ShouldOverrideLegsCheckPantsInfo = null;
    private static Asset<Texture2D> GlimmeringJibbitEquip = null;

    public override void Load()
    {
        DrawSittingLegsInfo = typeof(PlayerDrawLayers).GetMethod("DrawSittingLegs", BindingFlags.NonPublic | BindingFlags.Static);
        ShouldOverrideLegsCheckPantsInfo = typeof(PlayerDrawLayers).GetMethod("ShouldOverrideLegs_CheckPants", BindingFlags.NonPublic | BindingFlags.Static);
        GlimmeringJibbitEquip = ModContent.Request<Texture2D>("CroctoberMod/Content/Items/GlimmeringJibbit_Shoes");
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Shoes);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Player plr = drawInfo.drawPlayer;

        if (plr.shoe <= 0 || !plr.GetModPlayer<CrocPlayer>().hasCroc || ShouldOverrideLegsCheckPantsInfo.Invoke(null, [drawInfo]) is bool x && x)
            return;

        if (drawInfo.isSitting)
        {
            DrawSittingLegsInfo.Invoke(null, [drawInfo, GlimmeringJibbitEquip.Value, drawInfo.colorArmorLegs, plr.cShoe]);
            return;
        }

        int xPos = (int)(drawInfo.Position.X - drawInfo.drawPlayer.legFrame.Width / 2f + drawInfo.drawPlayer.width / 2);
        int yPos = (int)(drawInfo.Position.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.legFrame.Height + 4f);
        var pos = new Vector2(xPos, yPos) - Main.screenPosition;
        Vector2 finalPos = pos + plr.legPosition + drawInfo.legVect;
        Color color = Color.Lerp(drawInfo.colorArmorLegs, Color.Pink, MathF.Sin((float)Main.timeForVisualEffects * 0.03f) * 0.25f + 0.75f);
        DrawData item = new(GlimmeringJibbitEquip.Value, finalPos, plr.legFrame, color, plr.legRotation, drawInfo.legVect, 1f, drawInfo.playerEffect);
        item.shader = plr.cShoe;

        drawInfo.DrawDataCache.Add(item);
    }
}