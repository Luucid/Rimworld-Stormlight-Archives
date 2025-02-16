using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace StormlightMod {



    [HarmonyPatch(typeof(PawnFlyer))]
    [HarmonyPatch("RespawnPawn")]
    public static class Patch_PP {
        static void Postfix() {

            Log.Message($"respawn pawn");
        }
    }


    public class PawnFlyerWorker_LashUp : PawnFlyerWorker {
        public PawnFlyerWorker_LashUp(PawnFlyerProperties props) : base(props) { }

        // Then override these two methods:
        public override float AdjustedProgress(float baseProgress) {
            // E.g. keep it simple or implement acceleration logic
            return baseProgress;
        }

        public override float GetHeight(float progress) {
            // E.g. a simple parabola from 0..20..0
            float maxHeight = 20f;
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
