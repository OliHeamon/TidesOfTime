using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TidesOfTime.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Legs)]
    public class MountainClimberLeggings : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26; // Width of the item
            Item.height = 24; // Height of the item
            Item.rare = ItemRarityID.Green; // The rarity of the item
        }
    }
}