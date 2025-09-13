using System;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Utilities;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class CrimsonCroc : Croc
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<CrimsonCroc>();
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(44, 24);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<CrimsonPlayer>().active = SportsMode;
        player.GetModPlayer<CrimsonPlayer>().instance = Item;
    }
}

internal class CrimsonPlayer : ModPlayer
{
    private static WeightedRandom<int> RarityPool;
    private static WeightedRandom<int> GlimmeringRarityPool;

    public bool? active = null;
    public Item instance = null;
    public int? proj = null;

    public override void Load()
    {
        On_Item.NewItem_Inner += GuaranteeRarity;

        RarityPool = new();
        RarityPool.Add(PrefixID.Lucky);
        RarityPool.Add(PrefixID.Warding);
        RarityPool.Add(PrefixID.Menacing);
        RarityPool.Add(PrefixID.Quick);
        RarityPool.Add(PrefixID.Violent);
        RarityPool.Add(PrefixID.Godly);
        RarityPool.Add(PrefixID.Demonic);
        RarityPool.Add(PrefixID.Superior);
        RarityPool.Add(PrefixID.Unpleasant);
        RarityPool.Add(PrefixID.Murderous);
        RarityPool.Add(PrefixID.Deadly);
        RarityPool.Add(PrefixID.Savage);
        RarityPool.Add(PrefixID.Legendary);
        RarityPool.Add(PrefixID.Legendary2);
        RarityPool.Add(PrefixID.Rapid);
        RarityPool.Add(PrefixID.Hasty);
        RarityPool.Add(PrefixID.Intimidating);
        RarityPool.Add(PrefixID.Deadly);
        RarityPool.Add(PrefixID.Staunch);
        RarityPool.Add(PrefixID.Mystic);
        RarityPool.Add(PrefixID.Masterful);
        RarityPool.Add(PrefixID.Mythical);
        RarityPool.Add(PrefixID.Unreal);

        GlimmeringRarityPool = new();
        GlimmeringRarityPool.Add(PrefixID.Legendary);
        GlimmeringRarityPool.Add(PrefixID.Legendary2);
        GlimmeringRarityPool.Add(PrefixID.Light);
        GlimmeringRarityPool.Add(PrefixID.Demonic);
        GlimmeringRarityPool.Add(PrefixID.Unreal);
        GlimmeringRarityPool.Add(PrefixID.Mythical);
        GlimmeringRarityPool.Add(PrefixID.Ruthless);
        GlimmeringRarityPool.Add(PrefixID.Warding);
        GlimmeringRarityPool.Add(PrefixID.Lucky);
        GlimmeringRarityPool.Add(PrefixID.Menacing);
    }

    private static int GuaranteeRarity(On_Item.orig_NewItem_Inner orig, IEntitySource source, int X, int Y, int Width, int Height, Item itemToClone, int Type, int Stack, 
        bool noBroadcast, int pfix, bool noGrabDelay, bool reverseLookup)
    {
        Player player = GetPlayersNearby(X, Y);
        ref bool? active = ref player.GetModPlayer<CrimsonPlayer>().active;
        Item sampleItem = ContentSamples.ItemsByType[Type];

        if (active is null || player.DistanceSQ(new Vector2(X, Y)) > 1500 * 1500 || source is not EntitySource_Loot { Entity: NPC } || sampleItem.IsACoin)
            return orig(source, X, Y, Width, Height, itemToClone, Type, Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);

        if (active is true && Stack < sampleItem.maxStack && !sampleItem.master && !sampleItem.expert)
        {
            ref int stack = ref Stack;

            if (itemToClone is not null)
                stack = ref itemToClone.stack;

            int oldStack = stack;
            stack += Main.rand.Next(Math.Max(stack / 2, 1), Stack + 3);
            stack = Math.Clamp(stack, 1, sampleItem.maxStack);

            AdvancedPopupRequest request = default;
            request.DurationInFrames = 80;
            request.Velocity = new Vector2(0, -18);
            request.Text = "+" + (stack - oldStack).ToString() + "x";
            request.Color = Color.IndianRed;

            Vector2 pos = new(X, Y);

            if (player.GetModPlayer<CrimsonPlayer>().proj is int proj && player.whoAmI == Main.myPlayer)
            {
                Projectile projectile = Main.projectile[proj];
                pos = projectile.Center - new Vector2(0, 20);
                projectile.velocity = Vector2.Zero;
                projectile.netUpdate = true;

                for (int i = 0; i < 15; ++i)
                    Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.BloodWater);
            }

            PopupText.NewText(request, pos);
        }

