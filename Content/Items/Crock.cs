using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Biomes;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class Crock : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(50, 32);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<CrockPlayer>().active = SportsMode;
    }
}

internal class CrockPlayer : ModPlayer
{
    public bool? active = null;
    public Item instance = null;
    public int? proj = null;

    public override void ResetEffects()
    {
        active = null;
        instance = null;
    }

    public override void PostUpdateEquips()
    {
        if (active is null || Main.myPlayer != Player.whoAmI)
        {
            return;
        }

        if (proj.HasValue)
        {
            Projectile projectile = Main.projectile[proj.Value];

            if (!projectile.active || projectile.type != ModContent.ProjectileType<CrockProj>())
                proj = null;
        }

        proj ??= Projectile.NewProjectile(new EntitySource_ItemUse(Player, instance), Player.Center, Vector2.Zero, ModContent.ProjectileType<CrockProj>(), 0, 0, Player.whoAmI);
    }
}

internal class CrockProj : ModProjectile
{
    private static Asset<Texture2D> Glow = null;
    private static Asset<Texture2D> Aura = null;

    private Player Owner => Main.player[Projectile.owner];
    private bool? SportsMode => Owner.GetModPlayer<CrockPlayer>().active;

    private ref float ViewAngle => ref Projectile.ai[0];
    private ref float RandomizationAngle => ref Projectile.ai[1];

    public override void SetStaticDefaults()
    {
        Glow = ModContent.Request<Texture2D>(Texture + "Glow");
        Aura = ModContent.Request<Texture2D>(Texture + "Aura");
    }

    public override void SetDefaults()
    {
        Projectile.timeLeft = 2;
        Projectile.Size = new Vector2(24, 46);
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
    }

