using CroctoberMod.Content.Items;

namespace CroctoberMod;

internal class ChestGeneration : ModSystem
{
    public override void PostWorldGen()
    {
        for (int i = 0; i < Main.maxChests; ++i)
        {
            Chest chest = Main.chest[i];

            if (chest != null)
            {
                if (Main.tile[chest.x, chest.y].TileFrameX == 36 && WorldGen.genRand.NextBool(2))
                    AddItemToChest(chest, ModContent.ItemType<TheGoldenCroc>());

                if (Main.tile[chest.x, chest.y].TileFrameX == 0 && WorldGen.genRand.NextBool(2))
                    AddItemToChest(chest, ModContent.ItemType<SimpleCrocs>());
            }
        }
    }

    private static void AddItemToChest(Chest chest, int type)
    {
        for (int j = 0; j < chest.item.Length; ++j)
        {
            Item item = chest.item[j];

            if (item is null || item.IsAir)
            {
                item.SetDefaults(type);
                item.Prefix(-1);
                break;
            }
        }
    }
}