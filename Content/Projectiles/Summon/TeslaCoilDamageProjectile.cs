using Terraria;
using Terraria.ModLoader;

namespace TidesOfTime.Content.Projectiles.Summon
{
    public class TeslaCoilDamageProjectile : ModProjectile
    {
        public override string Texture => "TidesOfTime/Assets/Invisible";

        public override void SetDefaults()
        {
            Projectile.tileCollide = false;

            Projectile.timeLeft = 2;

            Projectile.width = 32;
            Projectile.height = 32;

            Projectile.penetrate = -1;

            Projectile.DamageType = DamageClass.Summon;

            Projectile.friendly = true;
        }
    }
}
