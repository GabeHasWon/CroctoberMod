using CroctoberMod.Content.Items;
using NetEasy;
using System;

namespace CroctoberMod.Content.Syncing;

[Serializable]
public class SpawnVFXModule(Vector2 position, SpawnVFXModule.EffectType type) : Module
{
    public enum EffectType
    {
        MarbleTeleport,
    }

    public readonly Vector2 position = position;
    public readonly EffectType type = type;

    protected override void Receive()
    {
        if (Main.netMode != NetmodeID.Server)
            SetSports();
        else
            Send(-1, -1, false);
    }

    private void SetSports()
    {
        if (type == EffectType.MarbleTeleport)
            MarblePlayer.SpawnTeleportDust(position, true);
    }
}

