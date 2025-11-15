using CroctoberMod.Content.Syncing;
using System.Collections.Generic;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;

namespace CroctoberMod.Content.Items;

internal abstract class Croc : ModItem
{
    protected override bool CloneNewInstances => true;

    public static Dictionary<int, (LocalizedText on, LocalizedText off, LocalizedText jibbit, LocalizedText jibbitSports)> TooltipsByType = [];

    protected readonly static object[] EmptyObjects = [];

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
        player.GetModPlayer<CrocPlayer>().Crocs.Add(Type, Item);

        if (Main.myPlayer == player.whoAmI && Main.HoverItem.ModItem is Croc croc && croc.Equipped && Main.mouseMiddle && Main.mouseMiddleRelease)
        {
            SportsMode = !SportsMode;

            SoundEngine.PlaySound(new SoundStyle("CroctoberMod/Assets/Sound/SoftFlip"));

            if (Main.netMode != NetmodeID.SinglePlayer)
                Item.NetStateChanged();
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

        tooltips.Insert(1, SportsMode ? new TooltipLine(Mod, "SportsOn", TooltipsByType[Type].on.Format(GetSportsArgs())) 
            : new TooltipLine(Mod, "SportsOff", TooltipsByType[Type].off.Format(GetSportsArgs())));

        if (Main.LocalPlayer.GlimmeringJibbit())
        {
            LocalizedText line = SportsMode ? TooltipsByType[Type].jibbitSports : TooltipsByType[Type].jibbit;
            tooltips.Insert(4, new TooltipLine(Mod, "Jibbit", line.Format(GlimmeringJibbit.GetShimmerGradient().Hex3())));
        }
    }

    /// <summary>
    /// Determines what is passed to the "SportsOn/Off" tooltip line.
    /// </summary>
    protected virtual object[] GetSportsArgs() => EmptyObjects;

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

        spriteBatch.Draw(tex, Item.Center - Main.screenPosition, frame, lightColor, rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0);
        return false;
    }

    public override void NetSend(BinaryWriter writer) => writer.Write(SportsMode);
    public override void NetReceive(BinaryReader reader) => SportsMode = reader.ReadBoolean();
}

public class CrocPlayer : ModPlayer
{
    public bool HasCroc => Crocs.Count > 0;

    public Dictionary<int, Item> Crocs = [];

    public override void ResetEffects() => Crocs.Clear();
}