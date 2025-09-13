using Terraria.DataStructures;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class Hellcroc : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(50, 32);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<HellPlayer>().active = SportsMode;
    }
}

internal class HellPlayer : ModPlayer
{
    public bool? active = null;

    public override void ResetEffects() => active = null;

    public override void PostUpdateEquips()
    {
        if (active is null)
        {
            return;
        }

        if (active is true && Player.lavaWet)
        {
            Player.accFlipper = true;

            if (Player.GlimmeringJibbit())
                Player.ignoreWater = true;
        }

        if (Main.myPlayer == Player.whoAmI)
        {
            bool aboveLava = AboveLava();

            if (!aboveLava || active is not false)
                return;

            if (!Player.controlDown)
            {
                Player.gravity = 0;

                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Player.whoAmI);
            }

            if (Player.GlimmeringJibbit() && Player.controlUp && Player.velocity.Y > -6)
                Player.velocity.Y = MathHelper.Max(Player.velocity.Y - 0.2f, -6);
        }
    }

    public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
    {
        if (active is true && damageSource.SourceOtherIndex == 2)
            return false;

        return true;
    }

    private bool AboveLava()
    {
        Point position = Player.Bottom.ToTileCoordinates();
        int originalY = position.Y;

        while (true)
        {
            Tile tile = Main.tile[position];

            if (WorldGen.SolidTile(tile) || tile.LiquidAmount > 0 && tile.LiquidType != LiquidID.Lava)
                return false;

            if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Lava)
                return true;

            if (position.Y > Main.maxTilesY - 20 || position.Y > originalY + 50)
                return false;

            position.Y++;
        }
    }
}
