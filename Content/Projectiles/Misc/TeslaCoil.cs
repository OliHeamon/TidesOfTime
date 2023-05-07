using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using TidesOfTime.Common.Math;
using TidesOfTime.Common.Rendering;

namespace TidesOfTime.Content.Projectiles.Misc
{
    public class TeslaCoil : ModProjectile
    {
        private const int MaxRange = 16 * 48;

        private const int FrameCount = 4;

        private const int TicksPerFrame = 5;

        private const int LightningMinPoints = 12;

        private const int LightningMaxPoints = 32;

        private const float PointOffset = 5;

        private const int ArcChangeFrequency = 10;

        private const int ArcOffsetLength = 60;

        private const int ArcWidth = 14;

        private static readonly Vector2 LightningOffset = new(32, 2);

        private float Target
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float DistanceToTarget
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private readonly Texture2D activeTexture;

        private readonly Trail lightningTrail;

        private readonly BezierCurve curve;

        private float arcOffset;

        private RenderTarget2D target;

        public TeslaCoil()
        {
            if (!Main.dedServ)
            {
                activeTexture = ModContent.Request<Texture2D>($"{Texture}_Active", AssetRequestMode.ImmediateLoad).Value;

                lightningTrail = new Trail(Main.graphics.GraphicsDevice, LightningMaxPoints, new TriangularTip(ArcWidth), factor => factor == 0 ? 0 : ArcWidth, texCoords => Color.White);

                curve = new BezierCurve(new Vector2[3]);

                Main.QueueMainThreadAction(() =>
                {
                    target = new RenderTarget2D(Main.graphics.GraphicsDevice, 1920, 1080, false, SurfaceFormat.Color, DepthFormat.None, 1, RenderTargetUsage.DiscardContents);
                });
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
                    DistanceToTarget = closestDistance;
                }
                else
                {
                    Target = -1;
                    DistanceToTarget = -1;
                }

                Projectile.netUpdate = true;
            }

            if (!Main.dedServ && Target != -1)
            {
                ManageTrail();
            }
        }

        private void ManageTrail()
        {
            int pointCount = (int)MathHelper.Min((int)(DistanceToTarget / MaxRange * LightningMaxPoints) + LightningMinPoints, LightningMaxPoints);

            Vector2 targetPosition = Main.npc[(int)Target].Center;

            Vector2[] vectors = GetBezierCurve(Projectile.position + LightningOffset, targetPosition, pointCount);

            for (int i = 0; i < pointCount; i++)
            {
                Vector2 normal;

                if (i != pointCount - 1)
                {
                    normal = (vectors[i + 1] - vectors[i]).SafeNormalize(Vector2.Zero);
                }
                else
                {
                    normal = (targetPosition - vectors[i]).SafeNormalize(Vector2.Zero);
                }

                Vector2 rotatedNormal = normal.RotatedBy(MathHelper.PiOver2);

                float randomOffset = Main.rand.NextFloat(-PointOffset, PointOffset);

                if (i != 0 && i != pointCount - 1)
                {
                    vectors[i] = vectors[i] + (rotatedNormal * (float)(randomOffset + (Math.Sin(-Main.GameUpdateCount + i) * PointOffset * 2)));
                }
            }

            for (int i = pointCount; i < LightningMaxPoints; i++) 
            {
                vectors[i] = targetPosition;
            }

            lightningTrail.Positions = vectors;
            lightningTrail.NextPosition = targetPosition;
        }

        private Vector2[] GetBezierCurve(Vector2 start, Vector2 end, int pointCount)
        {
            Vector2 midpoint = (start + end) / 2;

            Vector2 normal = (end - start).SafeNormalize(Vector2.Zero);

            // With dot product, 1 means parallel vectors and 0 means perpendicular.
            float rotationFactor = Vector2.Dot(normal, -Vector2.UnitY);

            if (rotationFactor < -0.25)
            {
                normal = Vector2.UnitX.RotatedBy(MathHelper.PiOver4 / 2);
            }

            Vector2 rotatedNormal = normal.RotatedBy(MathHelper.PiOver2) * -Math.Sign(normal.X);

            // This will allow the distance to reflect how vertical the enemy is. If the enemy is right above the tesla coil there will be no horizontal offset resulting.
            // However, a horizontal enemy may have a large vertical offset.
            float arcMultiplier = 1 - rotationFactor;

            float distanceMultiplier = MathHelper.Lerp(1, 2, DistanceToTarget / MaxRange);

            if (Main.rand.NextBool(ArcChangeFrequency))
            {
                arcOffset = Main.rand.NextFloat(0.5f, 2) * ArcOffsetLength * arcMultiplier * distanceMultiplier;
            }

            arcOffset += 7.5f;

            curve[0] = start;
            curve[1] = midpoint + (rotatedNormal * arcOffset);
            curve[2] = end;

            List<Vector2> curvePoints = curve.GetPoints(pointCount);

            Vector2[] vectors = new Vector2[LightningMaxPoints];

            for (int i = 0; i < pointCount; i++)
            {
                vectors[i] = curvePoints[i];
            }

            return vectors;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Target != -1)
            {
                TidesOfTimeUtils.DrawAnimatedTexture(activeTexture, FrameCount, TicksPerFrame, Projectile.position - Main.screenPosition, lightColor, Vector2.Zero, 1);

                Main.spriteBatch.End();

                /*RenderTargetBinding[] targets = Main.graphics.GraphicsDevice.GetRenderTargets();

                Main.graphics.GraphicsDevice.SetRenderTarget(target);*/

                Effect effect = Filters.Scene["TeslaCoilLightning"].GetShader().Shader;

                Matrix world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
                Matrix view = Main.GameViewMatrix.ZoomMatrix;
                Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                effect.Parameters["transformMatrix"].SetValue(world * view * projection);

                lightningTrail?.Render(effect);

                /*Main.graphics.GraphicsDevice.SetRenderTargets(targets);

                /Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone);

                Main.spriteBatch.Draw(target, new Rectangle(0, 0, 1920, 1080), Color.White);

                Main.spriteBatch.End();*/
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                return false;
            }

            return base.PreDraw(ref lightColor);
        }
    }
}
