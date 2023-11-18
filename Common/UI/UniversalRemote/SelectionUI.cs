using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader.IO;
using Terraria.UI;
using TidesOfTime.Common.UI.Abstract;
using TidesOfTime.Common.UI.Themes;
using TidesOfTime.Common.UI.UniversalRemote.SelectionMenu;
using TidesOfTime.Content.Players;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks.BuilderDroneTasks;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote;
using TidesOfTime.Localization;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks.BreakerDroneTasks;

namespace TidesOfTime.Common.UI.UniversalRemote
{
    public class SelectionUI : SmartUIState
    {
        private ConfirmPanel panel;

        private DroneItemSlot slot;

        private StampPanel stampPanel;

        public WorldSelectionProvider SelectionProvider { get; private set; }

        public SelectionUI()
        {
            SelectionProvider = new WorldSelectionProvider(this);
        }

        public override int InsertionIndex(List<GameInterfaceLayer> layers)
        {
            return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        }

        public override void OnInitialize()
        {
            panel = new ConfirmPanel(this);
            Append(panel);

            slot = new(item =>
            {
                if (item.IsAir || item.createWall > -1)
                {
                    return true;
                }

                // Don't allow multitiles.
                if (item.createTile > -1)
                {
                    return !Main.tileFrameImportant[item.createTile];
                }

                return false;
            }, $"UniversalRemote.SlotInfo", SelectionProvider);
            Append(slot);

            stampPanel = new StampPanel(SelectionProvider);
            Append(stampPanel);
        }

        public bool SlotContainsEnoughBlocks()
        {
            int area = SelectionProvider.SelectionMode == DroneTaskMode.Stamp ? stampPanel.GetStampBlockCount() : SelectionProvider.GetBlockCount();

            if (slot.Item.stack >= area)
            {
                return true;
            }

            return false;
        }

        public void StartAndConsumeBlocks()
        {
            int area = SelectionProvider.SelectionMode == DroneTaskMode.Stamp ? stampPanel.GetStampBlockCount() : SelectionProvider.GetBlockCount();

            int remainingItems = slot.Item.stack - area;

            if (remainingItems > 0)
            {
                Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_FromThis(), slot.Item, remainingItems);
            }

            PlacementType placementType = slot.Item.createTile > -1 ? PlacementType.Tile : PlacementType.Wall;

            int blockType = slot.Item.createTile > -1 ? slot.Item.createTile : slot.Item.createWall;

            DroneTask task = null;

            if (SelectionProvider.SelectionMode == DroneTaskMode.RectangleOutline || SelectionProvider.SelectionMode == DroneTaskMode.RectangleFill)
            {
                task = new RectangleBlockPlacementTask(SelectionProvider.GetSelectedRectangle(),
                    blockType, slot.Item.type, slot.Item.Name, placementType, SelectionProvider.SelectionMode);
            }
            else if (SelectionProvider.SelectionMode == DroneTaskMode.CircleOutline || SelectionProvider.SelectionMode == DroneTaskMode.CircleFill)
            {
                task = new CircleBlockPlacementTask(SelectionProvider.GetSelectedRectangle(),
                    blockType, slot.Item.type, slot.Item.Name, placementType, SelectionProvider.SelectionMode);
            }
            else if (SelectionProvider.SelectionMode == DroneTaskMode.Stamp)
            {
                task = new StampPlacementTask(SelectionProvider.StartPoint,
                    blockType, slot.Item.type, slot.Item.Name, placementType, stampPanel.EncodeGridToString());
            }
            else
            {
                bool shouldBreakWalls = TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().TaskSelector.BreakerDroneShouldBreakWalls;

                switch (SelectionProvider.SelectionMode)
                {
                    case DroneTaskMode.BreakRectangle:
                        task = new RectangleBreakTask(SelectionProvider.GetSelectedRectangle(), SelectionProvider.SelectionMode, shouldBreakWalls);
                        break;
                    case DroneTaskMode.BreakCircle:
                        task = new CircleBreakTask(SelectionProvider.GetSelectedRectangle(), SelectionProvider.SelectionMode, shouldBreakWalls);
                        break;
                    case DroneTaskMode.VacuumLiquid:
                        task = new RectangleBreakTask(SelectionProvider.GetSelectedRectangle(), SelectionProvider.SelectionMode, shouldBreakWalls);
                        break;
                    case DroneTaskMode.Clentaminate:
                        task = new RectangleBreakTask(SelectionProvider.GetSelectedRectangle(), SelectionProvider.SelectionMode, shouldBreakWalls);
                        break;
                }

                Main.LocalPlayer.GetModPlayer<WorkerDronePlayer>().GetWorkerDrone<BreakerDrone>().AddTask(task);

                return;
            }

            Main.LocalPlayer.GetModPlayer<WorkerDronePlayer>().GetWorkerDrone<BuilderDrone>().AddTask(task);

            slot.ClearSlot();
        }

        public void SaveSlotData(TagCompound tag)
        {
            tag.Set("TidesOfTime:SlotItemType", slot.Item.type);
            tag.Set("TidesOfTime:SlotItemStack", slot.Item.stack);
        }

        public void LoadSlotData(TagCompound tag)
        {
            Item item = new();

            if (!tag.TryGet<int>("TidesOfTime:SlotItemType", out var type))
            {
                return;
            }

            int stack = tag.GetInt("TidesOfTime:SlotItemStack");

            item.SetDefaults(type);
            item.stack = stack;

            slot.SetItem(item);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            slot.Left.Set(panel.Left.Pixels + panel.Width.Pixels + 16, 0);
            slot.Top.Set(panel.Top.Pixels, 0);

            stampPanel.Left.Set(panel.Left.Pixels + panel.Width.Pixels + 16, 0);
            stampPanel.Top.Set(panel.Top.Pixels - stampPanel.Height.Pixels - 16, 0);
            Recalculate();

            DrawSelection(spriteBatch);

            Main.LocalPlayer.mouseInterface = true;

            base.Draw(spriteBatch);

            DrawSlotCounter(spriteBatch);
        }

