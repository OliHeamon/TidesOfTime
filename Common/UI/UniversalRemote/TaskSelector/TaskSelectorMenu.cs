using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.UI;
using TidesOfTime.Common.UI.Abstract;
using TidesOfTime.Common.UI.Themes;
using TidesOfTime.Common.UI.UniversalRemote.SelectionMenu;
using TidesOfTime.Common.UI.UniversalRemote.TaskSelector.TaskButtons;
using TidesOfTime.Content.Players;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks.ScannerDroneTasks;

namespace TidesOfTime.Common.UI.UniversalRemote.TaskSelector
{
    public class TaskSelectorMenu : SmartUIElement
    {
        private readonly List<TaskButton> buttons;

        public bool BreakerDroneShouldBreakWalls => (buttons[13] as ToggleSettingButton).Toggled;

        public TaskSelectorMenu()
        {
             buttons = new();
        }

        public override void OnInitialize()
        {
            InitializeScannerButtons();
            InitializeBuilderButtons();
            InitializeBreakerButtons();

            int widthOffset = 0;

            for (int i = 0; i < buttons.Count; i++)
            {
                if (i != 0 && i % 5 == 0)
                {
                    widthOffset += 40;
                }

                TaskButton button = buttons[i];

                button.Left.Set(16 + widthOffset, 0);
                button.Top.Set(16 + (i % 5 * 40), 0);

                Append(button);
            }
        }

        public void ResetToggles()
        {
            foreach (UIElement element in Children)
            {
                if (element is ToggleSettingButton button)
                {
                    button.Toggled = false;
                }
            }
        }

        private void InitializeScannerButtons()
        {
            TaskButton scannerSpelunkerPulse = new("ScannerSpelunkerPulse", Color.Blue);
            scannerSpelunkerPulse.OnLeftClick += (evt, args) => Main.LocalPlayer.GetModPlayer<WorkerDronePlayer>().GetWorkerDrone<ScannerDrone>().AddTask(new SpelunkerPulseTask());
            buttons.Add(scannerSpelunkerPulse);

            TaskButton scannerHunterPulse = new("ScannerHunterPulse", Color.Blue);
            scannerHunterPulse.OnLeftClick += (evt, args) => Main.LocalPlayer.GetModPlayer<WorkerDronePlayer>().GetWorkerDrone<ScannerDrone>().AddTask(new HunterPulseTask());
            buttons.Add(scannerHunterPulse);

            TaskButton scannerSensorArray = new("ScannerSensorArray", Color.Blue);
            buttons.Add(scannerSensorArray);

            ToggleSettingButton scannerSpotlight = new("ScannerSpotlight", Color.Blue);
            scannerSpotlight.OnLeftClick += (evt, args) => Main.LocalPlayer.GetModPlayer<WorkerDronePlayer>().GetWorkerDrone<ScannerDrone>().ToggleSpotlight();
            buttons.Add(scannerSpotlight);

            TaskButton scannerEvilPulse = new("ScannerEvilPulse", Color.Blue);
            scannerEvilPulse.OnLeftClick += (evt, args) => Main.LocalPlayer.GetModPlayer<WorkerDronePlayer>().GetWorkerDrone<ScannerDrone>().AddTask(new EvilPulseTask());
            buttons.Add(scannerEvilPulse);
        }

        private void InitializeBuilderButtons()
        {
            SelectionMenuButton builderRectangleOutline = new("BuilderRectangleOutline", Color.Green);
            builderRectangleOutline.OnLeftClick += (evt, args) =>
                TidesOfTimeUILoader.GetUIState<SelectionUI>().SelectionProvider.SelectionMode = DroneTaskMode.RectangleOutline;
            buttons.Add(builderRectangleOutline);

            SelectionMenuButton builderRectangleFill = new("BuilderRectangleFill", Color.Green);
            builderRectangleFill.OnLeftClick += (evt, args) =>
                TidesOfTimeUILoader.GetUIState<SelectionUI>().SelectionProvider.SelectionMode = DroneTaskMode.RectangleFill;
            buttons.Add(builderRectangleFill);

            SelectionMenuButton builderCircleOutline = new("BuilderCircleOutline", Color.Green);
            builderCircleOutline.OnLeftClick += (evt, args) =>
                TidesOfTimeUILoader.GetUIState<SelectionUI>().SelectionProvider.SelectionMode = DroneTaskMode.CircleOutline;
            buttons.Add(builderCircleOutline);

            SelectionMenuButton builderCircleFill = new("BuilderCircleFill", Color.Green);
            builderCircleFill.OnLeftClick += (evt, args) =>
                TidesOfTimeUILoader.GetUIState<SelectionUI>().SelectionProvider.SelectionMode = DroneTaskMode.CircleFill;
            buttons.Add(builderCircleFill);

            SelectionMenuButton builderStamp = new("BuilderStamp", Color.Green);
            builderStamp.OnLeftClick += (evt, args) =>
                TidesOfTimeUILoader.GetUIState<SelectionUI>().SelectionProvider.SelectionMode = DroneTaskMode.Stamp;
            buttons.Add(builderStamp);
        }

        private void InitializeBreakerButtons()
        {
            SelectionMenuButton breakerRectangle = new("BreakerRectangle", Color.Red);
            breakerRectangle.OnLeftClick += (evt, args) =>
            {
                TidesOfTimeUILoader.GetUIState<SelectionUI>().SelectionProvider.SelectionMode = DroneTaskMode.BreakRectangle;
            };
            buttons.Add(breakerRectangle);

            SelectionMenuButton breakerCircle = new("BreakerCircle", Color.Red);
            breakerCircle.OnLeftClick += (evt, args) =>
            {
                TidesOfTimeUILoader.GetUIState<SelectionUI>().SelectionProvider.SelectionMode = DroneTaskMode.BreakCircle;
            };
            buttons.Add(breakerCircle);

            SelectionMenuButton breakerLiquidVacuum = new("BreakerLiquidVacuum", Color.Red);
            breakerLiquidVacuum.OnLeftClick += (evt, args) =>
            {
                TidesOfTimeUILoader.GetUIState<SelectionUI>().SelectionProvider.SelectionMode = DroneTaskMode.VacuumLiquid;
            };
            buttons.Add(breakerLiquidVacuum);

            ToggleSettingButton breakerModeSwitch = new("BreakerModeSwitch", Color.Red);
            buttons.Add(breakerModeSwitch);

            SelectionMenuButton breakerClentaminate = new("BreakerClentaminate", Color.Red);
            breakerClentaminate.OnLeftClick += (evt, args) =>
            {
                TidesOfTimeUILoader.GetUIState<SelectionUI>().SelectionProvider.SelectionMode = DroneTaskMode.Clentaminate;
            };
                
            buttons.Add(breakerClentaminate);
        }

        public override void SafeUpdate(GameTime gameTime)
        {
            if (GetDimensions().ToRectangle().Contains(Main.MouseScreen.ToPoint()))
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            UIHelper.DrawBox(spriteBatch, drawBox, ThemeSystem.BackgroundColor);

            base.Draw(spriteBatch);
        }
    }
}
