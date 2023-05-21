using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace TidesOfTime.Content.Rarities
{
    public class Steampunk : ModRarity
    {
        private static readonly Color Pink = new(255, 61, 108);
        private static readonly Color Blue = new(21, 189, 220);

        private static readonly float LerpSpeed = 0.05f;

        public override Color RarityColor
        {
            get
            {
                float t = (float)(Math.Sin(Main.GameUpdateCount * LerpSpeed) + 1) / 2;

                return Color.Lerp(Pink, Blue, t);
            }
        }
    }
}
