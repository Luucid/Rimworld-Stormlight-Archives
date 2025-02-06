using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
namespace StormlightMod {
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Tick")]
    public static class Pawn_HighstormPushPatch {
        static void Postfix(Pawn __instance) {
            if (Find.TickManager.TicksGame % 20 != 0) return;

            if (!IsPawnValidForStorm(__instance))
                return;

            if (!IsHighstormActive(__instance.Map))
                return;

            damageAndMovePawn(__instance);
        }

        private static bool IsHighstormActive(Map map) {
            // Checks if our custom GameCondition is present
            var condition = map.gameConditionManager.GetActiveCondition<GameCondition_Highstorm>();
            return (condition != null);
        }
        private static bool IsPawnValidForStorm(Pawn pawn) {
            if (pawn == null)
                return false;
            if (pawn.Dead || pawn.Destroyed || !pawn.Spawned)
                return false;
            if (pawn.Map == null)
                return false;
            if (!pawn.Position.IsValid)
                return false;
            if (!pawn.Position.InBounds(pawn.Map))
                return false;
            if (pawn.Position.Roofed(pawn.Map))
                return false;

            return true;
        }

        private static bool checkIfSheltered(Pawn pawn) {
            for (int i = 1; i <= 3; i++) {
                IntVec3 shelterPos = pawn.Position + (IntVec3.East * i);
                if (shelterPos.IsValid == false) {
                    return false;
                }
                Building blockingBuilding = shelterPos.GetEdifice(pawn.Map);
                if (blockingBuilding != null && blockingBuilding.def != null) {
                    return true;
                }
            }
            return false;
        }



        private static void damageAndMovePawn(Pawn __instance) {
            // Storm always blows from east → pushes pawns west
            IntVec3 newPos = __instance.Position + IntVec3.West;

            // Check new position validity
            if (!newPos.IsValid || !newPos.InBounds(__instance.Map) || !newPos.Walkable(__instance.Map))
                return;

            if (checkIfSheltered(__instance))
                return;

            bool isRadiant = false;
            if (__instance.story != null && __instance.RaceProps.Humanlike) {
                foreach (Trait t in __instance.story.traits.allTraits) {
                    if (t.def.defName.StartsWith("Radiant")) { isRadiant = true; }
                }
            }

            __instance.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 1));
            if (isRadiant == false) {

                __instance.Position = newPos;
            }
            else {
                var stormlightComp = __instance.TryGetComp<CompStormlight>(); 
                if (stormlightComp != null) {
                    stormlightComp.infuseStormlight(25f); // 25 units per check
                }
            }
        }
    }
}

