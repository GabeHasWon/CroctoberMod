using System;
using Terraria.Audio;

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

                if (Main.myPlayer == Player.whoAmI)
                {
                    var vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Player.JibbitModifier(Main.rand.NextFloat(6, 7), Main.rand.NextFloat(7, 9)));
                    Vector2 pos = Player.BottomLeft + new Vector2(Main.rand.Next(Player.width), -10);
                    int damage = (int)Player.JibbitModifier(instance.damage, instance.damage * 1.5f);
                    Projectile.NewProjectile(Player.GetSource_FromAI(), pos, vel, ModContent.ProjectileType<AncientLaser>(), damage, 0.5f, Player.whoAmI);
                }

                flightCooldown = 6;

                float speed = 3.5f;

                if (Player.GlimmeringJibbit())
                {
                    if (active is false)
                        speed = 4.5f;
                    else
                        speed = 4;
                }

                float max = Player.JibbitModifier(-14, -15);

                if (Player.velocity.Y > max)
                    Player.velocity.Y = MathF.Max(Player.velocity.Y - speed, max);

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