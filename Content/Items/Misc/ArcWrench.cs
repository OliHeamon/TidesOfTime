using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TidesOfTime.Content.Items.Abstract;
using TidesOfTime.Content.Rarities;

namespace TidesOfTime.Content.Items.Misc
{
    public class ArcWrench : AnimatedInventoryItem
    {
        public override int FrameCount => 8;

        public override int TicksPerFrame => 5;

        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 58;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;

            Item.damage = 100;
            Item.DamageType = DamageClass.Summon;
            Item.noMelee = true;

            Item.mana = 20;

            Item.rare = ModContent.RarityType<Futuristic>();
        }
    }
}
