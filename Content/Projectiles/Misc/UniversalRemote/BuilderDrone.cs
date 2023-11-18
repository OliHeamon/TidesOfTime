using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote
{
    public class BuilderDrone : WorkerDrone
    {
        public override float Laziness => 1;

        public override void TaskCompletionCelebration(ref Vector2 scale, ref Vector2 offset, ref SpriteEffects spriteEffects, int celebrationTimer)
        {
            float sin = MathF.Sin(MathHelper.ToRadians(celebrationTimer * 12));

            scale.X = MathF.Abs(sin);

            if (sin < 0)
            {
                spriteEffects = Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }

            if (Projectile.direction == 1)
            {
                offset.X -= scale.X * (Projectile.width / 2);
            }
            else
            {
                offset.X += Projectile.direction * scale.X * (Projectile.width / 2);
            }
        }

        public override void OnComplete()
        {
            SoundEngine.PlaySound(SoundID.AchievementComplete);

            GreenFireworks(Projectile.Center - new Vector2(0, 128));
        }

        private void GreenFireworks(Vector2 position)
        {
            for (int num764 = 0; num764 < 400; num764++)
            {
                float num765 = 2f * (num764 / 100f);
                if (num764 > 100)
                    num765 = 10f;

                if (num764 > 250)
                    num765 = 13f;

                int num767 = Dust.NewDust(new Vector2(position.X, position.Y), 6, 6, DustID.Firework_Green, 0f, 0f, 100);
                float num768 = Main.dust[num767].velocity.X;
                float y3 = Main.dust[num767].velocity.Y;
                if (num768 == 0f && y3 == 0f)
                    num768 = 1f;

                float num769 = (float)Math.Sqrt(num768 * num768 + y3 * y3);
                num769 = num765 / num769;
                if (num764 <= 200)
                {
                    num768 *= num769;
                    y3 *= num769;
                }
                else
                {
                    num768 = num768 * num769 * 1.25f;
                    y3 = y3 * num769 * 0.75f;
                }

                Dust dust2 = Main.dust[num767];
                dust2.velocity *= 0.5f;
                Main.dust[num767].velocity.X += num768;
                Main.dust[num767].velocity.Y += y3;
                if (num764 > 100)
                {
                    Main.dust[num767].scale = 1.3f;
                    Main.dust[num767].noGravity = true;
                }
            }
        }
    }
}
