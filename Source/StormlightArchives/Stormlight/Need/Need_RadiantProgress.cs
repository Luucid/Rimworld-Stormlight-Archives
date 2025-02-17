﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StormlightMod {
    public class Need_RadiantProgress : Need {
        private const float LEVEL_NEW_SQUIRE = 3000f;
        private const float LEVEL_EXPERIENCED_SQUIRE = LEVEL_NEW_SQUIRE*2f;
        private const float LEVEL_KNIGHT_RADIANT = LEVEL_EXPERIENCED_SQUIRE*2f;
        
        private float currentXp = 0f;
        public Need_RadiantProgress(Pawn pawn) : base(pawn) {
            //this.threshPercents = new List<float> { 0.2f, 0.4f, 0.6f, 0.8f }; // Visual bar markers
        }

        public override void NeedInterval() {
            // **No passive decay**, since XP should only increase when events happen
        }

        public void GainXP(float amount) {
            currentXp += amount;
        }

        public void UpdateRadiantTrait(Pawn pawn) {
            Trait radiantTrait = pawn.story.traits.GetTrait(StormlightModDefs.Radiant);

            if (radiantTrait != null) {
                int currentDegree = radiantTrait.Degree;
                int newDegree = GetDegreeFromXP(currentXp);

                if (newDegree > currentDegree) {
                    // Remove old trait and add the upgraded one
                    pawn.story.traits.RemoveTrait(radiantTrait);
                    pawn.story.traits.GainTrait(new Trait(StormlightModDefs.Radiant, newDegree));

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


        public override int GUIChangeArrow => 0; // No arrow (need doesn’t decay)
    }

    [DefOf]
    public static class StormlightModDefs {
        public static NeedDef RadiantProgress;
        public static TraitDef Radiant; 

        static StormlightModDefs() {
            DefOfHelper.EnsureInitializedInCtor(typeof(StormlightModDefs));
        }
    }

    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    public static class Patch_RadiantProgress_Need {
        public static void Postfix(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result, Pawn ___pawn) {
            if (nd == StormlightModDefs.RadiantProgress && ___pawn.story?.traits?.HasTrait(StormlightModDefs.Radiant) ==true) { 
                __result = true;
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
