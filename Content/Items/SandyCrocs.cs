using CroctoberMod.Content.Items.Vials;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class SandyCrocs : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(36, 26);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<SandyPlayer>().active = SportsMode;
        player.accFishingLine = true;

        if (!SportsMode)
            player.fishingSkill += (int)player.JibbitModifier(5, 10);
    }
}

internal class SandyPlayer : ModPlayer
{
    public bool? active = null;
    public bool hasGottenCrocs = false;

    public override void ResetEffects() => active = null;

    public override void PostUpdateEquips()
    {
        if (active is null)
        {
            return;
        }
    }

    public override void ModifyFishingAttempt(ref FishingAttempt attempt)
    {
        if (active is false)
            attempt.waterQuality = 1f;
    }

    public override void ModifyCaughtFish(Item fish)
    {
        if (fish.rare == ItemRarityID.Gray && active is true)
        {
            WeightedRandom<int> types = new();
            types.Add(ModContent.ItemType<JustifiedVial>(), 1);
            types.Add(ModContent.ItemType<SturdyVial>(), 1);
            types.Add(ModContent.ItemType<FuriousVial>(), 1);

            if (Player.GlimmeringJibbit())
                types.Add(ModContent.ItemType<GlimmeringVial>(), 0.667f);

            fish.SetDefaults(types);
            fish.stack = 1;
        }
    }

    public override void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems)
    {
        if (!hasGottenCrocs)
        {
            hasGottenCrocs = true;
            rewardItems.Add(new Item(ModContent.ItemType<SandyCrocs>()));
        }
    }

    public override void SaveData(TagCompound tag) => tag.Add("hasCrocs", hasGottenCrocs);
    public override void LoadData(TagCompound tag) => hasGottenCrocs = tag.GetBool("hasCrocs");
}

internal class SandyProjectile : GlobalProjectile
{
    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.aiStyle == ProjAIStyleID.Bobber;

    public override bool PreAI(Projectile projectile)
    {
        if (projectile.TryGetOwner(out var owner) && owner.GetModPlayer<SandyPlayer>().active is { } active)
            Lighting.AddLight(projectile.Center, new Vector3(189, 160, 123) / (active ? 255f : 400f) * owner.JibbitModifier(1, 1.3f));

        return true;
    }
}
