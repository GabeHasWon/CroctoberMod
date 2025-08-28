using StructureHelper.API;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
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
        player.runAcceleration *= player.JibbitModifier(1.5f, 1.8f);
        player.maxRunSpeed *= player.JibbitModifier(1.5f, 1.8f);
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

        float mul = player.GetModPlayer<SkycrocPlayer>().active is false ? player.JibbitModifier(1.3f, 1.7f) : player.JibbitModifier(0.9f, 1.1f);

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

        for (int i = 0; i < player.JibbitModifier(30, 45); i++)
        {
            Vector2 position = player.BottomLeft + new Vector2(Main.rand.NextFloat(player.width), 0);
            Vector2 velocity = -(player.velocity * new Vector2(0.4f, 0.4f)).RotatedByRandom(0.9f) * Main.rand.NextFloat(0.8f, 1.5f);
            var dust = Dust.NewDustPerfect(position, Main.rand.NextBool() ? DustID.Cloud : DustID.RainCloud, velocity, Scale: Main.rand.NextFloat(2, 3));
            dust.noGravity = true;
        }
    }
}

public class SkycrocTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [18];
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.addTile(Type);

        RegisterItemDrop(ModContent.ItemType<Skycroc>());
        AddMapEntry(new Color(53, 178, 241));

        DustType = DustID.Skyware;
        HitSound = SoundID.Dig;
    }
}

public class SkycrocGeneration : ModSystem
{
    public override void Load() => On_WorldGen.CloudIsland += AddInSkycrocs;

    private void AddInSkycrocs(On_WorldGen.orig_CloudIsland orig, int i, int j)
    {
        orig(i, j);

        if (WorldGen.genRand.NextBool(3))
            return;

        for (int k = 0; k < 10000; ++k)
        {
            int variant = WorldGen.genRand.Next(6);
            int x = i + WorldGen.genRand.Next(50, 90) * (WorldGen.genRand.NextBool() ? -1 : 1);
            int y = j - WorldGen.genRand.Next(-10, 40);
            string structure = "Structures/SkycrocCloud_" + variant;
            Point16 size = Generator.GetStructureDimensions(structure, Mod);

            if (Collision.SolidCollision(new Vector2(x, y) * 16, size.X * 16, size.Y * 16))
                continue;

            Generator.GenerateStructure(structure, new Point16(x, y), Mod);
            break;
        }
    }
}