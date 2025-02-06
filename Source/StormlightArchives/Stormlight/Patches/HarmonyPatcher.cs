using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace StormlightMod {
    [StaticConstructorOnStartup]
    public static class HarmonyPatcher {
        static HarmonyPatcher() {
            var harmony = new Harmony("com.lucidMods.stormlightArchives");
            harmony.PatchAll();
        }

        //for debugging purposes
        [HarmonyPatch(typeof(ThingWithComps), "PostMake")]
        public static class Patch_SpherePouch_SpawnWithSpheres {
            public static void Postfix(ThingWithComps __instance) {
                if (__instance.TryGetComp<CompSpherePouch>() is CompSpherePouch pouchComp) {
                    Log.Message("[StormlightMod] Debug: Populating newly spawned Sphere Pouch with spheres.");
                    for (int i = 0; i < 5; i++) { // Adjust number of spheres as needed
                        Thing sphere = ThingMaker.MakeThing(ThingDef.Named("Sphere_Garnet"));
                        Thing sphere2 = ThingMaker.MakeThing(ThingDef.Named("Sphere_Diamond"));
                        sphere.stackCount = 1;
                        pouchComp.storedSpheres.Add(sphere);
                        pouchComp.storedSpheres.Add(sphere2);
                    }
                }
            }
        }

    }
}
