﻿using HarmonyLib;
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


    [StaticConstructorOnStartup]
    public static class Highstorm_StorytellerPatch {
        private static int lastHighstormTick = 0;
        private const int intervalTicks = 8 * 60000; // 1 in-game days
        private const int warningOffsetTicks = 60000; // Half a day (0.5 * 60000 ticks)


        static Highstorm_StorytellerPatch() {
            var harmony = new Harmony("com.lucidMods.HighstormPatch");
            harmony.Patch(
                original: AccessTools.Method(typeof(Storyteller), "StorytellerTick"),
                postfix: new HarmonyMethod(typeof(Highstorm_StorytellerPatch), nameof(StorytellerTick_Postfix))
            );
        }

        private static void StorytellerTick_Postfix(Storyteller __instance) {
            int currentTick = Find.TickManager.TicksGame;

            if ((currentTick % intervalTicks) == (intervalTicks - warningOffsetTicks)) {
                ShowWarning();
            }

            if ((currentTick % intervalTicks) == 0) {
                TryTriggerHighstorm();
            }

        }

        private static void TryTriggerHighstorm() {
            if (Find.CurrentMap == null) return; // Ensure a valid map exists

            IncidentParms parms = new IncidentParms {
                target = Find.CurrentMap, // Ensure the incident happens in the current map
                forced = true // Forces it to trigger even if it has a low chance
            };

            bool success = Find.Storyteller.incidentQueue.Add(DefDatabase<IncidentDef>.GetNamed("HighstormIncident"), Find.TickManager.TicksGame, parms);

            if (success) {
                Log.Message("[StormlightMod] Highstorm triggered every 8 days!");
            }
        }


        private static void ShowWarning() {
            if (Find.CurrentMap == null) return; // Ensure a valid map exists

            // Show a warning message to the player
            Find.LetterStack.ReceiveLetter(
                "Approaching Highstorm",
                "A Highstorm is approaching! Seek shelter immediately. The storm will arrive in a day.",
                LetterDefOf.NeutralEvent,
                new LookTargets(Find.CurrentMap.mapPawns.FreeColonists.RandomElement()) // Random colonist as a reference
            );
        }
    }
}
