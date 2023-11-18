using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;
using TidesOfTime.Common.UI.Abstract;
using TidesOfTime.Common.UI.Themes;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks;
using TidesOfTime.Localization;

namespace TidesOfTime.Common.UI.UniversalRemote.TaskList
{
    public class EmergencyStopButton : SmartUIElement
    {
        private bool mouseDown;

        private readonly DroneTask task;

        public EmergencyStopButton(DroneTask task)
        {
            this.task = task;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            Texture2D icon = ThemeSystem.GetIcon("RedButton");

            Rectangle frame = new(mouseDown ? icon.Width / 2 : 0, 0, icon.Width / 2, icon.Height);

            spriteBatch.Draw(icon, new Vector2(drawBox.X, drawBox.Y), frame, Color.White);

            if (IsMouseHovering)
            {
                Main.instance.MouseText(LocalizationHelper.GetGUIText("UniversalRemote.EmergencyStop"));
            }

            base.Draw(spriteBatch);
        }

        public override void SafeClick(UIMouseEvent evt)
        {
            task.Abort();

            TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().TaskList.RemoveTask(task);
        }

        public override void SafeMouseOver(UIMouseEvent evt)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public override void SafeMouseDown(UIMouseEvent evt)
        {
            if (IsMouseHovering)
            {
                SoundEngine.PlaySound(new("TidesOfTime/Assets/Sounds/UI/ButtonPress"));

                mouseDown = true;
            }
        }

        public override void SafeMouseUp(UIMouseEvent evt)
        {
            mouseDown = false;
        }
    }
}
