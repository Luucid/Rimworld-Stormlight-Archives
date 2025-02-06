using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace StormlightMod {
    public class CompProperties_AbilitySpawnEquipment : CompProperties_AbilityEffect {
        public ThingDef thingDef; // The weapon to spawn
        public bool onlyIfNoWeaponEquipped = true; // Prevent duplicate weapons

        public CompProperties_AbilitySpawnEquipment() {
            this.compClass = typeof(CompAbilityEffect_SpawnEquipment);
        }
    }

    public class CompAbilityEffect_SpawnEquipment : CompAbilityEffect {
        public new CompProperties_AbilitySpawnEquipment Props => (CompProperties_AbilitySpawnEquipment)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            // Ensure the target is valid; default to the caster if null
            if (target == null || target.Cell == null) {
                Log.Warning("[StormlightMod] SpawnEquipment target is null, defaulting to caster.");
                target = new LocalTargetInfo(parent.pawn); // Default to the caster
            }

            // Ensure thingDef is not null
            if (Props.thingDef == null) {
                Log.Error("[StormlightMod] SpawnEquipment failed: thingDef not set.");
                return;
            }

            // Spawn the equipment
            try {
                Pawn pawn = parent.pawn;
                // Assign a default stuff material if the weapon is madeFromStuff
                ThingDef stuffDef = null;
                if (Props.thingDef.MadeFromStuff) {
                    stuffDef = DefDatabase<ThingDef>.GetNamed("ShardStuff", true);
                }
                // Create the equipment
                ThingWithComps equipment = (ThingWithComps)ThingMaker.MakeThing(Props.thingDef);

                // Check if the pawn already has equipment
                if (pawn.equipment.Primary != null) {
                    // Drop the existing weapon
                    ThingWithComps droppedWeapon;
                    pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out droppedWeapon, pawn.Position, forbid: false);
                    Log.Message($"[StormlightMod] Dropped existing weapon: {droppedWeapon.def.defName}");
                }

                // Equip the new weapon
                pawn.equipment.AddEquipment(equipment);
                Log.Message($"[StormlightMod] Equipped {Props.thingDef.defName} to {pawn.LabelCap}.");
            }
            catch (Exception ex) {
                Log.Error($"[StormlightMod] Exception in SpawnEquipment: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false) {
            if (target.Pawn == null)
                return false;

            // Check if pawn already has a weapon (if required)
            if (Props.onlyIfNoWeaponEquipped && target.Pawn.equipment.Primary != null) {
                if (throwMessages)
                    Messages.Message("You already have a weapon equipped!", target.Pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return base.Valid(target, throwMessages);
        }
    }


    public class CompProperties_AbilityDraft : CompProperties_AbilityEffect {
        public CompProperties_AbilityDraft() {
            this.compClass = typeof(CompAbilityEffect_Draft);
        }
    }

    public class CompAbilityEffect_Draft : CompAbilityEffect {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            base.Apply(target, dest);

            if (target.Pawn == null) {
                Log.Error("[StormlightMod] CompAbilityEffect_Draft failed: Target is null.");
                return;
            }

            Pawn pawn = target.Pawn;

            if (!pawn.Drafted) {
                pawn.drafter.Drafted = true;
                Messages.Message($"{pawn.LabelShort} is now battle-ready!", pawn, MessageTypeDefOf.PositiveEvent, false);
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false) {
            return target.Pawn != null && !target.Pawn.Downed && base.Valid(target, throwMessages);
        }
    }
}
