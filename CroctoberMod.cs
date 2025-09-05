global using Terraria.ModLoader;
global using Terraria;
global using Terraria.ID;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;

namespace CroctoberMod;

public class CroctoberMod : Mod
{
    public override void Load() => NPCUtils.NPCUtils.TryLoadBestiaryHelper();
    public override void Unload() => NPCUtils.NPCUtils.UnloadBestiaryHelper();
}
