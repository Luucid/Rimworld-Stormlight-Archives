using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using UnityEngine;
using Verse.AI;
using Verse.Noise;

namespace StormlightMod {



    public class Building_SprenTrapper : Building {
        public CompSprenTrapper compTrapper;
        public CompGlower compGlower;

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            compTrapper = GetComp<CompSprenTrapper>();
            compGlower = GetComp<CompGlower>();
        }

        public override void TickRare() {
            compTrapper.tryCaptureSpren();
            compTrapper.checkTrapperState();
            toggleGlow(compTrapper.sprenCaptured);
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


    }



    public class CompSprenTrapper : ThingComp, IGemstoneHandler {
        public CompProperties_SprenTrapper Props => (CompProperties_SprenTrapper)props;
        public Thing insertedGemstone = null;
        public bool sprenCaptured = false;
        public Spren targetSpren = Spren.None;

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostExposeData() {
            base.PostExposeData();
        }

        public void tryCaptureSpren() {

            if (insertedGemstone == null || targetSpren == Spren.None || sprenCaptured == true) return;

            switch (targetSpren) {
                case Spren.Flame:
                    tryCaptureFlameSpren();
                    break;
                case Spren.Cold:
                    tryCaptureColdSpren();
                    break;
                default:
                    break;
            }
        }

        private void tryCaptureFlameSpren() {
            if (StormlightUtilities.IsAnyFireNearby(parent as Building, 3f)) {

                const float optimalStormlight = 0.7f;
                const float sigma = 0.15f;
                var stormlightcomp = insertedGemstone.TryGetComp<CompStormlight>();
                if (stormlightcomp != null) {
                    float normalizedStormlight = stormlightcomp.Stormlight / stormlightcomp.CurrentMaxStormlight;

                    float probability = Mathf.Exp(-Mathf.Pow(normalizedStormlight - optimalStormlight, 2) / (2 * sigma * sigma));

                    probability = Mathf.Clamp(probability, 0.001f, 0.20f);

                    if (Rand.Chance(probability)) {
                        Log.Message($"Captured FlameSpren! Probability was: {probability * 100}%");
                        insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Flame;
                    }
                    Log.Message("Fire nearby!");
                }
            }
        }

        private void tryCaptureColdSpren() {
            float averageSuroundingTemperature = StormlightUtilities.GetAverageSuroundingTemperature(parent as Building, 3f);
            if (averageSuroundingTemperature < 0f) { 
                const float optimalStormlight = 0.7f;
                const float sigma = 0.15f;
                var stormlightcomp = insertedGemstone.TryGetComp<CompStormlight>();
                if (stormlightcomp != null) {
                    float normalizedStormlight = stormlightcomp.Stormlight / stormlightcomp.CurrentMaxStormlight;
                    float probability = Mathf.Exp(-Mathf.Pow(normalizedStormlight - optimalStormlight, 2) / (2 * sigma * sigma));
                    probability = Mathf.Clamp(probability, 0.01f, Mathf.Abs(averageSuroundingTemperature)/100f);
                    if (Rand.Chance(probability)) {
                        Log.Message($"Captured ColdSpren! Probability was: {probability * 100}%");
                        insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Cold;
                    }
                    Log.Message($"Average Temperature: {averageSuroundingTemperature}, probability: {probability * 100}%"); 
                }
            }
        }

        public void checkTrapperState() {
            if (insertedGemstone != null && insertedGemstone.TryGetComp<CompCutGemstone>() is CompCutGemstone compCutGemstone) {
                if (compCutGemstone.capturedSpren != Spren.None) {
                    sprenCaptured = true;
                    return;
                }
            }
            sprenCaptured = false;
        }

        public void InstallCage(ThingWithComps cage) {
        }

        public void AddGemstone(ThingWithComps gemstone) {
            var gemstoneComp = gemstone.GetComp<CompCutGemstone>();
            if (gemstoneComp != null) {
                insertedGemstone = gemstoneComp.parent;
                if (insertedGemstone.def == StormlightModDefs.whtwl_CutSapphire) 
                {
                    targetSpren = Spren.Cold;
                }
                else if (insertedGemstone.def == StormlightModDefs.whtwl_CutRuby || insertedGemstone.def == StormlightModDefs.whtwl_CutEmerald) {
                    targetSpren = Spren.Flame; 
                } 
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
                   selPawn.Map.listerThings.AllThings.Where(thing => (StormlightUtilities.isThingCutGemstone(thing)) && (thing.TryGetComp<CompCutGemstone>().HasSprenInside == false) && (thing.TryGetComp<CompStormlight>().HasStormlight)), 500f);


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

    public class CompProperties_SprenTrapper : CompProperties {
        public CompProperties_SprenTrapper() {
            this.compClass = typeof(CompSprenTrapper);
        }
    }

}
