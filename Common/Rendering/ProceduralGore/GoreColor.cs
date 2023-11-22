using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace TidesOfTime.Common.Rendering.ProceduralGore
{
    public class GoreColor
    {
        private static readonly Dictionary<int, Vector3> colors;

        private static readonly List<int> skeletons;

        private static readonly List<int> none;

        static GoreColor()
        {
            colors = new Dictionary<int, Vector3>();
            skeletons = new List<int>();
            none = new List<int>();

            PopulateLists();
        }

        public static Vector3 GetGoreColor(int id) => colors.TryGetValue(id, out var color) ? color : AddColor(id);

        private static Vector3 AddColor(int id)
        {
            Color color = Color.Red;

            if (skeletons.Contains(id)) 
            {
                color = new(71, 71, 71);
            }
            else if (none.Contains(id))
            {
                color = Color.Transparent;
            }
            else
            {
                foreach (IBestiaryInfoElement element in Main.BestiaryDB.FindEntryByNPCID(id).Info)
                {
                    if (element is SpawnConditionBestiaryInfoElement spawnCondition)
                    {
                        string key = spawnCondition.GetDisplayNameKey();

                        if (key.Contains("Crimson"))
                        {
                            color = new(218, 168, 27);
                            break;
                        }
                        else if (key.Contains("Corrupt"))
                        {
                            color = new(44, 89, 54);
                            break;
                        }
                        else if (key.Contains("Martian"))
                        {
                            color = new(251, 25, 255);
                            break;
                        }
                    }
                }
            }

            Vector3 colorVector = color.ToVector3();

            colors[id] = colorVector;

            return colorVector;
        }

        private static void PopulateLists()
        {
            skeletons.AddRange(new int[]
            {
                NPCID.Skeleton,
                NPCID.CursedSkull,
                NPCID.SkeletronHead,
                NPCID.SkeletronHand,
                NPCID.BoneSerpentHead,
                NPCID.BoneSerpentBody,
                NPCID.BoneSerpentTail,
                NPCID.UndeadMiner,
                NPCID.Tim,
                NPCID.DoctorBones,
                NPCID.DungeonGuardian,
                NPCID.SkeletonArcher,
                NPCID.UndeadViking,
                NPCID.RustyArmoredBonesAxe,
                NPCID.RustyArmoredBonesFlail,
                NPCID.RustyArmoredBonesSword,
                NPCID.RustyArmoredBonesSwordNoArmor,
                NPCID.BlueArmoredBones,
                NPCID.BlueArmoredBonesMace,
                NPCID.BlueArmoredBonesNoPants,
                NPCID.BlueArmoredBonesSword,
                NPCID.HellArmoredBones,
                NPCID.HellArmoredBonesSpikeShield,
                NPCID.HellArmoredBonesMace,
                NPCID.HellArmoredBonesSword,
                NPCID.RaggedCaster,
                NPCID.RaggedCasterOpenCoat,
                NPCID.Necromancer,
                NPCID.NecromancerArmored,
                NPCID.DiabolistRed,
                NPCID.DiabolistWhite,
                NPCID.BoneLee,
                NPCID.GiantCursedSkull,
                NPCID.SkeletonSniper,
                NPCID.TacticalSkeleton,
                NPCID.SkeletonCommando,
                NPCID.AngryBonesBig,
                NPCID.AngryBonesBigMuscle,
                NPCID.AngryBonesBigHelmet,
                NPCID.SkeletonTopHat,
                NPCID.SkeletonAstonaut,
                NPCID.SkeletonAlien,
                NPCID.BoneThrowingSkeleton,
                NPCID.BoneThrowingSkeleton2,
                NPCID.BoneThrowingSkeleton3,
                NPCID.BoneThrowingSkeleton4,
                NPCID.SkeletonMerchant,
                NPCID.GreekSkeleton,
                NPCID.RuneWizard
            });

            none.AddRange(new int[] 
            {
                NPCID.MeteorHead,
                NPCID.BurningSphere,
                NPCID.ChaosBall,
                NPCID.WaterSphere,
                NPCID.BlazingWheel,
                NPCID.CursedHammer,
                NPCID.EnchantedSword,
                NPCID.Mimic,
                NPCID.VileSpit,
                NPCID.PossessedArmor,
                NPCID.CrimsonAxe,
                NPCID.AngryNimbus,
                NPCID.Reaper,
                NPCID.Spore,
                NPCID.DungeonSpirit,
                NPCID.Ghost,
                NPCID.MourningWood,
                NPCID.Splinterling,
                NPCID.Pumpking,
                NPCID.PumpkingBlade,
                NPCID.Poltergeist,
                NPCID.Everscream,
                NPCID.Flocko,
                NPCID.ForceBubble,
                NPCID.CultistDragonHead,
                NPCID.CultistDragonBody1,
                NPCID.CultistDragonBody2,
                NPCID.CultistDragonBody3,
                NPCID.CultistDragonBody4,
                NPCID.CultistDragonTail,
                NPCID.ShadowFlameApparition,
                NPCID.GraniteFlyer,
                NPCID.GraniteGolem,
                NPCID.TargetDummy,
                NPCID.PirateShip,
                NPCID.PirateShipCannon,
                NPCID.SolarGoop,
                NPCID.AncientCultistSquidhead,
                NPCID.AncientLight,
                NPCID.AncientDoom,
                NPCID.TorchGod,
                NPCID.ChaosBallTim,
                NPCID.VileSpitEaterOfWorlds,
                NPCID.SkeletronPrime,
                NPCID.PrimeCannon,
                NPCID.PrimeSaw,
                NPCID.PrimeVice,
                NPCID.PrimeLaser,
                NPCID.TheDestroyer,
                NPCID.TheDestroyerBody,
                NPCID.TheDestroyerTail,
                NPCID.Probe,
                NPCID.Golem,
                NPCID.GolemHead,
                NPCID.GolemFistLeft,
                NPCID.GolemFistRight,
                NPCID.GolemHeadFree,
                NPCID.PresentMimic,
                NPCID.SantaNK1,
                NPCID.ElfCopter,
                NPCID.MartianTurret,
                NPCID.MartianDrone,
                NPCID.ChatteringTeethBomb,
                NPCID.GigaZapper,
                NPCID.MartianSaucer,
                NPCID.MartianSaucerTurret,
                NPCID.MartianSaucerCannon,
                NPCID.MartianSaucerCore,
                NPCID.MartianProbe,
                NPCID.DeadlySphere
            });
        }
    }
}
