using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace TidesOfTime.Common.Rendering.Tiles
{
    public class TileColouringSystem : ModSystem
    {
        private readonly Dictionary<int, TileHit> tileHits;

        private readonly List<int> clearList;

        public TileColouringSystem()
        {
            tileHits = new();
            clearList = new();
        }

        public override void Load()
        {
            IL_TileDrawing.DrawSingleTile += IL_TileDrawing_DrawSingleTile;
        }

        public override void PostUpdateEverything()
        {
            foreach (int key in tileHits.Keys)
            {
                TileHit hit = tileHits[key];

                hit.Timer--;

                if (hit.Timer <= 0)
                {
                    clearList.Add(key);
                }

                tileHits[key] = hit;
            }

            foreach (int key in clearList)
            {
                tileHits.Remove(key);
            }

            clearList.Clear();
        }

        private void IL_TileDrawing_DrawSingleTile(ILContext il)
        {
            ILCursor cursor = new(il);

            // Navigate to the first time GetTileDrawData is called, which is near the start of the method. This is needed because it's after tileLight is initially assigned.
            // Moves after because it'd be in the middle of the method's arguments loading otherwise.
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCall(typeof(TileDrawing).GetMethod("GetTileDrawData", BindingFlags.Instance | BindingFlags.Public))))
            {
                throw new Exception("Could not locate GetTileDrawData method in TileDrawing.DrawSingleTile");
            }

            // Load the second argument onto the stack (index 1). Since it's an instance method, the second argument is the first listed one (a TileDrawInfo instance here).
            cursor.Emit(OpCodes.Ldarg, 1);

            // These arguments are the x and y positions of the tile.
            cursor.Emit(OpCodes.Ldarg, 6);
            cursor.Emit(OpCodes.Ldarg, 7);

            // Use the TileDrawInfo instance to change the tileLight parameter to the desired color.
            cursor.EmitDelegate<Action<TileDrawInfo, int, int>>((drawInfo, x, y) =>
            {
                int hash = TidesOfTimeUtils.UniquePair(x, y);

                if (tileHits.ContainsKey(hash))
                {
                    float factor = (MathF.Sin(Main.GameUpdateCount / 7.5f) + 1) / 2;

                    Color lerpColor = Color.Lerp(tileHits[hash].Color, Color.White, factor);

                    drawInfo.tileLight = lerpColor;
                }
            });
        }

        public void AddHit(int x, int y, Color color, int duration)
        {
            int hash = TidesOfTimeUtils.UniquePair(x, y);

            tileHits[hash] = new TileHit(color, duration);
        }
    }
}
