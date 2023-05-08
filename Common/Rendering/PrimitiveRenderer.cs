using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;

namespace TidesOfTime.Common.Rendering
{
    public class PrimitiveRenderer
    {
        private readonly Dictionary<RenderingStep, RenderingStepData> renderData;

        public PrimitiveRenderer()
        {
            renderData = new Dictionary<RenderingStep, RenderingStepData>();

            Main.QueueMainThreadAction(() =>
            {
                foreach (RenderingStep step in Enum.GetValues(typeof(RenderingStep)))
                {
                    renderData[step] = new();
                }
            });
        }

        public void Load()
        {
            On_Main.DoDraw += PreparePrimitives;
            On_Main.DrawProjectiles += DrawRenderTarget;
        }

        private void PreparePrimitives(On_Main.orig_DoDraw orig, Main self, GameTime gameTime)
        {
            if (Main.gameMenu || Main.dedServ)
            {
                orig(self, gameTime);
                return;
            }

            GraphicsDevice device = Main.graphics.GraphicsDevice;

            RenderTargetBinding[] bindings = device.GetRenderTargets();

            device.SetRenderTarget(renderData[RenderingStep.PreDraw].RenderTarget);
            device.Clear(Color.Transparent);

            for (int i = 0; i < renderData[RenderingStep.PreDraw].RenderActions.Count; i++)
            {
                renderData[RenderingStep.PreDraw].RenderActions[i].Invoke();
            }

            device.SetRenderTargets(bindings);

            Finish(RenderingStep.PreDraw);

            orig(self, gameTime);
        }

        private void DrawRenderTarget(On_Main.orig_DrawProjectiles orig, Main self)
        {
            orig(self);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            Main.spriteBatch.Draw(renderData[RenderingStep.PreDraw].RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
            Main.spriteBatch.End();
        }

        public void TargetsNeedResizing()
        {
            Unload();

            foreach (RenderingStep step in Enum.GetValues(typeof(RenderingStep)))
            {
                renderData[step] = new();
            }
        }

        public void Unload()
        {
            foreach (RenderingStepData data in renderData.Values)
            {
                data.RenderTarget.Dispose();
            }
        }


        public void QueueRenderAction(RenderingStep step, Action renderAction)
        {
            renderData[step].RenderActions.Add(renderAction);
        }

        private void Finish(RenderingStep step)
        {
            renderData[step].RenderActions.Clear();
        }

        private struct RenderingStepData
        {
            public List<Action> RenderActions;

            public RenderTarget2D RenderTarget;

            public RenderingStepData()
            {
                RenderActions = new List<Action>();

                RenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2, false,
                    SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            }
        }
    }
}
