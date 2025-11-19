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
                Tile tile = Main.tile[chest.x, chest.y];

                if (tile.TileType == TileID.Containers)
                {
                    if (tile.TileFrameX == 36 && WorldGen.genRand.NextBool(2))
                        AddItemToChest(chest, ModContent.ItemType<TheGoldenCroc>());

                    if (tile.TileFrameX == 0 && WorldGen.genRand.NextBool(2))
                        AddItemToChest(chest, ModContent.ItemType<SimpleCrocs>());

                    if (tile.TileFrameX == 612 && WorldGen.genRand.NextBool(2))
                        AddItemToChest(chest, ModContent.ItemType<SandyCrocs>());

                    if (tile.TileFrameX == 144 && WorldGen.genRand.NextBool(2))
                        AddItemToChest(chest, ModContent.ItemType<Hellcroc>());
                }
                else if (tile.TileType == TileID.Containers2)
                {
                    if (tile.TileFrameX == 360 && WorldGen.genRand.NextBool(2))
                        AddItemToChest(chest, ModContent.ItemType<DesertCrocs>());
                }
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