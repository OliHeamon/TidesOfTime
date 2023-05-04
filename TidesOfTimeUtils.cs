using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace TidesOfTime
{
    public static class TidesOfTimeUtils
    {
        public static void DrawAnimatedTexture(Texture2D texture, int frameCount, int ticksPerFrame, Vector2 position, Color drawColor, Vector2 origin, float scale)
        {
            int frameWidth = texture.Width / frameCount;

            int currentFrame = (int)(Main.GameUpdateCount / ticksPerFrame) % frameCount;

            Rectangle sourceRectangle = new(currentFrame * frameWidth, 0, frameWidth, texture.Height);

            Main.spriteBatch.Draw(texture, position, sourceRectangle, drawColor, 0, origin, scale, SpriteEffects.None, 0);
        }

        public static T[] FastUnion<T>(this T[] front, T[] back)
        {
            var combined = new T[front.Length + back.Length];

            Array.Copy(front, combined, front.Length);
            Array.Copy(back, 0, combined, front.Length, back.Length);

            return combined;
        }

        public static Vector3 ToVector3(this Vector2 vector2) => new Vector3(vector2.X, vector2.Y, 0);
    }
}