    public override void AI()
    {
        if (SportsMode is null)
        {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;
        Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center - new Vector2(0, 60), 0.1f);

        if (SportsMode is false)
        {
            RandomizationAngle = 0;
            Vector2 dir = ViewAngle.ToRotationVector2();

            for (int i = 0; i < 18; ++i)
            {
                float mul = 0.5f + i / 36f;
                Lighting.AddLight(Projectile.Center + dir * i * 32, new Vector3(0.35f, 0.35f, 0.6f) * mul * Owner.JibbitModifier(1, 1.3f));
            }

            if (Main.myPlayer == Projectile.owner)
            {
                ViewAngle = Utils.AngleLerp(ViewAngle, Projectile.AngleTo(Main.MouseWorld), Owner.JibbitModifier(0.1f, 0.3f));

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
                }
            }
        }
        else
        {
            if (RandomizationAngle == 0)
            {
                RandomizationAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            }

            Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.8f, 1.5f) * Owner.JibbitModifier(1, 1.5f));
            ViewAngle = MathHelper.Lerp(ViewAngle, -MathHelper.PiOver2, 0.1f);
        }

        Projectile.rotation = ViewAngle + MathHelper.PiOver2;
    }

    public override void PostDraw(Color lightColor)
    {
        Vector2 position = Projectile.Center - Main.screenPosition;
        Color color = Color.White * 0.2f;
        float rot = Projectile.rotation;
        Texture2D tex = Glow.Value;
        Main.EntitySpriteDraw(tex, position, null, color, rot, tex.Size() / 2f, 1f + GetSine(0), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(tex, position, null, color * 0.8f, rot, tex.Size() / 2f, 1.15f + GetSine(MathHelper.PiOver4 * 0.5f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(tex, position, null, color * 0.5f, rot, tex.Size() / 2f, 1.5f + GetSine(MathHelper.PiOver4), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(tex, position, null, color * 0.15f, rot, tex.Size() / 2f, 1.8f + GetSine(MathHelper.PiOver4 * 1.5f), SpriteEffects.None, 0);

        if (SportsMode is true)
        {
            rot += RandomizationAngle;
            Main.EntitySpriteDraw(Aura.Value, position, null, color, rot, Aura.Value.Size() / 2f, 1f + GetSine(0f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(Aura.Value, position, null, color * 0.5f, rot + MathHelper.PiOver2, Aura.Value.Size() / 2f, 1.2f + GetSine(0.33f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(Aura.Value, position, null, color * 0.2f, rot + 2.44f, Aura.Value.Size() / 2f, 1.5f + GetSine(0.67f), SpriteEffects.None, 0);
        }
    }

    private static float GetSine(float offset) => MathF.Sin((float)Main.timeForVisualEffects * 0.08f + offset) * 0.1f;
}

public class CrockGeneration : ModSystem
{
    public override void Load() => On_GraniteBiome.CleanupTiles += EditGranite;

    private void EditGranite(On_GraniteBiome.orig_CleanupTiles orig, GraniteBiome self, Point tileOrigin, Rectangle magmaMapArea)
    {
        orig(self, tileOrigin, magmaMapArea);

        HashSet<Point16> positions = [];

        for (int i = magmaMapArea.Left; i < magmaMapArea.Right; i++)
        {
            for (int j = magmaMapArea.Top; j < magmaMapArea.Bottom; j++)
            {
                int x = i + tileOrigin.X;
                int y = j + tileOrigin.Y;

                if (!WorldGen.SolidTile(x, y))
                    continue;

                positions.Add(new Point16(x, y));
            }
        }

        WeightedRandom<int> rand = new(WorldGen.genRand);
        rand.Add(0, 0.3f);
        rand.Add(1);
        rand.Add(2, 0.66f);
        rand.Add(3, 0.2f);

        int reps = rand;

        for (int i = 0; i < reps; ++i)
            PlaceGeode(WorldGen.genRand.Next([.. positions]));
    }

    internal static void PlaceGeode(Point16 position)
    {
        int size = WorldGen.genRand.Next(5, 10);
        int gem = WorldGen.genRand.Next(4);

        ushort gemWall = gem switch
        {
            0 => WallID.SapphireUnsafe,
            1 => WallID.EmeraldUnsafe,
            2 => WallID.RubyUnsafe,
            _ => WallID.DiamondUnsafe
        };

        for (int i = position.X - size; i < position.X + size; i++)
        {
            for (int j = position.Y - size; j < position.Y + size; j++)
            {
                float distance = Vector2.Distance(position.ToVector2(), new Vector2(i, j)) * WorldGen.genRand.NextFloat(1, 1.2f);
                Tile tile = Main.tile[i, j];

                if (distance < size / 3f + WorldGen.genRand.NextFloat())
                {
                    tile.HasTile = false;
                    tile.TileType = TileID.Dirt;
                    tile.WallType = WallID.GraniteUnsafe;
                }
                else if (distance < size / 1.5f + WorldGen.genRand.NextFloat())
                {
                    tile.HasTile = false;
                    tile.TileType = TileID.Dirt;

                    if (WorldGen.genRand.NextBool(5))
                    {
                        WorldGen.PlaceTile(i, j, TileID.ExposedGems, true, false, -1, gem + 2);
                        WorldGen.TileFrame(i, j);
                    }

                    tile.WallType = WorldGen.genRand.NextBool(3) ? gemWall : WallID.GraniteUnsafe;
                }
                else if (distance < size)
                {
                    tile.HasTile = true;
                    tile.TileType = TileID.Granite;
                }
            }
        }

        WorldGen.PlaceObject(position.X, position.Y, ModContent.TileType<CrockTile>(), true);
    }
}

public class CrockTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileLighted[Type] = true;

        TileID.Sets.FramesOnKillWall[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.AnchorTop = AnchorData.Empty;
        TileObjectData.newTile.AnchorWall = true;
        TileObjectData.addTile(Type);

        DustType = DustID.Granite;

        RegisterItemDrop(ModContent.ItemType<Crock>());
        AddMapEntry(new Color(0, 192, 255));
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => (r, g, b) = (0.05f, 0.05f, 0.5f);
}