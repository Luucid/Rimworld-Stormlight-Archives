﻿using HarmonyLib;
using RimWorld;
using System.Security.Policy;
using UnityEngine;
using Verse;

namespace StormlightMod {



    [HarmonyPatch(typeof(PawnFlyer))]
    [HarmonyPatch("RespawnPawn")]
    public class Patch_PP {
        static private Pawn flyingPawn = null;
        static void Prefix(ThingOwner<Thing> ___innerContainer) {
            if (___innerContainer.InnerListForReading.Count <= 0) {
                Log.Message("It was null");
                return;
            }
            flyingPawn = ___innerContainer.InnerListForReading[0] as Pawn; 
        }
        static void Postfix(AbilityDef ___triggeringAbility) {
            if (___triggeringAbility != null && flyingPawn != null) {
                Log.Message($"respawn pawn, ability: {___triggeringAbility.defName}");
                if (___triggeringAbility.defName == "lucidLashingUpward") {

                    flyingPawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 100));
                }
            }
        }
    }


    public class PawnFlyerWorker_LashUp : PawnFlyerWorker {
        public PawnFlyerWorker_LashUp(PawnFlyerProperties props) : base(props) { }
        static public float maxHeight = 20f;
        // Then override these two methods:
        public override float AdjustedProgress(float baseProgress) {
            // E.g. keep it simple or implement acceleration logic
            return baseProgress;
        }

        public override float GetHeight(float progress) {
            // E.g. a simple parabola from 0..20..0
            float p = 4f * progress * (1f - progress); // peak at p=0.5
            return maxHeight * p;
        }
    }


    public class PawnFlyerWorker_WindRunnerFlight : PawnFlyerWorker {
        public PawnFlyerWorker_WindRunnerFlight(PawnFlyerProperties props) : base(props) { }

        // Then override these two methods:
        public override float AdjustedProgress(float baseProgress) {
            // E.g. keep it simple or implement acceleration logic
            return baseProgress;
        }

        public override float GetHeight(float progress) {
            // E.g. a simple parabola from 0..20..0
            float maxHeight = 5f;
            float p = 4f * progress * (1f - progress); // peak at p=0.5
            return maxHeight * p;
        }
    }
}
