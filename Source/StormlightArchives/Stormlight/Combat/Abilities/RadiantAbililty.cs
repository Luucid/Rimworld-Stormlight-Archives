using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace StormlightMod {
    public class RadiantAbility : Ability {
        private Mote moteCast;

        private static readonly float MoteCastFadeTime = 0.4f;
        private static readonly float MoteCastScale = 1f;
        private static readonly Vector3 MoteCastOffset = new Vector3(0f, 0f, 0.48f);

        public RadiantAbility(Pawn pawn) : base(pawn) {
        }

        public RadiantAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
        }

        public override bool CanCast {
            get {
                if (!base.CanCast) {
                    return false;
                }
           
                if (!pawn.story.traits.HasTrait(TraitDef.Named("Radiant"))) {
                    return false;
                }

                return true;
            }
        }

        public override IEnumerable<Command> GetGizmos() {
            if (gizmo == null) {
                gizmo = new Command_RadiantAbility(this, pawn);
            }
            yield return gizmo;
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
            if (!CanApplyRadiantEffect(target)) {
                MoteMaker.ThrowText(target.CenterVector3, pawn.Map, "Target not valid for Radiant ability.");
                return false;
            }

            // Optionally add special effects
            if (def.HasAreaOfEffect) {
                FleckMaker.Static(target.Cell, pawn.Map, FleckDefOf.PsycastAreaEffect, def.EffectRadius);
            }

            return base.Activate(target, dest);
        }

        protected override void ApplyEffects(IEnumerable<CompAbilityEffect> effects, LocalTargetInfo target, LocalTargetInfo dest) {
            foreach (CompAbilityEffect effect in effects) {
                effect.Apply(target, dest);
            }
        }

        private bool CanApplyRadiantEffect(LocalTargetInfo target) {
            // Example logic to ensure the ability can be applied

            if (target.Pawn != null && !target.Pawn.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness)) {
                return false;
            }
            return true;
        }

        public override void AbilityTick() {
            base.AbilityTick();

            if (pawn.Spawned && base.Casting) {
                if (moteCast == null || moteCast.Destroyed) {
                    moteCast = MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_CastPsycast, MoteCastOffset, MoteCastScale, base.verb.verbProps.warmupTime - MoteCastFadeTime);
                }
                else {
                    moteCast.Maintain();
                }
            }
        }
    }

    public class Command_RadiantAbility : Command {
        private readonly Ability ability;
        private readonly Pawn pawn;

        public Command_RadiantAbility(Ability ability, Pawn pawn) {
            this.ability = ability;
            this.pawn = pawn;
            defaultLabel = ability.def.label;
            defaultDesc = ability.def.description;
            icon = ability.def.uiIcon;
        }

        public override void ProcessInput(Event ev) {
            base.ProcessInput(ev);

            if (pawn.Drafted && pawn.story.traits.HasTrait(TraitDef.Named("Radiant"))) {
                ability.Activate(new LocalTargetInfo(pawn.Position), new LocalTargetInfo(pawn.Position));
            }
        }

        public override bool InheritInteractionsFrom(Gizmo other) {
            return false;
        }
    }
}
