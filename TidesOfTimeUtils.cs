using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace TidesOfTime
{
    public static class TidesOfTimeUtils
    {
        public static void DrawAnimatedTexture(Texture2D texture, int frameCount, int ticksPerFrame, Vector2 position, Color drawColor, Vector2 origin, Vector2 scale, SpriteEffects effects)
        {
            int frameWidth = texture.Width / frameCount;

            int currentFrame = (int)(Main.GameUpdateCount / ticksPerFrame) % frameCount;

            Rectangle sourceRectangle = new(currentFrame * frameWidth, 0, frameWidth, texture.Height);

            Main.spriteBatch.Draw(texture, position, sourceRectangle, drawColor, 0, origin, scale, effects, 0);
        }

        public static T[] FastUnion<T>(this T[] front, T[] back)
        {
            var combined = new T[front.Length + back.Length];

            Array.Copy(front, combined, front.Length);
            Array.Copy(back, 0, combined, front.Length, back.Length);

            return combined;
        }

        public static Vector3 ToVector3(this Vector2 vector2) => new(vector2.X, vector2.Y, 0);

        public static bool IsInInscribedEllipse(Vector2 point, Rectangle tileRectangle)
        {
            Vector2 startPoint = tileRectangle.TopLeft();
            Vector2 endPoint = tileRectangle.BottomRight();

            int xLength = (int)MathF.Abs(endPoint.X - startPoint.X);
            int yLength = (int)MathF.Abs(endPoint.Y - startPoint.Y);

            int x = (int)(point.X - startPoint.X);
            int y = (int)(point.Y - startPoint.Y);

            int startX = startPoint.X < endPoint.X ? (int)startPoint.X : (int)endPoint.X;
            int startY = startPoint.Y < endPoint.Y ? (int)startPoint.Y : (int)endPoint.Y;

            if (!WorldGen.InWorld(startX + x, startY + y))
            {
                return false;
            }

            // The following calculates whether a given tile in the selection area is inside the area's inscribed ellipse.
            int a = xLength / 2;
            int b = yLength / 2;

            Vector2 ellipseCenter = new(startX + a, startY + b);

            Vector2 tilePoint = new(startX + x + 0.5f, startY + y + 0.5f);

            float formulaX = (float)((tilePoint.X - ellipseCenter.X) * (tilePoint.X - ellipseCenter.X)) / (a * a);
            float formulaY = (float)((tilePoint.Y - ellipseCenter.Y) * (tilePoint.Y - ellipseCenter.Y)) / (b * b);

            // Tile point is inside the ellipse.
            if (formulaX + formulaY <= 1)
            {
                return true;
            }

            return false;
        }

        public static int UniquePair(int a, int b) => (int)((0.5 * (a + b) * (a + b + 1)) + b);
    }
}
