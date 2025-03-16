using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace StormlightMod {

    static public class StormlightUtilities {

        public static bool IsAnyFireNearby(Building building, float radius = 5f) {
            IntVec3 position = building.Position;
            Map map = building.Map;

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, radius, true)) {
                foreach (Thing thing in cell.GetThingList(map)) {
                    if (thing.def == ThingDefOf.Fire || thing.def.category == ThingCategory.Building && thing.TryGetComp<CompRefuelable>()?.Props.fuelConsumptionPerTickInRain > 0f) {
                        return true;
                    }
                    if (thing.def.defName.Contains("Torch") || thing.def.defName.Contains("Campfire")) {
                        return true;
                    }
                }
            }
            return false;
        }


        static public Trait GetRadiantTrait(Pawn pawn) {
            return pawn.story.traits.allTraits.FirstOrDefault(t => StormlightModUtilities.RadiantTraits.Contains(t.def));
        }

        private static readonly List<ThingDef> gems = new List<ThingDef>
        {
        StormlightModDefs. whtwl_RawDiamond,
        StormlightModDefs. whtwl_RawGarnet,
        StormlightModDefs. whtwl_RawRuby,
        StormlightModDefs. whtwl_RawSapphire,
        StormlightModDefs. whtwl_RawEmerald
      };

        private static readonly Random rng = new Random();

        public static ThingDef RollForRandomGemSpawn() {
            foreach (var gem in gems) { 
                if (RollTheDice(0, (int)gem.GetCompProperties<CompProperties_RawGemstone>().spawnChance, 1)) {
                    return gem;   
                }
            }
            return null;
        }
        public static bool RollTheDice(int min, int max, int lowerThreshold) {
            return rng.Next(min, max) <= lowerThreshold;
        }
        public static int RollTheDice(int min, int max) {
            return rng.Next(min, max);
        }
        public static string RollForRandomString(List<string> stringList) {
            int i = rng.Next(stringList.Count);
            return stringList[i]; 
        }
        public static int RollForRandomIntFromList(List<int> intList) { 
            int i = rng.Next(intList.Count);
            return intList[i];
        }
    }

}
