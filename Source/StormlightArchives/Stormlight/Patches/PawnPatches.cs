using HarmonyLib;
using StormlightMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StormLight.Patches {
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Patch_Pawn_Kill {
        static void Prefix(Pawn __instance, DamageInfo? dinfo) {
            if (__instance != null && __instance.RaceProps.Humanlike) {
                Log.Message($"[DEBUG] {__instance.Name} is dying!");
                CompAbilityEffect_SpawnEquipment abilityComp = __instance.GetAbilityComp<CompAbilityEffect_SpawnEquipment>("SummonShardblade");
                if (abilityComp == null) return;
                if (abilityComp.bladeObject != null) {
                    {
                        CompShardblade shardBladeComp = abilityComp.bladeObject.GetComp<CompShardblade>();
                        if (shardBladeComp != null) {
                            Log.Message("summon blade");
                            shardBladeComp.summon();
                            Log.Message("sever bond to blade");
                            shardBladeComp.severBond(__instance);
                            Log.Message("drop it");
                            shardBladeComp.dismissBlade(__instance);
                        }
                    }
                }
            }
        }

    }
}
