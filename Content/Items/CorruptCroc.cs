using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace CroctoberMod.Content.Items;

[AutoloadEquip(EquipType.Shoes)]
internal class CorruptCroc : Croc
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(44, 24);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.GetModPlayer<CorruptPlayer>().active = SportsMode;
        player.GetModPlayer<CorruptPlayer>().instance = Item;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        base.ModifyTooltips(tooltips);

        tooltips.Insert(3, new TooltipLine(Mod, "MiddleClickNotice", this.GetLocalization("MiddleClickNotice").Value));
    }
}

internal class CorruptPlayer : ModPlayer
{
    public bool? active = null;
    public Item instance = null;
    public int? proj = null;

    public override void ResetEffects()
    {
        active = null;
        instance = null;
    }

    public override void PostUpdateEquips()
    {
        if (active is null)
        {
            return;
        }

        if (proj.HasValue)
        {
            Projectile projectile = Main.projectile[proj.Value];

            if (!projectile.active || projectile.type != ModContent.ProjectileType<MuncherOfOre>())
                proj = null;
        }

        proj ??= Projectile.NewProjectile(new EntitySource_ItemUse(Player, instance), Player.Center, Vector2.Zero, ModContent.ProjectileType<MuncherOfOre>(), 0, 0, Player.whoAmI);
    }
}

public class MuncherOfOre : ModProjectile
{
    public class Segment : Entity
    {
        private readonly Entity _parent;
        private readonly bool _isTail;

        public float opacity = 1f;

        private float _rot = 0;

        public Segment(Vector2 pos, Entity parent, bool isTail)
        {
            position = pos;
            _parent = parent;
            _isTail = isTail;
        }

        public void Update()
        {
            float bodyLength = _parent is Projectile ? 6 : 12;

            if (DistanceSQ(_parent.Center) > bodyLength * bodyLength)
                Center += this.SafeDirectionTo(_parent.Center) * (Distance(_parent.Center) - bodyLength);

            if (_parent.Center != Center)
                _rot = AngleTo(_parent.Center) + MathHelper.PiOver2;
        }

        public void Draw()
        {
            if (_parent is Projectile proj && proj.Opacity < 0.2f)
                opacity *= 0.85f;
            else if (_parent is Segment segment && segment.opacity < 0.2f)
                opacity *= 0.85f;

            var tex = TextureAssets.Projectile[ModContent.ProjectileType<MuncherOfOre>()].Value;
            var col = Lighting.GetColor(Center.ToTileCoordinates());
            var src = new Rectangle(8, _isTail ? 44 : 30, 12, 12);
            Main.EntitySpriteDraw(tex, Center - Main.screenPosition, src, col * opacity, _rot, src.Size() / 2f, 1f, SpriteEffects.None, 0);
        }
    }

    private Player Owner => Main.player[Projectile.owner];
    private ref bool? Active => ref Owner.GetModPlayer<CorruptPlayer>().active;

    private ref float State => ref Projectile.ai[0];

    private Vector2 TargetTile
    {
        get => new(Projectile.ai[1], Projectile.ai[2]);
        set => (Projectile.ai[1], Projectile.ai[2]) = (value.X, value.Y);
    }

    private ref float Timer => ref Projectile.localAI[0];

