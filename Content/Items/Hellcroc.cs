using Steamworks;
using System;
using System.Collections.Generic;
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
    private readonly static HashSet<int> Blocklist = [TileID.DemonAltar, TileID.Trees, TileID.MushroomTrees, TileID.PalmTree, TileID.ShadowOrbs, TileID.Cactus];

    public bool? active = null;

    private float lastVelY = 0;

    public override void ResetEffects()
    {
        active = null;
        lastVelY = Player.velocity.Y;
    }

    public override void PostUpdateEquips()
    {
        if (active is null)
        {
            return;
        }

        if (active is false)
        {
            Player.maxFallSpeed *= 1.75f;
        }
        else
        {
            Player.maxFallSpeed *= 3f;
        }
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if (active is null && modifiers.DamageSource.SourceOtherIndex == 0)
        {
            return;
        }

        if (active is false)
        {
            modifiers.FinalDamage -= 0.75f;
        }
        else
        {
            modifiers.FinalDamage -= 0.4f;
        }

        //Impact();
    }

    private void Impact()
    {
        Point16 bottom = Player.Bottom.ToTileCoordinates16();
        float dist = MathF.Sqrt(lastVelY) * 2;
        Main.NewText(dist);

        for (int x = bottom.X - (int)dist; x < bottom.X + (int)dist; ++x)
        {
            for (int y = bottom.Y; y < bottom.Y + (int)dist; ++y)
            {
                if (Vector2.Distance(bottom.ToVector2(), new Vector2(x, y)) < dist)
                {
                    Player.PickTile(x, y, 100);
                }
            }
        }
    }
}
