using System;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.Utilities;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class TheGoldenCroc : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(36, 36);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<GoldenPlayer>().active = SportsMode;
    }
}

internal class GoldenPlayer : ModPlayer
{
    public static WeightedRandom<(int type, Range stackRange)> DropPool;

    public bool? active = null;
    public bool hasHit = false;

    public override void Load()
    {
        DropPool = new(Main.rand);
        DropPool.Add((ItemID.None, 0..0), 5f);
        DropPool.Add((ItemID.CopperCoin, 10..50), 15f);
        DropPool.Add((ItemID.SilverCoin, 2..8), 2f);
        DropPool.Add((ItemID.GoldCoin, 1..2), 0.2f);
        DropPool.Add((ItemID.PlatinumCoin, 1..1), 0.01f);

        DropPool.Add((ItemID.Amethyst, 1..3));
        DropPool.Add((ItemID.Topaz, 1..3));
        DropPool.Add((ItemID.Sapphire, 1..2), 0.8f);
        DropPool.Add((ItemID.Emerald, 1..2), 0.8f);
        DropPool.Add((ItemID.Ruby, 1..1), 0.6f);
        DropPool.Add((ItemID.Amber, 1..1), 0.6f);
        DropPool.Add((ItemID.Diamond, 1..1), 0.5f);
    }

    public override void ResetEffects() => active = null;

    public override void PostUpdateEquips()
    {
        if (active is null)
            return;

        Vector2 head = Player.position - new Vector2(0, 6);

        if (Player.velocity.Y == 0)
            hasHit = true;
        else if (Player.velocity.Y < 1 && Collision.SolidCollision(head, Player.width, 6) && hasHit)
        {
            Point16 tilePos = head.ToTileCoordinates16();
            int pick = Player.GetBestPickaxe().pick;
            hasHit = false;

            if (active is true)
                pick /= 2;

            PickAtTile(tilePos.X, tilePos.Y, pick);
            PickAtTile(tilePos.X + 1, tilePos.Y, pick);
        }
    }

    private void PickAtTile(int x, int y, int pick)
    {
        if (Player.HasEnoughPickPowerToHurtTile(x, y) && WorldGen.SolidTile(x, y))
        {
            Player.PickTile(x, y, pick);
            Player.velocity.Y = 0.001f;

            if (active is false && Main.rand.NextFloat() < Player.JibbitModifier(0, 0.1f))
                return;

            Tile tile = Main.tile[x, y];

            if (!tile.HasTile)
            {
                Vector2 velocity = WorldGen.SolidTile(x, y - 1) ? new Vector2(0, Main.rand.NextFloat()) : new Vector2(0, Main.rand.NextFloat(-8, -6f));
                (int type, Range stackRange) = DropPool.Get();
                
                if (Player.GlimmeringJibbit() && type == ItemID.None)
                    (type, stackRange) = DropPool.Get();

                if (type == ItemID.None)
                    return;

                int stack = Main.rand.Next(stackRange.Start.Value, stackRange.End.Value + 1);
                int item = Item.NewItem(new EntitySource_TileBreak(x, y), new Rectangle(x * 16, y * 16, 16, 16), type, stack);
                Main.item[item].velocity = velocity;
                Main.item[item].noGrabDelay = 80;
            }
        }
    }
}