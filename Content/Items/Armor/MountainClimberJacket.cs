using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TidesOfTime.Content.Items.Armor
{
    
    [AutoloadEquip(EquipType.Body)]
    public class MountainClimberJacket : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38; 
            Item.height = 30; 
            Item.rare = ItemRarityID.Green; 

        }
    }
}