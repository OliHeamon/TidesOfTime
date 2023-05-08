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

            Projectile.width = 24;
            Projectile.height = 24;

            Projectile.penetrate = -1;

            Projectile.DamageType = DamageClass.Summon;

            Projectile.friendly = true;
        }
    }
}
