using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace StormlightMod {

    /// LASH UP ABILITY
    public class CompProperties_AbilityLashUpward : CompProperties_AbilityEffect {
        public ThingDef thingDef;
        public float stormLightCost;

        public CompProperties_AbilityLashUpward() {
            this.compClass = typeof(CompAbilityEffect_AbilityLashUpward);
        }
    }
    public class CompAbilityEffect_AbilityLashUpward : CompAbilityEffect {
        public new CompProperties_AbilityLashUpward Props => this.props as CompProperties_AbilityLashUpward;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            // 1) Validate target
            if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
                Log.Warning("[LashUpward] Invalid target.");
                return;
            }

            // 2) Validate the "flyer" ThingDef
            if (Props.thingDef == null) {
                Log.Error("[LashUpward] No valid flyer ThingDef to spawn!");
                return;
            }

            Pawn caster = this.parent.pawn as Pawn;
            if (caster == null) {
                return;
            }

            if (caster.GetComp<CompStormlight>() == null || caster.GetComp<CompStormlight>().Stormlight < Props.stormLightCost) {
                Log.Message($"[LashUpward] not enough stormlight!");
                return;
            }
            caster.GetComp<CompStormlight>().drawStormlight(Props.stormLightCost);

            // 3) Fling the target
            flightFunction(target.Thing);
        }



        private void flightFunction(Thing targetThing) {
            Map map = targetThing.Map;
            IntVec3 cell = targetThing.Position;

            Pawn targetPawn = targetThing as Pawn;
            if (targetPawn == null) {
                return;
            }
            Log.Message($"TargetPawn: {targetPawn.Name}");
            // Create the custom flyer
            PawnFlyer flyer = PawnFlyer.MakeFlyer(
              flyingDef: Props.thingDef,    // must have the <pawnFlyer> XML extension
              pawn: targetPawn,             // the Pawn to fly
              destCell: cell,               // an IntVec3 on the same map
              flightEffecterDef: null,      // optional visual effect
              landingSound: null,           // optional landing sound
              flyWithCarriedThing: false,   // whether the pawn’s carried item should come along
              overrideStartVec: null,       // or a custom Vector3 start pos
              triggeringAbility: null,      // pass an Ability if relevant
              target: LocalTargetInfo.Invalid
          );

            // Finally, spawn the flyer in the same position
            GenSpawn.Spawn(flyer, cell, map);
            RadiantUtility.GiveRadiantXP(targetPawn, 50f);
        }
    }




}
