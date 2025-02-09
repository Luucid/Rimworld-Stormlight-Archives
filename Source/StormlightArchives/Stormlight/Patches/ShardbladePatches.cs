using HarmonyLib;
using RimWorld;
using Verse;

namespace StormlightMod {
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.TryDropEquipment))]
    public static class ShardbladePatches {
        static void Postfix(ThingWithComps eq, ThingWithComps resultingEq, IntVec3 pos) {
            if (eq != null) {
                if (eq.def.defName == "MeleeWeapon_Shardblade") {
                    CompShardblade blade = eq.GetComp<CompShardblade>();
                    if (blade != null) {
                        if (blade.isBonded()) { eq.DeSpawn(); }
                    }
                }
            }
        }
    }
}

