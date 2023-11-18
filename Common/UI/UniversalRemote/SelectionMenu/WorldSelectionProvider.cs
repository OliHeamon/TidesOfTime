using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.UI;
using TidesOfTime.Localization;

namespace TidesOfTime.Common.UI.UniversalRemote.SelectionMenu
{
    public class WorldSelectionProvider
    {
        public Vector2 StartPoint => startPoint;

        public DroneTaskMode SelectionMode { get; set; }

        private readonly SelectionUI selectionUI;

        private Vector2 startPoint;

        private Vector2 endPoint;

        private bool oldMouseLeft;

        public WorldSelectionProvider(SelectionUI selectionUI)
        {
            this.selectionUI = selectionUI;
        }

        public void UpdateSelection()
        {
            if (SelectionMode == DroneTaskMode.None)
            {
                startPoint = Vector2.Zero;
                endPoint = Vector2.Zero;
            }

            foreach (UIElement child in selectionUI.Children)
            {
                if (child is IConditionalElement element)
                {
                    if (element.Condition && child.ContainsPoint(Main.MouseScreen))
                    {
                        return;
                    }
                }
                else if (child.ContainsPoint(Main.MouseScreen))
                {
                    return;
                }
            }

            // Might not be necessary but I don't want inventory slots interacting with the selection. Maybe disable it while dragging?
            if (Main.playerInventory)
            {
                return;
            }

            if (!TidesOfTimeUILoader.GetUIState<SelectionUI>().Visible || (!Main.mouseLeft && !ValidSelection()))
            {
                startPoint = Vector2.Zero;
                endPoint = Vector2.Zero;
            }

            switch (SelectionMode)
            {
                case DroneTaskMode.None:
                    break;
                case DroneTaskMode.RectangleOutline:
                case DroneTaskMode.RectangleFill:
                case DroneTaskMode.BreakRectangle:
                case DroneTaskMode.VacuumLiquid:
                case DroneTaskMode.Clentaminate:
                    HandleRectangularSelection();
                    break;
                case DroneTaskMode.CircleOutline:
                case DroneTaskMode.CircleFill:
                case DroneTaskMode.BreakCircle:
                    HandleCircularSelection();
                    break;
                case DroneTaskMode.Stamp:
                    if (!oldMouseLeft && Main.mouseLeft)
                    {
                        startPoint = RoundToNearest16(Main.MouseWorld);
                    }
                    break;
            }

            oldMouseLeft = Main.mouseLeft;
        }

        public bool ValidSelection()
        {
            if (SelectionMode == DroneTaskMode.RectangleOutline || SelectionMode == DroneTaskMode.RectangleFill || SelectionMode == DroneTaskMode.BreakRectangle
                || SelectionMode == DroneTaskMode.VacuumLiquid || SelectionMode == DroneTaskMode.Clentaminate)
            {
                if (startPoint == endPoint)
                {
                    return false;
                }

                if (startPoint.X - endPoint.X == 0 || startPoint.Y - endPoint.Y == 0)
                {
                    return false;
                }

                int xLength = (int)MathF.Abs(endPoint.X - startPoint.X) / 16;
                int yLength = (int)MathF.Abs(endPoint.Y - startPoint.Y) / 16;

                int area = xLength * yLength;

                // Area contains more than 9999 tiles (max stack).
                if (area > 9999)
                {
                    Main.hoverItemName = LocalizationHelper.GetGUIText("UniversalRemote.TooManyBlocks");

                    return false;
                }
            }
            else if (SelectionMode == DroneTaskMode.CircleOutline || SelectionMode == DroneTaskMode.CircleFill || SelectionMode == DroneTaskMode.BreakCircle)
            {
                if (startPoint == endPoint)
                {
                    return false;
                }

                if (startPoint.X - endPoint.X == 0 || startPoint.Y - endPoint.Y == 0)
                {
                    return false;
                }

                // Area contains more than 9999 tiles (max stack).
                if (GetBlockCount() > 9999)
                {
                    Main.hoverItemName = LocalizationHelper.GetGUIText("UniversalRemote.TooManyBlocks");

                    return false;
                }
            }
            else if (SelectionMode == DroneTaskMode.Stamp)
            {
                if (startPoint == endPoint || startPoint == Vector2.Zero)
                {
                    return false;
                }
            }

            return true;
        }

