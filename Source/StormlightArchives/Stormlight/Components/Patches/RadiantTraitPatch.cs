using HarmonyLib;
using RimWorld;
using Verse;

namespace StormlightMod {
    [HarmonyPatch(typeof(TraitSet), "GainTrait")]
    public static class RadiantGainTraitPatch {
        static void Postfix(Pawn ___pawn, Trait trait) {
            if (trait.def == StormlightModDefs.whtwl_Radiant_Windrunner || trait.def == StormlightModDefs.whtwl_Radiant_Truthwatcher) {
                //Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

                if (___pawn != null && ___pawn.RaceProps.Humanlike) {
                    Log.Message($"{___pawn.Name} has become Radiant!");


                    givePawnStormlight(___pawn);
                    givePawnGlow(___pawn);
                    if (trait.Degree >= 3) {

                        givePawnShardbladeComp(___pawn);
                    }
                    if (trait.Degree == 0) {
                        ___pawn.needs.AddOrRemoveNeedsAsAppropriate();
                        Need_RadiantProgress progress = ___pawn.needs?.TryGetNeed<Need_RadiantProgress>();
                        if (progress != null) {
                            Log.Message("init xp");
                            progress.GainXP(0);
                        }
                    }
                }
            }
        }

        static private void givePawnShardbladeComp(Pawn pawn) {

            ThingDef stuffDef = DefDatabase<ThingDef>.GetNamed("whtwl_ShardMaterial", true);
            ThingDef shardThing = DefDatabase<ThingDef>.GetNamed("whtwl_MeleeWeapon_Shardblade", true);
            ThingWithComps blade = (ThingWithComps)ThingMaker.MakeThing(shardThing, stuffDef);

            CompShardblade comp = blade.GetComp<CompShardblade>();
            if (comp != null) {
                comp.Initialize(new CompProperties_Shardblade {
                });
                comp.bondWithPawn(pawn, false);
                Log.Message($"{pawn.Name} gained shardbalde storage!");
            }
        }

        static private void givePawnStormlight(Pawn pawn) {
            CompStormlight stormlightComp = pawn.GetComp<CompStormlight>();
            if (stormlightComp != null && stormlightComp.isActivatedOnPawn == false) {
                //CompStormlight stormlightComp = new CompStormlight();
                //pawn.AllComps.Add(stormlightComp);
                //stormlightComp.parent = pawn;
                //stormlightComp.Initialize(new CompProperties_Stormlight {
                //    maxStormlight = 3000f,
                //    drainRate = 1.0f
                //});
                stormlightComp.isActivatedOnPawn = true;
                stormlightComp.CompInspectStringExtra(); 
                Log.Message($"{pawn.Name} gained Stormlight storage!");
            }

        }

        static private void givePawnGlow(Pawn pawn) {
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
