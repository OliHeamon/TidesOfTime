using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria;
using TidesOfTime.Common.Rendering.Tiles;
using Microsoft.Xna.Framework;
using TidesOfTime.Common.Rendering;
using System;
using Terraria.Graphics.Effects;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks.ScannerDroneTasks
{
    public abstract class TileScanTask : DroneTask, IDisposable
    {
        private bool tilesDetected;

        private float pulseFactor;

        private int pulseDelay;

        private Vector2 desiredPosition;

        private readonly Trail scanTrail;

        private readonly Vector2[] points;

        private Vector2 nextPosition;

        public abstract Color Color { get; }

        public TileScanTask() : base(DroneType.Scanner)
        {
            if (!Main.dedServ)
            {
                scanTrail = new Trail(Main.graphics.GraphicsDevice, 128, new NoTip(), factor => 20, uv => Color * MathF.Min(uv.Y * (1 - pulseFactor), 0.3f));
                points = new Vector2[128];
            }
        }

        public override bool AI()
        {
            if (!tilesDetected)
            {
                desiredPosition = WorkerDrone.Projectile.Center;

                TileColouringSystem tileColouringSystem = ModContent.GetInstance<TileColouringSystem>();

                int circleSize = 24;

                for (int x = -circleSize; x < circleSize + 1; x++)
                {
                    for (int y = -circleSize; y < circleSize + 1; y++)
                    {
                        int centerX = (int)(WorkerDrone.Projectile.Center.X / 16);
                        int centerY = (int)(WorkerDrone.Projectile.Center.Y / 16);

                        int tileX = centerX + x;
                        int tileY = centerY + y;

                        float distanceSquared = (x * x) + (y * y);

                        if (WorldGen.InWorld(tileX, tileY) && distanceSquared < circleSize * circleSize && TileMeetsCondition(tileX, tileY))
                        {
                            tileColouringSystem.AddHit(tileX, tileY, Color, ScannerDrone.ScanTimeSeconds * 60);
                        }
                    }
                }

                tilesDetected = true;
            }

            PulseFactor();

            for (int i = 0; i < 128; i++)
            {
                int size = (int)(pulseFactor * 24 * 16);

                Vector2 offset = Vector2.UnitX.RotatedBy(MathF.PI * 2 * (i / (float)(points.Length - 1))) * size;

                Vector2 point = WorkerDrone.Projectile.Center + offset;

                points[i] = point;
            }

            if (Main.GameUpdateCount % 3 == 0)
            {
                Progress += 3f / (ScannerDrone.ScanTimeSeconds * 60);
            }

            if (Progress >= 1)
            {
                Progress = 1;
                IsComplete = true;
            }

            WorkerDrone.Move(desiredPosition, 1);

            return false;
        }

        private void PulseFactor()
        {
            pulseFactor += 0.0125f;

            if (pulseFactor >= 1f)
            {
                pulseDelay++;

                pulseFactor = 1;
            }

            if (pulseDelay >= 30)
            {
                pulseDelay = 0;
                pulseFactor = 0;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Color lightColor)
        {
            Lighting.AddLight(WorkerDrone.Projectile.Center, Color.Lerp(Color.White, Color.Blue, 0.6f).ToVector3());

            ModContent.GetInstance<PrimitiveSystem>().QueueRenderAction("Standard", () =>
            {
                Effect effect = Filters.Scene["ScanPulse"].GetShader().Shader;

                Matrix world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
                Matrix view = Matrix.Identity;
                Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                effect.Parameters["transformMatrix"].SetValue(world * view * projection);

                scanTrail.Positions = points;
                scanTrail.NextPosition = nextPosition;

                scanTrail.Render(effect);
            });
        }

        public abstract bool TileMeetsCondition(int x, int y);

        public void Dispose()
        {
            scanTrail?.Dispose();
        }
    }
}
