global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria;
global using Terraria.ID;
global using Terraria.ModLoader;

using System.IO;

namespace CroctoberMod;

public class CroctoberMod : Mod
{
    public override void Load() => NPCUtils.NPCUtils.TryLoadBestiaryHelper();
    public override void Unload() => NPCUtils.NPCUtils.UnloadBestiaryHelper();
    public override void PostSetupContent() => NetEasy.NetEasy.Register(this);
    public override void HandlePacket(BinaryReader reader, int whoAmI) => NetEasy.NetEasy.HandleModule(reader, whoAmI);
}

public static class Extensions
{
    public static Vector2 DirectionTo(this Entity entity, Entity other) => entity.DirectionTo(other.Center);
    public static Vector2 SafeNormalize(ref this Vector2 vector) => vector = (vector == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vector));
    public static Vector2 SafeDirectionTo(this Entity entity, Vector2 worldPosition) => Utils.SafeNormalize(worldPosition - entity.Center, Vector2.Zero);
    public static Vector2 SafeDirectionTo(this Entity entity, Entity other) => Utils.SafeNormalize(other.Center - entity.Center, Vector2.Zero);
}