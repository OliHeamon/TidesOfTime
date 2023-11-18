using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using TidesOfTime.Common.UI.UniversalRemote.SelectionMenu;
using TidesOfTime.Localization;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks.BreakerDroneTasks
{
    public class RectangleBreakTask : DroneTask
    {
        private readonly Rectangle rectangle;

        private readonly DroneTaskMode selectionMode;

        private readonly bool breakWalls;

        private int brokenIndex;

        public RectangleBreakTask(Rectangle rectangle, DroneTaskMode selectionMode, bool breakWalls) : base(DroneType.Breaker)
        {
            this.rectangle = rectangle;
            this.selectionMode = selectionMode;
            this.breakWalls = breakWalls;
        }

        public override string TaskDescription => LocalizationHelper.GetGUIText("UniversalRemote.BreakRectangleInfo",
            rectangle.Width / 16, rectangle.Height / 16);

        public override bool AI()
        {
            int area = rectangle.Width * rectangle.Height / (16 * 16);

            if (brokenIndex < area)
            {
                int tileWidth = rectangle.Width / 16;

                Point startPos = new(rectangle.X / 16, rectangle.Y / 16);

                Point desiredPlacementPosition = new(startPos.X + (brokenIndex % tileWidth), startPos.Y + (brokenIndex / tileWidth));
                Vector2 desiredPlacementPositionWorld = desiredPlacementPosition.ToVector2() * 16;

                Tile tile = Main.tile[desiredPlacementPosition.X, desiredPlacementPosition.Y];

                while ((selectionMode == DroneTaskMode.VacuumLiquid) ? tile.LiquidAmount == 0 : !tile.HasTile && (breakWalls ? tile.WallType == 0 : true))
                {
                    brokenIndex++;

                    if (brokenIndex >= area)
                    {
                        return false;
                    }

                    desiredPlacementPosition = new(startPos.X + (brokenIndex % tileWidth), startPos.Y + (brokenIndex / tileWidth));
                    desiredPlacementPositionWorld = desiredPlacementPosition.ToVector2() * 16;

                    tile = Main.tile[desiredPlacementPosition.X, desiredPlacementPosition.Y];
                }

                WorkerDrone.Move(desiredPlacementPositionWorld, 1);

                if ((WorkerDrone.Projectile.Center - desiredPlacementPositionWorld).LengthSquared() < 4 * 4)
                {
                    Player owner = Main.player[WorkerDrone.Projectile.owner];

                    switch (selectionMode)
                    {
                        case DroneTaskMode.BreakRectangle:

                            if (owner.HasEnoughPickPowerToHurtTile(desiredPlacementPosition.X, desiredPlacementPosition.Y))
                            {
                                WorldGen.KillTile(desiredPlacementPosition.X, desiredPlacementPosition.Y);
                            }

                            if (breakWalls)
                            {
                                WorldGen.KillWall(desiredPlacementPosition.X, desiredPlacementPosition.Y);
                            }

                            for (int i = 0; i < 6; i++)
                            {
                                Dust.NewDust(WorkerDrone.Projectile.Center, WorkerDrone.Projectile.width, WorkerDrone.Projectile.height, DustID.FlameBurst);
                            }

                            break;
                        case DroneTaskMode.VacuumLiquid:

                            tile.LiquidAmount = 0;

                            for (int i = 0; i < 6; i++)
                            {
                                Dust.NewDust(WorkerDrone.Projectile.Center, WorkerDrone.Projectile.width, WorkerDrone.Projectile.height, DustID.Smoke);
                            }

                            break;
                        case DroneTaskMode.Clentaminate:

                            WorldGen.Convert(desiredPlacementPosition.X, desiredPlacementPosition.Y, BiomeConversionID.Purity, 0);

                            for (int i = 0; i < 6; i++)
                            {
                                Dust.NewDust(WorkerDrone.Projectile.Center, WorkerDrone.Projectile.width, WorkerDrone.Projectile.height, DustID.Clentaminator_Green);
                            }

                            break;
                    }


                    brokenIndex++;
                }

                Progress = brokenIndex / (float)area;
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
            Player owner = Main.player[WorkerDrone.Projectile.owner];

            Item pickaxe = owner.GetBestPickaxe();

            if (pickaxe == null)
            {
                return;
            }

            Texture2D itemTexture = TextureAssets.Item[pickaxe.type].Value;

            Vector2 offset = WorkerDrone.Projectile.direction == 1 ? new(12, 0) : new(-24, 0);

            SpriteEffects effects = WorkerDrone.Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(itemTexture, WorkerDrone.Projectile.Center + offset - Main.screenPosition, null, lightColor, 0, Vector2.Zero, 0.5f, effects, 0);
        }
    }
}
