using Microsoft.Xna.Framework;
using Terraria;
using TidesOfTime.Localization;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks.ScannerDroneTasks
{
    public class SpelunkerPulseTask : TileScanTask
    {
        public override string TaskDescription => LocalizationHelper.GetGUIText("UniversalRemote.ScannerSpelunkerPulseTaskInfo");

        public override Color Color => Color.Orange;

        public override bool TileMeetsCondition(int x, int y) => Main.IsTileSpelunkable(x, y);
    }
}
