using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StormlightMod {
    [DefOf]
    public static class StormlightModDefs {
        public static NeedDef whtwl_RadiantProgress;
        public static TraitDef whtwl_Radiant;
        public static JobDef whtwl_RefuelSphereLamp;
        public static AbilityDef whtwl_SummonShardblade;
        public static AbilityDef whtwl_UnbondBlade;
        public static AbilityDef whtwl_BreathStormlight;

        static StormlightModDefs() {
            DefOfHelper.EnsureInitializedInCtor(typeof(StormlightModDefs));
        }
    }
}
