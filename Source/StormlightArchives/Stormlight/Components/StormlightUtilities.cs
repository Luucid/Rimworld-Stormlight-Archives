using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace StormlightMod {

    static public class StormlightUtilities {
      
        static public Trait GetRadiantTrait(Pawn pawn) {
            return pawn.story.traits.allTraits.FirstOrDefault(t => StormlightModUtilities.RadiantTraits.Contains(t.def));

        }
    }

}
