using Microsoft.Xna.Framework.Graphics;
using Terraria.GameInput;
using Terraria.UI;
using Terraria;
using TidesOfTime.Common.UI.Abstract;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using System;
using TidesOfTime.Common.UI.Themes;
using Terraria.UI.Chat;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;
using TidesOfTime.Localization;

namespace TidesOfTime.Common.UI.UniversalRemote.SelectionMenu
{
    public class DroneItemSlot : SmartUIElement, IConditionalElement
    {
        private readonly Func<Item, bool> validItem;

        private readonly WorldSelectionProvider worldSelectionProvider;

        private readonly string slotInfo;

        private Item storedItem;

        public Item Item => storedItem.Clone();

        public bool Condition
        {
            get
            {
                DroneTaskMode mode = worldSelectionProvider.SelectionMode;

                return mode == DroneTaskMode.RectangleFill || mode == DroneTaskMode.RectangleOutline ||
                    mode == DroneTaskMode.CircleOutline || mode == DroneTaskMode.CircleFill ||
                    mode == DroneTaskMode.Stamp;
            }
        }

        public DroneItemSlot(Func<Item, bool> validItem, string slotInfo, WorldSelectionProvider worldSelectionProvider)
        {
            this.validItem = validItem;
            this.slotInfo = slotInfo;
            this.worldSelectionProvider = worldSelectionProvider;

            storedItem = new Item();
            storedItem.SetDefaults();

            Width.Set(48, 0f);
            Height.Set(48, 0f);
        }

        public void ClearSlot()
        {
            storedItem = new Item();
            storedItem.SetDefaults();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DroneTaskMode mode = worldSelectionProvider.SelectionMode;

            if (Condition)
            {
                float oldScale = Main.inventoryScale;

                Main.inventoryScale = 1;

                Rectangle rectangle = GetDimensions().ToRectangle();

                if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)
                {
                    Main.LocalPlayer.mouseInterface = true;

                    if (validItem == null || validItem(Main.mouseItem))
                    {
                        HandleLeftClicks();
                        HandleRightClicks();
                        HandleMouseHover();
                    }
                }

                UIHelper.DrawBox(spriteBatch, rectangle, ThemeSystem.BackgroundColor);
                DrawItem(spriteBatch, rectangle);

                Main.inventoryScale = oldScale;

                base.Draw(spriteBatch);
            }
        }

        private void DrawItem(SpriteBatch spriteBatch, Rectangle rectangle)
        {
            ItemSlot.DrawItemIcon(storedItem, ItemSlot.Context.InventoryItem,
                spriteBatch, rectangle.TopLeft() + rectangle.Size() / 2f, Main.inventoryScale, 32f, Color.White);

            if (storedItem.stack > 1)
            {
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value,
                    storedItem.stack.ToString(), rectangle.TopLeft() + new Vector2(10f, 26f) * Main.inventoryScale,
                    Color.White, 0f, Vector2.Zero, new Vector2(Main.inventoryScale), -1f, Main.inventoryScale);
            }
        }

        public void SetItem(Item item, bool clone = false)
        {
            storedItem = clone ? item.Clone() : item;
        }

        public void SetItem(int itemType, int stack = 1)
        {
            storedItem.SetDefaults(itemType);
            storedItem.stack = stack;
        }

        private void HandleLeftClicks()
        {
            if (!(Main.mouseLeftRelease && Main.mouseLeft))
            {
                return;
            }

            int context = ItemSlot.Context.GuideItem;

            if (Main.mouseItem.maxStack <= 1 || storedItem.type != Main.mouseItem.type || storedItem.stack == storedItem.maxStack || Main.mouseItem.stack == Main.mouseItem.maxStack)
            {
                Utils.Swap(ref storedItem, ref Main.mouseItem);
            }

            if (storedItem.stack > 0)
            {
                ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(storedItem, 21, context, storedItem.stack));
            }
            else
            {
                ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(Main.mouseItem, context, 21, Main.mouseItem.stack));
            }

            if (storedItem.type == ItemID.None || storedItem.stack < 1)
            {
                storedItem = new Item();
            }

            if (AreTheSame(Main.mouseItem, storedItem) && storedItem.stack != storedItem.maxStack && Main.mouseItem.stack != Main.mouseItem.maxStack && ItemLoader.TryStackItems(storedItem, Main.mouseItem, out var numTransferred))
            {
                ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(storedItem, 21, context, numTransferred));
            }

            if (Main.mouseItem.type == ItemID.None || Main.mouseItem.stack < 1)
            {
                Main.mouseItem = new Item();
            }

            if (Main.mouseItem.type > ItemID.None || storedItem.type > ItemID.None)
            {
                Recipe.FindRecipes();
                SoundEngine.PlaySound(SoundID.Grab);
            }
        }

        private bool AreTheSame(Item item1, Item item2)
        {
            if (item1.netID == item2.netID)
            {
                return item1.type == item2.type;
            }

            return false;
        }

        private void HandleMouseHover()
        {
            int context = ItemSlot.Context.GuideItem;

            if (storedItem.type > ItemID.None && storedItem.stack > 0)
            {
                Main.hoverItemName = storedItem.Name;

                if (storedItem.stack > 1)
                {
                    Main.hoverItemName = Main.hoverItemName + " (" + storedItem.stack + ")";
                }

                Main.HoverItem = storedItem.Clone();
                Main.HoverItem.tooltipContext = context;
            }
            else
            {
                Main.hoverItemName = LocalizationHelper.GetGUIText(slotInfo);
            }
        }

        private void HandleRightClicks()
        {
            int context = ItemSlot.Context.GuideItem;

            if (!Main.mouseRight)
            {
                return;
            }

            if (Main.stackSplit > 1)
            {
                return;
            }

            int num = Main.superFastStack + 1;

            for (int i = 0; i < num; i++)
            {
                if ((AreTheSame(Main.mouseItem, storedItem) && ItemLoader.CanStack(Main.mouseItem, storedItem) || Main.mouseItem.type == ItemID.None) && (Main.mouseItem.stack < Main.mouseItem.maxStack || Main.mouseItem.type == ItemID.None))
                {
                    PickupItemIntoMouse(context);

                    SoundEngine.PlaySound(SoundID.MenuTick);

                    ItemSlot.RefreshStackSplitCooldown();
                }
            }
        }

        private void PickupItemIntoMouse(int context)
        {
            if (Main.mouseItem.type == ItemID.None)
            {
                Main.mouseItem = ItemLoader.TransferWithLimit(storedItem, 1);

                ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(storedItem, context, 21));
            }
            else
            {
                ItemLoader.StackItems(Main.mouseItem, storedItem, out var _, false, 1);
            }

            if (storedItem.stack <= 0)
            {
                storedItem = new Item();
            }

            Recipe.FindRecipes();
        }
    }
}
