using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;

namespace CroctoberMod.Content.Items;

internal abstract class Croc : ModItem
{
    protected override bool CloneNewInstances => true;

    public static Dictionary<int, (LocalizedText on, LocalizedText off)> TooltipsByType = [];

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
        TooltipsByType.Add(Type, (this.GetLocalization("SportsOn"), this.GetLocalization("SportsOff")));
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
