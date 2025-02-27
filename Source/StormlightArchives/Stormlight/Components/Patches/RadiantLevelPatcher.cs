using HarmonyLib;
using RimWorld;
using StormlightMod;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using static HarmonyLib.Code;

namespace StormlightMod {
    public class CompProperties_PawnStats : CompProperties {
        public string ExperiencedPersonalCrisis;
        public string ProtectedSomeoneInNeed;

        public CompProperties_PawnStats() {
            this.compClass = typeof(PawnStats);
        }
    }
    public class PawnStats : ThingComp {
        public new CompProperties_PawnStats Props => this.props as CompProperties_PawnStats;
        public Dictionary<string, Dictionary<int, Dictionary<string, Radiant_Requirements>>> requirementMap = new Dictionary<string, Dictionary<int, Dictionary<string, Radiant_Requirements>>>();
        public bool hasFormedBond = false;


        public override void Initialize(CompProperties props) {
            base.Initialize(props);

            requirementMap.Add(StormlightModDefs.whtwl_Radiant_Windrunner.defName, new Dictionary<int, Dictionary<string, Radiant_Requirements>>());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName].Add(0, new Dictionary<string, Radiant_Requirements>());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][0].Add(Props.ExperiencedPersonalCrisis, new Radiant_Requirements());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][0].Add(Props.ProtectedSomeoneInNeed, new Radiant_Requirements());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][0].Add("req_3", new Radiant_Requirements());

            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName].Add(1, new Dictionary<string, Radiant_Requirements>());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][1].Add("req_1", new Radiant_Requirements());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][1].Add("req_2", new Radiant_Requirements());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][1].Add("req_3", new Radiant_Requirements());

            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName].Add(2, new Dictionary<string, Radiant_Requirements>());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][2].Add("req_1", new Radiant_Requirements());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][2].Add("req_2", new Radiant_Requirements());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][2].Add("req_3", new Radiant_Requirements());

            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName].Add(3, new Dictionary<string, Radiant_Requirements>());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][3].Add("req_1", new Radiant_Requirements());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][3].Add("req_2", new Radiant_Requirements());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][3].Add("req_3", new Radiant_Requirements());

            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName].Add(4, new Dictionary<string, Radiant_Requirements>());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][4].Add("req_1", new Radiant_Requirements());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][4].Add("req_2", new Radiant_Requirements());
            requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][4].Add("req_3", new Radiant_Requirements());
        }
    }
}


namespace StormlightMod {
    [HarmonyPatch(typeof(MentalBreaker), nameof(MentalBreaker.MentalBreakerTick))]
    public static class Whtwl_MentalBreakExperiences {
        static void Postfix(MentalBreaker __instance, Pawn ___pawn) {
            if (___pawn.NonHumanlikeOrWildMan()) { return; }
            PawnStats pawnStats = ___pawn.GetComp<PawnStats>();

            if (pawnStats != null) {
                if (pawnStats.hasFormedBond == false) {
                    if (__instance.BreakExtremeIsImminent) {
                        pawnStats.requirementMap["whtwl_Radiant_Windrunner"][0][pawnStats.Props.ExperiencedPersonalCrisis].Value += 0.05f;
                    }
                    else if (__instance.BreakMajorIsImminent) {
                        pawnStats.requirementMap["whtwl_Radiant_Windrunner"][0][pawnStats.Props.ExperiencedPersonalCrisis].Value += 0.025f;
                    }
                }
            }
        }
    }
}