using HarmonyLib;
using RimWorld;
using Verse;

namespace StormlightMod {
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.TryDropEquipment))]
    public static class ShardbladePatches {
        static void Postfix(Pawn ___pawn, ThingWithComps eq, ThingWithComps resultingEq, IntVec3 pos) {
            if (eq != null) {
                if (eq.def.defName == "MeleeWeapon_Shardblade") {
                    CompShardblade blade = eq.GetComp<CompShardblade>();
                    if (blade != null) {
                        Log.Message($"dropping sword with ID: {blade.GetHashCode()}, {blade.Props.GetHashCode()}, {eq.thingIDNumber}");
                        if (blade.isBonded(___pawn)) { eq.DeSpawn(); }
                    }
                }
            }
        }
    }
}

