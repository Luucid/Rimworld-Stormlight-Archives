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
        public Dictionary<string, List<Spren>> gemstonePossibleSprenDict = new Dictionary<string, List<Spren>>() { };

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            compTrapper = GetComp<CompSprenTrapper>();
            compGlower = GetComp<CompGlower>();
            gemstonePossibleSprenDict.Add(StormlightModDefs.whtwl_CutDiamond.defName, new List<Spren> { Spren.Logic, Spren.Light, Spren.Exhaustion });
            gemstonePossibleSprenDict.Add(StormlightModDefs.whtwl_CutGarnet.defName, new List<Spren> { Spren.Rain, Spren.Exhaustion, Spren.Pain });
            gemstonePossibleSprenDict.Add(StormlightModDefs.whtwl_CutSapphire.defName, new List<Spren> { Spren.Wind, Spren.Motion, Spren.Cold });
            gemstonePossibleSprenDict.Add(StormlightModDefs.whtwl_CutRuby.defName, new List<Spren> { Spren.Flame, Spren.Anger });
            gemstonePossibleSprenDict.Add(StormlightModDefs.whtwl_CutEmerald.defName, new List<Spren> { Spren.Flame, Spren.Life, Spren.Cultivation, Spren.Rain, Spren.Glory });
        }

        public override void TickRare() {
            CaptureLoop();
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

        private void CaptureLoop() {
            if (compTrapper.insertedGemstone != null && compTrapper.sprenCaptured == false) {
                List<Spren> sprenList;
                gemstonePossibleSprenDict.TryGetValue(compTrapper.insertedGemstone.def.defName, out sprenList);
                foreach (Spren spren in sprenList) {
                    compTrapper.tryCaptureSpren(spren);
                    compTrapper.checkTrapperState();
                    if (compTrapper.sprenCaptured) break;
                }
            }
            compTrapper.checkTrapperState();
        }

    }



    public class CompSprenTrapper : ThingComp, IGemstoneHandler {
        public CompProperties_SprenTrapper Props => (CompProperties_SprenTrapper)props;
        public Thing insertedGemstone = null;
        public bool sprenCaptured = false;

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
        }

      
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
            Scribe_Values.Look(ref sprenCaptured, "sprenCaptured");
        }
        public void tryCaptureSpren(Spren targetSpren) {

            if (insertedGemstone == null || targetSpren == Spren.None || sprenCaptured == true) return;
            switch (targetSpren) {
                case Spren.Flame:
                    tryCaptureFlameSpren();
                    break;
                case Spren.Cold:
                    tryCaptureColdSpren();
                    break;    
                case Spren.Pain:
                    tryCapturePainSpren();
                    break;
                default:
                    break;
            }
        }
        private void displayCaptureMessage(Spren spren) {
            Messages.Message($"One of your traps captured a {spren.ToStringSafe()}spren!", parent, MessageTypeDefOf.PositiveEvent);
        }

        private float getProbability(CompStormlight stormlightComp, float probabilityFactor, float baseMaxNormalizedValue) {

            float baseProbability = StormlightUtilities.SprenBaseCaptureProbability(stormlightComp.Stormlight, 0f, stormlightComp.CurrentMaxStormlight);
            float probability = StormlightUtilities.Normalize(baseProbability, 0f, 100f, 0f, baseMaxNormalizedValue);
            return probability * probabilityFactor;
        }

        private void tryCaptureFlameSpren() {

            if (StormlightUtilities.IsAnyFireNearby(parent as Building, 3f)) {
                var stormlightcomp = insertedGemstone.TryGetComp<CompStormlight>();
                if (stormlightcomp != null) {
                    float probabilityFactor = (float)StormlightUtilities.GetNumberOfFiresNearby(parent as Building, 3f);
                    float probability = getProbability(stormlightcomp, probabilityFactor, 0.1f);

                    if (Rand.Chance(probability)) {
                        insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Flame;
                        stormlightcomp.RemoveAllStormlight(); 
                        displayCaptureMessage(Spren.Flame);
                    }
                }
            }

        }

        private void tryCaptureColdSpren() {
            float averageSuroundingTemperature = StormlightUtilities.GetAverageSuroundingTemperature(parent as Building, 3f);
            if (averageSuroundingTemperature < 0f) {
                var stormlightcomp = insertedGemstone.TryGetComp<CompStormlight>();
                if (stormlightcomp != null) {
                    float probabilityFactor = Mathf.Abs(averageSuroundingTemperature);
                    float probability = getProbability(stormlightcomp, probabilityFactor, 0.01f);

                    if (Rand.Chance(probability)) {
                        insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Cold;
                        displayCaptureMessage(Spren.Cold);
                        stormlightcomp.RemoveAllStormlight();
                    }
                }
            }
        }



        private void tryCapturePainSpren() {
            float sumOfPain = StormlightUtilities.GetSuroundingPain(parent as Building, 3f);
            if (sumOfPain > 0f) {
                var stormlightcomp = insertedGemstone.TryGetComp<CompStormlight>();
                if (stormlightcomp != null) {
                    float probabilityFactor = sumOfPain*5;
                    float probability = getProbability(stormlightcomp, probabilityFactor, 0.01f);

                    if (Rand.Chance(probability)) {
                        insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren = Spren.Pain; 
                        displayCaptureMessage(Spren.Pain);
                        stormlightcomp.RemoveAllStormlight(); 
                    }
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