        int item = orig(source, X, Y, Width, Height, itemToClone, Type, Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);

        if (active is false)
        {
            ref int prefix = ref pfix;

            if (itemToClone is not null)
                prefix = ref itemToClone.prefix;

            int tries = 0;

            do
            {
                prefix = (player.GlimmeringJibbit() ? GlimmeringRarityPool : RarityPool).Get();
                tries++;
            } while (!sampleItem.CanRollPrefix(prefix) && tries < 100);


            if (tries < 100)
                Main.item[item].Prefix(prefix);
        }

        return item;
    }

    private static Player GetPlayersNearby(int X, int Y)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return Main.player[Player.FindClosest(new Vector2(X, Y), 1, 1)];

        Player player = null;

        foreach (Player other in Main.ActivePlayers)
        {
            if (other.DistanceSQ(new Vector2(X, Y)) < 1600 * 1600)
            {
                player = other;
            }
        }

        return player;
    }

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

            if (!projectile.active || projectile.type != ModContent.ProjectileType<ThoughtProjectile>())
                proj = null;
        }

        int type = ModContent.ProjectileType<ThoughtProjectile>();
        proj ??= Projectile.NewProjectile(new EntitySource_ItemUse(Player, instance), Player.Center, Vector2.Zero, type, 0, 0, Player.whoAmI);
    }
}

public class ThoughtProjectile : ModProjectile
{
    private Player Owner => Main.player[Projectile.owner];
    private ref bool? Active => ref Owner.GetModPlayer<CrimsonPlayer>().active;

    private ref float State => ref Projectile.ai[0];

    public override void SetDefaults()
    {
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.Size = new Vector2(64);
        Projectile.tileCollide = false;
        Projectile.netImportant = true;
    }

    public override bool? CanCutTiles() => false;
    public override bool? CanDamage() => false;

    public override void AI()
    {
        if (Active is null)
        {
            Projectile.Kill();
            return;
        }

        Projectile.rotation = Projectile.velocity.X * 0.1f;
        Projectile.timeLeft++;
        Projectile.frame = (Projectile.frameCounter += 2) % 60 / 20;

        if (Owner.DistanceSQ(Projectile.Center) > 1200 * 1200)
            Projectile.Center = Owner.Center - new Vector2(0, 80);

        if (State == 0)
        {
            Vector2 target = Owner.Center - new Vector2(0, 120);

            if (Projectile.DistanceSQ(target) > 80 * 80)
                Projectile.velocity += Projectile.SafeDirectionTo(target) * 0.3f;
            else
            {
                if (Projectile.velocity.LengthSquared() == 0 && Main.myPlayer == Projectile.owner)
                {
                    Projectile.velocity = Main.rand.NextVector2Circular(0.1f, 0.1f);
                    Projectile.netUpdate = true;
                }

                Vector2 rot = Projectile.velocity.RotatedBy(-0.04f);
                Projectile.velocity = Vector2.Lerp(rot, rot * 6, 0.005f);
            }

            if (Projectile.velocity.LengthSquared() > 6 * 6)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 6;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var tex = TextureAssets.Projectile[ModContent.ProjectileType<ThoughtProjectile>()].Value;
        Color col = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity;
        Rectangle rect = new(0, Projectile.frame * 38, 48, 36);
        float opacity = MathF.Sin(Main.GameUpdateCount * 0.03f) * 0.25f + 0.75f;

        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, rect, col * opacity, Projectile.rotation, rect.Size() / 2f, 1f, SpriteEffects.None, 0);

        Vector2 eyeDir = Projectile.DirectionTo(Owner.Center) * 2;
        rect = new(50, 0, 12, 6);
        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + eyeDir, rect, col, Projectile.rotation, rect.Size() / 2f, 1f, SpriteEffects.None, 0);

        rect = new(50, 8, 2, 4);
        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + eyeDir, rect, col, Projectile.rotation, rect.Size() / 2f, 1f, SpriteEffects.None, 0);

        return false;
    }
}

public class CrimsonCrocDropping : GlobalTile
{
    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        Tile tile = Main.tile[i, j];

        if (type == TileID.ShadowOrbs && !noItem && tile.TileFrameX == 36 && tile.TileFrameY is 0 or 36 && Main.rand.NextBool(3))
            Item.NewItem(new EntitySource_TileBreak(i, j), new Rectangle(i * 16, j * 16, 32, 32), ModContent.ItemType<CrimsonCroc>(), prefixGiven: -1);
    }
}