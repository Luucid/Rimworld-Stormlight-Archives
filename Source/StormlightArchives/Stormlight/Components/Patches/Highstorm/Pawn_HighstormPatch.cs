using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using Verse.Noise;

namespace StormlightMod {
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Tick")]
    public static class Pawn_HighstormPushPatch {
        private static Random m_Rand = new Random();
        static void Postfix(Pawn __instance) {
            if (Find.TickManager.TicksGame % 20 != 0) return;

            if (!IsPawnValidForStorm(__instance))
                return;

            if (IsHighstormActive(__instance.Map)) {

                damageAndMovePawn(__instance);
                if (Find.TickManager.TicksGame % 100 == 0) {  
                    tryToBondPawn(__instance, StormlightModDefs.whtwl_Radiant_Windrunner);
                }
                return;
            }
            if (__instance.Map.weatherManager.curWeather.defName == "Fog") {
                if (Find.TickManager.TicksGame % 100 == 0) {  
                    tryToBondPawn(__instance, StormlightModDefs.whtwl_Radiant_Truthwatcher);
                }
            }
        }

        private static void tryToBondPawn(Pawn pawn, TraitDef traitDef) {

            if (pawn != null && pawn.RaceProps.Humanlike) {
                if (StormlightUtilities.GetRadiantTrait(pawn) != null) return;

                PawnStats pawnStats = pawn.GetComp<PawnStats>();
                float crisisValue = pawnStats.GetRequirements(traitDef, pawnStats.Props.Req_0_1).Value;
                int upperNumber = 10000 - (int)crisisValue;
                if (upperNumber <= 1) upperNumber = 2;
                int number = m_Rand.Next(1, upperNumber);
                if (number == 1) {
                    Find.WindowStack.Add(new Dialog_MessageBox(
                        text: $"{pawn.NameShortColored} pauses. A presence lingers at the edge of their awareness—watching, waiting. It has seen their struggles, their moments of strength, their failures. And yet, it remains.\r\n\r\nA faint whisper echoes in the back of their mind, words unbidden yet undeniable:\r\n\"Life before death. Strength before weakness. Journey before destination.\"",
                        buttonAText: "Speak the words",
                        buttonAAction: () => {
                            pawn.story.traits.GainTrait(new Trait(traitDef, 0));
                            pawnStats.hasFormedBond = true;
                        },
                        buttonBText: $"Say nothing",
                        buttonBAction: () => {
                        },
                        title: "A Whisper in the Mind.."
                    ));
                }
            }
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
                    //if (t.def.defName.StartsWith("Radiant")) { isRadiant = true; }
                    if (t.def == StormlightModDefs.whtwl_Radiant_Windrunner || t.def == StormlightModDefs.whtwl_Radiant_Truthwatcher) { isRadiant = true; }
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

