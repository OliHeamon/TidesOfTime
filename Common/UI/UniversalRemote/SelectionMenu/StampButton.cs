using Microsoft.Xna.Framework.Graphics;
using TidesOfTime.Common.UI.Abstract;
using TidesOfTime.Common.UI.Themes;
using Microsoft.Xna.Framework;

namespace TidesOfTime.Common.UI.UniversalRemote.SelectionMenu
{
    public class StampButton : SmartUIElement
    {
        public bool Toggled { get; set; }

        private readonly Color color;

        public StampButton(Color color)
        {
            this.color = color;
        }

        public override void OnInitialize()
        {
            Width.Set(16, 0);
            Height.Set(16, 0);

            base.OnInitialize();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color backgroundColor = Color.Lerp(ThemeSystem.BackgroundColor, color, 0.5f);
            Color iconColor = Color.White;

            if (IsMouseHovering)
            {
                backgroundColor.A = iconColor.A = 64;
            }

            Draw(spriteBatch, backgroundColor);

            base.Draw(spriteBatch);
        }

        private void Draw(SpriteBatch spriteBatch, Color backgroundColor)
        {
            if (Toggled)
            {
                backgroundColor = Color.Lerp(ThemeSystem.BackgroundColor, Color.White, 0.75f);
            }

            UIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), backgroundColor);
        }
    }
}
