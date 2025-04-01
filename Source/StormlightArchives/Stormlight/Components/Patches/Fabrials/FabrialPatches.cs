using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StormlightMod {
    [HarmonyPatch(typeof(Plant), "get_GrowthRate")]
    public static class CultivationSprenPatch {
        private static List<Building_Fabrial_Basic_Augmenter> activeLifeSprenBuildings = new List<Building_Fabrial_Basic_Augmenter>();


        public static void RegisterBuilding(Building_Fabrial_Basic_Augmenter building) {
            if (building.GetComp<CompBasicFabrialAugumenter>()?.CurrentSpren == Spren.Life) {
                activeLifeSprenBuildings.Add(building);
            }
        }

        public static void UnregisterBuilding(Building_Fabrial_Basic_Augmenter building) {
            activeLifeSprenBuildings.Remove(building);
        }

        public static void Postfix(Plant __instance, ref float __result) {
            if (__instance.Spawned && IsNearLifeSprenBuilding(__instance)) {
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
        private static bool IsNearLifeSprenBuilding(Plant plant) {
            var map = plant.Map;
            if (map == null) return false;
            var plantPos = plant.Position;

            foreach (var thing in activeLifeSprenBuildings) { 
                if (thing is Building_Fabrial_Basic_Augmenter building &&
                    plantPos.DistanceTo(building.Position) <= 5f) {
                    var comp = building.GetComp<CompBasicFabrialAugumenter>();
                    if (comp != null && comp.PowerOn && comp.CurrentSpren == Spren.Life) {
                        return true;
                    }
                }
            }
            return false;
        }
        //private static bool IsNearLifeSprenBuilding(Plant plant) {
        //    var map = plant.Map;
        //    var position = plant.Position;
        //    if (map == null) return false;
        //    foreach (var cell in GenRadial.RadialCellsAround(position, 5f, true)) {
        //        if(cell == null || cell.OnEdge(map) || !cell.InBounds(map)) continue;
        //        foreach (var thing in cell.GetThingList(map)) {
        //            if (thing.def == StormlightModDefs.whtwl_BasicFabrial_Augmenter &&
        //                thing is Building_Fabrial_Basic_Augmenter building
        //               ) {
        //                CompBasicFabrialAugumenter comp = building.GetComp<CompBasicFabrialAugumenter>();
        //                if (comp != null && comp.PowerOn == true && comp.CurrentSpren == Spren.Life) {
        //                    return true;
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}
    }
}


