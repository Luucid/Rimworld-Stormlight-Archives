﻿using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using UnityEngine;
using Verse.AI;
using LudeonTK;
using Verse.Noise;
using System.Runtime.Remoting.Messaging;

namespace StormlightMod {



    public class CompApparelFabrialDiminisher : ThingComp {
        public CompProperties_ApparelFabrialDiminisher Props => (CompProperties_ApparelFabrialDiminisher)props;
        public Thing insertedGemstone = null;
        public bool PowerOn = false;
        public bool HasGemstone => insertedGemstone != null;
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
            Scribe_Values.Look(ref PowerOn, "PowerOn");
        }
        public override void Notify_Equipped(Pawn pawn) {
            base.Notify_Equipped(pawn);

            if (!pawn.health.hediffSet.HasHediff(StormlightModDefs.whtwl_apparel_painrial_diminisher_hediff)) {
                var hediff = HediffMaker.MakeHediff(StormlightModDefs.whtwl_apparel_painrial_diminisher_hediff, pawn);
                pawn.health.AddHediff(hediff);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
        }
        public override void CompTick() {
            checkPower();
        }

        public void InfuseStormlight(float amount) {
            if (HasGemstone) {
                var stormlightComp = (insertedGemstone as ThingWithComps).GetComp<CompStormlight>();
                stormlightComp.infuseStormlight(amount);
            }
        }

        public bool checkPower() {
            if (insertedGemstone != null) {
                var stormlightComp = (insertedGemstone as ThingWithComps).GetComp<CompStormlight>();
                if (stormlightComp != null) {
                    PowerOn = stormlightComp.HasStormlight;
                    if (PowerOn) {
                        stormlightComp.drainFactor = 0.25f;
                        stormlightComp.CompTick();
                    }
                }
                else { PowerOn = false; }
            }
            else { PowerOn = false; }
            return PowerOn;
        }

        public override string CompInspectStringExtra() {
            string gemName = "No gem in fabrial.";

            if (insertedGemstone != null) {
                ThingWithComps gemstone = insertedGemstone as ThingWithComps;
                gemName = "Spren: " + gemstone.GetComp<CompCutGemstone>().capturedSpren.ToString() + "\nStormlight: " + gemstone.GetComp<CompStormlight>().Stormlight.ToString("F0");
            }
            return gemName;
        }
    }

    public class CompProperties_ApparelFabrialDiminisher : CompProperties {
        public CompProperties_ApparelFabrialDiminisher() {
            this.compClass = typeof(CompApparelFabrialDiminisher);
        }
    }



    public class HediffComp_FabrialPainDiminisher : HediffComp {
        public bool getActive() {
            Pawn pawn = this.Pawn;
            if (pawn?.apparel?.WornApparel == null)
                return false;
            foreach (var apparel in pawn.apparel.WornApparel) {
                var comp = apparel.GetComp<CompApparelFabrialDiminisher>();
                if (comp == null)
                    continue;

                return comp.checkPower();
            }
            return false;
        }
        public override void CompPostTick(ref float severityAdjustment) {
            base.CompPostTick(ref severityAdjustment);

            Pawn pawn = this.Pawn;
            if (pawn?.apparel?.WornApparel == null)
                return;

            foreach (var apparel in pawn.apparel.WornApparel) {
                var comp = apparel.GetComp<CompApparelFabrialDiminisher>();
                if (comp == null)
                    continue;

                if (!comp.checkPower()) {
                    var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(StormlightModDefs.whtwl_apparel_painrial_diminisher_hediff);
                    if (hediff != null) {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
                return;
            }

            // If none of the worn Fabrials are powered, remove this Hediff
            if (!pawn.Dead) {
                pawn.health.RemoveHediff(parent);
            }
        }

        public override string CompLabelInBracketsExtra {
            get {
                var comp = Pawn?.apparel?.WornApparel?
                    .FirstOrDefault(app => app.GetComp<CompApparelFabrialDiminisher>() != null)?
                    .GetComp<CompApparelFabrialDiminisher>();

                if (comp?.insertedGemstone?.TryGetComp<CompStormlight>() is CompStormlight stormlight) {
                    return $"Stormlight: {stormlight.Stormlight:F0}";
                }

                return null;
            }
        }
    }

    public class HediffCompProperties_FabrialPainDiminisher : HediffCompProperties {
        public HediffCompProperties_FabrialPainDiminisher() {
            this.compClass = typeof(HediffComp_FabrialPainDiminisher);
        }
    }
}


namespace StormlightMod {

    [HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]
    public static class Patch_JobOnThing_BlockBadGems {
        static bool Prefix(ref Job __result, Pawn pawn, Thing thing, bool forced, WorkGiver_DoBill __instance) {
            IBillGiver billGiver = thing as IBillGiver;
            if (billGiver == null || !billGiver.BillStack.AnyShouldDoNow)
                return true;

            foreach (Bill bill in billGiver.BillStack) {
                if (bill.recipe.defName != "whtwl_Make_Apparel_Fabrial_Painrial_Diminisher")
                    continue;

                bool hasValidGem = false;

                foreach (Thing gem in pawn.Map.listerThings.ThingsOfDef(ThingDef.Named("whtwl_CutEmerald"))) {
                    if (!pawn.CanReserveAndReach(gem, PathEndMode.ClosestTouch, Danger.Deadly))
                        continue;

                    var comp = gem.TryGetComp<CompCutGemstone>();
                    if (comp != null && comp.capturedSpren == Spren.Pain) {
                        hasValidGem = true;
                        break;
                    }
                }

                if (!hasValidGem) {
                    __result = null;
                    return false;
                }
            }

            return true;
        }
    }
}
