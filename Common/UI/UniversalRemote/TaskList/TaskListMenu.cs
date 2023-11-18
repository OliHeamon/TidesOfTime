using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TidesOfTime.Common.UI.Abstract;
using TidesOfTime.Common.UI.Themes;
using Terraria;
using TidesOfTime.Localization;
using Terraria.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.GameContent;
using Terraria.ModLoader;
using ReLogic.Content;
using Terraria.GameContent.UI;
using System.Collections.Generic;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;

namespace TidesOfTime.Common.UI.UniversalRemote.TaskList
{
    public class TaskListMenu : DraggableUIElement
    {
        private readonly UserInterface userInterface;

        private readonly Texture2D breakerDroneIcon;

        private readonly List<TaskElement> taskElements;

        private FixedUIScrollbar bar;

        private UIGrid taskGrid;

        public override Rectangle DragBox => GetDimensions().ToRectangle();

        public override Vector2 DefaultPosition => new(0.7f, 0.4f);

        public TaskListMenu(UserInterface userInterface)
        {
            this.userInterface = userInterface;

            breakerDroneIcon = ModContent.Request<Texture2D>("TidesOfTime/Assets/Projectiles/Misc/UniversalRemote/BreakerDrone", AssetRequestMode.ImmediateLoad).Value;

            taskElements = new List<TaskElement>();
        }

        public override void SafeOnInitialize()
        {
            IconButton exitButton = new("Exit", "UniversalRemote.ExitMenuButton", Color.Red);
            exitButton.Width.Set(32, 0);
            exitButton.Height.Set(32, 0);
            exitButton.Left.Set(GetDimensions().Width - 48, 0);
            exitButton.Top.Set(16, 0);
            exitButton.OnLeftClick += (evt, args) =>
            {
                TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().Visible = false;

                SoundEngine.PlaySound(SoundID.MenuClose);
            };

            Append(exitButton);

            AddGrid();
        }

        private void AddGrid()
        {
            taskGrid = new();
            taskGrid.Width.Set(GetDimensions().Width - 96, 0);
            taskGrid.Height.Set(GetDimensions().Height - 96, 0);
            taskGrid.Left.Set(32, 0);
            taskGrid.Top.Set(64, 0);

            bar = new(userInterface);
            bar.Width.Set(24, 0);
            bar.Height.Set(GetDimensions().Height - 96, 0);
            bar.Left.Set(taskGrid.Width.Pixels + 48, 0);
            bar.Top.Set(64, 0);

            taskGrid.SetScrollbar(bar);

            Append(bar);
            Append(taskGrid);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            taskGrid.UpdateOrder();

            Rectangle drawBox = GetDimensions().ToRectangle();

            Rectangle smallerBox = new(drawBox.X + 32, drawBox.Y + 64, drawBox.Width - 96, drawBox.Height - 96);

            UIHelper.DrawBox(spriteBatch, drawBox, ThemeSystem.BackgroundColor);
            UIHelper.DrawBox(spriteBatch, smallerBox, Color.Lerp(ThemeSystem.BackgroundColor, Color.Black, 0.5f));

            Utils.DrawBorderStringBig(spriteBatch, LocalizationHelper.GetGUIText("UniversalRemote.Tasks"), GetDimensions().Position() + new Vector2(32, 8), Color.White);

            if (taskGrid.Count == 0)
            {
                string text = LocalizationHelper.GetGUIText("UniversalRemote.NoTasks");

                float width = FontAssets.MouseText.Value.MeasureString(text).X;

                Vector2 middlePosition = taskGrid.GetDimensions().Position() + new Vector2(taskGrid.GetDimensions().Width / 2, 8);

                Utils.DrawBorderString(spriteBatch, text, middlePosition - new Vector2(width / 2, 0), Color.White);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

                spriteBatch.Draw(breakerDroneIcon, taskGrid.GetDimensions().Center(), new Rectangle(0, 0, breakerDroneIcon.Width / 4, breakerDroneIcon.Height),
                    Color.White, 0, new Vector2(breakerDroneIcon.Width / 8, breakerDroneIcon.Height / 2), 3, SpriteEffects.None, 0);

                DrawAngerSymbol(spriteBatch, taskGrid.GetDimensions().Center() + new Vector2(45, 0));

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            }

            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            base.Draw(spriteBatch);
        }

        private void DrawAngerSymbol(SpriteBatch spriteBatch, Vector2 position)
        {
            Texture2D value = TextureAssets.Extra[48].Value;
            SpriteEffects effect = SpriteEffects.None;

            int frames = 2 + (EmoteID.Count - 1) / 4;
            int emote = EmoteID.EmotionAnger;

            Rectangle value2 = value.Frame(8, frames);

            Vector2 origin = new(value2.Width / 2, value2.Height);

            spriteBatch.Draw(value, position, value.Frame(8, frames, emote * 2 % 8, 1 + emote / 4), Color.White, 0f, origin, 3, effect, 0f);
        }

        public void AddTask(DroneTask task)
        {
            TaskElement element = new(task);

            element.Width.Set(taskGrid.Width.Pixels, 0);
            element.Height.Set(112, 0);

            taskGrid.Add(element);
            taskElements.Add(element);

            element.OnInitialize();
        }

        public void RemoveTask(DroneTask task)
        {
            TaskElement element = taskElements.FirstOrDefault(taskElement => taskElement.Task == task);

            if (element == null)
            {
                return;
            }

            taskGrid.Remove(element);
            taskElements.Remove(element);
        }

        public void RemoveAllTasks()
        {
            taskGrid.Clear();
            taskElements.Clear();
        }
    }
}
