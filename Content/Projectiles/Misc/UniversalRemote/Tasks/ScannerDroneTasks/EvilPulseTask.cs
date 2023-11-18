using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TidesOfTime.Localization;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks.ScannerDroneTasks
{
    public class EvilPulseTask : TileScanTask
    {
        public override string TaskDescription => LocalizationHelper.GetGUIText("UniversalRemote.ScannerEvilPulseTaskInfo");

        public override Color Color => Color.Red;

        public override bool TileMeetsCondition(int x, int y)
        {
            int id = Main.tile[x, y].TileType;

            return TileID.Sets.Corrupt[id] || TileID.Sets.Crimson[id];
        }
    }
}
