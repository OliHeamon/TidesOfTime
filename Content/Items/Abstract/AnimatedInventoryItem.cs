using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace TidesOfTime.Content.Items.Abstract
{
    public abstract class AnimatedInventoryItem : ModItem
    {
        public abstract int FrameCount { get; }

        public abstract int TicksPerFrame { get; }

        private string InventoryTexture => $"{Texture}_Inventory";

        private readonly Texture2D inventoryTexture;

        public AnimatedInventoryItem()
        {
            if (!Main.dedServ)
            {
                inventoryTexture = ModContent.Request<Texture2D>(InventoryTexture, AssetRequestMode.ImmediateLoad).Value;
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            int frameWidth = inventoryTexture.Width / FrameCount;

            int currentFrame = (int)(Main.GameUpdateCount / TicksPerFrame) % FrameCount;

            Rectangle sourceRectangle = new(currentFrame * frameWidth, 0, frameWidth, inventoryTexture.Height);

            spriteBatch.Draw(inventoryTexture, position, sourceRectangle, drawColor, 0, origin, scale, SpriteEffects.None, 0);

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            int frameWidth = inventoryTexture.Width / FrameCount;

            int currentFrame = (int)(Main.GameUpdateCount / TicksPerFrame) % FrameCount;

            Rectangle sourceRectangle = new(currentFrame * frameWidth, 0, frameWidth, inventoryTexture.Height);

            spriteBatch.Draw(inventoryTexture, Item.position - Main.screenPosition, sourceRectangle, lightColor, 0, Vector2.Zero, scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
