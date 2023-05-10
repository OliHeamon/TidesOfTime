using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ModLoader;

namespace TidesOfTime.Common.Rendering
{
    public struct Palette
    {
        private const int ColorLimit = 16;

        public bool NoCorrection { get; private set; }

        public Vector3[] Colors { get; private set; }

        public int ColorCount { get; private set; }

        public Palette()
        {
            NoCorrection = true;
        }

        public Palette(params Vector3[] colors)
        {
            if (colors.Length > ColorLimit)
            {
                throw new ArgumentException($"Palette cannot have more than {ColorLimit} colours!");
            }

            NoCorrection = false;
            ColorCount = colors.Length;

            // Pad out the rest of the colour array with black if it is not full.
            if (colors.Length < ColorLimit)
            {
                Vector3[] colors16 = new Vector3[ColorLimit];

                for (int i = 0; i < ColorLimit; i++)
                {
                    if (i < colors.Length)
                    {
                        colors16[i] = colors[i];
                    }
                    else
                    {
                        colors16[i] = Vector3.Zero;
                    }
                }

                colors = colors16;
            }

            Colors = colors;
        }

        public static Palette From(string path)
        {
            Texture2D texture = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;

            Color[] data = new Color[texture.Width * texture.Height];

            texture.GetData(data);

            Vector3[] colours = new Vector3[data.Length];

            for (int i = 0; i < colours.Length; i++)
            {
                colours[i] = data[i].ToVector3();
            }

            return new Palette(colours);
        }
    }
}
