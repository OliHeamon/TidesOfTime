using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TidesOfTime.Common.UI.UniversalRemote;
using TidesOfTime.Content.Items.Misc;
using TidesOfTime.Content.Projectiles.Misc.UniversalRemote;

namespace TidesOfTime.Content.Players
{
    public class WorkerDronePlayer : ModPlayer
    {
        private readonly int[] droneIds;

        public WorkerDronePlayer()
        {
            droneIds = new int[3];
        }

        public override void PostUpdateMiscEffects()
        {
            UniversalRemoteUI universalRemoteUI = TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>();

            int remoteType = ModContent.ItemType<UniversalRemote>();

            bool hasRemote = !Player.dead && (Player.HasItem(remoteType) || Main.mouseItem.type == remoteType);

            int[] projectileTypes = new[] { 
                ModContent.ProjectileType<ScannerDrone>(),
                ModContent.ProjectileType<BuilderDrone>(),
                ModContent.ProjectileType<BreakerDrone>() 
            };

            bool anyDronesActive = false;

            for (int i = 0; i < projectileTypes.Length; i++)
            {
                if (Player.ownedProjectileCounts[projectileTypes[i]] > 0)
                {
                    anyDronesActive = true;
                }
            }

            // If worker drones are currently active and no remote, despawn drones.
            // Else if no worker drones are active and player has remote, respawn drones.
            if (anyDronesActive)
            {
                if (!hasRemote)
                {
                    if (Main.myPlayer == Player.whoAmI)
                    {
                        bool visible = TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().Visible;

                        universalRemoteUI.TaskSelector.ResetToggles();
                        universalRemoteUI.TaskList.RemoveAllTasks();
                        universalRemoteUI.Visible = false;

                        if (visible)
                        {
                            SoundEngine.PlaySound(SoundID.MenuClose);
                        }
                    }

                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        for (int type = 0; type < projectileTypes.Length; type++)
                        {
                            if (Main.projectile[i].type == projectileTypes[type])
                            {
                                Main.projectile[i].Kill();
                            }
                        }
                    }
                }

                // Scanner drone passive.
                Player.accThirdEye = true;
                Player.accOreFinder = true;
                Player.accCritterGuide = true;
            }
            else
            {
                if (hasRemote && Main.myPlayer == Player.whoAmI)
                {
                    for (int type = 0; type < projectileTypes.Length; type++)
                    {
                        droneIds[type] = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, projectileTypes[type], 0, 0, Player.whoAmI, type);
                    }
                }
            }
        }

        public override void LoadData(TagCompound tag) => TidesOfTimeUILoader.GetUIState<SelectionUI>().LoadSlotData(tag);

        public override void SaveData(TagCompound tag) => TidesOfTimeUILoader.GetUIState<SelectionUI>().SaveSlotData(tag);

        public T GetWorkerDrone<T>() where T : WorkerDrone 
        {
            int type = -1;

            if (typeof(T) == typeof(ScannerDrone))
            {
                type = 0;
            }
            else if (typeof(T) == typeof(BuilderDrone))
            {
                type = 1;
            }
            else if (typeof(T) == typeof(BreakerDrone))
            {
                type = 2;
            }

            return Main.projectile[droneIds[type]].ModProjectile as T;
        } 
    }
}
