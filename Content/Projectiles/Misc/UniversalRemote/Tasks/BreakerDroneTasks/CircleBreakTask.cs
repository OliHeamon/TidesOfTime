using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria;
using TidesOfTime.Localization;
using TidesOfTime.Common.UI.UniversalRemote.SelectionMenu;
using Terraria.ID;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks.BreakerDroneTasks
{
    public class CircleBreakTask : DroneTask
    {
        private readonly Rectangle rectangle;

        private readonly DroneTaskMode selectionMode;

        private readonly bool breakWalls;

        private int brokenIndex;

        public CircleBreakTask(Rectangle rectangle, DroneTaskMode selectionMode, bool breakWalls) : base(DroneType.Breaker)
        {
            this.rectangle = rectangle;
            this.selectionMode = selectionMode;
            this.breakWalls = breakWalls;
        }

        public override string TaskDescription => LocalizationHelper.GetGUIText("UniversalRemote.BreakCircleInfo",
            (rectangle.Width / 16) * (rectangle.Height / 16));

        public override bool AI()
        {
            int area = rectangle.Width * rectangle.Height / (16 * 16);

            if (brokenIndex < area)
            {
                int tileWidth = rectangle.Width / 16;

                Point startPos = new(rectangle.X / 16, rectangle.Y / 16);

                Point desiredPlacementPosition = new(startPos.X + (brokenIndex % tileWidth), startPos.Y + (brokenIndex / tileWidth));
                Vector2 desiredPlacementPositionWorld = desiredPlacementPosition.ToVector2() * 16;

                Rectangle tileRectangle = new(rectangle.X / 16, rectangle.Y / 16, tileWidth, rectangle.Height / 16);

                while (!TidesOfTimeUtils.IsInInscribedEllipse(desiredPlacementPosition.ToVector2(), tileRectangle))
                {
                    brokenIndex++;

                    if (brokenIndex >= area)
                    {
                        return false;
                    }

                    desiredPlacementPosition = new(startPos.X + (brokenIndex % tileWidth), startPos.Y + (brokenIndex / tileWidth));
                    desiredPlacementPositionWorld = desiredPlacementPosition.ToVector2() * 16;
                }

                if (selectionMode == DroneTaskMode.CircleOutline)
                {
                    Rectangle shrunkTileRectangle = new(tileRectangle.X + 1, tileRectangle.Y + 1, tileRectangle.Width - 2, tileRectangle.Height - 2);

                    while (TidesOfTimeUtils.IsInInscribedEllipse(desiredPlacementPosition.ToVector2(), shrunkTileRectangle))
                    {
                        brokenIndex++;

                        if (brokenIndex >= area)
                        {
                            return false;
                        }

                        desiredPlacementPosition = new(startPos.X + (brokenIndex % tileWidth), startPos.Y + (brokenIndex / tileWidth));
                        desiredPlacementPositionWorld = desiredPlacementPosition.ToVector2() * 16;
                    }
                }

                WorkerDrone.Move(desiredPlacementPositionWorld, 1);

                if ((WorkerDrone.Projectile.Center - desiredPlacementPositionWorld).LengthSquared() < 4 * 4)
                {
                    Player owner = Main.player[WorkerDrone.Projectile.owner];


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
