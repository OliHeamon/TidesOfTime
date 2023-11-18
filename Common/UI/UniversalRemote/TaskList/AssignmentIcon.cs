using Microsoft.Xna.Framework.Graphics;
using Terraria;
using TidesOfTime.Common.UI.Abstract;
using TidesOfTime.Common.UI.Themes;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using TidesOfTime.Localization;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace TidesOfTime.Common.UI.UniversalRemote.TaskList
{
    public class AssignmentIcon : SmartUIElement
    {
        private readonly DroneTask task;

        public AssignmentIcon(DroneTask task)
        {
            this.task = task;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            Texture2D icon = ThemeSystem.GetIcon("Clipboard");

            Color color = Color.White;

            if (IsMouseHovering)
            {
                Main.instance.MouseText(task.TaskDescription);

                color.A = 64;
            }

            spriteBatch.Draw(icon, new Vector2(drawBox.X, drawBox.Y), color);

            string assignmentText = LocalizationHelper.GetGUIText("UniversalRemote.Assignment");

            Vector2 stringSize = FontAssets.MouseText.Value.MeasureString(assignmentText);

            Utils.DrawBorderString(spriteBatch, assignmentText,
               new Vector2(drawBox.X, drawBox.Y) - new Vector2(stringSize.X + 8, -(stringSize.Y / 2) - 8), Color.White);

            base.Draw(spriteBatch);
        }

        public override void SafeMouseOver(UIMouseEvent evt)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }
}
