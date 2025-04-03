using HarmonyLib;
using RimWorld;
using StormlightMod;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using static HarmonyLib.Code;

namespace StormlightMod {

    public class Radiant_Requirements : IExposable {
        public bool IsSatisfied = false;
        public float Value = 0f;
        public int Count = 0;

        public void ExposeData() {
            Scribe_Values.Look(ref IsSatisfied, "IsSatisfied");
            Scribe_Values.Look(ref Value, "Value");
            Scribe_Values.Look(ref Count, "Count");
        }
    }
    public class RequirementMapEntry : IExposable {
        public string outerKey;
        public Dictionary<string, Radiant_Requirements> innerDict;

        public void ExposeData() {
            Scribe_Values.Look(ref outerKey, "outerKey");
            Scribe_Collections.Look(ref innerDict, "innerDict", LookMode.Value, LookMode.Deep);
        }
    }

    public class CompProperties_PawnStats : CompProperties {
        public string Req_0_1;
        public string Req_1_2;
        public string Req_2_3_wr;
        public string Req_3_4_wr;
        public string Req_2_3_tw;
        public string Req_3_4_tw;     
        public string Req_2_3_ed;
        public string Req_3_4_ed;

        public CompProperties_PawnStats() {
            this.compClass = typeof(PawnStats);
        }
    }
    public class PawnStats : ThingComp {
        public new CompProperties_PawnStats Props => this.props as CompProperties_PawnStats;
        public Dictionary<string, Dictionary<string, Radiant_Requirements>> requirementMap = new Dictionary<string, Dictionary<string, Radiant_Requirements>>();
        private List<RequirementMapEntry> requirementMapSerialized = new List<RequirementMapEntry>();
        public List<Pawn> PatientList = new List<Pawn>();
        public bool PatientDied = false;
        public bool PatientSaved = false;
        public bool EnemyPatientDied = false;
        public bool EnemyPatientSaved = false;
        public bool hasFormedBond = false;


        public override void PostExposeData() {
            Scribe_Collections.Look(ref PatientList, "PatientList", LookMode.Reference);
            Scribe_Values.Look(ref PatientDied, "PatientDied", false);
            Scribe_Values.Look(ref PatientSaved, "PatientSaved", false);
            Scribe_Values.Look(ref EnemyPatientDied, "EnemyPatientDied", false);
            Scribe_Values.Look(ref EnemyPatientSaved, "EnemyPatientSaved", false);
            Scribe_Values.Look(ref hasFormedBond, "hasFormedBond", false);
            if (Scribe.mode == LoadSaveMode.Saving) {
                requirementMapSerialized.Clear();
                foreach (var outerPair in requirementMap) {
                    requirementMapSerialized.Add(new RequirementMapEntry {
                        outerKey = outerPair.Key,
                        innerDict = outerPair.Value
                    });
                }
            }

            Scribe_Collections.Look(ref requirementMapSerialized, "requirementMapSerialized", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                requirementMap.Clear();
                if (requirementMapSerialized != null) {
                    foreach (var entry in requirementMapSerialized) {
                        if (entry != null && entry.outerKey != null) {
                            requirementMap[entry.outerKey] = entry.innerDict;
                        }
                    }
                }
            }
        }


