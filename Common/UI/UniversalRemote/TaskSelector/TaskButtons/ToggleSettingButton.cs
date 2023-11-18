using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using TidesOfTime.Common.UI.Themes;

namespace TidesOfTime.Common.UI.UniversalRemote.TaskSelector.TaskButtons
{
    public class ToggleSettingButton : TaskButton
    {
        public bool Toggled { get; set; }

        public ToggleSettingButton(string icon, Color color) : base(icon, color)
        {
        }

        public override void SafeClick(UIMouseEvent evt)
        {
            Toggled = !Toggled;

            base.SafeClick(evt);
        }

        protected override void Draw(SpriteBatch spriteBatch, Color iconColor, Color backgroundColor)
        {
            if (Toggled)
            {
                backgroundColor = Color.Lerp(ThemeSystem.BackgroundColor, Color.White, 0.75f);
            }

            base.Draw(spriteBatch, iconColor, backgroundColor);
        }
    }
}
