using ReLogic.Content;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Localization;

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

        if (self.GetModPlayer<MarblePlayer>().active is not { } active)
            return;

        if (HasAnyStatues(self, out Projectile statue))
        {
            SpawnTeleportDust(statue);

            self.Teleport(statue.Center, -1);

            SoundEngine.PlaySound(SoundID.Item70, self.Center);

            if (active)
                self.AddBuff(ModContent.BuffType<StatueBuff>(), StatueBuff.MaxTime);

            statue.active = false;
            statue.netUpdate = true;

            SpawnTeleportDust(statue);

            return;
        }

        Projectile.NewProjectile(self.GetSource_FromThis(), self.Center, Vector2.Zero, ModContent.ProjectileType<CrocStatueProj>(), 0, 0, self.whoAmI, active ? 1 : 0);
    }

    private static void SpawnTeleportDust(Projectile statue)
    {
        for (int i = 0; i < 30; ++i)
        {
            short type = Main.rand.NextBool(4) ? DustID.TeleportationPotion : DustID.Marble;
            Dust.NewDust(statue.position, statue.width, statue.height, type, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
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