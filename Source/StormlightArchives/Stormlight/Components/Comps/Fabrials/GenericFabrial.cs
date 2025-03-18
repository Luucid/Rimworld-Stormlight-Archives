using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using UnityEngine;
using Verse.AI;

namespace StormlightMod {

    public class Building_Fabrial_Basic_Augmenter : Building {
        public CompBasicFabrialAugumenter compBasicFabrialAugumenter;
        public CompFlickable compFlickerable;
        public CompGlower compGlower;


        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            compBasicFabrialAugumenter = GetComp<CompBasicFabrialAugumenter>();
            compFlickerable = GetComp<CompFlickable>();
            compGlower = GetComp<CompGlower>();
        }
        public override void TickRare() {
            compBasicFabrialAugumenter.checkPower(compFlickerable.SwitchIsOn);
            if (compBasicFabrialAugumenter.PowerOn) {
                float ambientTemperature = base.AmbientTemperature;
                float num = ((ambientTemperature < 20f) ? 1f : ((!(ambientTemperature > 120f)) ? Mathf.InverseLerp(120f, 20f, ambientTemperature) : 0f));
                float num2 = GenTemperature.ControlTemperatureTempChange(this.Position, this.Map, 15f, 18f);
                bool flag = !Mathf.Approximately(num2, 0f);
                if (flag) {
                    this.GetRoom().Temperature += num2;
                }
            }
            toggleGlow(compBasicFabrialAugumenter.PowerOn);
            compBasicFabrialAugumenter.usePower();
        }
        private void toggleGlow(bool on) {
            if (this.Map != null) {
                if (on) {
                    this.Map.glowGrid.RegisterGlower(compGlower);
                }
                else {
                    this.Map.glowGrid.DeRegisterGlower(compGlower);
                }
            }
        }

        public override void Print(SectionLayer layer) {
            base.Print(layer);
            if (compBasicFabrialAugumenter.HasGemstone) {
                if (compBasicFabrialAugumenter.insertedGemstone.def == StormlightModDefs.whtwl_CutRuby) {
                    this.def.graphicData.attachments[0].Graphic.Print(layer, this, 0f);
                }
                //else if(...)
            }
        }
    }

    public class CompBasicFabrialAugumenter : ThingComp, IGemstoneHandler {
        public CompProperties_BasicFabrialAugmenter Props => (CompProperties_BasicFabrialAugmenter)props;
        public CompGlower GlowerComp => parent.GetComp<CompGlower>();
        public Thing insertedGemstone = null;
        public bool PowerOn = false;
        public bool HasGemstone => insertedGemstone != null;

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_References.Look(ref insertedGemstone, "insertedGemstone");
        }

        public void checkPower(bool flickeredOn) {
            if (insertedGemstone != null) {
                var stormlightComp = (insertedGemstone as ThingWithComps).GetComp<CompStormlight>();
                if (stormlightComp != null) {
                    PowerOn = stormlightComp.HasStormlight && flickeredOn;
                    return;
                }
            }
            PowerOn = false;
        }

        public void usePower() {
            if (PowerOn && insertedGemstone != null && insertedGemstone.TryGetComp<CompStormlight>() is CompStormlight stormlightComp) {
                stormlightComp.drainStormLight(7f);
                switch (insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren) {
                    case Spren.Flame:
                        doFlameSprenPower();
                        break;
                    case Spren.Cold:
                        doColdSprenPower();
                        break;
                    default:
                        break;

                }
            }
        }

        private void doFlameSprenPower() 
        {
        
        }
        private void doColdSprenPower() {

        }


        public void AddGemstone(ThingWithComps gemstone) {
            var gemstoneComp = gemstone.GetComp<CompCutGemstone>();
            if (gemstoneComp != null) {
                insertedGemstone = gemstoneComp.parent;
            }
        }

        public void RemoveGemstone() {
            if (insertedGemstone != null) {
                var gemstoneToDrop = insertedGemstone;
                insertedGemstone = null;
                IntVec3 dropPosition = parent.Position;
                dropPosition.z -= 1;
                GenPlace.TryPlaceThing(gemstoneToDrop, dropPosition, parent.Map, ThingPlaceMode.Near);

            }
        }
        public override string CompInspectStringExtra() {
            string gemName = "No gem in fabrial.";

            if (insertedGemstone != null) {
                ThingWithComps gemstone = insertedGemstone as ThingWithComps;
                gemName = gemstone.GetComp<CompCutGemstone>().GetFullLabel + "(" + gemstone.GetComp<CompStormlight>().Stormlight.ToString("F0") + ")";
            }
            return gemName;
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {

            var cutGemstone = GenClosest.ClosestThing_Global(
                   selPawn.Position,
                   selPawn.Map.listerThings.AllThings.Where(
                       thing => (thing.def == StormlightModDefs.whtwl_CutRuby
                     || thing.def == StormlightModDefs.whtwl_CutEmerald
                     || thing.def == StormlightModDefs.whtwl_CutDiamond
                     || thing.def == StormlightModDefs.whtwl_CutSapphire
                     || thing.def == StormlightModDefs.whtwl_CutGarnet)), 500f);

            Action replaceGemAction = null;
            string replaceGemText = "No suitable gem available";
            if (cutGemstone != null) {

                replaceGemAction = () => {
                    Job job = JobMaker.MakeJob(StormlightModDefs.whtwl_RefuelFabrial, parent, cutGemstone);
                    if (job.TryMakePreToilReservations(selPawn, errorOnFailed: true)) {
                        selPawn.jobs.TryTakeOrderedJob(job);
                    }
                };
                replaceGemText = $"Replace with {cutGemstone.Label}";
            }
            yield return new FloatMenuOption(replaceGemText, replaceGemAction);


            Action removeGemAction = null;
            string removeGemText = "Remove Gemstone";
            if (insertedGemstone != null) {

                removeGemAction = () => {
                    Job job = JobMaker.MakeJob(StormlightModDefs.whtwl_RemoveFromFabrial, parent);
                    if (job.TryMakePreToilReservations(selPawn, errorOnFailed: true)) {
                        selPawn.jobs.TryTakeOrderedJob(job);
                    }
                };
            }
            yield return new FloatMenuOption(removeGemText, removeGemAction);




        }
    }

    public class CompProperties_BasicFabrialAugmenter : CompProperties {
        public CompProperties_BasicFabrialAugmenter() {
            this.compClass = typeof(CompBasicFabrialAugumenter);
        }
    }
}
