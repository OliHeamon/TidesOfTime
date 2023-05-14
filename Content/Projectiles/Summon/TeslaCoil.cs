using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using TidesOfTime.Common.Math;
using TidesOfTime.Common.Rendering;

namespace TidesOfTime.Content.Projectiles.Summon
{
    public class TeslaCoil : ModProjectile
    {
        private const int MaxRange = 16 * 48;

        private const int FrameCount = 4;

        private const int TicksPerFrame = 5;

        private const int LightningMinPoints = 8;

        private const int LightningMaxPoints = 32;

        private const float PointOffset = 6;

        private const int ArcChangeFrequency = 10;

        private const int ArcOffsetLength = 60;

        private const int ArcWidth = 12;

        private const int StrikeFrequency = 2;

        private static readonly Vector2 LightningOffset = new(32, 2);

        private static readonly SoundStyle Ambience = new("TidesOfTime/Assets/Sounds/Projectiles/Summon/TeslaCoil_Hum")
        {
            IsLooped = true,
            MaxInstances = 0
        };

        private static readonly SoundStyle Strike = new("TidesOfTime/Assets/Sounds/Projectiles/Summon/TeslaCoil_Strike")
        {
            PitchVariance = 0.1f,
            Volume = 0.8f
        };

        private static readonly SoundStyle Extinguish = new("TidesOfTime/Assets/Sounds/Projectiles/Summon/TeslaCoil_Extinguish")
        {
            PitchVariance = 0.1f,
            Volume = 0.8f
        };

        private static readonly SoundStyle Arcing = new("TidesOfTime/Assets/Sounds/Projectiles/Summon/TeslaCoil_Arcing")
        {
            IsLooped = true,
            MaxInstances = 0
        };

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

        private readonly Trail lighterTrail;

        private readonly BezierCurve curve;

        private float arcOffset;

        private readonly List<TrailInfo> trails;

        private readonly List<TrailInfo> lighterTrails;

        private SlotId soundSlot;

        private int oldTrailCount;

        private int timeArcsHaveExisted;

        public TeslaCoil()
        {
            if (!Main.dedServ)
            {
                activeTexture = ModContent.Request<Texture2D>($"{Texture}_Active", AssetRequestMode.ImmediateLoad).Value;

                lightningTrail = new Trail(Main.graphics.GraphicsDevice, LightningMaxPoints, new TriangularTip(ArcWidth), factor => factor == 0 ? 0 : ArcWidth, texCoords => Color.White);

                lighterTrail = new Trail(Main.graphics.GraphicsDevice, LightningMaxPoints, new TriangularTip(ArcWidth), factor => factor == 0 ? 0 : ArcWidth, texCoords => Color.Pink);
            }

            curve = new BezierCurve(new Vector2[3]);

            trails = new List<TrailInfo>();
            lighterTrails = new List<TrailInfo>();
        }

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 80;

            Projectile.sentry = true;

            Projectile.tileCollide = false;

            Projectile.timeLeft = 60 * 60 * 3;

            Projectile.DamageType = DamageClass.Summon;
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

            if (Main.GameUpdateCount % StrikeFrequency == 0)
            {
                trails.Clear();
                lighterTrails.Clear();

                int adjacentCoils = 1;

                if (Target != -1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 start = Projectile.position + LightningOffset;
                        Vector2 end = Main.npc[(int)Target].Center;

                        // On the second iteration it creates the same trails but in reverse - 2 trails overlapping provides a more chaotic look.
                        if (i == 1)
                        {
                            end = Projectile.position + LightningOffset;
                            start = Main.npc[(int)Target].Center;
                        }

                        Vector2[] targetTrail = ManageTrail(0, start, end, DistanceToTarget);
                        Vector2[] lighterTargetTrail = ManageTrail(20, start, end, DistanceToTarget);

                        trails.Add(new(targetTrail, end));
                        lighterTrails.Add(new(lighterTargetTrail, end));
                    }
                }

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile projectile = Main.projectile[i];

                    if (projectile.active && projectile.type == Projectile.type && projectile.whoAmI != Projectile.whoAmI)
                    {
                        float distance = Vector2.Distance(Projectile.Center, projectile.Center);

                        if (distance < MaxRange)
                        {
                            // Trails to other tesla coils.
                            Vector2[] targetTrail = ManageTrail(0, Projectile.position + LightningOffset, projectile.position + LightningOffset, distance);
                            Vector2[] lighterTargetTrail = ManageTrail(20, Projectile.position + LightningOffset, projectile.position + LightningOffset, distance);

                            trails.Add(new(targetTrail, projectile.position + LightningOffset));
                            lighterTrails.Add(new(lighterTargetTrail, projectile.position + LightningOffset));

                            adjacentCoils++;
                        }
                    }
                }

