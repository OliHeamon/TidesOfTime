using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using TidesOfTime.Common.UI.UniversalRemote.SelectionMenu;
using Terraria.GameContent;
using TidesOfTime.Localization;
using Terraria.ID;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks.BuilderDroneTasks
{
    public class CircleBlockPlacementTask : DroneTask
    {
        private readonly Rectangle rectangle;

        private readonly int blockType;

        private readonly int itemType;

        private readonly string itemName;

        private readonly PlacementType placementType;

        private readonly DroneTaskMode selectionMode;

        private readonly int tilesInTask;

        private int blocksPlaced;

        private int placementIndex;

        public override string TaskDescription => LocalizationHelper.GetGUIText("UniversalRemote.BuilderCirclePlacementInfo",
            tilesInTask, itemName);

        public CircleBlockPlacementTask(Rectangle rectangle, int blockType, int itemType, string itemName, PlacementType placementType, DroneTaskMode selectionMode) : base(DroneType.Builder)
        {
            this.rectangle = rectangle;
            this.blockType = blockType;
            this.itemType = itemType;
            this.itemName = itemName;
            this.placementType = placementType;
            this.selectionMode = selectionMode;

            tilesInTask = CountTilesInTask();
        }

        public override bool AI()
        {
            int area = rectangle.Width * rectangle.Height / (16 * 16);

            if (placementIndex < area)
            {
                int tileWidth = rectangle.Width / 16;

                Point startPos = new(rectangle.X / 16, rectangle.Y / 16);

                Point desiredPlacementPosition = new(startPos.X + (placementIndex % tileWidth), startPos.Y + (placementIndex / tileWidth));
                Vector2 desiredPlacementPositionWorld = desiredPlacementPosition.ToVector2() * 16;

                Rectangle tileRectangle = new(rectangle.X / 16, rectangle.Y / 16, tileWidth, rectangle.Height / 16);

                while (!TidesOfTimeUtils.IsInInscribedEllipse(desiredPlacementPosition.ToVector2(), tileRectangle))
                {
                    placementIndex++;

                    if (placementIndex >= area)
                    {
                        return false;
                    }

                    desiredPlacementPosition = new(startPos.X + (placementIndex % tileWidth), startPos.Y + (placementIndex / tileWidth));
                    desiredPlacementPositionWorld = desiredPlacementPosition.ToVector2() * 16;
                }

                if (selectionMode == DroneTaskMode.CircleOutline)
                {
                    Rectangle shrunkTileRectangle = new(tileRectangle.X + 1, tileRectangle.Y + 1, tileRectangle.Width - 2, tileRectangle.Height - 2);

                    while (TidesOfTimeUtils.IsInInscribedEllipse(desiredPlacementPosition.ToVector2(), shrunkTileRectangle))
                    {
                        placementIndex++;

                        if (placementIndex >= area)
                        {
                            return false;
                        }

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
            int blocksLeft = tilesInTask - blocksPlaced;

            if (blocksLeft > 0)
            {
                Main.player[WorkerDrone.Projectile.owner].QuickSpawnItem(Main.player[WorkerDrone.Projectile.owner].GetSource_FromThis(), itemType, blocksLeft);
            }
        }

        private int CountTilesInTask()
        {
            Rectangle tileRectangle = new(rectangle.X / 16, rectangle.Y / 16, rectangle.Width / 16, rectangle.Height / 16);

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

            if (selectionMode == DroneTaskMode.CircleOutline)
            {
                mainCount -= CountTiles(shrunkTileRectangle);
            }

            return mainCount;
        }
    }
}