        public Rectangle GetSelectedRectangle()
        {
            int width = (int)MathF.Abs(endPoint.X - startPoint.X);
            int height = (int)MathF.Abs(endPoint.Y - startPoint.Y);

            int x = startPoint.X < endPoint.X ? (int)startPoint.X : (int)endPoint.X;
            int y = startPoint.Y < endPoint.Y ? (int)startPoint.Y : (int)endPoint.Y;

            return new(x, y, width, height);
        }

        private void HandleRectangularSelection()
        {
            // Mouse was just pressed.
            if (!oldMouseLeft && Main.mouseLeft)
            {
                startPoint = RoundToNearest16(Main.MouseWorld);
            }

            if (Main.mouseLeft)
            {
                endPoint = RoundToNearest16(Main.MouseWorld);
            }
        }

        private void HandleCircularSelection()
        {
            if (!oldMouseLeft && Main.mouseLeft)
            {
                startPoint = RoundToNearest16(Main.MouseWorld);
            }

            if (Main.mouseLeft)
            {
                endPoint = RoundToNearest16(Main.MouseWorld);
            }
        }

        private Vector2 RoundToNearest16(Vector2 vector2)
        {
            float x = MathF.Round(vector2.X / 16f) * 16f;
            float y = MathF.Round(vector2.Y / 16f) * 16f;

            return new(x, y);
        }

        public int GetBlockCount()
        {
            if (SelectionMode == DroneTaskMode.RectangleOutline || SelectionMode == DroneTaskMode.RectangleFill)
            {
                int width = (int)MathF.Abs(endPoint.X - startPoint.X) / 16;
                int height = (int)MathF.Abs(endPoint.Y - startPoint.Y) / 16;

                int area = width * height;

                int smallerArea = 0;

                if (SelectionMode == DroneTaskMode.RectangleOutline && width > 1 && height > 1)
                {
                    smallerArea = (width - 2) * (height - 2);
                }

                return area - smallerArea;
            }
            else if (SelectionMode == DroneTaskMode.CircleOutline || SelectionMode == DroneTaskMode.CircleFill)
            {
                int width = (int)MathF.Abs(endPoint.X - startPoint.X) / 16;
                int height = (int)MathF.Abs(endPoint.Y - startPoint.Y) / 16;

                int x = (startPoint.X < endPoint.X ? (int)startPoint.X : (int)endPoint.X) / 16;
                int y = (startPoint.Y < endPoint.Y ? (int)startPoint.Y : (int)endPoint.Y) / 16;

                Rectangle tileRectangle = new(x, y, width, height);

                Rectangle shrunkTileRectangle = new(tileRectangle.X + 1, tileRectangle.Y + 1, tileRectangle.Width - 2, tileRectangle.Height - 2);

                int CountTiles(Rectangle tileRectangle)
                {
                    Vector2 startPoint = tileRectangle.TopLeft();
                    Vector2 endPoint = tileRectangle.BottomRight();

                    int xLength = (int)MathF.Abs(endPoint.X - startPoint.X);
                    int yLength = (int)MathF.Abs(endPoint.Y - startPoint.Y);

                    int count = 0;

                    for (int x = 0; x < xLength; x++)
                    {
                        for (int y = 0; y < yLength; y++)
                        {
                            int startX = startPoint.X < endPoint.X ? (int)startPoint.X : (int)endPoint.X;
                            int startY = startPoint.Y < endPoint.Y ? (int)startPoint.Y : (int)endPoint.Y;

                            if (!WorldGen.InWorld(startX + x, startY + y))
                            {
                                continue;
                            }

                            // The following calculates whether a given tile in the selection area is inside the area's inscribed ellipse.

                            int a = xLength / 2;
                            int b = yLength / 2;

                            Vector2 ellipseCenter = new(startX + a, startY + b);

                            Vector2 tilePoint = new(startX + x + 0.5f, startY + y + 0.5f);

                            float formulaX = (float)((tilePoint.X - ellipseCenter.X) * (tilePoint.X - ellipseCenter.X)) / (a * a);
                            float formulaY = (float)((tilePoint.Y - ellipseCenter.Y) * (tilePoint.Y - ellipseCenter.Y)) / (b * b);

                            // Tile point is inside the ellipse.
                            if (formulaX + formulaY <= 1)
                            {
                                count++;
                            }
                        }
                    }

                    return count;
                }

                int mainCount = CountTiles(tileRectangle);

                if (SelectionMode == DroneTaskMode.CircleOutline && width > 1 && height > 1)
                {
                    mainCount -= CountTiles(shrunkTileRectangle);
                }

                return mainCount;
            }

            return 0;
        }
    }
}
