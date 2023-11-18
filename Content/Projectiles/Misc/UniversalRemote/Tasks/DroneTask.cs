using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TidesOfTime.Content.Projectiles.Misc.UniversalRemote.Tasks
{
    public abstract class DroneTask
    {
        public WorkerDrone WorkerDrone { get; set; }

        public bool HasStarted { get; set; }

        public bool IsComplete { get; protected set; }

        public float Progress { get; protected set; }

        public DroneType DroneType { get; private set; }

        public abstract string TaskDescription { get; }

        public DroneTask(DroneType droneType)
        {
            DroneType = droneType;
        }

        public abstract bool AI();

        public abstract void Draw(SpriteBatch spriteBatch, Color lightColor);

        public virtual void OnCompleted()
        {
        }

        public void Abort()
        {
            IsComplete = true;
        }
    }
}
