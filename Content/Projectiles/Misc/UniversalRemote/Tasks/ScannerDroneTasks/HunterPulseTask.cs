using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using TidesOfTime.Common.Rendering;
using TidesOfTime.Localization;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks.ScannerDroneTasks
{
    public class HunterPulseTask : DroneTask
    {
        private bool initialBuff;

        private float pulseFactor;

        private int pulseDelay;

        private readonly Trail scanTrail;

        private readonly Vector2[] points;

        private Vector2 nextPosition;

        public HunterPulseTask() : base(DroneType.Scanner)
        {
            if (!Main.dedServ)
            {
                scanTrail = new Trail(Main.graphics.GraphicsDevice, 128, new NoTip(), factor => 20, uv => Color.Orange * MathF.Min(uv.Y * (1 - pulseFactor), 0.8f));
                points = new Vector2[128];
            }
        }

        public override string TaskDescription => LocalizationHelper.GetGUIText("UniversalRemote.ScannerHunterPulseTaskInfo");

        public override bool AI()
        {
            if (!initialBuff)
            {
                Main.player[WorkerDrone.Projectile.owner].AddBuff(BuffID.Hunter, 300);

                initialBuff = true;
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

            return true;
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
                effect.Parameters["opacity"].SetValue(1.0f);

                scanTrail.Positions = points;
                scanTrail.NextPosition = nextPosition;

                scanTrail.Render(effect);
            });
        }
    }
}
