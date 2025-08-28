using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;

namespace CroctoberMod.Content.Items;

internal abstract class Croc : ModItem
{
    protected override bool CloneNewInstances => true;

    public static Dictionary<int, (LocalizedText on, LocalizedText off, LocalizedText jibbit, LocalizedText jibbitSports)> TooltipsByType = [];

    public bool SportsMode = false;
    public bool Equipped = false;

    public override ModItem Clone(Item newEntity)
    {
        var croc = base.Clone(newEntity) as Croc;
        croc.SportsMode = SportsMode;
        return croc;
    }

    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(2, 2) { NotActuallyAnimating = true });
        TooltipsByType.Add(Type, (this.GetLocalization("SportsOn"), this.GetLocalization("SportsOff"), this.GetLocalization("Jibbit"), this.GetLocalization("JibbitSports")));

        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<GlimmeringJibbit>();
    }

    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new Vector2(34, 22);
        Item.value = Item.buyPrice(0, 0, 50, 0);
        Item.rare = ItemRarityID.Blue;
    }

    public override void UpdateInventory(Player player) => Equipped = false;

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        Equipped = true;
        player.GetModPlayer<CrocPlayer>().hasCroc = true;

        if (Main.myPlayer == player.whoAmI && Main.HoverItem.ModItem is Croc croc && croc.Equipped && Main.mouseMiddle && Main.mouseMiddleRelease)
        {
            SportsMode = !SportsMode;

            SoundEngine.PlaySound(new SoundStyle("CroctoberMod/Assets/Sound/SoftFlip"));
        }
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (Equipped)
        {
            tooltips.Insert(1, new TooltipLine(Mod, "MiddleClick", Language.GetTextValue("Mods.CroctoberMod.MiddleClickToFlip",
                Language.GetTextValue("Mods.CroctoberMod." + (SportsMode ? "Enable" : "Disable")))));
        }

        tooltips.Insert(1, new TooltipLine(Mod, "SportsMode", Language.GetTextValue("Mods.CroctoberMod.SportsModeTooltip",
            Language.GetTextValue("Mods.CroctoberMod." + (SportsMode ? "On" : "Off")))));

        tooltips.Insert(1, SportsMode ? new TooltipLine(Mod, "SportsOn", TooltipsByType[Type].on.Value) : new TooltipLine(Mod, "SportsOff", TooltipsByType[Type].off.Value));

        if (Main.LocalPlayer.GlimmeringJibbit())
        {
            LocalizedText line = SportsMode ? TooltipsByType[Type].jibbitSports : TooltipsByType[Type].jibbit;
            tooltips.Insert(4, new TooltipLine(Mod, "Jibbit", line.Format(GlimmeringJibbit.GetShimmerGradient().Hex3())));
        }
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D tex = TextureAssets.Item[Type].Value;

        if (SportsMode)
        {
            frame.Y = tex.Height / 2;
        }

        spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0);
        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Texture2D tex = TextureAssets.Item[Type].Value;
        Rectangle frame = new(0, 0, tex.Width, tex.Height / 2);

        if (SportsMode)
        {
            frame.Y = tex.Height / 2;
        }

        spriteBatch.Draw(tex, Item.Center - Main.screenPosition, frame, lightColor, 0f, frame.Size() / 2f, scale, SpriteEffects.None, 0);
        return false;
    }
}

public class CrocPlayer : ModPlayer
{
    public bool hasCroc = false;

    public override void ResetEffects() => hasCroc = false;
}