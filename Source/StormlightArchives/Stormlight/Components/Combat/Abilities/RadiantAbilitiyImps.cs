using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace StormlightMod {

    /// SUMMON BLADE ABILITY
    public class CompProperties_AbilitySpawnEquipment : CompProperties_AbilityEffect {
        public ThingDef thingDef; // The weapon to spawn


        public CompProperties_AbilitySpawnEquipment() {
            this.compClass = typeof(CompAbilityEffect_SpawnEquipment);
        }
    }

    public class CompAbilityEffect_SpawnEquipment : CompAbilityEffect {
        public new CompProperties_AbilitySpawnEquipment Props => (CompProperties_AbilitySpawnEquipment)this.props;

        public ThingWithComps bladeObject = null;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            if (target == null || target.Cell == null) {
                Log.Warning("[StormlightMod] SpawnEquipment target is null, defaulting to caster.");
                target = new LocalTargetInfo(parent.pawn); // Default to the caster
            }

            if (Props.thingDef == null) {
                Log.Error("[StormlightMod] SpawnEquipment failed: thingDef not set.");
                return;
            }

            Pawn pawn = parent.pawn;

            checkAndDropWeapon(ref pawn);
            toggleBlade(ref pawn);
        }

        private void checkAndDropWeapon(ref Pawn pawn) {
            if (pawn.equipment.Primary != null) {

                if (pawn.equipment.Primary.def.defName.Equals(Props.thingDef.ToString())) return;

                // Drop the existing weapon
                ThingWithComps droppedWeapon;
                pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out droppedWeapon, pawn.Position, forbid: false);

            }
        }

        private void toggleBlade(ref Pawn pawn) {
            CompAbilityEffect_SpawnEquipment abilityComp = pawn.GetAbilityComp<CompAbilityEffect_SpawnEquipment>("SummonShardblade");
            if (abilityComp == null) return;

            CompShardblade blade = abilityComp.bladeObject.GetComp<CompShardblade>();
            if (blade == null) return;

            if (blade.isBladeSpawned() == false) {
                Log.Message($"[StormlightMod] Radiant {pawn.Name} summoned shard blade!");
                blade.summon();
            }
            else {
                Log.Message($"[StormlightMod] Radiant {pawn.Name} dismissed the blade");
                blade.dismissBlade(pawn);
            }
        }
    }



    /// BREAK BOND ABILITY
    public class CompProperties_AbilityBreakBond : CompProperties_AbilityEffect {
        public ThingDef thingDef;

        public CompProperties_AbilityBreakBond() {
            this.compClass = typeof(CompAbilityEffect_BreakBondWithSword);
        }
    }
    public class CompAbilityEffect_BreakBondWithSword : CompAbilityEffect {
        public new CompProperties_AbilityBreakBond Props => (CompProperties_AbilityBreakBond)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            if (target == null || target.Cell == null) {
                Log.Warning("[StormlightMod] break bond target is null, defaulting to caster.");
                target = new LocalTargetInfo(parent.pawn); // Default to the caster
            }

            if (Props.thingDef == null) {
                Log.Error("[StormlightMod] break bond failed: thingDef not set.");
                return;
            }

            Pawn pawn = parent.pawn;
            checkAndDropWeapon(ref pawn);

        }

        private void checkAndDropWeapon(ref Pawn pawn) {
            if (pawn.equipment.Primary != null) {
                Log.Message("[StormlightMod] try to break bond.");

                if (pawn.equipment.Primary.def.defName.Equals(Props.thingDef.ToString())) {
                    CompShardblade blade = pawn.equipment.Primary.GetComp<CompShardblade>();
                    Log.Message($"[StormlightMod] {pawn.Name} unbonded its shardblade {blade.GetHashCode()}");
                    blade.severBond(pawn);
                }
                else {
                    Log.Error($"[StormlightMod] eq name is '{pawn.equipment.Primary.def.defName}', thingDef is '{Props.thingDef.ToString()}'");
                }
            }
        }


    }


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
            RadiantUtility.GiveRadiantXP(targetPawn, 1000f);
        }
    }


    /// WIND RUNNER FLIGHT
    public class CompProperties_AbilityWindRunnerFlight : CompProperties_AbilityEffect {
        public ThingDef thingDef;
        public float stormLightCost;

        public CompProperties_AbilityWindRunnerFlight() {
            this.compClass = typeof(CompAbilityEffect_AbilityWindRunnerFlight);
        }
    }
    public class CompAbilityEffect_AbilityWindRunnerFlight : CompAbilityEffect {
        public new CompProperties_AbilityWindRunnerFlight Props => this.props as CompProperties_AbilityWindRunnerFlight;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            // 1) Validate target
            if (target == null || target.Cell == null) {
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

            double distance = Math.Sqrt(Math.Pow(target.Cell.x - caster.Position.x, 2) + Math.Pow(target.Cell.z - caster.Position.z, 2));
            float totalCost = (float)(Props.stormLightCost * distance);

            Log.Message($"totalcost: {totalCost}, distance: {distance}, stormlight cost per tile: {Props.stormLightCost}");
            if (caster.GetComp<CompStormlight>() == null || caster.GetComp<CompStormlight>().Stormlight < totalCost) {
                Log.Message($"[LashUpward] not enough stormlight!");
                return;
            }
            caster.GetComp<CompStormlight>().drawStormlight(totalCost);

            // 3) Fling the target
            flightFunction(caster.Map, target.Cell, distance);
        }



        private void flightFunction(Map map, IntVec3 cell, double distance) {

            Pawn targetPawn = this.parent.pawn as Pawn;
            if (targetPawn == null) {
                return;
            }
            Log.Message($"TargetPawn: {targetPawn.Name}");
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
            RadiantUtility.GiveRadiantXP(targetPawn, (float)distance/10f);
        }

    }
}
