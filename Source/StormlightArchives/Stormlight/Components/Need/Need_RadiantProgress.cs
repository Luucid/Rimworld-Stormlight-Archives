using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StormlightMod {


    public class Need_RadiantProgress : Need {
        private const float LEVEL_NEW_SQUIRE = 500f;
        private const float LEVEL_EXPERIENCED_SQUIRE = LEVEL_NEW_SQUIRE * 6f;
        private const float LEVEL_KNIGHT_RADIANT = LEVEL_EXPERIENCED_SQUIRE * 2f;
        private const float LEVEL_KNIGHT_RADIANT_MASTER = LEVEL_KNIGHT_RADIANT * 2f;
        private const float MAX_XP = LEVEL_KNIGHT_RADIANT_MASTER + 10f;
        private float currentXp = 0f;
        private int CurrentDegree = 0;

        public int IdealLevel { get { return CurrentDegree + 1; } }





        public Need_RadiantProgress(Pawn pawn) : base(pawn) {
            //this.threshPercents = new List<float> { 0.083f, 0.5f, 0.75f }; // Visual bar markers
        }

        public override void NeedInterval() {
            // **No passive decay**, since XP should only increase when events happen
        }

        public void GainXP(float amount) {
            currentXp += amount;
            CurLevel = Mathf.Max(0f, Mathf.Min(1f, (currentXp / MAX_XP)));
        }

        public void UpdateRadiantTrait(Pawn pawn) {
            Trait radiantTrait = StormlightUtilities.GetRadiantTrait(pawn);
            if (radiantTrait != null) {
                int currentDegree = radiantTrait.Degree;
                int newDegree = GetDegreeFromXP(currentXp);

                if (newDegree > currentDegree && isEligibleForRankup(pawn, radiantTrait)) {
                    // Remove old trait and add the upgraded one
                    pawn.story.traits.RemoveTrait(radiantTrait);
                    pawn.story.traits.GainTrait(new Trait(radiantTrait.def, newDegree));
                    CurrentDegree = newDegree;
                    Messages.Message($"{pawn.Name} has grown stronger as a Radiant!", pawn, MessageTypeDefOf.PositiveEvent);
                }
                else if (newDegree > currentDegree) {
                    currentXp = GetXpFromDegree(currentDegree);
                }
            }
        }

        private bool isEligibleForRankup(Pawn pawn, Trait trait) {
            bool eligible = false;
            PawnStats pawnStats = pawn.GetComp<PawnStats>();

            switch (IdealLevel) {
                case 1:
                    Whtwl_RadiantNeedLevelupChecker.UpdateIsSatisfiedReq1_2(pawnStats);
                    eligible = pawnStats.GetRequirements(trait.def, pawnStats.Props.Req_1_2).IsSatisfied;
                    break;
                case 2:
                    Whtwl_RadiantNeedLevelupChecker.UpdateIsSatisfiedReq2_3(pawnStats);
                    if (trait.def == StormlightModDefs.whtwl_Radiant_Windrunner) {
                        eligible = pawnStats.GetRequirements(trait.def, pawnStats.Props.Req_2_3_wr).IsSatisfied;
                    }
                    else if (trait.def == StormlightModDefs.whtwl_Radiant_Truthwatcher) {
                        eligible = pawnStats.GetRequirements(trait.def, pawnStats.Props.Req_2_3_tw).IsSatisfied;
                    }
                    Log.Message($"2 satisfied: {eligible}");
                    break;
                case 3:
                    Whtwl_RadiantNeedLevelupChecker.UpdateIsSatisfiedReq3_4(pawnStats);
                    if (trait.def == StormlightModDefs.whtwl_Radiant_Windrunner) {
                        eligible = pawnStats.GetRequirements(trait.def, pawnStats.Props.Req_3_4_wr).IsSatisfied;
                    }
                    else if (trait.def == StormlightModDefs.whtwl_Radiant_Truthwatcher) {
                        eligible = pawnStats.GetRequirements(trait.def, pawnStats.Props.Req_3_4_tw).IsSatisfied;
                    }
                    Log.Message($"3 satisfied: {eligible}");
                    break;
                default:
                    eligible = true;
                    break;
            }

            return eligible;
        }

        private int GetDegreeFromXP(float xp) {
            if (xp >= LEVEL_KNIGHT_RADIANT_MASTER) return 4;     // Knight Radiant Master
            if (xp >= LEVEL_KNIGHT_RADIANT) return 3;            // Knight Radiant
            if (xp >= LEVEL_EXPERIENCED_SQUIRE) return 2;        // Experienced Squire
            if (xp >= LEVEL_NEW_SQUIRE) return 1;                // New Squire
            return 0;                                            // Bonded (Base Level)
        }

        private float GetXpFromDegree(int degree) {
            if (degree == 4) return LEVEL_KNIGHT_RADIANT_MASTER;     // Knight Radiant Master
            if (degree == 3) return LEVEL_KNIGHT_RADIANT_MASTER;     // Knight Radiant Master
            if (degree == 2) return LEVEL_KNIGHT_RADIANT;            // Knight Radiant
            if (degree == 1) return LEVEL_EXPERIENCED_SQUIRE;        // Experienced Squire
            if (degree == 0) return LEVEL_NEW_SQUIRE;                // New Squire
            return LEVEL_NEW_SQUIRE;                                            // Bonded (Base Level)
        }

        public override int GUIChangeArrow => 0; // No arrow (need doesn’t decay)
    }



    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    public static class Patch_RadiantProgress_Need {
        public static void Postfix(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result, Pawn ___pawn) {

            //if (nd == StormlightModDefs.whtwl_RadiantProgress) {
            //    __result = ___pawn.story?.traits?.allTraits.FirstOrDefault(t => StormlightModUtilities.RadiantTraits.Contains(t.def)) != null;
            //}

            if (nd == StormlightModDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(StormlightModDefs.whtwl_Radiant_Windrunner) == true) {
                __result = true;
            }
            else if (nd == StormlightModDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(StormlightModDefs.whtwl_Radiant_Truthwatcher) == true) {
                __result = true;
            }
            else if (nd == StormlightModDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(StormlightModDefs.whtwl_Radiant_Windrunner) == false) {
                __result = false;
            }
            else if (nd == StormlightModDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(StormlightModDefs.whtwl_Radiant_Truthwatcher) == false) {
                __result = false;
            }
        }
    }


    public static class RadiantUtility {
        public static void GiveRadiantXP(Pawn pawn, float amount) {
            if (pawn == null) {
                return;
            }

            Need_RadiantProgress progress = pawn.needs?.TryGetNeed<Need_RadiantProgress>();
            if (progress != null) {
                progress.GainXP(amount);
                progress.UpdateRadiantTrait(pawn);
            }
        }
    }
}