        public override void Initialize(CompProperties props) {
            base.Initialize(props);

            //WINDRUNNER
            var windrunnerDefName = StormlightModDefs.whtwl_Radiant_Windrunner.defName;
            requirementMap.Add(windrunnerDefName, new Dictionary<string, Radiant_Requirements>());
            requirementMap[windrunnerDefName].Add(Props.Req_0_1, new Radiant_Requirements());
            requirementMap[windrunnerDefName].Add(Props.Req_1_2, new Radiant_Requirements());
            requirementMap[windrunnerDefName].Add(Props.Req_2_3_wr, new Radiant_Requirements());
            requirementMap[windrunnerDefName].Add(Props.Req_3_4_wr, new Radiant_Requirements());


            //TRUTHWATCHER
            var truthwatcherDefName = StormlightModDefs.whtwl_Radiant_Truthwatcher.defName;
            requirementMap.Add(truthwatcherDefName, new Dictionary<string, Radiant_Requirements>());
            requirementMap[truthwatcherDefName].Add(Props.Req_0_1, new Radiant_Requirements());
            requirementMap[truthwatcherDefName].Add(Props.Req_1_2, new Radiant_Requirements());
            requirementMap[truthwatcherDefName].Add(Props.Req_2_3_tw, new Radiant_Requirements());
            requirementMap[truthwatcherDefName].Add(Props.Req_3_4_tw, new Radiant_Requirements());
            
            //TRUTHWATCHER
            var edgedancerDefName = StormlightModDefs.whtwl_Radiant_Edgedancer.defName;
            requirementMap.Add(edgedancerDefName, new Dictionary<string, Radiant_Requirements>());
            requirementMap[edgedancerDefName].Add(Props.Req_0_1, new Radiant_Requirements());
            requirementMap[edgedancerDefName].Add(Props.Req_1_2, new Radiant_Requirements());
            requirementMap[edgedancerDefName].Add(Props.Req_2_3_ed, new Radiant_Requirements());
            requirementMap[edgedancerDefName].Add(Props.Req_3_4_ed, new Radiant_Requirements());


            requirementMap[edgedancerDefName][Props.Req_1_2].IsSatisfied = true;       // for now it is true default
            requirementMap[edgedancerDefName][Props.Req_2_3_ed].IsSatisfied = true;       // for now it is true default
            requirementMap[edgedancerDefName][Props.Req_3_4_ed].IsSatisfied = true;       // for now it is true default
        }

        public Radiant_Requirements GetRequirements(TraitDef trait, string req) {
            return requirementMap?[trait.defName]?[req];
        }
    }
}


namespace StormlightMod {

    public static class Whtwl_RadiantNeedLevelupChecker {
        public static void UpdateIsSatisfiedReq1_2(PawnStats pawnStats) {
            var windrunnerRequirement = pawnStats.requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][pawnStats.Props.Req_1_2];
            if (windrunnerRequirement.Count >= 1 && pawnStats.PatientSaved) {
                windrunnerRequirement.IsSatisfied = true;
            }

