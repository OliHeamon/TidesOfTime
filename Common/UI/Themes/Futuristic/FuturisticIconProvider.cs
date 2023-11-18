using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.ModLoader;
using TidesOfTime.Common.UI.Themes.Providers;

namespace TidesOfTime.Common.UI.Themes.Futuristic
{
    public class FuturisticIconProvider : ThemeIconProvider
    {
        public override string NameKey => "FuturisticIcons";

        public override void PopulateIcons(Dictionary<string, Texture2D> icons)
        {
            foreach (string key in defaultKeys)
            {
                icons.Add(key, ModContent.Request<Texture2D>($"TidesOfTime/Assets/UI/Themes/Futuristic/Icons/{key}", AssetRequestMode.ImmediateLoad).Value);
            }
        }
    }
}
