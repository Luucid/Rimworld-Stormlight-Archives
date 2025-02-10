using HarmonyLib;
using RimWorld;
using Verse;

namespace StormlightMod {
    [HarmonyPatch(typeof(TraitSet), "GainTrait")]
    public static class RadiantTraitPatch {
        static void Postfix(TraitSet __instance, Trait trait) {
            if (trait.def.defName == "Radiant") {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

                if (pawn != null && pawn.RaceProps.Humanlike) {
                    Log.Message($"{pawn.Name} has become Radiant!");


                    givePawnStormlight(ref pawn);
                    givePawnGlow(ref pawn);
                    givePawnShardbladeComp(ref pawn);
                }
            }
        }

       static private void givePawnShardbladeComp(ref Pawn pawn) {
            if (pawn.GetComp<CompShardBlade>() == null) {
                CompShardBlade bladeComp = new CompShardBlade();
                pawn.AllComps.Add(bladeComp);
                bladeComp.parent = pawn;
                bladeComp.Initialize(new CompProperties_ShardBlade {
                      isBondedByPawn = false,
                      isSpawned = false,
                      owner = pawn,
                      thisBladeThing = null
                });
                Log.Message($"{pawn.Name} gained shardbalde storage!");
            }
        }

        static private void givePawnStormlight(ref Pawn pawn) {
            if (pawn.GetComp<CompStormlight>() == null) {
                CompStormlight stormlightComp = new CompStormlight();
                pawn.AllComps.Add(stormlightComp);
                stormlightComp.parent = pawn;
                stormlightComp.Initialize(new CompProperties_Stormlight {
                    maxStormlight = 3000f,
                    drainRate = 1.0f
                });
                Log.Message($"{pawn.Name} gained Stormlight storage!");
            }

        }

        static private void givePawnGlow(ref Pawn pawn) {
            if (pawn.GetComp<CompGlower>() == null) {
                CompGlower glowerComp = new CompGlower();
                pawn.AllComps.Add(glowerComp);
                glowerComp.parent = pawn;

                CompProperties_Glower glowProps = new CompProperties_Glower {
                    glowRadius = 0,  // Start with no glow
                    overlightRadius = 2.0f,  // Overlight radius (if needed)
                    glowColor = new ColorInt(66, 245, 245, 0)
                };

                glowerComp.Initialize(glowProps);
                Log.Message($"{pawn.Name} now glows when infused with Stormlight!");
            }

        }
    }

}
