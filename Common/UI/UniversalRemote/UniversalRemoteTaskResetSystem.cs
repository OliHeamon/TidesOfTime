using Terraria.ModLoader;

namespace TidesOfTime.Common.UI.UniversalRemote
{
    public class UniversalRemoteTaskResetSystem : ModSystem
    {
        public override void PreSaveAndQuit()
        {
            TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().TaskList.RemoveAllTasks();
        }
    }
}
