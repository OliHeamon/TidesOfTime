using Microsoft.Xna.Framework;

namespace TidesOfTime.Common.Rendering.Tiles
{
    public struct TileHit
    {
        public Color Color { get; private set; }

        public int Timer { get; set; }

        public TileHit(Color color, int timer)
        {
            Color = color;
            Timer = timer;
        }
    }
}
