using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using TidesOfTime.Common.UI.Abstract;
using TidesOfTime.Common.UI.Themes;
using TidesOfTime.Common.UI.UniversalRemote.TaskList;

namespace TidesOfTime.Common.UI.UniversalRemote.SelectionMenu
{
    public class StampPanel : SmartUIElement, IConditionalElement
    {
        private StampToggleButton eraser;

        private StampToggleButton small;

        private StampToggleButton medium;

        private StampToggleButton large;

        private StampButton[,] stampButtons;

        private readonly WorldSelectionProvider worldSelectionProvider;

        public bool Condition => worldSelectionProvider.SelectionMode == DroneTaskMode.Stamp;

        public StampPanel(WorldSelectionProvider worldSelectionProvider)
        {
            this.worldSelectionProvider = worldSelectionProvider;
        }

        public override void OnInitialize()
        {
            Width.Set(272, 0);
            Height.Set(312, 0);

            stampButtons = new StampButton[16, 16];

            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    StampButton stampButton = new(Color.Black);

                    stampButton.Left.Set((x * 16) + 8, 0);
                    stampButton.Top.Set((y * 16) + 48, 0);

                    int i = x;
                    int j = y;

                    stampButton.OnMouseOver += (evt, args) => WriteToCanvas(i, j);
                    stampButton.OnLeftMouseDown += (evt, args) => WriteToCanvas(i, j);

                    Append(stampButton);

                    stampButtons[x, y] = stampButton;
                }
            }

            IconButton button = new("Undo", "UniversalRemote.ResetGrid", Color.Gray);
            button.Width.Set(32, 0);
            button.Height.Set(32, 0);
            button.Left.Set(8, 0);
            button.Top.Set(8, 0);
            button.OnLeftClick += (evt, args) =>
            {
                foreach (UIElement element in Children)
                {
                    if (element is StampButton stampButton)
                    {
                        stampButton.Toggled = false;
                    }
                }
            };
            Append(button);

            eraser = new("Eraser", Color.Gray);
            eraser.Width.Set(32, 0);
            eraser.Height.Set(32, 0);
            eraser.Left.Set(48, 0);
            eraser.Top.Set(8, 0);
            Append(eraser);

            CreateSizeButtons();
        }

        private void CreateSizeButtons()
        {
            small = new("SmallBrush", Color.Gray);
            small.Width.Set(32, 0);
            small.Height.Set(32, 0);
            small.Left.Set(88, 0);
            small.Top.Set(8, 0);
            small.OnLeftClick += (evt, args) =>
            {
                medium.Toggled = false;
                large.Toggled = false;
            };
            small.Toggled = true;
            Append(small);

            medium = new("MediumBrush", Color.Gray);
            medium.Width.Set(32, 0);
            medium.Height.Set(32, 0);
            medium.Left.Set(128, 0);
            medium.Top.Set(8, 0);
            medium.OnLeftClick += (evt, args) =>
            {
                small.Toggled = false;
                large.Toggled = false;
            };
            Append(medium);

            large = new("LargeBrush", Color.Gray);
            large.Width.Set(32, 0);
            large.Height.Set(32, 0);
            large.Left.Set(168, 0);
            large.Top.Set(8, 0);
            large.OnLeftClick += (evt, args) =>
            {
                small.Toggled = false;
                medium.Toggled = false;
            };
            Append(large);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Condition)
            {
                UIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeSystem.BackgroundColor);

                base.Draw(spriteBatch);
            }
        }

        private void WriteToCanvas(int i, int j)
        {
            if (!Main.mouseLeft)
            {
                return;
            }

            int radius = 1;

            if (medium.Toggled)
            {
                radius += 1;
            }

            if (large.Toggled)
            {
                radius += 2;
            }

            for (int x = i - radius + 1; x < i + radius; x++)
            {
                for (int y = j - radius + 1; y < j + radius; y++)
                {
                    if (x < 0 || y < 0 || x > 15 || y > 15)
                    {
                        continue;
                    }

                    stampButtons[x, y].Toggled = !eraser.Toggled;
                }
            }
        }

        public string EncodeGridToString()
        {
            string grid = "";

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    grid += stampButtons[x, y].Toggled ? "1" : "0";
                }
            }

            return grid;
        }

        public int GetStampBlockCount()
        {
            int count = 0;

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (stampButtons[x, y].Toggled)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}
