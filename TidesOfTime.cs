using Microsoft.Xna.Framework;
using ReLogic.Content.Sources;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using TidesOfTime.Common.Rendering;
using TidesOfTime.Common.Rendering.ProceduralGore;

namespace TidesOfTime
{
	public class TidesOfTime : Mod
	{
        public static TidesOfTime Instance { get; private set; }

        public static readonly Color FuturisticBackgroundColor = new Color(34, 31, 38) * 0.875f;

        public static readonly Color FuturisticButtonColor = new Color(34, 31, 38) * 0.875f;

        public TidesOfTime()
        {
            Instance = this;
        }

        public override void PostSetupContent()
        {
            ModContent.GetInstance<PrimitiveSystem>().RegisterRenderTarget("Standard");
            ModContent.GetInstance<PrimitiveSystem>().RegisterRenderTargetWithPalette("Electricity", "TidesOfTime/Assets/Palettes/Electricity");
        }

        public override IContentSource CreateDefaultContentSource()
        {
            TmodFile file = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this, null) as TmodFile;

            return new TidesOfTimeContentLoader(file);
        }
    }

    public class ProceduralGoreTestItem : ModItem
    {
        public override string Texture => "TidesOfTime/Assets/Invisible";

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 15;
            Item.useTime = 15;
        }

        public override bool? UseItem(Player player)
        {
            NPC npc = new();
            npc.SetDefaults(/*Main.rand.Next(NPCLoader.NPCCount)*/491);

            ModContent.GetInstance<ProceduralGoreSystem>().AddGore(npc, Main.MouseWorld, DivisionType.RandomQuads);

            return true;
        }
    }
}