    private int StoredTileType
    {
        get => (int)Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    private bool LastActive
    {
        get => Projectile.localAI[2] == 1;
        set => Projectile.localAI[2] = value ? 1 : 0;
    }

    private readonly List<Segment> _segments = [];

    public override void SetDefaults()
    {
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.Size = new Vector2(64);
        Projectile.tileCollide = false;
    }

    public override bool? CanCutTiles() => false;
    public override bool? CanDamage() => false;

    public override void AI()
    {
        if (Active is null)
        {
            Projectile.Kill();
            return;
        }

        if (LastActive != Active)
        {
            State = 0;
        }

        LastActive = Active is true;

        if (Projectile.velocity.LengthSquared() > 0.1f)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        Projectile.timeLeft++;
        Projectile.frameCounter += (int)(State == 0 ? Owner.JibbitModifier(5, 8) : Owner.JibbitModifier(25, 30));

        foreach (var item in _segments)
            item.Update();

        if (Owner.DistanceSQ(Projectile.Center) > 1800 * 1800)
        {
            Projectile.Center = Owner.Center - new Vector2(Main.rand.NextFloat(-5, 5), 80);

            foreach (var item in _segments)
                item.Center = Projectile.Center;
        }

        if (State == 0)
        {
            Vector2 target = Owner.Center - new Vector2(0, 60);

            if (Projectile.DistanceSQ(target) > 28 * 28)
                Projectile.velocity += Projectile.SafeDirectionTo(target) * 0.3f;

            if (Projectile.velocity.LengthSquared() > 9 * 9)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 9;

            if (Main.myPlayer == Projectile.owner && Owner.HeldItem.pick > 0 && !Owner.ItemTimeIsZero)
            {
                Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];

                if (TileID.Sets.Ore[tile.TileType] && tile.HasTile)
                {
                    StoredTileType = tile.TileType;
                    TargetTile = new Vector2(Player.tileTargetX, Player.tileTargetY) * 16;
                    State = 1f;

                    Projectile.netUpdate = true;
                }
            }
        }
        else if (State == 1)
        {
            Tile tile = Main.tile[TargetTile.ToTileCoordinates()];

            if (!tile.HasTile && Main.myPlayer == Owner.whoAmI)
            {
                if (FindNearbyTile(TargetTile.ToTileCoordinates(), StoredTileType, Active is true, out Vector2 newPos))
                    TargetTile = newPos;
                else
                    State = 0;

                Projectile.netUpdate = true;
            }

            if (TargetTile.DistanceSQ(Projectile.Center) > 10 * 10)
            {
                Projectile.velocity += Projectile.SafeDirectionTo(TargetTile) * 0.9f;

                if (Projectile.velocity.LengthSquared() > 6 * 6)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 6;

                Projectile.velocity *= 0.95f;
            }
            else
            {
                Projectile.velocity *= 0.75f;

                Timer++;

                if (tile.HasTile && Timer >= (Active is true ? Owner.JibbitModifier(25, 18) : Owner.JibbitModifier(60, 50)))
                {
                    Timer = 0;
                    Item item = Owner.GetBestPickaxe();

                    if (item is not null)
                        Owner.PickTile((int)TargetTile.X / 16, (int)TargetTile.Y / 16, (int)(Active is true ? item.pick * 0.8f : item.pick));
                    else
                        State = 0;
                }
            }
        }
    }

    private static bool FindNearbyTile(Point position, int storedTileType, bool sports, out Vector2 newPosition)
    {
        int distance = sports ? 2 : 8;
        HashSet<Point16> positions = [];

        for (int i = position.X - distance; i < position.X + distance; ++i)
        {
            for (int j = position.Y - distance; j < position.Y + distance; ++j)
            {
                Tile tile = Main.tile[i, j];

                if (tile.HasTile && tile.TileType == storedTileType)
                    positions.Add(new Point16(i, j));
            }
        }

        if (positions.Count == 0)
        {
            newPosition = Vector2.Zero;
            return false;
        }

        newPosition = Main.rand.Next([.. positions]).ToWorldCoordinates();
        return true;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (_segments.Count == 0)
            SpawnBody();

        foreach (var item in _segments)
            item.Draw();

        var tex = TextureAssets.Projectile[ModContent.ProjectileType<MuncherOfOre>()].Value;
        Color col = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity;
        var headFrame = new Rectangle(30 * (Projectile.frameCounter * 0.05f % 30 < 15 ? 0 : 1), 0, 28, 28);
        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, headFrame, col, Projectile.rotation, new(14), 1f, SpriteEffects.None, 0);

        return false;
    }

    private void SpawnBody()
    {
        const int Length = 7;

        Segment parent = null;

        for (int i = 0; i < Length; ++i)
        {
            parent = new Segment(Projectile.Center, parent ?? (Entity)Projectile, i == Length - 1);
            _segments.Add(parent);
        }
    }
}

public class CorruptCrocDropping : GlobalTile
{
    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        Tile tile = Main.tile[i, j];

        if (type == TileID.ShadowOrbs && !noItem && tile.TileFrameX == 0 && tile.TileFrameY is 0 or 36 && Main.rand.NextBool(3))
            Item.NewItem(new EntitySource_TileBreak(i, j), new Rectangle(i * 16, j * 16, 32, 32), ModContent.ItemType<CorruptCroc>(), prefixGiven: -1);
    }
}