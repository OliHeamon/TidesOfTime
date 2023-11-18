using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TidesOfTime.Common.UI.Abstract;
using TidesOfTime.Common.UI.Themes;
using TidesOfTime.Common.UI.UniversalRemote.TaskList;

namespace TidesOfTime.Common.UI.UniversalRemote.SelectionMenu
{
    public class ConfirmPanel : SmartUIElement
    {
        private readonly SelectionUI selectionUI;

        public ConfirmPanel(SelectionUI selectionUI)
        {
            this.selectionUI = selectionUI;
        }

        public override void OnInitialize()
        {
            Left.Set(0, 0.5f);
            Top.Set(0, 0.5f);
            Width.Set(88, 0);
            Height.Set(48, 0);

            IconButton button = new("Check", "UniversalRemote.CheckSelectionButton", Color.Green);
            button.Width.Set(32, 0);
            button.Height.Set(32, 0);
            button.Left.Set(8, 0);
            button.Top.Set(8, 0);
            button.ClickableCondition = () => selectionUI.SelectionProvider.ValidSelection() && selectionUI.SlotContainsEnoughBlocks();
            button.ClickWhenEnabled += () =>
            {
                SelectionUI selectionUI = TidesOfTimeUILoader.GetUIState<SelectionUI>();

                selectionUI.Visible = false;
                selectionUI.StartAndConsumeBlocks();
                selectionUI.SelectionProvider.SelectionMode = DroneTaskMode.None;

                TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().Visible = true;

                SoundEngine.PlaySound(SoundID.MenuClose);
            };

            Append(button);

            button = new("Exit", "UniversalRemote.ExitSelectionButton", Color.Red);
            button.Width.Set(32, 0);
            button.Height.Set(32, 0);
            button.Left.Set(48, 0);
            button.Top.Set(8, 0);
            button.OnLeftClick += (evt, args) =>
            {
                SelectionUI selectionUI = TidesOfTimeUILoader.GetUIState<SelectionUI>();

                selectionUI.Visible = false;
                selectionUI.SelectionProvider.SelectionMode = DroneTaskMode.None;

                TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().Visible = true;

                SoundEngine.PlaySound(SoundID.MenuClose);
            };

            Append(button);

            base.OnInitialize();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            FixDimensions();

            Rectangle drawBox = GetDimensions().ToRectangle();

            UIHelper.DrawBox(spriteBa​tch, drawBox, ThemeSystem.BackgroundColor);

            base.Draw(spriteBatch);
        }

        private void FixDimensions()
        {
            Left.Set(Main.screenWidth * 0.5f - Width.Pixels / 2, 0);
            Top.Set(Main.screenHeight * 0.6f - Height.Pixels / 2, 0);
            Recalculate();
        }
    }
}
