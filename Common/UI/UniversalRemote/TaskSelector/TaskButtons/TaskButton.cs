using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;
using TidesOfTime.Common.UI.Abstract;
using TidesOfTime.Common.UI.Themes;
using TidesOfTime.Localization;

namespace TidesOfTime.Common.UI.UniversalRemote.TaskSelector.TaskButtons
{
    public class TaskButton : SmartUIElement
    {
        private readonly string icon;

        private readonly string info;

        private readonly Color color;

        public TaskButton(string icon, Color color)
        {
            this.icon = icon;
            info = $"UniversalRemote.{icon}";
            this.color = color;
        }

        public override void OnInitialize()
        {
            Width.Set(32, 0);
            Height.Set(32, 0);

            base.OnInitialize();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color backgroundColor = Color.Lerp(ThemeSystem.BackgroundColor, color, 0.25f);
            Color iconColor = Color.White;

            if (IsMouseHovering)
            {
                Main.instance.MouseText(LocalizationHelper.GetGUIText(info));

                backgroundColor.A = iconColor.A = 64;
            }

            Draw(spriteBatch, iconColor, backgroundColor);

            base.Draw(spriteBatch);
        }

        protected virtual void Draw(SpriteBatch spriteBatch, Color iconColor, Color backgroundColor)
        {
            UIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), backgroundColor);

            Texture2D iconTexture = ThemeSystem.GetIcon(icon);

            spriteBatch.Draw(iconTexture, GetDimensions().Position() + new Vector2(4), iconColor);
        }

        public override void SafeMouseOver(UIMouseEvent evt)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public override void SafeClick(UIMouseEvent evt)
        {
            SoundEngine.PlaySound(new("TidesOfTime/Assets/Sounds/UI/ButtonPress"));
        }
    }
}
