using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using TidesOfTime.Common.UI.UniversalRemote.SelectionMenu;
using TidesOfTime.Localization;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks.BuilderDroneTasks
{
    public class RectangleBlockPlacementTask : DroneTask
    {
        private readonly Rectangle rectangle;

        private readonly int blockType;

        private readonly int itemType;

        private readonly string itemName;

        private readonly PlacementType placementType;

        private readonly DroneTaskMode selectionMode;

        private int blocksPlaced;

        private int placementIndex;

        public RectangleBlockPlacementTask(Rectangle rectangle, int blockType, int itemType, string itemName, PlacementType placementType, DroneTaskMode selectionMode) : base(DroneType.Builder)
        {
            this.rectangle = rectangle;
            this.blockType = blockType;
            this.itemType = itemType;
            this.itemName = itemName;
            this.placementType = placementType;
            this.selectionMode = selectionMode;
        }

        public override string TaskDescription => LocalizationHelper.GetGUIText("UniversalRemote.BuilderRectanglePlacementInfo",
            CountTilesInTask(), itemName);

        public override bool AI()
        {
            int area = rectangle.Width * rectangle.Height / (16 * 16);

            if (placementIndex < area)
            {
                int tileWidth = rectangle.Width / 16;

                Point startPos = new(rectangle.X / 16, rectangle.Y / 16);

                Point desiredPlacementPosition = new(startPos.X + (placementIndex % tileWidth), startPos.Y + (placementIndex / tileWidth));
                Vector2 desiredPlacementPositionWorld = desiredPlacementPosition.ToVector2() * 16;

                if (selectionMode == DroneTaskMode.RectangleOutline)
                {
                    Rectangle shrunkRectangle = new(rectangle.X + 16, rectangle.Y + 16, rectangle.Width - 32, rectangle.Height - 32);

                    while (shrunkRectangle.Contains(desiredPlacementPositionWorld.ToPoint()))
                    {
                        placementIndex++;

                        desiredPlacementPosition = new(startPos.X + (placementIndex % tileWidth), startPos.Y + (placementIndex / tileWidth));
                        desiredPlacementPositionWorld = desiredPlacementPosition.ToVector2() * 16;
                    }
                }

                WorkerDrone.Move(desiredPlacementPositionWorld, 1);

                if ((WorkerDrone.Projectile.Center - desiredPlacementPositionWorld).LengthSquared() < 4 * 4)
                {
                    if (placementType == PlacementType.Tile)
                    {
                        Player owner = Main.player[WorkerDrone.Projectile.owner];

                        Tile tile = Main.tile[desiredPlacementPosition.X, desiredPlacementPosition.Y];

                        if (!tile.HasTile)
                        {
                            WorldGen.PlaceTile(desiredPlacementPosition.X, desiredPlacementPosition.Y, (ushort)blockType);
                        }
                        else if (!owner.HasEnoughPickPowerToHurtTile(desiredPlacementPosition.X, desiredPlacementPosition.Y))
                        {
                            placementIndex++;

                            return false;
                        }
                        else if (blockType != tile.TileType && WorldGen.WouldTileReplacementWork((ushort)blockType, desiredPlacementPosition.X, desiredPlacementPosition.Y))
                        {
                            WorldGen.ReplaceTile(desiredPlacementPosition.X, desiredPlacementPosition.Y, (ushort)blockType, 0);
                        }
                        else
                        {
                            placementIndex++;

                            return false;
                        }
                    }
                    else
                    {
                        Tile tile = Main.tile[desiredPlacementPosition.X, desiredPlacementPosition.Y];

                        if (tile.WallType == WallID.None)
                        {
                            WorldGen.PlaceWall(desiredPlacementPosition.X, desiredPlacementPosition.Y, blockType);
                        }
                        else if (blockType != tile.WallType)
                        {
                            WorldGen.ReplaceWall(desiredPlacementPosition.X, desiredPlacementPosition.Y, (ushort)blockType);
                        }
                        else
                        {
                            placementIndex++;

                            return false;
                        }
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        Dust.NewDust(WorkerDrone.Projectile.Center, WorkerDrone.Projectile.width, WorkerDrone.Projectile.height, DustID.MartianSaucerSpark);
                    }

                    placementIndex++;
                    blocksPlaced++;
                }

                Progress = placementIndex / (float)area;
            }
            else
            {
                Progress = 1;
                IsComplete = true;
            }

            return false;
        }

        public override void Draw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D itemTexture = TextureAssets.Item[itemType].Value;

            Vector2 offset = WorkerDrone.Projectile.direction == 1 ? new(12, 0) : new(-24, 0);

            spriteBatch.Draw(itemTexture, WorkerDrone.Projectile.Center + offset - Main.screenPosition, lightColor);
        }

        public override void OnCompleted()
        {
            int blocksLeft = CountTilesInTask() - blocksPlaced;

            if (blocksLeft > 0)
            {
                Main.player[WorkerDrone.Projectile.owner].QuickSpawnItem(Main.player[WorkerDrone.Projectile.owner].GetSource_FromThis(), itemType, blocksLeft);
            }
        }

        private int CountTilesInTask()
        {
            int xLength = rectangle.Width / 16;
            int yLength = rectangle.Height / 16;

            int area = xLength * yLength;

            int smallerArea = 0;

            if (selectionMode == DroneTaskMode.RectangleOutline && xLength > 1 && yLength > 1)
            {
                smallerArea = (xLength - 2) * (yLength - 2);
            }

            return area - smallerArea;
        }
    }
}
