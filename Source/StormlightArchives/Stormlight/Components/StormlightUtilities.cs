using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Noise;
using UnityEngine;

namespace StormlightMod {

    static public class StormlightUtilities {

        public static int GetGraphicId(Thing thing) {
            if (thing == null) return 0;
            CompShardblade comp = thing.TryGetComp<CompShardblade>();
            if(comp == null) return 0;
            return comp.graphicId;
        }

        public static float Normalize(float value, float v_min, float v_max, float t_min, float t_max) {
            return ((value - v_min) / (v_max - v_min)) * (t_max - t_min) + t_min;
        }

        public static float SprenBaseCaptureProbability(float currentStormlight, float minStormlight, float maxStormlight) {
            float x = Normalize(currentStormlight, minStormlight, maxStormlight, 0f, 100f);
            const float a = 1.1585f;
            const float b = 1690.6f;
            const float c = 100f;
            const float d = 1f;

            return -(a / b) * x * (x - c) * (x - d);
        }

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
        public static int GetNumberOfFiresNearby(Building building, float radius = 5f) {
            IntVec3 position = building.Position;
            Map map = building.Map;
            int numberOfFires = 0;
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, radius, true)) {
                foreach (Thing thing in cell.GetThingList(map)) {
                    if (thing.def == ThingDefOf.Fire || thing.def.category == ThingCategory.Building && thing.TryGetComp<CompRefuelable>()?.Props.fuelConsumptionPerTickInRain > 0f) {
                        numberOfFires++;
                    }
                    else if (thing.def.defName.Contains("Torch") || thing.def.defName.Contains("Campfire")) {
                        numberOfFires++;
                    }
                }
            }
            return numberOfFires;
        }
        public static float GetAverageSuroundingTemperature(Building building, float radius = 5f) {
            IntVec3 position = building.Position;
            Map map = building.Map;
            List<float> temps = new List<float>();
            var cells = GenRadial.RadialCellsAround(position, radius, true);
            foreach (IntVec3 cell in cells) {
                temps.Add(cell.GetTemperature(building.Map));
            }
            return temps.Average();
        }

        public static float GetSuroundingPain(Building building, float radius = 5f) {
            IntVec3 position = building.Position;
            Map map = building.Map;
            List<float> pains = new List<float>();
            var cells = GenRadial.RadialCellsAround(position, radius, true);
            foreach (IntVec3 cell in cells) {
                Pawn pawn = cell.GetFirstPawn(map);
                if (pawn != null) {
                    pains.Add(pawn.health.hediffSet.PainTotal);
                }
            }
            return pains.Sum();
        }

        public static float GetSuroundingPlants(Building building, float radius = 5f) {
            IntVec3 position = building.Position;
            Map map = building.Map;
            int plants = 0;
            var cells = GenRadial.RadialCellsAround(position, radius, true);
            foreach (IntVec3 cell in cells) {
                if (cell.InBounds(map)) {
                    foreach (Thing thing in cell.GetThingList(map)) {
                        if (thing is Plant plant) {
                            plants++;
                        }
                    }
                }
            }
            return plants;
        }
        public static bool ResearchBeingDoneNearby(Building building, float radius = 5f) {
            IntVec3 position = building.Position;
            Map map = building.Map;
            var cells = GenRadial.RadialCellsAround(position, radius, true);
            foreach (IntVec3 cell in cells) {
                Pawn pawn = cell.GetFirstPawn(map);
                if (pawn != null && pawn.jobs?.curJob != null && pawn.jobs.curJob.def == JobDefOf.Research) {
                    return true;
                }
            }
            return false;
        }

        public static bool isThingCutGemstone(Thing thing) {
            return thing.def == StormlightModDefs.whtwl_CutRuby
                         || thing.def == StormlightModDefs.whtwl_CutEmerald
                         || thing.def == StormlightModDefs.whtwl_CutDiamond
                         || thing.def == StormlightModDefs.whtwl_CutSapphire
                         || thing.def == StormlightModDefs.whtwl_CutGarnet;
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

        private static readonly System.Random rng = new System.Random();

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


        public static void SetThingGraphic(Thing thing, string texPath, float drawSize = 1.5f) {
            if (thing == null || thing.def == null) return;

            Graphic newGraphic = GraphicDatabase.Get<Graphic_Single>(
                texPath,
                ShaderDatabase.Cutout,
                new Vector2(drawSize, drawSize),
                Color.white
            );

            // Set internal field directly
            AccessTools.Field(typeof(Thing), "graphicInt").SetValue(thing, newGraphic);

            // Force refresh
            thing.Notify_ColorChanged();
        }
    }

}
