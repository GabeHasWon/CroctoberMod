using CroctoberMod.Content.Items;
using NetEasy;
using System;

namespace CroctoberMod.Content.Syncing;

[Serializable]
public class SyncGlimmeringJibbitModule(byte who, bool hasJibbit) : Module
{
    private readonly byte who = who;
    private readonly bool hasJibbit = hasJibbit;

    protected override void Receive()
    {
        Main.player[who].GetModPlayer<GlimmeringPlayer>().usedJibbit = hasJibbit;

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
    }
}

