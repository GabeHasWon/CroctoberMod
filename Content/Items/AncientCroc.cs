using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class AncientCroc : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(50, 22);
        Item.damage = 50;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<AncientPlayer>().active = SportsMode;
        player.GetModPlayer<AncientPlayer>().instance = Item;
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        if (player.GetModPlayer<AncientPlayer>().active is true)
            damage *= 1.5f;
    }
}

internal class AncientPlayer : ModPlayer
{
    public bool? active = null;
    public Item instance = null;
    public float flightTime = 0;
    public int flightCooldown = 0;

    public override void Load() => On_Player.RefreshMovementAbilities += ResetFlight;

    private static void ResetFlight(On_Player.orig_RefreshMovementAbilities orig, Player self, bool doubleJumps)
    {
        orig(self, doubleJumps);
        self.GetModPlayer<AncientPlayer>().flightTime = GetFlightTime(self);
    }

    private static float GetFlightTime(Player self) => MathF.Max(self.wingTimeMax * 5, self.JibbitModifier(12, 15) * 30);

    public override void ResetEffects()
    {
        active = null;
        instance = null;
    }

    public override void PostUpdateEquips()
    {
        if (active is null)
        {
            return;
        }

        if (Player.velocity.Y == 0 || Player.pulley)
            flightTime = GetFlightTime(Player);

        Player.wingTime = 0;
        Player.wingTimeMax = 0;
        Player.rocketBoots = 0;

        if (Player.mount.Active)
            return;

        if (Player.controlJump && !Player.AnyExtraJumpUsable() && flightTime > 0 && Player.jump == 0)
        {
            Player.moveSpeed *= 1.5f;
            flightCooldown--;

            if (flightCooldown < 0)
            {
                flightTime -= active.Value ? 30 : Player.JibbitModifier(20, 18);

                float speed = 3.5f;

                if (Player.GlimmeringJibbit())
                {
                    if (active is false)
                        speed = 4.5f;
                    else
                        speed = 4;
                }

                if (Main.myPlayer == Player.whoAmI)
                {
                    var vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Player.JibbitModifier(Main.rand.NextFloat(6, 7), Main.rand.NextFloat(7, 9)));
                    Vector2 pos = Player.BottomLeft + new Vector2(Main.rand.Next(Player.width), -10);
                    int damage = (int)Player.JibbitModifier(instance.damage, instance.damage * 1.5f);
                    Projectile.NewProjectile(Player.GetSource_FromAI(), pos, vel, ModContent.ProjectileType<AncientLaser>(), damage, 0.5f, Player.whoAmI);

                    float max = Player.JibbitModifier(-14, -15);

                    if (Player.velocity.Y > max)
                        Player.velocity.Y = MathF.Max(Player.velocity.Y - speed, max);

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Player.whoAmI);
                }

                flightCooldown = 6;

                for (int i = 0; i < 3; i++)
                {
                    var vel = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(5, 7));
                    Vector2 pos = Player.BottomLeft + new Vector2(Main.rand.Next(Player.width), -10);
                    Dust.NewDust(pos, 1, 1, Main.rand.NextBool(3) ? DustID.Lihzahrd : DustID.GemTopaz, vel.X, vel.Y);
                }

                SoundEngine.PlaySound(SoundID.Item91, Player.Bottom);
            }
        }
    }
}

public class AncientLaser : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.Bullet);
        Projectile.timeLeft = 120;
        Projectile.Size = new(6, 6);
        Projectile.Opacity = 1f;
        Projectile.aiStyle = -1;
        Projectile.extraUpdates = 2;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

        if (Main.rand.NextBool(20))
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemTopaz);
    }
}

public class AncientCrocTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileLighted[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(177, 92, 31));

        DustType = DustID.Lihzahrd;
        HitSound = SoundID.Tink;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => (r, g, b) = (0.1f, 0.1f, 0);

    public override bool RightClick(int i, int j)
    {
        if (Main.LocalPlayer.HeldItem.type != ItemID.LihzahrdPowerCell)
            return false;

        Main.LocalPlayer.HeldItem.stack--;

        if (Main.LocalPlayer.HeldItem.stack <= 0)
            Main.LocalPlayer.HeldItem.TurnToAir();

        Main.LocalPlayer.QuickSpawnItem(new EntitySource_TileInteraction(Main.LocalPlayer, i, j), ModContent.ItemType<AncientCroc>());
        return true;
    }

    public override void MouseOver(int i, int j)
    {
        Main.LocalPlayer.cursorItemIconEnabled = true;
        Main.LocalPlayer.cursorItemIconID = ItemID.LihzahrdPowerCell;
    }
}

public class AncientCrocTileItem : ModItem
{
    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<AncientCrocTile>());
    public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.LihzahrdBrick, 20).AddIngredient(ItemID.LihzahrdPowerCell).AddTile(TileID.MythrilAnvil).Register();
}

public class AncientGeneration : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int index = tasks.FindIndex(x => x.Name == "Lihzahrd Altars");

        if (index != -1 && tasks[index].Enabled)
            tasks.Insert(index + 1, new PassLegacy("Ancient Crocs", GenerateAncientCrocs));
    }

    private void GenerateAncientCrocs(GenerationProgress progress, GameConfiguration configuration)
    {
        int successes = 0;

        for (int i = 0; i < 20000; ++i)
        {
            int x = Main.dungeonX > Main.maxTilesX / 2 ? WorldGen.genRand.Next(100, Main.maxTilesX / 2) : WorldGen.genRand.Next(Main.maxTilesX / 2, Main.maxTilesX - 100);
            int y = WorldGen.genRand.Next((int)Main.worldSurface, Main.maxTilesY - 200);
            Tile tile = Main.tile[x, y];
            Tile above = Main.tile[x, y - 1];

            if (tile.HasTile && tile.TileType == TileID.LihzahrdBrick && above.WallType is WallID.LihzahrdBrickUnsafe or WallID.LihzahrdBrick)
            {
                Tile tileRight = Main.tile[x + 1, y];

                tile.IsHalfBlock = false;
                tile.Slope = SlopeType.Solid;

                tileRight.IsHalfBlock = false;
                tileRight.Slope = SlopeType.Solid;

                WorldGen.PlaceObject(x, y - 1, ModContent.TileType<AncientCrocTile>(), true, WorldGen.genRand.Next(3));

                if (above.HasTile && above.TileType == ModContent.TileType<AncientCrocTile>())
                {
                    successes++;

                    if (successes >= 3)
                        return;
                }
                else if (successes <= 0)
                    i--;
            }
        }
    }
}