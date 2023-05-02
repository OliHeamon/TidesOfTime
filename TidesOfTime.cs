using ReLogic.Content.Sources;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TidesOfTime
{
	public class TidesOfTime : Mod
	{
        public override IContentSource CreateDefaultContentSource()
        {
            TmodFile file = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this, null) as TmodFile;

            return new TidesOfTimeContentSource(file);
        }

        public override void Load()
        {
        }
    }
}