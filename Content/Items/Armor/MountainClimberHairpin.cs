using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TidesOfTime.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class MountainClimberHairpin : ModItem
    {
        public override void SetStaticDefaults()
        {
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 22; 
            Item.height = 22; 
            Item.rare = ItemRarityID.Green; 
        }
    }
}