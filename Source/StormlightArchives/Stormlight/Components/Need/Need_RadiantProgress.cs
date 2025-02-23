using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StormlightMod {
    public class Need_RadiantProgress : Need {
        private const float LEVEL_NEW_SQUIRE = 500f;
        private const float LEVEL_EXPERIENCED_SQUIRE = LEVEL_NEW_SQUIRE*6f;
        private const float LEVEL_KNIGHT_RADIANT = LEVEL_EXPERIENCED_SQUIRE*2f;
        private const float MAX_XP = LEVEL_KNIGHT_RADIANT + 10f;
        private float currentXp = 0f;

        public Need_RadiantProgress(Pawn pawn) : base(pawn) {
            this.threshPercents = new List<float> { 0.083f, 0.5f}; // Visual bar markers
        }

        public override void NeedInterval() {
            // **No passive decay**, since XP should only increase when events happen
        }

        public void GainXP(float amount) {
            currentXp += amount;
            CurLevel = Mathf.Max(0f, Mathf.Min(1f, (currentXp/ MAX_XP))); 
        }

        public void UpdateRadiantTrait(Pawn pawn) {
            Trait radiantTrait = pawn.story.traits.GetTrait(StormlightModDefs.whtwl_Radiant);

            if (radiantTrait != null) {
                int currentDegree = radiantTrait.Degree;
                int newDegree = GetDegreeFromXP(currentXp);

                if (newDegree > currentDegree) {
                    // Remove old trait and add the upgraded one
                    pawn.story.traits.RemoveTrait(radiantTrait);
                    pawn.story.traits.GainTrait(new Trait(StormlightModDefs.whtwl_Radiant, newDegree));

                    Messages.Message($"{pawn.Name} has grown stronger as a Radiant!", pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        private int GetDegreeFromXP(float xp) {
            if (xp >= LEVEL_KNIGHT_RADIANT) return 3;     // Knight Radiant
            if (xp >= LEVEL_EXPERIENCED_SQUIRE) return 2; // Experienced Squire
            if (xp >= LEVEL_NEW_SQUIRE) return 1;         // New Squire
            return 0; // Bonded (Base Level)
        }


        public override int GUIChangeArrow => 1; // No arrow (need doesn’t decay)
    }

   

    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    public static class Patch_RadiantProgress_Need {
        public static void Postfix(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result, Pawn ___pawn) {
            if (nd == StormlightModDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(StormlightModDefs.whtwl_Radiant) == true) {
                __result = true;       
            }
            else if(nd == StormlightModDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(StormlightModDefs.whtwl_Radiant) == false)
            {
                __result = false;
            }
        }
    }


    public static class RadiantUtility {
        public static void GiveRadiantXP(Pawn pawn, float amount) {
            if (pawn == null) {
                Log.Message("pawn was null");
                return;
            }

            Need_RadiantProgress progress = pawn.needs?.TryGetNeed<Need_RadiantProgress>();
            if (progress != null) {
                progress.GainXP(amount);
                progress.UpdateRadiantTrait(pawn);
            }
            else {
                Log.Message("progress was null");
            }
        }
    }
}
