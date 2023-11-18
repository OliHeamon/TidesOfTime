using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using TidesOfTime.Common.UI.Abstract;
using TidesOfTime.Common.UI.Themes;
using TidesOfTime.Localization;

namespace TidesOfTime.Common.UI.UniversalRemote.SelectionMenu
{
    public class StampToggleButton : SmartUIElement
    {
        public bool Toggled { get; set; }

        private readonly string icon;

        private readonly Color color;

        public StampToggleButton(string icon, Color color) 
        {
            this.icon = icon;
            this.color = color;
        }

        public override void SafeClick(UIMouseEvent evt)
        {
            Toggled = !Toggled;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            float factor = IsMouseHovering ? 1 : 0.75f;

            Color lerpColor = Color.Lerp(ThemeSystem.BackgroundColor, color, factor);
            Color iconColor = Color.White;

            if (Toggled)
            {
                lerpColor = Color.Lerp(ThemeSystem.BackgroundColor, Color.White, 0.9f);
            }

            UIHelper.DrawBox(spriteBa​tch, drawBox, Color.Lerp(ThemeSystem.BackgroundColor, lerpColor, factor));

            Texture2D iconTexture = ThemeSystem.GetIcon(icon);

            spriteBatch.Draw(iconTexture, new Vector2(drawBox.X + 2, drawBox.Y + 2), iconColor);

            if (IsMouseHovering)
            {
                Main.instance.MouseText(LocalizationHelper.GetGUIText($"UniversalRemote.{icon}"));
            }
        }
    }
}
