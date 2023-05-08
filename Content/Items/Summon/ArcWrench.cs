using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TidesOfTime.Content.Items.Abstract;
using TidesOfTime.Content.Projectiles.Summon;
using TidesOfTime.Content.Rarities;

namespace TidesOfTime.Content.Items.Summon
{
    public class ArcWrench : AnimatedInventoryItem
    {
        public override int FrameCount => 8;

        public override int TicksPerFrame => 5;

        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 58;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;

            Item.damage = 225;
            Item.DamageType = DamageClass.Summon;
            Item.noMelee = true;

            Item.mana = 20;

            Item.rare = ModContent.RarityType<Futuristic>();

            Item.shoot = ModContent.ProjectileType<TeslaCoil>();

            Item.sentry = true;
        }

        public override bool CanUseItem(Player player)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int x = (int)(Main.MouseWorld.X / 16) + i;
                    int y = (int)(Main.MouseWorld.Y / 16) + j;

                    if (!WorldGen.InWorld(x, y))
                    {
                        return false;
                    }

                    Tile tile = Main.tile[x, y];

                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        return false;
                    }
                }
            }

            return base.CanUseItem(player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            player.FindSentryRestingSpot(type, out int worldX, out int worldY, out _);

            float pushYUp = ContentSamples.ProjectilesByType[Item.shoot].height / 2f;

            position = new Vector2(worldX, worldY - pushYUp);
            velocity = Vector2.Zero;
        }
    }
}
