using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TidesOfTime.Common.UI.UniversalRemote;
using TidesOfTime.Content.Rarities;

namespace TidesOfTime.Content.Items.Misc
{
    public class UniversalRemote : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;

            Item.rare = ModContent.RarityType<Futuristic>();
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                bool visible = TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().Visible;

                TidesOfTimeUILoader.GetUIState<UniversalRemoteUI>().Visible = !visible;

                SoundEngine.PlaySound(visible ? SoundID.MenuClose : SoundID.MenuOpen);
            }

            return true;
        }
    }
}