        public override void SafeUpdate(GameTime gameTime)
        {
            SelectionProvider.UpdateSelection();   
        }

        private void DrawSelection(SpriteBatch spriteBatch)
        {
            Rectangle rectangle = SelectionProvider.SelectionMode == DroneTaskMode.Stamp ?
                new Rectangle((int)SelectionProvider.StartPoint.X, (int)SelectionProvider.StartPoint.Y, 256, 256)
                : SelectionProvider.GetSelectedRectangle();

            bool valid = SelectionProvider.ValidSelection();

            Rectangle screenRectangle = new(rectangle.X - (int)Main.screenPosition.X, rectangle.Y - (int)Main.screenPosition.Y,
                rectangle.Width, rectangle.Height);

            bool intersectsUI = false;

            foreach (UIElement element in Children)
            {
                if (element.ContainsPoint(Main.MouseScreen))
                {
                    intersectsUI = true;
                }
            }

            Color validColor = (SelectionProvider.SelectionMode == DroneTaskMode.BreakRectangle
                || SelectionProvider.SelectionMode == DroneTaskMode.BreakCircle
                || SelectionProvider.SelectionMode == DroneTaskMode.VacuumLiquid
                || SelectionProvider.SelectionMode == DroneTaskMode.Clentaminate) ? Color.Orange : Color.Green;

            if (SelectionProvider.SelectionMode == DroneTaskMode.RectangleOutline || SelectionProvider.SelectionMode == DroneTaskMode.RectangleFill || SelectionProvider.SelectionMode == DroneTaskMode.BreakRectangle
                || SelectionProvider.SelectionMode == DroneTaskMode.VacuumLiquid || SelectionProvider.SelectionMode == DroneTaskMode.Clentaminate)
            {
                if (screenRectangle.Width != 0 && screenRectangle.Height != 0)
                {
                    Texture2D boxTexture = ThemeSystem.GetIcon("WhiteEmptyBox");

                    UIHelper.DrawBoxWith(spriteBatch, boxTexture, screenRectangle, PulseColor(valid ? validColor : Color.Red));
                }
            }
            else if (SelectionProvider.SelectionMode == DroneTaskMode.CircleOutline || SelectionProvider.SelectionMode == DroneTaskMode.CircleFill
                || SelectionProvider.SelectionMode == DroneTaskMode.BreakCircle)
            {
                if (screenRectangle.Width != 0 && screenRectangle.Height != 0)
                {
                    Texture2D boxTexture = ThemeSystem.GetIcon("WhiteEmptyBox");

                    UIHelper.DrawBoxWith(spriteBatch, boxTexture, screenRectangle, PulseColor(valid ? validColor : Color.Red));
                }
            }
            else if (SelectionProvider.SelectionMode == DroneTaskMode.Stamp)
            {
                Texture2D boxTexture = ThemeSystem.GetIcon("WhiteEmptyBox");

                UIHelper.DrawBoxWith(spriteBatch, boxTexture, screenRectangle, PulseColor(valid ? Color.Green : Color.Red));

                string grid = stampPanel.EncodeGridToString();

                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        int stringIndex = (y * 16) + x;

                        if (grid[stringIndex] == '1')
                        {
                            Rectangle smallRectangle = new(screenRectangle.X + (x * 16), screenRectangle.Y + (y * 16), 16, 16);

                            UIHelper.DrawBoxWith(spriteBatch, boxTexture, smallRectangle, PulseColor(valid ? Color.Green : Color.Red));
                        }
                    }
                }
            }

            if (SelectionProvider.SelectionMode != DroneTaskMode.None && !intersectsUI)
            {
                Main.hoverItemName = LocalizationHelper.GetGUIText("UniversalRemote.SelectionArea", rectangle.Width / 16, rectangle.Height / 16);
            }
        }

        private void DrawSlotCounter(SpriteBatch spriteBatch)
        {
            if (SelectionProvider.SelectionMode == DroneTaskMode.BreakRectangle
                || SelectionProvider.SelectionMode == DroneTaskMode.BreakCircle
                || SelectionProvider.SelectionMode == DroneTaskMode.VacuumLiquid
                || SelectionProvider.SelectionMode == DroneTaskMode.Clentaminate)
            {
                return;
            }

            int area = SelectionProvider.SelectionMode == DroneTaskMode.Stamp ? stampPanel.GetStampBlockCount() : SelectionProvider.GetBlockCount();

            if (area == 0)
            {
                return;
            }

            string noBlockInfo = LocalizationHelper.GetGUIText("UniversalRemote.InsertBlock");

            string info = slot.Item.IsAir ? noBlockInfo : $"{slot.Item.Name}: {slot.Item.stack}/{area}";

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(info);

            Vector2 textPosition = slot.GetDimensions().Position() + (slot.GetDimensions().ToRectangle().Size() / 2)
                - (textSize / 2) + new Vector2(0, 48);

            Utils.DrawBorderString(spriteBatch, info, textPosition, SlotContainsEnoughBlocks() ? Color.Green : Color.Red);
        }

        private Color PulseColor(Color color)
        {
            float pulseFactor = (MathF.Sin(Main.GameUpdateCount / 10f) + 1) / 3;

            color.A = (byte)(pulseFactor * 255);

            return color;
        }
    }
}
