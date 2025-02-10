using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace StormlightMod {
    public class CompProperties_AbilitySpawnEquipment : CompProperties_AbilityEffect {
        public ThingDef thingDef; // The weapon to spawn


        public CompProperties_AbilitySpawnEquipment() {
            this.compClass = typeof(CompAbilityEffect_SpawnEquipment);
        }
    }

    public class CompProperties_AbilityBreakBond : CompProperties_AbilityEffect {
        public ThingDef thingDef; // The weapon to spawn

        public CompProperties_AbilityBreakBond() {
            this.compClass = typeof(CompAbilityEffect_BreakBondWithSword);
        }
    }

    ///-----------------------------------------------------------------------///

    public class CompAbilityEffect_SpawnEquipment : CompAbilityEffect {
        public new CompProperties_AbilitySpawnEquipment Props => (CompProperties_AbilitySpawnEquipment)this.props;

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

            if (checkAndDropWeapon(ref pawn) == false) {
                return;
            }

            toggleBlade(ref pawn);
        }

        private bool checkAndDropWeapon(ref Pawn pawn) {
            if (pawn.equipment.Primary != null) {
                // Drop the existing weapon
                ThingWithComps droppedWeapon;
                pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out droppedWeapon, pawn.Position, forbid: false);
                if (pawn.equipment.Primary.def.defName.Equals("MeleeWeapon_Shardblade")) { //replace check to check droppedWeapon instead.
                    return false;
                }
            }
            return true;
        }

        private void toggleBlade(ref Pawn pawn) {
            CompShardblade blade = pawn.GetComp<CompShardblade>(); 

            if (blade != null && blade.Props.isSpawned == false) {
                Log.Message($"Radiant {pawn.Name} summoned his shard blade!");
                Props.blade.summon();
            }
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
                ThingWithComps droppedWeapon;
                if (pawn.equipment.Primary.def.defName.Equals("MeleeWeapon_Shardblade")) {
                    CompShardblade blade = pawn.equipment.Primary.GetComp<CompShardblade>();
                    blade.severBond();
                    pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out droppedWeapon, pawn.Position, forbid: false);
                }
            }
        }


    }
}