                // Apply damage to enemies.
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (Main.GameUpdateCount % StrikeFrequency == 0)
                    {
                        for (int i = 0; i < trails.Count; i++)
                        {
                            TrailInfo trailInfo = trails[i];

                            for (int j = 0; j < trailInfo.Points.Length; j++)
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), trailInfo.Points[j], Vector2.Zero,
                                    ModContent.ProjectileType<TeslaCoilDamageProjectile>(), Projectile.damage * adjacentCoils, 0, Projectile.owner);
                            }
                        }
                    }
                }
            }

            ManageSound();

            if (trails.Count > 0)
            {
                timeArcsHaveExisted++;
            }
            else
            {
                timeArcsHaveExisted = 0;
            }

            oldTrailCount = trails.Count;
        }

        private Vector2[] ManageTrail(int offset, Vector2 start, Vector2 end, float distance)
        {
            int pointCount = (int)MathHelper.Min((int)(distance / MaxRange * LightningMaxPoints) + LightningMinPoints, LightningMaxPoints);

            Vector2[] vectors = GetBezierCurve(start, end, pointCount);

            for (int i = 0; i < pointCount; i++)
            {
                Vector2 normal;

                if (i != pointCount - 1)
                {
                    normal = (vectors[i + 1] - vectors[i]).SafeNormalize(Vector2.Zero);
                }
                else
                {
                    normal = (end - vectors[i]).SafeNormalize(Vector2.Zero);
                }

                Vector2 rotatedNormal = normal.RotatedBy(MathHelper.PiOver2);

                float randomOffset = Main.rand.NextFloat(-PointOffset, PointOffset);

                if (i != 0 && i != pointCount - 1)
                {
                    // Offset is an arbitrary parameter that just provides an offset for the secondary trail when applied, by offsetting the sine's x-component.
                    vectors[i] = vectors[i] + (rotatedNormal * (float)(randomOffset + (Math.Sin(-Main.GameUpdateCount + i + offset) * PointOffset * 1.5)));
                }
            }

            for (int i = pointCount; i < LightningMaxPoints; i++) 
            {
                vectors[i] = end;
            }

            return vectors;
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
            if (trails.Count > 0 && !Main.dedServ)
            {
                TidesOfTimeUtils.DrawAnimatedTexture(activeTexture, FrameCount, TicksPerFrame, Projectile.position - Main.screenPosition, lightColor, Vector2.Zero, 1);

                ModContent.GetInstance<PrimitiveSystem>().QueueRenderAction("Electricity", () =>
                {
                    Effect effect = Filters.Scene["TeslaCoilLightning"].GetShader().Shader;

                    Matrix world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
                    Matrix view = Matrix.Identity;
                    Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                    effect.Parameters["transformMatrix"].SetValue(world * view * projection);
                    effect.Parameters["opacity"].SetValue(1.0f);

                    for (int i = 0; i < trails.Count; i++)
                    {
                        TrailInfo trailInfo = trails[i];

                        lightningTrail.Positions = trailInfo.Points;
                        lightningTrail.NextPosition = trailInfo.NextPosition;

                        lightningTrail.Render(effect);
                    }

                    effect.Parameters["opacity"].SetValue(0.7f);

                    for (int i = 0; i < lighterTrails.Count; i++)
                    {
                        TrailInfo trailInfo = lighterTrails[i];

                        lighterTrail.Positions = trailInfo.Points;
                        lighterTrail.NextPosition = trailInfo.NextPosition;

                        lighterTrail.Render(effect);
                    }
                });

                ManageLight();

                return false;
            }

            return base.PreDraw(ref lightColor);
        }

        private void ManageLight()
        {
            Lighting.AddLight(Projectile.Center, Color.Yellow.ToVector3());

            for (int i = 0; i < trails.Count; i++)
            {
                TrailInfo trailInfo = trails[i];

                for (int j = 0; j < trailInfo.Points.Length; j++)
                {
                    Lighting.AddLight(trailInfo.Points[j], Color.Lerp(Color.Pink, Color.Purple, Main.rand.NextFloat()).ToVector3());
                }
            }
        }

        private void ManageSound()
        {
            if (!Main.dedServ)
            {
                // If there are any arcs active, play arc sound.
                if (trails.Count > 0)
                {
                    // Tesla coil just started arcing.
                    if (oldTrailCount == 0)
                    {
                        SoundEngine.PlaySound(Strike, Projectile.position + LightningOffset);
                    }
                    
                    // Plays the arcing sound if the arc has been there a few ticks, as if the arc is there for a very short period of time it's not preferable to play the main sound.
                    if (timeArcsHaveExisted > 10)
                    {
                        if (!SoundEngine.TryGetActiveSound(soundSlot, out var _))
                        {
                            ProjectileAudioTracker tracker = new(Projectile);

                            soundSlot = SoundEngine.PlaySound(Arcing, Projectile.position + LightningOffset, soundInstance => ActiveCallback(tracker, soundInstance));
                        }
                    }
                }
                // If none, only play ambience.
                else
                {
                    if (oldTrailCount != 0)
                    {
                        SoundEngine.PlaySound(Extinguish, Projectile.position + LightningOffset);
                    }

                    if (!SoundEngine.TryGetActiveSound(soundSlot, out var _))
                    {
                        ProjectileAudioTracker tracker = new(Projectile);

                        soundSlot = SoundEngine.PlaySound(Ambience, Projectile.position + LightningOffset, soundInstance => AmbienceCallback(tracker, soundInstance));
                    }
                }
            }
        }

        private bool AmbienceCallback(ProjectileAudioTracker tracker, ActiveSound soundInstance)
        {
            soundInstance.Position = Projectile.position + LightningOffset;

            return tracker.IsActiveAndInGame() && trails.Count == 0;
        }

        private bool ActiveCallback(ProjectileAudioTracker tracker, ActiveSound soundInstance)
        {
            soundInstance.Position = Projectile.position + LightningOffset;

            return tracker.IsActiveAndInGame() && trails.Count > 0;
        }

        private struct TrailInfo
        {
            public Vector2[] Points;

            public Vector2 NextPosition;

            public TrailInfo(Vector2[] points, Vector2 nextPosition)
            {
                Points = points;
                NextPosition = nextPosition;
            }
        }
    }
}
