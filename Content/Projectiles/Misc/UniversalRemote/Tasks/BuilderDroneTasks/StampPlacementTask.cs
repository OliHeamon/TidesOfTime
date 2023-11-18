using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;
using Terraria;
using TidesOfTime.Localization;
using Microsoft.Xna.Framework;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks.BuilderDroneTasks
{
    public class StampPlacementTask : DroneTask
    {
        private readonly Vector2 origin;

        private readonly int blockType;

        private readonly int itemType;

        private readonly string itemName;

        private readonly PlacementType placementType;

        private readonly string stamp;

        private readonly int tilesInTask;

        private int blocksPlaced;

        private int placementIndex;

        public StampPlacementTask(Vector2 origin, int blockType, int itemType, string itemName, PlacementType placementType, string stamp) : base(DroneType.Builder)
        {
            this.origin = origin / 16;
            this.blockType = blockType;
            this.itemType = itemType;
            this.itemName = itemName;
            this.placementType = placementType;
            this.stamp = stamp;

            tilesInTask = CountTilesInTask();
        }

        public override string TaskDescription => LocalizationHelper.GetGUIText("UniversalRemote.BuilderStampPlacementInfo",
            tilesInTask, itemName);

        public override bool AI()
        {
            int area = 256;

            if (placementIndex < area)
            {
                Point desiredPlacementPosition = new((int)origin.X + (placementIndex % 16), (int)origin.Y + (placementIndex / 16));
                Vector2 desiredPlacementPositionWorld = desiredPlacementPosition.ToVector2() * 16;

                while (stamp[placementIndex] != '1')
                {
                    placementIndex++;

                    if (placementIndex >= area)
                    {
                        return false;
                    }

                    desiredPlacementPosition = new((int)origin.X + (placementIndex % 16), (int)origin.Y + (placementIndex / 16));
                    desiredPlacementPositionWorld = desiredPlacementPosition.ToVector2() * 16;
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
            int count = 0;

            for (int i = 0; i < stamp.Length; i++)
            {
                if (stamp[i] == '1')
                {
                    count++;
                }
            }

            return count;
        }
    }
}
