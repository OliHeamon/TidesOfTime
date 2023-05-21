using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TidesOfTime.Content.Rarities;

namespace TidesOfTime.Content.Items.Ranged
{
    public class SteamThrower : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 76;
            Item.height = 48;

            Item.useTime = 4;
            Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            //Item.UseSound = SoundID.DD2_DefenseTowerSpawn;

            Item.damage = 30;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;

            Item.rare = ModContent.RarityType<Steampunk>();

            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 10;

            Item.value = Item.sellPrice(gold: 5);
        }
    }
}
