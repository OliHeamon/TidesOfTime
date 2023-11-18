using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote
{
    public class ScannerDrone : WorkerDrone
    {
        private static readonly SoundStyle Ping = new("TidesOfTime/Assets/Sounds/Projectiles/Misc/SonarPing")
        {
            PitchVariance = 0.1f,
            MaxInstances = 0
        };

        public const int ScanTimeSeconds = 5;

        private bool Spotlight
        {
            get => Projectile.ai[1] == 1;
            set => Projectile.ai[1] = value ? 1 : 0;
        }

        public override float Laziness => 0.8f;

        public override void AI()
        {
            if (Tasks.Count > 0 && !Tasks[0].HasStarted)
            {
                SoundEngine.PlaySound(Ping, Projectile.Center);
            }

            base.AI();

            if (Spotlight && Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 32; i++)
                {
                    Vector2 vectorToMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);

                    Vector2 position = Projectile.Center + (vectorToMouse * i * 16);

                    if (Collision.SolidTiles(position, 1, 1))
                    {
                        return;
                    }

                    Lighting.AddLight(position, Color.Lerp(Color.White, Color.Blue, 0.6f).ToVector3());
                }
            }
        }

        public override void TaskCompletionCelebration(ref Vector2 scale, ref Vector2 offset, ref SpriteEffects spriteEffects, int celebrationTimer)
        {
            
        }

        public override void OnComplete()
        {
            
        }

        public void ToggleSpotlight()
        {
            Spotlight = !Spotlight;
        }
    }
}
