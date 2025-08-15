using Terraria.Audio;

namespace CroctoberMod.Content.Items;

internal class Skycroc : Croc
{
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetJumpState<SkycrocDoubleJump>().Enable();
        player.GetModPlayer<SkycrocPlayer>().active = SportsMode;
    }
}

internal class SkycrocPlayer : ModPlayer
{
    public bool? active = null;
    public int jumps = 0;

    public override void ResetEffects() => active = null;
}

public class SkycrocDoubleJump : ExtraJump
{
    public override Position GetDefaultPosition() => new Before(CloudInABottle);
    public override float GetDurationMultiplier(Player player) => 0;

    public override void UpdateHorizontalSpeeds(Player player)
    {
        player.runAcceleration *= 1.5f;
        player.maxRunSpeed *= 1.5f;
    }

    public override void OnRefreshed(Player player)
    {
        bool? active = player.GetModPlayer<SkycrocPlayer>().active;
        ref int jumps = ref player.GetModPlayer<SkycrocPlayer>().jumps;

        if (active is false)
            jumps = 1;
        else if (active is true)
            jumps = 3;
    }

    public override void OnStarted(Player player, ref bool playSound)
    {
        playSound = false;
        SoundEngine.PlaySound(SoundID.DoubleJump with { Pitch = 0, PitchVariance = 0.04f }, player.Bottom);

        ref int jumps = ref player.GetModPlayer<SkycrocPlayer>().jumps;

        jumps--;

        if (jumps > 0)
        {
            player.GetJumpState(this).Available = true;
        }

        float mul = player.GetModPlayer<SkycrocPlayer>().active is false ? 1.3f : 0.9f;

        if (player.controlRight)
        {
            player.velocity.X = 10 * mul;
            player.velocity.Y += 1f;
        }
        else if (player.controlLeft)
        {
            player.velocity.X = -10 * mul;
            player.velocity.Y += 1f;
        }
        else
            player.velocity.Y -= 8 * mul;

        for (int i = 0; i < 30; i++)
        {
            Vector2 position = player.BottomLeft + new Vector2(Main.rand.NextFloat(player.width), 0);
            Vector2 velocity = -(player.velocity * new Vector2(0.4f, 0.4f)).RotatedByRandom(0.9f) * Main.rand.NextFloat(0.8f, 1.5f);
            var dust = Dust.NewDustPerfect(position, Main.rand.NextBool() ? DustID.Cloud : DustID.RainCloud, velocity, Scale: Main.rand.NextFloat(2, 3));
            dust.noGravity = true;
        }

    }
}