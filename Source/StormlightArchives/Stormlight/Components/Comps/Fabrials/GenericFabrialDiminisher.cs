using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using UnityEngine;
using Verse.AI;
using LudeonTK;
using Verse.Noise;

namespace StormlightMod {

    public class Building_Fabrial_Basic_Diminisher : Building {
        public CompBasicFabrialDiminisher compBasicFabrialDiminisher;
        public CompFlickable compFlickerable;
        public CompGlower compGlower;


        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            compBasicFabrialDiminisher = GetComp<CompBasicFabrialDiminisher>();
            compFlickerable = GetComp<CompFlickable>();
            compGlower = GetComp<CompGlower>();
        }
        public override void Tick() {
            compBasicFabrialDiminisher.checkPower(compFlickerable.SwitchIsOn);
            toggleGlow(compBasicFabrialDiminisher.PowerOn);
            compBasicFabrialDiminisher.usePower();
        }
        private void toggleGlow(bool on) {
            if (this.Map != null) {
                if (on) {
                    var stormlightComp = (compBasicFabrialDiminisher.insertedGemstone as ThingWithComps).GetComp<CompStormlight>();
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
            if (compBasicFabrialDiminisher.HasGemstone) {
                if (compBasicFabrialDiminisher.insertedGemstone.def == StormlightModDefs.whtwl_CutRuby) { this.def.graphicData.attachments[0].Graphic.Print(layer, this, 0f); }
                else if (compBasicFabrialDiminisher.insertedGemstone.def == StormlightModDefs.whtwl_CutDiamond) { this.def.graphicData.attachments[1].Graphic.Print(layer, this, 0f); }
                else if (compBasicFabrialDiminisher.insertedGemstone.def == StormlightModDefs.whtwl_CutGarnet) { this.def.graphicData.attachments[2].Graphic.Print(layer, this, 0f); }
                else if (compBasicFabrialDiminisher.insertedGemstone.def == StormlightModDefs.whtwl_CutEmerald) { this.def.graphicData.attachments[3].Graphic.Print(layer, this, 0f); }
                else if (compBasicFabrialDiminisher.insertedGemstone.def == StormlightModDefs.whtwl_CutSapphire) { this.def.graphicData.attachments[4].Graphic.Print(layer, this, 0f); }
            }
        }
    }

    public class CompBasicFabrialDiminisher : ThingComp, IGemstoneHandler {
        public CompProperties_BasicFabrialDiminisher Props => (CompProperties_BasicFabrialDiminisher)props;
        public CompGlower GlowerComp => parent.GetComp<CompGlower>();
        public Thing insertedGemstone = null;
        public bool PowerOn = false;
        public bool HasGemstone => insertedGemstone != null;
        private float TempWhenTurnedOn = 0f;


        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
            if (insertedGemstone != null) {
                CultivationSprenPatch.RegisterBuilding(this.parent as Building_Fabrial_Basic_Diminisher);
            }
        }

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
            Scribe_Values.Look(ref PowerOn, "PowerOn");
            Scribe_Values.Look(ref TempWhenTurnedOn, "TempWhenTurnedOn", 0f);
        }

        public void checkPower(bool flickeredOn) {
            if (insertedGemstone != null) {
                var stormlightComp = (insertedGemstone as ThingWithComps).GetComp<CompStormlight>();
                if (stormlightComp != null) {
                    bool power = stormlightComp.HasStormlight && flickeredOn;
                    if (!PowerOn && power) {
                        TempWhenTurnedOn = parent.GetRoom().Temperature;
                    }
                    PowerOn = power;
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
            if (PowerOn && parent.IsOutside() == false) { 
                int gemstoneSize = insertedGemstone.TryGetComp<CompCutGemstone>().gemstoneSize * 3; //3, 15, 60
                float targetTemp = TempWhenTurnedOn;
                float currentTemp = parent.GetRoom().Temperature;
                if (currentTemp > TempWhenTurnedOn) {
                    GenTemperature.PushHeat(parent.Position, parent.Map, (0f - gemstoneSize));
                }
            }
        }

        private void doColdSprenPower() {
            if (PowerOn && parent.IsOutside() == false) {
                int gemstoneSize = insertedGemstone.TryGetComp<CompCutGemstone>().gemstoneSize * 3; //3, 15, 60
                float targetTemp = TempWhenTurnedOn;
                float currentTemp = parent.GetRoom().Temperature;
                if (currentTemp < targetTemp) {
                    GenTemperature.PushHeat(parent.Position, parent.Map, gemstoneSize);
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
                    if (pawn != null && pawn.health.hediffSet.GetFirstHediffOfDef(StormlightModDefs.whtwl_painrial_diminisher) == null && pawn.Position.InHorDistOf(position, 5f)) {
                        pawn.health.AddHediff(StormlightModDefs.whtwl_painrial_diminisher);
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
                        if (pawn != null && pawn.health.hediffSet.GetFirstHediffOfDef(StormlightModDefs.whtwl_logirial_diminisher) == null && pawn.Position.InHorDistOf(position, 5f)) {
                            pawn.health.AddHediff(StormlightModDefs.whtwl_logirial_diminisher);
                        }
                    }
                }
            }
        }
        public void AddGemstone(ThingWithComps gemstone) {
            var gemstoneComp = gemstone.GetComp<CompCutGemstone>();
            if (gemstoneComp != null) {
                insertedGemstone = gemstoneComp.parent;
                CultivationSprenPatch.RegisterBuilding(this.parent as Building_Fabrial_Basic_Diminisher);
            }
        }

        public void RemoveGemstone() {
            if (insertedGemstone != null) {
                var gemstoneToDrop = insertedGemstone;
                insertedGemstone = null;
                IntVec3 dropPosition = parent.Position;
                dropPosition.z -= 1;
                GenPlace.TryPlaceThing(gemstoneToDrop, dropPosition, parent.Map, ThingPlaceMode.Near);
                CultivationSprenPatch.UnregisterBuilding(this.parent as Building_Fabrial_Basic_Diminisher);
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

    public class CompProperties_BasicFabrialDiminisher : CompProperties {
        public CompProperties_BasicFabrialDiminisher() {
            this.compClass = typeof(CompBasicFabrialDiminisher);
        }
    }
}
