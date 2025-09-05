using NPCUtils;
using Terraria.GameContent.Bestiary;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class Crocodile : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(50, 32);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<CrocodilePlayer>().active = SportsMode;
    }
}

internal class CrocodilePlayer : ModPlayer
{
    public bool? active = null;

    public override void ResetEffects() => active = null;

    public override void PostUpdateEquips()
    {
        if (active is null)
        {
            return;
        }

        Player.breathEffectiveness += Player.JibbitModifier(2, 2.5f);

        if (!Player.wet)
        {
            return;
        }

        if (active.Value)
        {
            Player.accFlipper = true;
            Player.GetDamage(DamageClass.Melee) += Player.JibbitModifier(0.4f, 0.6f);
        }
        else
        {
            Player.ignoreWater = true;
            Player.moveSpeed += Player.JibbitModifier(0.2f, 0.25f);
            Player.GetDamage(DamageClass.Generic) += Player.JibbitModifier(0.1f, 0.2f);
        }
    }
}

public class CrocodileNPC : ModNPC
{
    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 6;

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.Piranha);
        NPC.damage = 14;
        NPC.lifeMax = 15;
        NPC.defense = 0;
        NPC.aiStyle = NPCAIStyleID.Piranha;

        AIType = NPCID.Piranha;
        AnimationType = NPCID.Piranha;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.damage = 14;
        NPC.lifeMax = 15;
        NPC.defense = 0;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.AddInfo(this, "Jungle");
        bestiaryEntry.UIInfoProvider = new CritterUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[Type]);
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Water && spawnInfo.Player.ZoneJungle ? 0.2f : 0;
    public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.AddCommon<Crocodile>();
}