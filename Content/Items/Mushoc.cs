using System.Collections.Generic;
using Terraria.DataStructures;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class Mushoc : Croc
{
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<MushocPlayer>().active = SportsMode;
    }
}

internal class MushocPlayer : ModPlayer
{
    public bool? active = null;

    private Dictionary<Point16, int> ProjectilesPerTwoTiles = [];
        
    public override void PostUpdateEquips()
    {
        if (active is false)
        {
            Point16 floor = (Player.Bottom / 2f).ToTileCoordinates16();

            if (Collision.SolidCollision(Player.BottomLeft, Player.width, 6) && !ProjectilesPerTwoTiles.ContainsKey(floor))
            {
                ProjectilesPerTwoTiles.Add(floor, Projectile.NewProjectile());
            }
        }
    }
}

internal class MushocMushroom : ModProjectile
{

}
