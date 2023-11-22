using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace TidesOfTime.Common.Rendering.ProceduralGore
{
    public class ProceduralGoreSystem : ModSystem
    {
        private readonly Dictionary<int, RenderTarget2D> perNpcTargets;

        private readonly List<int> requiresInitialising;

        private readonly Dictionary<int, List<SingleNPCGoreSet>> gores;

        private readonly Dictionary<int, Instance[]> instances;

        private readonly Dictionary<int, int> instanceCounts;

        private const int InflationOffset = 4;

        private const int MaxInstances = 65535;

        private DynamicVertexBuffer vertexBuffer;

        private DynamicIndexBuffer indexBuffer;

        private DynamicVertexBuffer instanceBuffer;

        private static readonly VertexPositionTexture[] GoreParticle = new[]
        {
            new VertexPositionTexture(Vector2.Zero.ToVector3(), Vector2.Zero),
            new VertexPositionTexture(Vector2.UnitX.ToVector3(), Vector2.UnitX),
            new VertexPositionTexture(Vector2.One.ToVector3(), Vector2.One),
            new VertexPositionTexture(Vector2.UnitY.ToVector3(), Vector2.UnitY),
        };

        private static readonly short[] GoreParticleIndices = new short[]
        {
            0, 1, 3, 1, 2, 3
        };

        private static readonly VertexDeclaration InstanceData = new
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Normal, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.Normal, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.Normal, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.Normal, 3),
            new VertexElement(64, VertexElementFormat.Vector4, VertexElementUsage.Color, 0)
        );

        public ProceduralGoreSystem()
        {
            perNpcTargets = new();
            requiresInitialising = new();
            gores = new();
            instances = new();
            instanceCounts = new();
        }

        public override void Load()
        {
            if (!Main.dedServ)
            {
                Main.QueueMainThreadAction(() =>
                {
                    GraphicsDevice device = Main.graphics.GraphicsDevice;

                    vertexBuffer = new(device, typeof(VertexPositionTexture), GoreParticle.Length, BufferUsage.None);
                    indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, GoreParticleIndices.Length, BufferUsage.None);

                    instanceBuffer = new(device, InstanceData, MaxInstances, BufferUsage.None);

                    SetVertices(GoreParticle);
                    SetIndices(GoreParticleIndices);
                });
            }

            On_Main.DoDraw += PrepareNPCTargets;
        }

        public override void Unload()
        {
            Main.QueueMainThreadAction(() =>
            {
                vertexBuffer?.Dispose();
                indexBuffer?.Dispose();
                instanceBuffer?.Dispose();

                foreach (RenderTarget2D renderTarget in perNpcTargets.Values)
                {
                    renderTarget?.Dispose();
                }
            });
        }

        public override void PostUpdateGores()
        {
            foreach (int key in gores.Keys)
            {
                for (int i = 0; i < gores[key].Count; i++)
                {
                    SingleNPCGoreSet set = gores[key][i];

                    for (int j = 0; j < set.SegmentPositions.Length; j++)
                    {
                        set.SegmentPositions[j] += set.SegmentVelocities[j];
                        set.SegmentRotations[j].X += set.SegmentRotations[j].Y;
                    }

                    gores[key][i] = set;
                }
            }
        }

        private void PrepareNPCTargets(On_Main.orig_DoDraw orig, Main self, GameTime gameTime)
        {
            if (Main.gameMenu || Main.dedServ)
            {
                orig(self, gameTime);
                return;
            }

            foreach (int id in requiresInitialising)
            {
                GraphicsDevice device = Main.graphics.GraphicsDevice;

                RenderTargetBinding[] bindings = device.GetRenderTargets();

                device.SetRenderTarget(perNpcTargets[id]);
                device.Clear(Color.Transparent);

                Main.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

                NPC npc = new();
                npc.SetDefaults(id);

                // Needed to fix lighting issues.
                npc.IsABestiaryIconDummy = true;

                Rectangle frame = GetFrameSize(npc);

                npc.Center = new(frame.Width / 2, frame.Height / 2);

                // Renders an NPC onto the rendertarget. The rendertarget is then stored to be used as a sample texture for the gores.
                Main.instance.DrawNPCDirect(Main.spriteBatch, npc, false, Vector2.Zero);

                Main.spriteBatch.End();

                device.SetRenderTargets(bindings);
            }

            requiresInitialising.Clear();

            ModContent.GetInstance<PrimitiveSystem>().QueueRenderAction("Standard", () =>
            {
                GraphicsDevice device = Main.graphics.GraphicsDevice;

                device.RasterizerState = RasterizerState.CullNone;

                // In order to make this as efficient as possible, all gores for a given NPC type will be rendered at the same time using instanced rendering.
                // This is possible because the texture used is constant for all gores of a given NPC type.
                Effect effect = Filters.Scene["InstancedGoreRenderer"].GetShader().Shader;

                Matrix world = Matrix.Identity;
                Matrix view = Matrix.Identity;
                Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                effect.Parameters["transformMatrix"].SetValue(world * view * projection);
                effect.Parameters["edgeIntensity"].SetValue(0.5f);
                effect.Parameters["threshold"].SetValue(0.825f);

                foreach (int id in gores.Keys)
                {
                    // Don't render if the rendertarget hasn't been initialised.
                    if (requiresInitialising.Contains(id))
                    {
                        continue;
                    }

                    List<SingleNPCGoreSet> goreSet = gores[id];

                    // Set the texture to the one corresponding to the current NPC type.
                    RenderTarget2D renderTarget = perNpcTargets[id];

                    effect.Parameters["goreTexture"].SetValue(renderTarget);
                    effect.Parameters["textureSize"].SetValue(new Vector2(renderTarget.Width, renderTarget.Height));
                    effect.Parameters["cutColor"].SetValue(GoreColor.GetGoreColor(id));

                    // Construct and set instance data corresponding to each particle.
                    CalculateInstanceData(id, renderTarget);

                    SetInstances(id);

                    // Instanced render all particles.
                    device.SetVertexBuffers(vertexBuffer, new VertexBufferBinding(instanceBuffer, 0, 1));
                    device.Indices = indexBuffer;

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3, instanceCounts[id]);
                    }
                }
            });

            orig(self, gameTime);
        }

        public void AddGore(NPC npc, Vector2 position, DivisionType divisionType)
        {
            Rectangle frame = GetFrameSize(npc);

            if (!perNpcTargets.ContainsKey(npc.type))
            {
                RenderTarget2D npcTarget = new(Main.graphics.GraphicsDevice, frame.Width, frame.Height);

                perNpcTargets[npc.type] = npcTarget;

                requiresInitialising.Add(npc.type);
            }

            if (!gores.ContainsKey(npc.type))
            {
                gores[npc.type] = new();
            }

            // All gore fragment sets are stored by NPC type to allow for instanced rendering (matching NPC types use the same texture).
            gores[npc.type].Add(new(position, frame.Width, frame.Height, divisionType));
            
            if (!instances.ContainsKey(npc.type))
            {
                instances[npc.type] = new Instance[MaxInstances];
                instanceCounts[npc.type] = 0;
            }
        }

        private void SetVertices(VertexPositionTexture[] vertices)
        {
            vertexBuffer?.SetData(0, vertices, 0, vertices.Length, VertexPositionTexture.VertexDeclaration.VertexStride, SetDataOptions.Discard);
        }

        private void SetIndices(short[] indices)
        {
            indexBuffer?.SetData(0, indices, 0, indices.Length, SetDataOptions.Discard);
        }

        private void SetInstances(int id)
        {
            instanceBuffer?.SetData(instances[id], 0, instanceCounts[id], SetDataOptions.Discard);
        }

        private void CalculateInstanceData(int id, RenderTarget2D renderTarget)
        {
            int index = 0;

            List<SingleNPCGoreSet> goreSet = gores[id];

            for (int i = 0; i < goreSet.Count; i++)
            {
                SingleNPCGoreSet set = goreSet[i];

                for (int j = 0; j < set.DivisionTextureCoordinates.Length; j++)
                {
                    float scaleX = set.DivisionTextureCoordinates[j].Z - set.DivisionTextureCoordinates[j].X;
                    float scaleY = set.DivisionTextureCoordinates[j].W - set.DivisionTextureCoordinates[j].Y;

                    Vector2 initialOffset = new(-renderTarget.Width / 2, -renderTarget.Height / 2);

                    Vector2 translation = initialOffset - Main.screenPosition + set.SegmentPositions[j];

                    Matrix rotation = Matrix.CreateRotationZ(set.SegmentRotations[j].X);
                    Matrix offset = Matrix.CreateTranslation(initialOffset.X / 2, initialOffset.Y / 2, 0);
                    Matrix reset = Matrix.CreateTranslation(-initialOffset.X / 2, -initialOffset.Y / 2, 0);

                    Matrix rotationMatrix = offset * rotation * reset;

                    instances[id][index] = new Instance(
                        Matrix.CreateScale(scaleX * renderTarget.Width, scaleY * renderTarget.Height, 1) *
                        rotationMatrix *
                        Matrix.CreateTranslation(translation.Truncate().ToVector3()),
                        set.DivisionTextureCoordinates[j]);

                    index++;

                    if (index >= MaxInstances)
                    {
                        goto ExitLoop;
                    }
                }
            }

            ExitLoop:

            instanceCounts[id] = index;
        }

        private static Rectangle GetFrameSize(NPC npc)
        {
            Rectangle frame = new(0, 0, npc.frame.Width, npc.frame.Height);

            frame.Inflate(InflationOffset, InflationOffset);

            return frame;
        }
    }
}
