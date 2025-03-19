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
        public static AbilityDef whtwl_SummonShardblade;
        public static AbilityDef whtwl_UnbondBlade;
        public static AbilityDef whtwl_BreathStormlight;
        public static AbilityDef whtwl_SurgeOfHealing;
        public static AbilityDef whtwl_SurgeOfGrowth;
        public static AbilityDef whtwl_LashingUpward;
        public static AbilityDef whtwl_WindRunnerFlight;

        //JobDef
        public static JobDef whtwl_RefuelSphereLamp;
        public static JobDef whtwl_RefuelFabrial;
        public static JobDef whtwl_RemoveFromFabrial;

        //ThingDef
        public static ThingDef whtwl_Apparel_SpherePouch;
        public static ThingDef whtwl_SphereLamp;
        public static ThingDef whtwl_FabrialCage_Pewter;

        //RAW GEMS
        public static ThingDef whtwl_RawDiamond;
        public static ThingDef whtwl_RawGarnet;
        public static ThingDef whtwl_RawRuby;
        public static ThingDef whtwl_RawSapphire;
        public static ThingDef whtwl_RawEmerald;

        //CUT GEMS
        public static ThingDef whtwl_CutDiamond;
        public static ThingDef whtwl_CutGarnet;
        public static ThingDef whtwl_CutRuby;
        public static ThingDef whtwl_CutSapphire;
        public static ThingDef whtwl_CutEmerald;

        //SPHERED GEMS
        public static ThingDef whtwl_Sphere_Diamond;
        public static ThingDef whtwl_Sphere_Garnet;
        public static ThingDef whtwl_Sphere_Ruby;
        public static ThingDef whtwl_Sphere_Sapphire;
        public static ThingDef whtwl_Sphere_Emerald;


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
        public static List<ThingDef> RawGems {
            get {
                return new List<ThingDef>
                {
                StormlightModDefs.whtwl_RawDiamond,
                StormlightModDefs.whtwl_RawGarnet,
                StormlightModDefs.whtwl_RawRuby,
                StormlightModDefs.whtwl_RawSapphire,
                StormlightModDefs.whtwl_RawEmerald
            };
            }
        }
        public static List<ThingDef> CutGems {
            get {
                return new List<ThingDef>
                {
                StormlightModDefs.whtwl_CutDiamond,
                StormlightModDefs.whtwl_CutGarnet,
                StormlightModDefs.whtwl_CutRuby,
                StormlightModDefs.whtwl_CutSapphire,
                StormlightModDefs.whtwl_CutEmerald
            };
            }
        }
    }

}
