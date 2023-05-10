using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace TidesOfTime.Common.Rendering
{
    public class PrimitiveSystem : ModSystem
    {
        private readonly Dictionary<string, RenderingStepData> renderData;

        public PrimitiveSystem()
        {
            renderData = new Dictionary<string, RenderingStepData>();
        }

        public override void Load()
        {
            On_Main.DoDraw += PreparePrimitives;
            On_Main.DrawProjectiles += DrawRenderTargets;

            Main.OnResolutionChanged += resolution =>
            {
                TargetsNeedResizing();
            };
        }

        public override void Unload()
        {
            Main.OnResolutionChanged -= resolution =>
            {
                TargetsNeedResizing();
            };

            foreach (RenderingStepData data in renderData.Values)
            {
                data.RenderTarget.Dispose();
            }
        }

        private void PreparePrimitives(On_Main.orig_DoDraw orig, Main self, GameTime gameTime)
        {
            if (Main.gameMenu || Main.dedServ)
            {
                orig(self, gameTime);
                return;
            }

            foreach (string id in renderData.Keys)
            {
                GraphicsDevice device = Main.graphics.GraphicsDevice;

                RenderTargetBinding[] bindings = device.GetRenderTargets();

                device.SetRenderTarget(renderData[id].RenderTarget);
                device.Clear(Color.Transparent);

                for (int i = 0; i < renderData[id].RenderEntries.Count; i++)
                {
                    renderData[id].RenderEntries[i].Invoke();
                }

                device.SetRenderTargets(bindings);

                Finish(id);
            }

            orig(self, gameTime);
        }

        private void DrawRenderTargets(On_Main.orig_DrawProjectiles orig, Main self)
        {
            orig(self);

            foreach (string id in renderData.Keys)
            {
                Palette palette = renderData[id].Palette;

                bool doNotApplyCorrection = palette.NoCorrection || Main.graphics.GraphicsProfile == GraphicsProfile.Reach;

                Effect paletteCorrection = doNotApplyCorrection ? null : Filters.Scene["PaletteCorrection"].GetShader().Shader;

                if (paletteCorrection != null)
                {
                    paletteCorrection.Parameters["palette"].SetValue(palette.Colors);
                    paletteCorrection.Parameters["colorCount"].SetValue(palette.ColorCount);
                }

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, paletteCorrection, Main.GameViewMatrix.TransformationMatrix);

                Main.spriteBatch.Draw(renderData[id].RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

                Main.spriteBatch.End();
            }
        }

        public void TargetsNeedResizing()
        {
            Unload();

            foreach (string id in renderData.Keys)
            {
                Palette palette = renderData[id].Palette;

                renderData[id] = new(palette);
            }
        }

        /// <summary>
        /// Registers a rendertarget for use with a drawing action or list of drawing actions. This is used so that all draw calls of a needed palette can be done with a single RT.
        /// </summary>
        /// <param name="id">ID of the rendertarget and its layer.</param>
        /// <param name="palette">The given palette.</param>
        public void RegisterRenderTargetWithPalette(string id, string palettePath)
        {
            Main.QueueMainThreadAction(() =>
            {
                Palette palette = Palette.From(palettePath);

                renderData[id] = new RenderingStepData(palette);
            });
        }

        public void QueueRenderAction(string id, Action renderAction)
        {
            renderData[id].RenderEntries.Add(renderAction);
        }

        private void Finish(string id)
        {
            renderData[id].RenderEntries.Clear();
        }

        private struct RenderingStepData
        {
            public List<Action> RenderEntries;

            public RenderTarget2D RenderTarget;

            public Palette Palette;

            public RenderingStepData(Palette palette)
            {
                RenderEntries = new List<Action>();

                RenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2, false,
                    SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

                Palette = palette;
            }
        }
    }
}
