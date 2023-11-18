using Microsoft.Xna.Framework;
using Terraria.UI;

namespace TidesOfTime.Common.UI.UniversalRemote.TaskSelector.TaskButtons
{
    public class SelectionMenuButton : TaskButton
    {
        public SelectionMenuButton(string icon, Color color) : base(icon, color)
        {
        }

        public override void SafeClick(UIMouseEvent evt)
        {
            TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().Visible = false;
            TidesOfTimeUILoader.GetUIState<SelectionUI>().Visible = true;

            base.SafeClick(evt);
        }
    }
}