            var truthwatcherRequirement = pawnStats.requirementMap[StormlightModDefs.whtwl_Radiant_Truthwatcher.defName][pawnStats.Props.Req_1_2];
            if (windrunnerRequirement.Count >= 1 && pawnStats.PatientSaved) {
                windrunnerRequirement.IsSatisfied = true;
            }
        }
        public static void UpdateIsSatisfiedReq2_3(PawnStats pawnStats) {//helped enemy in need
            var windrunnerRequirement = pawnStats.requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][pawnStats.Props.Req_2_3_wr];
            if (windrunnerRequirement.Count >= 1 && pawnStats.EnemyPatientSaved) {
                windrunnerRequirement.IsSatisfied = true;
            }

        }
        public static void UpdateIsSatisfiedReq3_4(PawnStats pawnStats) { //ally with bond died even tho tried to save
            var windrunnerRequirement = pawnStats.requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][pawnStats.Props.Req_3_4_wr];
            windrunnerRequirement.IsSatisfied = true;
        }
    }


    //Initial requirements, must have suffered crisis
    [HarmonyPatch(typeof(MentalBreaker), nameof(MentalBreaker.MentalBreakerTick))]
    public static class Whtwl_MentalBreakExperiences {
        private static int tickCounter = 0;

        static void Postfix(MentalBreaker __instance, Pawn ___pawn) {
            tickCounter = (tickCounter + 1) % 100;
            if (tickCounter != 0) return;
            if (___pawn.NonHumanlikeOrWildMan()) return;
            PawnStats pawnStats = ___pawn.GetComp<PawnStats>();

            if (pawnStats != null && pawnStats.hasFormedBond == false) {
                float increment = 0f;
                if (__instance.BreakExtremeIsImminent) {
                    increment = 0.09f * StormlightMod.settings.bondChanceMultiplier;
                }
                else if (__instance.BreakMajorIsImminent) {
                    increment = 0.05f * StormlightMod.settings.bondChanceMultiplier;
                }
                else if (__instance.BreakMinorIsImminent) {
                    increment = 0.01f * StormlightMod.settings.bondChanceMultiplier;
                }
                if (increment > 0f) {
                    pawnStats.requirementMap[StormlightModDefs.whtwl_Radiant_Truthwatcher.defName][pawnStats.Props.Req_0_1].Value += increment;
                    pawnStats.requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][pawnStats.Props.Req_0_1].Value += increment;
                    pawnStats.requirementMap[StormlightModDefs.whtwl_Radiant_Edgedancer.defName][pawnStats.Props.Req_0_1].Value += increment;
                }
            }
        }
    }


    ////second requirements, must help people in need
    [HarmonyPatch(typeof(TendUtility), nameof(TendUtility.DoTend))]
    public static class Whtwl_HelpSomeoneInNeed {
        static void Postfix(Pawn doctor, Pawn patient, Medicine medicine) {
            if (doctor == null || patient.NonHumanlikeOrWildMan() || doctor == patient) { return; }
            PawnStats pawnStats = doctor.GetComp<PawnStats>();

            if (pawnStats != null) {

                // WINDRUNNER
                var windrunnerRequirement1_2 = pawnStats.requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][pawnStats.Props.Req_1_2];
                if (!pawnStats.PatientList.Contains(patient)) {
                    pawnStats.PatientList.Add(patient);
                    Log.Message("Patient added to list");
                }
                windrunnerRequirement1_2.Count += 1;

                //2_3
                if (patient.IsPrisoner) {
                    var windrunnerRequirement2_3 = pawnStats.requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][pawnStats.Props.Req_2_3_wr];
                    windrunnerRequirement2_3.Count += 1;
                }

                // TRUTHWATCHER
                var truthwatcherRequirement = pawnStats.requirementMap[StormlightModDefs.whtwl_Radiant_Truthwatcher.defName][pawnStats.Props.Req_1_2];
                truthwatcherRequirement.Count += 1;
                if (truthwatcherRequirement.Count >= 25 && pawnStats.PatientSaved)
                    truthwatcherRequirement.IsSatisfied = true;
            }
        }
    }

    //add class to check for eligible that both can use, and call from both!

    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("HealthTick")]
    public static class Patch_Pawn_HealthTracker_HealthTick {
        static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn) {
            if (___pawn == null || ___pawn.NonHumanlikeOrWildMan()) { return; }
            PawnStats pawnStats = ___pawn.GetComp<PawnStats>();

            List<Pawn> patientsToRemove = new List<Pawn>();
            foreach (Pawn patient in pawnStats.PatientList) {
                if (patient.health.Dead && patient.IsPrisoner == false) {
                    pawnStats.PatientDied = true;
                    pawnStats.requirementMap[StormlightModDefs.whtwl_Radiant_Windrunner.defName][pawnStats.Props.Req_3_4_wr].IsSatisfied = true;
                    patientsToRemove.Add(patient);
                    Log.Message("Patient died..");
                }
                else if (NeedsNoTending(patient)) {
                    pawnStats.PatientSaved = true;
                    if (patient.IsPrisoner) {
                        pawnStats.EnemyPatientSaved = true;
                    }
                    patientsToRemove.Add(patient);
                }
            }
            foreach (var patient in patientsToRemove) {
                pawnStats.PatientList.Remove(patient);
            }
        }

        private static bool NeedsNoTending(Pawn pawn) {
            return !pawn.health.HasHediffsNeedingTendByPlayer()
                   && !HealthAIUtility.ShouldSeekMedicalRest(pawn);
        }
    }


}

//Messages.Message("MessageFullyHealed".Translate(pawn.LabelCap, pawn), pawn, MessageTypeDefOf.PositiveEvent);