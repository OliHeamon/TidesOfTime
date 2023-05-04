using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    }
}
