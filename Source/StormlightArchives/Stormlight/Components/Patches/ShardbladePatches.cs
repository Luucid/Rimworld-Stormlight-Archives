using HarmonyLib;
using RimWorld;
using Verse;

namespace StormlightMod {
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.TryDropEquipment))]
    public static class ShardbladePatchDrop {
        static void Postfix(Pawn ___pawn, ThingWithComps eq, ThingWithComps resultingEq, IntVec3 pos) {
            if (eq != null) {
                if (eq.def.defName == "MeleeWeapon_Shardblade") {
                    CompShardblade blade = eq.GetComp<CompShardblade>();
                    if (blade != null) {
                        if (blade.isBonded(___pawn)) { eq.DeSpawn(); }
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.AddEquipment))]
    public static class ShardbladePatchePickup {
        static void Postfix(Pawn ___pawn, ThingWithComps newEq) {
            if (newEq != null) {
                if (newEq.def.defName == "MeleeWeapon_Shardblade") {
                    CompShardblade blade = newEq.GetComp<CompShardblade>();
                    if (blade != null) {
                        if (blade.isBonded(null)) {
                            Log.Message($"[stormlight mod] {___pawn.Name} picked up an unbounded shardblade!");
                            blade.bondWithPawn(___pawn, true);
                        }
                    }
                }
            }
        }
    }
}

