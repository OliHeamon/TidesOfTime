using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TidesOfTime.Common.UI.Abstract;
using TidesOfTime.Common.UI.Themes;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks;
using TidesOfTime.Localization;

namespace TidesOfTime.Common.UI.UniversalRemote.TaskList
{
    public class TaskElement : SmartUIElement
    {
        public DroneTask Task { get; private set; }

        public TaskElement(DroneTask task)
        {
            Task = task;
        }

        public override void OnInitialize()
        {
            EmergencyStopButton emergencyStopButton = new(Task);
            emergencyStopButton.Width.Set(32, 0);
            emergencyStopButton.Height.Set(32, 0);
            emergencyStopButton.Left.Set(GetDimensions().ToRectangle().Width - 40, 0);
            emergencyStopButton.Top.Set(GetDimensions().ToRectangle().Height - 40, 0);

            AssignmentIcon assignmentIcon = new(Task);
            assignmentIcon.Width.Set(64, 0);
            assignmentIcon.Height.Set(64, 0);
            assignmentIcon.Left.Set(190, 0);
            assignmentIcon.Top.Set(4, 0);

            Append(emergencyStopButton);
            Append(assignmentIcon);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            UIHelper.DrawBox(spriteBatch, drawBox, ThemeSystem.BackgroundColor);

            DrawProgressBar(spriteBatch, drawBox);
            DrawDroneIcon(spriteBatch, drawBox);

            base.Draw(spriteBatch);
        }

        private void DrawProgressBar(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            Rectangle progressBar = new(drawBox.X + 8, drawBox.Y + drawBox.Height - 40, drawBox.Width - 52, 32);

            UIHelper.DrawBox(spriteBatch, progressBar, ThemeSystem.BackgroundColor);

            Texture2D icon = ThemeSystem.GetIcon("ProgressBar");

            spriteBatch.Draw(icon, new Vector2(progressBar.X + 4, progressBar.Y + 4), new Rectangle(0, 0, (int)(icon.Width * Task.Progress), icon.Height), Color.White);

            string barMessage;

            if (!Task.HasStarted)
            {
                barMessage = LocalizationHelper.GetGUIText("UniversalRemote.TaskQueued");
            }
            else
            {
                barMessage = System.Math.Round(Task.Progress * 100, 1).ToString() + "%";
            }

            float width = FontAssets.MouseText.Value.MeasureString(barMessage).X;

            Vector2 middle = new(progressBar.X + progressBar.Width / 2 - width / 2, progressBar.Y + 6);

            Utils.DrawBorderString(spriteBatch, barMessage, middle, Color.White);
        }

        private void DrawDroneIcon(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            Rectangle droneBox = new(drawBox.X + 8, drawBox.Y + 8, 60, 60);

            Texture2D droneTexture;
            Color lerpColor;
            Vector2 droneOffset;

            switch (Task.DroneType)
            {
                case DroneType.Scanner:
                    lerpColor = Color.Blue;
                    droneTexture = ModContent.Request<Texture2D>("TidesOfTime/Assets/Projectiles/Misc/UniversalRemote/ScannerDrone", AssetRequestMode.ImmediateLoad).Value;
                    droneOffset = new(12, 8);
                    break;
                case DroneType.Builder:
                    lerpColor = Color.Green;
                    droneTexture = ModContent.Request<Texture2D>("TidesOfTime/Assets/Projectiles/Misc/UniversalRemote/BuilderDrone", AssetRequestMode.ImmediateLoad).Value;
                    droneOffset = new(4, 6);
                    break;
                default:
                    lerpColor = Color.Red;
                    droneTexture = ModContent.Request<Texture2D>("TidesOfTime/Assets/Projectiles/Misc/UniversalRemote/BreakerDrone", AssetRequestMode.ImmediateLoad).Value;
                    droneOffset = new(6, 4);
                    break;
            }

            UIHelper.DrawBox(spriteBatch, droneBox, Color.Lerp(ThemeSystem.BackgroundColor, lerpColor, 0.25f));

            spriteBatch.Draw(droneTexture, new Vector2(droneBox.X, droneBox.Y) + droneOffset, new Rectangle(0, 0, droneTexture.Width / 4, droneTexture.Height), Color.White);
        }

        public override int CompareTo(object obj)
        {
            if (obj is TaskElement element)
            {
                return element.Task.Progress.CompareTo(Task.Progress);
            }

            return base.CompareTo(obj);
        }
    }
}
