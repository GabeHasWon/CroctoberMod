using CroctoberMod.Content.Syncing;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Biomes;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class MarbleCrocs : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(48, 44);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<MarblePlayer>().active = SportsMode;
    }

    protected override object[] GetSportsArgs() => [Language.GetTextValue("Mods.CroctoberMod." + (Main.ReversedUpDownArmorSetBonuses ? "Up" : "Down"))];
}

internal class MarblePlayer : ModPlayer
{
    public bool? active = null;

    public override void Load() => On_Player.KeyDoubleTap += HijackDoubleTap;

    private static void HijackDoubleTap(On_Player.orig_KeyDoubleTap orig, Player self, int keyDir)
    {
        orig(self, keyDir);

        int dir = 0;

        if (Main.ReversedUpDownArmorSetBonuses)
            dir = 1;

        if (keyDir != dir)
            return;

        if (self.GetModPlayer<MarblePlayer>().active is not { } active || Main.myPlayer != self.whoAmI)
            return;

        if (HasAnyStatues(self, out Projectile statue))
        {
            SpawnTeleportDust(self.position, false);

            self.Teleport(statue.Center, -1);

            SoundEngine.PlaySound(SoundID.Item70, self.Center);

            if (active)
                self.AddBuff(ModContent.BuffType<StatueBuff>(), StatueBuff.MaxTime);

            statue.timeLeft = 0;
            statue.active = false;
            statue.netUpdate = true;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.KillProjectile, -1, -1, null, statue.whoAmI);

            SpawnTeleportDust(self.position, false);

            return;
        }

        Projectile.NewProjectile(self.GetSource_FromThis(), self.Center, Vector2.Zero, ModContent.ProjectileType<CrocStatueProj>(), 0, 0, self.whoAmI, active ? 1 : 0);
    }

    internal static void SpawnTeleportDust(Vector2 position, bool fromNet)
    {
        if (!fromNet && Main.netMode == NetmodeID.MultiplayerClient)
        {
            new SpawnVFXModule(position, SpawnVFXModule.EffectType.MarbleTeleport).Send();
            return;
        }

        for (int i = 0; i < 30; ++i)
        {
            short type = Main.rand.NextBool(4) ? DustID.TeleportationPotion : DustID.Marble;
            Dust.NewDust(position, Player.defaultWidth, Player.defaultHeight, type, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
        }
    }

    private static bool HasAnyStatues(Player self, out Projectile statue)
    {
        foreach (Projectile proj in Main.ActiveProjectiles)
        {
            if (proj.type == ModContent.ProjectileType<CrocStatueProj>() && proj.owner == self.whoAmI)
            {
                statue = proj;
                return true;
            }
        }

        statue = null;
        return false;
    }

    public override void ResetEffects() => active = null;
}

public class CrocStatueProj : ModProjectile
{
    private static Asset<Texture2D> Glow = null;

    private bool Sports => Projectile.ai[0] == 1;

    private ref float Timer => ref Projectile.ai[1];

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 2;

        Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
    }

    public override void SetDefaults()
    {
        Projectile.timeLeft = 2;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.aiStyle = -1;
        Projectile.width = Projectile.height = 20;
        Projectile.tileCollide = true;
    }

    public override void AI()
    {
        if (!Projectile.TryGetOwner(out Player player))
            return;

        if (player.GetModPlayer<MarblePlayer>().active is null)
        {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft++;

        if (player.GlimmeringJibbit())
        {
            Projectile.frame = 1;

            Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.8f, 0.3f));
        }
        else
            Projectile.frame = 0;

        Timer++;
        Projectile.velocity = new Vector2(MathF.Sin(Timer * 0.02f), MathF.Sin(Timer * 0.0125f)) * 0.2f;
        Projectile.rotation = MathF.Sin(Timer * 0.02f + MathF.Cos(Timer * 0.01f)) * MathHelper.PiOver4 * 0.25f;

        player.statLifeMax2 = (int)(player.statLifeMax2 * (Sports ? player.JibbitModifier(0.5f, 0.6f) : player.JibbitModifier(0.75f, 0.8f)));
        player.statLife = Math.Min(player.statLife, player.statLifeMax2);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        int texHeight = tex.Height / Main.projFrames[Type];
        Rectangle frame = new(0, texHeight * Projectile.frame, tex.Width, texHeight - 2);
        Color color = Color.White * (MathF.Sin(Main.GameUpdateCount * 0.02f) * 0.5f + 0.5f);
        Vector2 pos = Projectile.Center - Main.screenPosition;
        
        Main.EntitySpriteDraw(tex, pos, frame, lightColor, Projectile.rotation, frame.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

        tex = Glow.Value;
        Main.EntitySpriteDraw(tex, pos, frame, color, Projectile.rotation, frame.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
}

public class StatueBuff : ModBuff
{
    public const int MaxTime = 5 * 60;

    public override void Update(Player player, ref int buffIndex)
    {
        int time = player.buffTime[buffIndex];
        float factor = time / (float)MaxTime * player.JibbitModifier(1, 1.25f);

        player.GetDamage(DamageClass.Generic) += 0.25f * factor;
        player.moveSpeed *= 1 + factor * 0.25f;
    }
}

public class MarbleCrocsStatue : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.RandomStyleRange = 1;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(203, 185, 151));
        RegisterItemDrop(ModContent.ItemType<MarbleCrocs>());

        DustType = DustID.Marble;
        HitSound = SoundID.Tink;
    }

    public override void MouseOver(int i, int j)
    {
        Main.LocalPlayer.cursorItemIconEnabled = true;
        Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<MarbleCrocs>();
    }
}

public class MarbleCrocGeneration : ModSystem
{
    public override void Load() => On_MarbleBiome.Place += AddCrocStatue;

    private bool AddCrocStatue(On_MarbleBiome.orig_Place orig, MarbleBiome self, Point origin, StructureMap structures)
    {
        bool success = orig(self, origin, structures);

        if (success && WorldGen.genRand.NextBool())
            PlaceMarbleCrocStatue(origin);

        return success;
    }

    private static void PlaceMarbleCrocStatue(Point origin)
    {
        const int MaxWidth = 150;
        const int MaxHeight = 60;

        for (int i = 0; i < 1500; ++i)
        {
            int x = origin.X + WorldGen.genRand.Next(MaxWidth);
            int y = origin.Y + WorldGen.genRand.Next(MaxHeight);
            Tile tile = Main.tile[x, y];
            Tile above = Main.tile[x, y - 1];

            if (tile.HasTile && tile.TileType == TileID.Marble && above.WallType is WallID.MarbleUnsafe)
            {
                Tile tileRight = Main.tile[x + 1, y];

                tile.IsHalfBlock = false;
                tile.Slope = SlopeType.Solid;

                tileRight.IsHalfBlock = false;
                tileRight.Slope = SlopeType.Solid;

                WorldGen.PlaceObject(x, y - 1, ModContent.TileType<MarbleCrocsStatue>(), true, WorldGen.genRand.Next(1));

                if (above.HasTile && above.TileType == ModContent.TileType<MarbleCrocsStatue>())
                    return;
            }
        }
    }
}