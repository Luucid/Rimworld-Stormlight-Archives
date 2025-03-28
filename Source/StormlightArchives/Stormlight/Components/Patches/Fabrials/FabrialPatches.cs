using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StormlightMod {
    [HarmonyPatch(typeof(Plant), "get_GrowthRate")]
    public static class CultivationSprenPatch {

        public static void Postfix(Plant __instance, ref float __result) {
            if (__instance.Spawned && IsNearCultivationSprenBuilding(__instance)) {
                bool resting = true;
                if (!(GenLocalDate.DayPercent(__instance) < 0.25f)) {
                    resting = GenLocalDate.DayPercent(__instance) > 0.8f;
                }
                if (__instance.LifeStage != PlantLifeStage.Growing || resting) {
                    __result *= 1f;
                }
                else {
                    __result *= 1.75f;
                }
            }
        }

        private static bool IsNearCultivationSprenBuilding(Plant plant) {
            var map = plant.Map;
            var position = plant.Position;

            foreach (var cell in GenRadial.RadialCellsAround(position, 5f, true)) {
                foreach (var thing in cell.GetThingList(map)) {
                    if (thing.def == StormlightModDefs.whtwl_BasicFabrial_Augmenter &&
                        thing is Building_Fabrial_Basic_Augmenter building
                       ) {
                        CompBasicFabrialAugumenter comp = building.GetComp<CompBasicFabrialAugumenter>();
                        if (comp != null && comp.PowerOn == true && comp.CurrentSpren == Spren.Cultivation) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}


