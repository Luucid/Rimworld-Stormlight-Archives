using HarmonyLib;
using RimWorld;
using StormlightMod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using static HarmonyLib.Code;

namespace StormlightMod {

    [HarmonyPatch(typeof(StockGenerator_MiscItems), "GenerateThings")]
    public static class StockGenerator_MiscItems_GenerateThings_Patch {
        public static void Postfix(ref IEnumerable<Thing> __result, int forTile, Faction faction = null) {
            var resultList = __result.ToList();
            if (!resultList.Any(t => t.def == StormlightModDefs.whtwl_AlloyPewter) && StormlightUtilities.RollTheDice(1, 4, 2)) {
                Thing newItem = StockGeneratorUtility.TryMakeForStockSingle(StormlightModDefs.whtwl_AlloyPewter, StormlightUtilities.RollTheDice(1, 20), faction); 
                resultList.Add(newItem);
            }
            __result = resultList;
        }
    }
}