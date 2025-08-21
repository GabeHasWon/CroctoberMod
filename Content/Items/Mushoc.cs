using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class Mushoc : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.damage = 12;
        Item.DamageType = DamageClass.Summon;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<MushocPlayer>().active = SportsMode;
        player.GetModPlayer<MushocPlayer>().instance = Item;
    }
}

internal class MushocPlayer : ModPlayer
{
    public bool? active = null;
    public Item instance = null;

    private readonly Dictionary<Point16, int> ProjectilesPerTwoTiles = [];

    public override void ResetEffects()
    {
        active = null;
        instance = null;
    }

    public override void PostUpdateEquips()
    {
        if (active is not null)
        {
            Point16 floor = (Player.Bottom / 2f).ToTileCoordinates16();

            if (Collision.SolidCollision(Player.BottomLeft, Player.width, 6) && !ProjectilesPerTwoTiles.ContainsKey(floor))
            {
                int type = ModContent.ProjectileType<MushocMushroom>();
                IEntitySource src = Player.GetSource_Accessory(instance);
                int damage = (int)Player.GetDamage(DamageClass.Summon).ApplyTo(instance.damage);
                ProjectilesPerTwoTiles.Add(floor, Projectile.NewProjectile(src, Player.Bottom, Vector2.Zero, type, damage, 0, Player.whoAmI, active is false ? 0 : 1));
            }
        }

        HashSet<Point16> removals = [];

        foreach (var pair in ProjectilesPerTwoTiles)
        {
            if (pair.Value != -1)
            {
                Projectile projectile = Main.projectile[pair.Value];

                if (!projectile.active || projectile.type != ModContent.ProjectileType<MushocMushroom>())
                    removals.Add(pair.Key);
            }
        }

        foreach (var item in removals)
            ProjectilesPerTwoTiles.Remove(item);
    }
}

internal class MushocMushroom : ModProjectile
{
    private int TimeLeft => Harmful ? 120 : 180;
    private bool Harmful => Projectile.ai[0] == 1;
    private ref float Timer => ref Projectile.ai[1];
    private ref float Offset => ref Projectile.ai[2];

    public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(18, 22);
        Projectile.timeLeft = TimeLeft;
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.frame = Main.rand.Next(3);
        Projectile.friendly = true;
        Projectile.hostile = false;
    }

    public override bool? CanHitNPC(NPC target) => Harmful ? null : false;
    public override bool? CanCutTiles() => false;

    public override void AI()
    {
        Timer++;

        if (Offset == 0)
        {
            Offset = Main.rand.NextFloat(30);
            Projectile.netUpdate = true;
        }

        if (Projectile.timeLeft < 60)
            Projectile.Opacity = Projectile.timeLeft / 60f;

        if (!Harmful)
            Lighting.AddLight(Projectile.Center, new Vector3(87, 137, 255) / 280f * Projectile.Opacity);
        else
            Lighting.AddLight(Projectile.Center, new Vector3(140, 137, 255) / 400f * Projectile.Opacity);

        Player owner = Main.player[Projectile.owner];

        if (owner.DistanceSQ(Projectile.Center) < 80 * 80)
            Projectile.timeLeft = Math.Min(Projectile.timeLeft + 2, TimeLeft);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        int frameHeight = tex.Height / Main.projFrames[Type];
        var src = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
        Vector2 pos = Projectile.Center - Main.screenPosition;
        float speed = Harmful ? 0.16f : 0.08f;
        Vector2 scale = new(MathF.Sin(Timer * speed + Offset) * 0.2f + 1, MathF.Cos(Timer * speed + Offset) * 0.2f + 1);
        Main.EntitySpriteDraw(tex, pos, src, Projectile.GetAlpha(lightColor), Projectile.rotation, src.Size() / 2f, scale, SpriteEffects.None, 0);
        return false;
    }
}

public class MushocGrowth : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileCut[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileMerge[TileID.MushroomVines][Type] = true;
        Main.tileMerge[Type][TileID.MushroomVines] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 16];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 1;
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.AnchorTop = new(Terraria.Enums.AnchorType.SolidBottom | Terraria.Enums.AnchorType.AlternateTile, 1, 0);
        TileObjectData.newTile.AnchorAlternateTiles = [TileID.MushroomVines];
        TileObjectData.newTile.DrawYOffset = -4;
        TileObjectData.addTile(Type);

        DustType = DustID.Iron;

        AddMapEntry(new Color(63, 135, 219));
        RegisterItemDrop(ModContent.ItemType<Mushoc>());
    }
}

public class MushocGrowing : GlobalTile
{
    public override void RandomUpdate(int i, int j, int type)
    {
        if (type == TileID.MushroomVines && Main.rand.NextBool(400) && !Main.tile[i, j + 1].HasTile)
        {
            WorldGen.PlaceObject(i, j + 2, ModContent.TileType<MushocGrowth>(), true);
            WorldGen.SquareTileFrame(i, j + 1, true);

            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendTileSquare(-1, i, j + 1, 1, 2);
        }
    }
}

public class MushocNaturalGen : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int task = tasks.FindIndex(x => x.Name == "Water Plants");

        if (task != -1 && tasks[task].Enabled)
        {
            tasks.Insert(task + 1, new PassLegacy("Mushocs", GenerateMushocs));
        }
    }

    private static void GenerateMushocs(GenerationProgress progress, GameConfiguration configuration)
    {
        int reps = (int)(Main.maxTilesY / 1200f * 24);
        int fails = 0;

        for (int i = 0; i < reps; ++i)
        {
            fails++;

            if (fails > 15000)
                return;

            int x = WorldGen.genRand.Next(120, Main.maxTilesX - 120);
            int y = WorldGen.genRand.Next((int)Main.worldSurface + 50, Main.maxTilesY - 250);
            Tile tile = Main.tile[x, y];
            Tile below = Main.tile[x, y + 1];

            if (tile.HasTile && tile.TileType == TileID.MushroomVines && !below.HasTile)
            {
                WorldGen.PlaceObject(x, y + 1, ModContent.TileType<MushocGrowth>(), true);

                if (below.HasTile && below.TileType == ModContent.TileType<MushocGrowth>())
                    i--;
            }
            else
                i--;
        }
    }
}