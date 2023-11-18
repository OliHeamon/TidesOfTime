using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;
using TidesOfTime.Common.UI.Abstract;
using TidesOfTime.Common.UI.Themes;
using TidesOfTime.Localization;

namespace TidesOfTime.Common.UI.UniversalRemote.TaskList
{
    public class IconButton : SmartUIElement
    {
        private readonly string icon;

        private readonly string tooltip;

        private readonly Color color;

        public event Action ClickWhenEnabled;

        public Func<bool> ClickableCondition { get; set; }

        public IconButton(string icon, string tooltip, Color color)
        {
            this.icon = icon;
            this.tooltip = tooltip;
            this.color = color;

            ClickableCondition = () => true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            float factor = IsMouseHovering ? 1 : 0.75f;

            Color lerpColor = Color.Lerp(ThemeSystem.BackgroundColor, color, factor);
            Color iconColor = Color.White;

            if (!ClickableCondition.Invoke())
            {
                lerpColor = Color.Lerp(lerpColor, Color.Black, 0.75f);

                iconColor = Color.Lerp(Color.White, Color.Black, 0.75f);
            }

            UIHelper.DrawBox(spriteBa​tch, drawBox, Color.Lerp(ThemeSystem.BackgroundColor, lerpColor, factor));

            Texture2D iconTexture = ThemeSystem.GetIcon(icon);

            spriteBatch.Draw(iconTexture, new Vector2(drawBox.X + 2, drawBox.Y + 2), iconColor);

            if (IsMouseHovering)
            {
                Main.instance.MouseText(LocalizationHelper.GetGUIText(tooltip));
            }

            base.Draw(spriteBatch);
        }

        public override void SafeMouseOver(UIMouseEvent evt)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public override void SafeClick(UIMouseEvent evt)
        {
            if (ClickableCondition.Invoke())
            {
                SoundEngine.PlaySound(new("TidesOfTime/Assets/Sounds/UI/ButtonPress"));

                ClickWhenEnabled?.Invoke();
            }
        }
    }
}
