using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TidesOfTime.Common.UI.UniversalRemote;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote
{
    public abstract class WorkerDrone : ModProjectile
    {
        private const int HoverDistance = 64;

        private const int FrameCount = 4;

        private const int TicksPerFrame = 5;

        private const int CelebrationDuration = 64;

        public List<DroneTask> Tasks { get; private set; }

        public abstract float Laziness { get; }

        private int Index
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public event Action<DroneTask> TaskAssigned;

        public event Action<DroneTask> TaskCompleted;

        public override string GlowTexture => $"{Texture}_Glowmask";

        private int celebrationTimer;

        public WorkerDrone()
        {
            Tasks = new List<DroneTask>();
        }

        public override void OnSpawn(IEntitySource source)
        {
            TaskAssigned += TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().TaskList.AddTask;
            TaskCompleted += TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().TaskList.RemoveTask;
        }

        public override void OnKill(int timeLeft)
        {
            foreach (DroneTask task in Tasks)
            {
                TaskCompleted?.Invoke(task);
            }

            TaskAssigned -= TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().TaskList.AddTask;
            TaskCompleted -= TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().TaskList.RemoveTask;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;

            Projectile.timeLeft = int.MaxValue;

            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // If no tasks then fly to idle position.
            if (Tasks.Count == 0)
            {
                IdleMovement();
            }
            else
            {
                // Execute first task given.
                DroneTask task = Tasks[0];

                task.WorkerDrone = Projectile.ModProjectile as WorkerDrone;

                task.HasStarted = true;

                if (task.AI())
                {
                    IdleMovement();
                }

                if (task.IsComplete)
                {
                    celebrationTimer = CelebrationDuration;

                    TaskCompleted?.Invoke(task);

                    task.OnCompleted();

                    OnComplete();

                    if (task is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    Tasks.RemoveAt(0);
                }
            }

            if (celebrationTimer > 0)
            {
                celebrationTimer--;
            }

            Projectile.netUpdate = true;
        }

        private void IdleMovement()
        {
            Vector2 ownerPosition = Main.player[Projectile.owner].Center;

            Vector2 targetPosition = ownerPosition + (-Vector2.UnitY * HoverDistance).RotatedBy(Index * (MathHelper.TwoPi / 3));

            Move(targetPosition, Laziness);
        }

        public void Move(Vector2 targetPosition, float speedMultiplier)
        {
            Vector2 delta = targetPosition - Projectile.Center;

            float desiredSpeed = (float)Math.Min(20, delta.Length() / 16);

            Projectile.velocity = delta.SafeNormalize(Vector2.Zero) * desiredSpeed * speedMultiplier;
        }

        public void AddTask(DroneTask task)
        {
            Tasks.Add(task);

            TaskAssigned?.Invoke(task);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 offset;

            if (Tasks.Count == 0)
            {
                offset = new(0, (float)Math.Sin((Main.GameUpdateCount / 10f) + Index) * 4);
            }
            else
            {
                offset = Vector2.Zero;
            }

            Texture2D texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>(GlowTexture, AssetRequestMode.ImmediateLoad).Value;

            Vector2 scale = Vector2.One;

            if (celebrationTimer > 0)
            {
                TaskCompletionCelebration(ref scale, ref offset, ref spriteEffects, celebrationTimer);
            }

            TidesOfTimeUtils.DrawAnimatedTexture(texture, FrameCount, TicksPerFrame, Projectile.position - Main.screenPosition + offset, lightColor, Vector2.Zero, scale, spriteEffects);
            TidesOfTimeUtils.DrawAnimatedTexture(glowTexture, FrameCount, TicksPerFrame, Projectile.position - Main.screenPosition + offset, Color.White, Vector2.Zero, scale, spriteEffects);

            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            if (Tasks.Count > 0)
            {
                DroneTask task = Tasks[0];

                task.WorkerDrone ??= Projectile.ModProjectile as WorkerDrone;

                task.Draw(Main.spriteBatch, lightColor);
            }
        }

        public virtual void OnComplete() { }

        public abstract void TaskCompletionCelebration(ref Vector2 scale, ref Vector2 offset, ref SpriteEffects spriteEffects, int celebrationTimer);
    }
}
