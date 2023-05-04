using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TidesOfTime.Content.Projectiles.Misc
{
    public class TeslaCoil : ModProjectile
    {
        private const int MaxRange = 16 * 48;

        private const int FrameCount = 4;

        private const int TicksPerFrame = 5;

        private float Target
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private readonly Texture2D activeTexture;

        public TeslaCoil()
        {
            if (!Main.dedServ)
            {
                activeTexture = ModContent.Request<Texture2D>($"{Texture}_Active", AssetRequestMode.ImmediateLoad).Value;
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 80;

            Projectile.sentry = true;

            Projectile.tileCollide = false;

            Projectile.timeLeft = ushort.MaxValue;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Main.player[Projectile.owner].UpdateMaxTurrets();
        }

        public override void AI()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC closest = null;

                float closestDistance = float.MaxValue;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (!npc.active)
                    {
                        continue;
                    }

                    if (!npc.CanBeChasedBy())
                    {
                        continue;
                    }

                    float distance = npc.Distance(Projectile.Center);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;

                        closest = npc;
                    }
                }

                if (closest != null && closestDistance < MaxRange)
                {
                    Target = closest.whoAmI;
                }
                else
                {
                    Target = -1;
                }

                Projectile.netUpdate = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Target != -1)
            {
                TidesOfTimeUtils.DrawAnimatedTexture(activeTexture, FrameCount, TicksPerFrame, Projectile.position - Main.screenPosition, lightColor, Vector2.Zero, 1);

                return false;
            }

            return base.PreDraw(ref lightColor);
        }

        public override void PostDraw(Color lightColor)
        {
            base.PostDraw(lightColor);
        }
    }
}
