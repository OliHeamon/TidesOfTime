using ReLogic.Content.Sources;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using TidesOfTime.Common.Rendering;

namespace TidesOfTime
{
	public class TidesOfTime : Mod
	{
        public static TidesOfTime Instance { get; private set; }

        public PrimitiveRenderer PrimitiveRenderer { get; private set; }

        public TidesOfTime()
        {
            Instance = this;
        }

        public override void Load()
        {
            PrimitiveRenderer = new();
            PrimitiveRenderer.Load();

            Main.OnResolutionChanged += resolution =>
            {
                PrimitiveRenderer.TargetsNeedResizing();
            };
        }

        public override void Unload()
        {
            PrimitiveRenderer.Unload();
        }

        public override IContentSource CreateDefaultContentSource()
        {
            TmodFile file = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this, null) as TmodFile;

            return new TidesOfTimeContentSource(file);
        }
    }
}