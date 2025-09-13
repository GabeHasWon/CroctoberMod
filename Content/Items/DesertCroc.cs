using Terraria.DataStructures;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class DesertCrocs : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(32, 24);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<DesertPlayer>().active = SportsMode;

        if (!SportsMode && player.GlimmeringJibbit())
        {
            const int Distance = 40;

            Point16 center = player.Center.ToTileCoordinates16();
            Vector3 baseColor = new(1.4f, 0.5f, 0.5f);

            for (int i = center.X - Distance; i < center.X + Distance; ++i)
            {
                for (int j = center.Y - Distance; j < center.Y + Distance; ++j)
                {
                    Tile tile = Main.tile[i, j];

                    if (tile.HasTile && TileID.Sets.IsAMechanism[tile.TileType])
                    {
                        Vector3 color = baseColor * (1 - Vector2.Distance(new Vector2(i, j), center.ToVector2()) / Distance);
                        Vector2 position = new Vector2(i, j).ToWorldCoordinates();
                        Lighting.AddLight(position, color);
                    }
                }
            }
        }
    }
}

internal class DesertPlayer : ModPlayer
{
    public bool? active = null;

    public override void ResetEffects() => active = null;

    public override bool CanBeHitByProjectile(Projectile proj) => active is not false || !proj.trap;

    public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
    {
        if (active is true && proj.trap)
            modifiers.FinalDamage -= Player.JibbitModifier(0.5f, 0.75f);
    }
}

internal class DesertNPC : GlobalNPC
{
    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if (projectile.trap && HasNearbyTrapBuffPlayer(npc.Center, out bool jibbit))
            modifiers.FinalDamage += jibbit ? 0.9f : 0.6f;
    }

    private static bool HasNearbyTrapBuffPlayer(Vector2 center, out bool isJibbit)
    {
        bool hasPlayer = false;

        foreach (Player player in Main.ActivePlayers)
        {
            if (player.DistanceSQ(center) < 800 * 800)
            {
                isJibbit = player.GlimmeringJibbit();
                hasPlayer = true;

                if (isJibbit)
                    return true;
            }
        }

        isJibbit = false;
        return hasPlayer;
    }
}
