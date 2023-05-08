using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TidesOfTime.Content.Items.Armor.Vanity.MountainClimber
{

    [AutoloadEquip(EquipType.Body)]
    public class MountainClimberJacket : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 30;
            Item.rare = ItemRarityID.Purple;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            for (int i = 0; i < tooltips.Count; i++)
            {
                if (tooltips[i].Name == "Tooltip0")
                {
                    tooltips[i].OverrideColor = Color.Pink;
                }
            }
        }
    }
}