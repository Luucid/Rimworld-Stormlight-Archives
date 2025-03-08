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

        public static TraitDef whtwl_Radiant_Windrunner;
        public static TraitDef whtwl_Radiant_Truthwatcher;
        public static JobDef whtwl_RefuelSphereLamp;
        public static AbilityDef whtwl_SummonShardblade;
        public static AbilityDef whtwl_UnbondBlade;
        public static AbilityDef whtwl_BreathStormlight;
        public static AbilityDef whtwl_SurgeOfHealing;
        public static AbilityDef whtwl_SurgeOfGrowth;
        public static AbilityDef whtwl_LashingUpward;
        public static AbilityDef whtwl_WindRunnerFlight;

        //ThingDef
        public static ThingDef whtwl_Sphere_Diamond;
        public static ThingDef whtwl_Sphere_Garnet;
        public static ThingDef whtwl_Sphere_Ruby;
        public static ThingDef whtwl_Sphere_Sapphire;
        public static ThingDef whtwl_Sphere_Emerald; 
        public static ThingDef whtwl_Apparel_SpherePouch; 
        public static ThingDef whtwl_SphereLamp;  

        static StormlightModDefs() {
            DefOfHelper.EnsureInitializedInCtor(typeof(StormlightModDefs));
        }
    }

    public static class StormlightModUtilities {
        public static List<TraitDef> RadiantTraits {
            get {
                return new List<TraitDef>
                {
                StormlightModDefs.whtwl_Radiant_Windrunner,
                StormlightModDefs.whtwl_Radiant_Truthwatcher
            };
            }
        }
    }

}
