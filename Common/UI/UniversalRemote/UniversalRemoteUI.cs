using Terraria.UI;
using TidesOfTime.Common.UI.Abstract;
using System.Collections.Generic;
using TidesOfTime.Common.UI.UniversalRemote.TaskList;
using TidesOfTime.Common.UI.UniversalRemote.TaskSelector;
using Microsoft.Xna.Framework;

namespace TidesOfTime.Common.UI.UniversalRemote
{
    public class UniversalRemoteUI : SmartUIState
    {
        public TaskListMenu TaskList { get; private set; }

        public TaskSelectorMenu TaskSelector { get; private set; }

        public override int InsertionIndex(List<GameInterfaceLayer> layers)
        {
            return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        }

        public override void OnInitialize()
        {
            TaskList = new TaskListMenu(UserInterface);
            TaskList.Width.Set(384, 0);
            TaskList.Height.Set(640, 0);

            TaskSelector = new TaskSelectorMenu();
            TaskSelector.Width.Set(144, 0);
            TaskSelector.Height.Set(224, 0);

            Append(TaskList);
            Append(TaskSelector);
        }

        public override void SafeUpdate(GameTime gameTime)
        {
            TaskSelector.Left.Set(TaskList.Left.Pixels - TaskSelector.Width.Pixels - 16, 0);
            TaskSelector.Top.Set(TaskList.Top.Pixels, 0);
            TaskSelector.Recalculate();
        }
    }
}
