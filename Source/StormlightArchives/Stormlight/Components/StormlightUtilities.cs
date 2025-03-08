using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace StormlightMod {

    static public class StormlightUtilities {

        static public Trait GetRadiantTrait(Pawn pawn) {
            return pawn.story.traits.allTraits.FirstOrDefault(t => StormlightModUtilities.RadiantTraits.Contains(t.def));

        }
        private static readonly List<ThingDef> spheres = new List<ThingDef>
        {
        StormlightModDefs.whtwl_Sphere_Emerald,
        StormlightModDefs.whtwl_Sphere_Sapphire,
        StormlightModDefs.whtwl_Sphere_Ruby,
        StormlightModDefs.whtwl_Sphere_Garnet,
        StormlightModDefs.whtwl_Sphere_Diamond
      };

        private static readonly Random rng = new Random();

        public static ThingDef RollForRandomSphereSpawn() {
            bool gotSphere = false;
            foreach (var sphere in spheres) {
                if (RollTheDice(0, (int)sphere.GetCompProperties<CompProperties_Stormlight>().maxStormlight, 1)) {
                    return sphere; 
                }
            }
            return null;
        }
        public static bool RollTheDice(int min, int max, int lowerThreshold) {
            return rng.Next(min, max) <= lowerThreshold;
        }
    }

}
