using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using UnityEngine;
using Verse.AI;
using LudeonTK;

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
        public override void Tick() {
            compBasicFabrialAugumenter.checkPower(compFlickerable.SwitchIsOn);
            toggleGlow(compBasicFabrialAugumenter.PowerOn);
            compBasicFabrialAugumenter.usePower();
        }
        private void toggleGlow(bool on) {
            if (this.Map != null) {
                if (on) {
                    var stormlightComp = (compBasicFabrialAugumenter.insertedGemstone as ThingWithComps).GetComp<CompStormlight>();
                    compGlower.GlowRadius = stormlightComp.MaximumGlowRadius;
                    compGlower.GlowColor = stormlightComp.GlowerComp.GlowColor;
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
                if (compBasicFabrialAugumenter.insertedGemstone.def == StormlightModDefs.whtwl_CutRuby) { this.def.graphicData.attachments[0].Graphic.Print(layer, this, 0f); }
                else if (compBasicFabrialAugumenter.insertedGemstone.def == StormlightModDefs.whtwl_CutDiamond) { this.def.graphicData.attachments[1].Graphic.Print(layer, this, 0f); }
                else if (compBasicFabrialAugumenter.insertedGemstone.def == StormlightModDefs.whtwl_CutGarnet) { this.def.graphicData.attachments[2].Graphic.Print(layer, this, 0f); }
                else if (compBasicFabrialAugumenter.insertedGemstone.def == StormlightModDefs.whtwl_CutEmerald) { this.def.graphicData.attachments[3].Graphic.Print(layer, this, 0f); }
                else if (compBasicFabrialAugumenter.insertedGemstone.def == StormlightModDefs.whtwl_CutSapphire) { this.def.graphicData.attachments[4].Graphic.Print(layer, this, 0f); }
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
            if (insertedGemstone != null) {
                CultivationSprenPatch.RegisterBuilding(this.parent as Building_Fabrial_Basic_Augmenter);
            }
        }

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
            Scribe_Values.Look(ref PowerOn, "PowerOn");
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

        public Spren CurrentSpren {
            get {
                if (HasGemstone) {
                    return insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren;
                }
                return Spren.None;
            }
        }

        public void usePower() {
            if (PowerOn && insertedGemstone != null && insertedGemstone.TryGetComp<CompStormlight>() is CompStormlight stormlightComp) {
                stormlightComp.drainFactor = 2f;
                stormlightComp.CompTick();
                switch (insertedGemstone.TryGetComp<CompCutGemstone>().capturedSpren) {
                    case Spren.Flame:
                        doFlameSprenPower();
                        break;
                    case Spren.Cold:
                        doColdSprenPower();
                        break;
                    case Spren.Pain:
                        doPainSprenPower();
                        break;
                    case Spren.Logic:        //diamond
                        doLogicSprenPower();
                        break;
                    case Spren.Life:  //emerald
                        //Handled by patch
                        break;
                    default:
                        break;

                }
            }
        }


        private void doFlameSprenPower() {
            if (PowerOn) {
                float maxEnergy = insertedGemstone.TryGetComp<CompCutGemstone>().gemstoneSize * 3; //3, 15, 60
                float targetTemp = insertedGemstone.TryGetComp<CompCutGemstone>().gemstoneSize; //1, 5, 20
                if (targetTemp < 20f) targetTemp *= 1.25f;
                float ambientTemperature = parent.AmbientTemperature;
                float num2 = GenTemperature.ControlTemperatureTempChange(parent.Position, parent.Map, maxEnergy, targetTemp);
                bool flag = !Mathf.Approximately(num2, 0f);
                if (flag) {
                    parent.GetRoom().Temperature += num2;
                }
            }
        }

        private void doColdSprenPower() {
            if (PowerOn && parent.IsOutside() == false) {
                int gemstoneSize = insertedGemstone.TryGetComp<CompCutGemstone>().gemstoneSize * 3; //3, 15, 60
                float targetTemp = -5f;
                float currentTemp = parent.GetRoom().Temperature;
                if (currentTemp > targetTemp) {
                    GenTemperature.PushHeat(parent.Position, parent.Map, (0f - gemstoneSize));
                    Log.Message($"Cooler target temp: {targetTemp}, pushing: {(0f - gemstoneSize)}");
                }
            }
        }

        private void doPainSprenPower() {
            if (PowerOn) {
                IntVec3 position = parent.Position;
                Map map = parent.Map;
                var cells = GenRadial.RadialCellsAround(position, 5f, true);
                foreach (IntVec3 cell in cells) {
                    Pawn pawn = cell.GetFirstPawn(map);
                    if (pawn != null && pawn.health.hediffSet.GetFirstHediffOfDef(StormlightModDefs.whtwl_painrial_agument) == null && pawn.Position.InHorDistOf(position, 5f)) {
                        pawn.health.AddHediff(StormlightModDefs.whtwl_painrial_agument);
                    }
                }
            }
        }

        private void doLogicSprenPower() {
            if (PowerOn) {
                IntVec3 position = parent.Position;
                Map map = parent.Map;
                var cells = GenRadial.RadialCellsAround(position, 5f, true);
                foreach (IntVec3 cell in cells) {
                    foreach (Thing thing in cell.GetThingList(map)) {
                        Pawn pawn = cell.GetFirstPawn(map);
                        if (pawn != null && pawn.health.hediffSet.GetFirstHediffOfDef(StormlightModDefs.whtwl_logirial_agument) == null && pawn.Position.InHorDistOf(position, 5f)) {
                            pawn.health.AddHediff(StormlightModDefs.whtwl_logirial_agument);
                        }
                    }
                }
            }
        }
        public void AddGemstone(ThingWithComps gemstone) {
            var gemstoneComp = gemstone.GetComp<CompCutGemstone>();
            if (gemstoneComp != null) {
                insertedGemstone = gemstoneComp.parent;
                CultivationSprenPatch.RegisterBuilding(this.parent as Building_Fabrial_Basic_Augmenter);
            }
        }

        public void RemoveGemstone() {
            if (insertedGemstone != null) {
                var gemstoneToDrop = insertedGemstone;
                insertedGemstone = null;
                IntVec3 dropPosition = parent.Position;
                dropPosition.z -= 1;
                GenPlace.TryPlaceThing(gemstoneToDrop, dropPosition, parent.Map, ThingPlaceMode.Near);
                CultivationSprenPatch.UnregisterBuilding(this.parent as Building_Fabrial_Basic_Augmenter);
            }
        }
        public override string CompInspectStringExtra() {
            string gemName = "No gem in fabrial.";

            if (insertedGemstone != null) {
                ThingWithComps gemstone = insertedGemstone as ThingWithComps;
                gemName = "Spren: " + gemstone.GetComp<CompCutGemstone>().capturedSpren.ToString() + "\nStormlight: " + gemstone.GetComp<CompStormlight>().Stormlight.ToString("F0");
            }
            return gemName;
        }

        //public string getHoursRemaining() 
        //    {
        //    var stormlightComp = insertedGemstone.TryGetComp<CompStormlight>();
        //    float stormlightPerHour = (50.0f * stormlightComp.GetDrainRate(2f));
        //    int hoursLeft = (int)(stormlightComp.Stormlight / stormlightPerHour);
        //    return hoursLeft.ToString();
        //}

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {

            var cutGemstone = GenClosest.ClosestThing_Global(
                   selPawn.Position,
                   selPawn.Map.listerThings.AllThings.Where(
                       thing => (StormlightUtilities.isThingCutGemstone(thing)) && (thing.TryGetComp<CompCutGemstone>().HasSprenInside && thing.TryGetComp<CompStormlight>().HasStormlight)), 500f);

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
