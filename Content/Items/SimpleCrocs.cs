using Terraria.DataStructures;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class SimpleCrocs : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(44, 26);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<SimplePlayer>().active = SportsMode;
    }
}

internal class SimplePlayer : ModPlayer
{
    public bool? active = null;

    public override void Load() => On_Player.GetPickaxeDamage += ModifyPickaxeDamage;

    private int ModifyPickaxeDamage(On_Player.orig_GetPickaxeDamage orig, Player self, int x, int y, int pickPower, int hitBufferIndex, Tile tileTarget)
    {
        int power = orig(self, x, y, pickPower, hitBufferIndex, tileTarget);
        bool? active = self.GetModPlayer<SimplePlayer>().active;

        // Vanilla uses power == 100 for Main.tileNoFail, so I avoid that
        if (active is true && power != 100)
        {
            Point16 pos = (self.BottomLeft + new Vector2(4, 4)).ToTileCoordinates16();

            if ((pos.X == x || pos.X + 1 == x) && pos.Y == y)
                power = (int)(power * self.JibbitModifier(2f, 2.5f));
        }

        return power;
    }

    public override void ResetEffects() => active = null;

    public override void PostUpdateEquips()
    {
        if (active is null)
        {
            return;
        }
    }

    public override float UseSpeedMultiplier(Item item)
    {
        Point16 pos = Player.BottomLeft.ToTileCoordinates16();

        if (item.createTile != -1 && active is false && Player.tileTargetY == pos.Y)
            return Player.JibbitModifier(1.6f, 2f);

        return 1f;
    }
}
