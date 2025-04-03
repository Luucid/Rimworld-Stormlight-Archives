using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;
using System;
using static UnityEngine.GraphicsBuffer;
using System.Security.Cryptography;
using static HarmonyLib.Code;
using Verse.Noise;

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
            bool hasRadiantTrait = pawn.story.traits.HasTrait(StormlightModDefs.whtwl_Radiant_Windrunner) 
                || pawn.story.traits.HasTrait(StormlightModDefs.whtwl_Radiant_Truthwatcher) 
                || pawn.story.traits.HasTrait(StormlightModDefs.whtwl_Radiant_Edgedancer);

            if (pawn.Drafted && (hasRadiantTrait || pawn.GetAbilityComp<CompAbilityEffect_SpawnEquipment>(StormlightModDefs.whtwl_SummonShardblade.defName) != null)) {
                if (abilityToggleStormlight())
                    return;
                if (abilitySummonShardblade())
                    return;
                if (abilityLashingUpward())
                    return;
                if (abilityWindRunnerFlight())
                    return;
                if (abilityHealSurge())
                    return;
                if (abilityPlantGrowSurge())
                    return;  
                if (abilityAbrasionSurge())
                    return;
            }
        }

        private bool abilityPlantGrowSurge() {
            if (ability.def == StormlightModDefs.whtwl_SurgeOfGrowth) {
                TargetingParameters tp = new TargetingParameters {
                    canTargetPawns = false,
                    canTargetAnimals = false,
                    canTargetItems = false,
                    canTargetBuildings = false,
                    canTargetLocations = true,
                    canTargetPlants = true,
                    validator = (TargetInfo info) => {
                        return info.IsValid &&
                               pawn != null &&
                               pawn.Spawned &&
                               pawn.Position.InHorDistOf(info.Cell, ability.verb.EffectiveRange);
                    }
                };

                StartCustomTargeting(pawn, ability.verb.EffectiveRange, tp, Color.green);
                return true;
            }
            return false;
        }

        private bool abilityAbrasionSurge() {
            if (ability.def == StormlightModDefs.whtwl_SurgeOfAbrasion) {
                ability.Activate(pawn, pawn);
                return true;
            }
            return false;
        }
        private bool abilityHealSurge() {
            if (ability.def == StormlightModDefs.whtwl_SurgeOfHealing) {
                TargetingParameters tp = new TargetingParameters {
                    canTargetPawns = true,
                    canTargetAnimals = true,
                    canTargetItems = false,
                    canTargetBuildings = false,
                    canTargetLocations = false,
                    validator = (TargetInfo info) => {
                        return info.IsValid &&
                               pawn != null &&
                               pawn.Spawned &&
                               pawn.Position.InHorDistOf(info.Cell, ability.verb.EffectiveRange);
                    }
                };
                StartCustomTargeting(pawn, ability.verb.EffectiveRange, tp);
                return true;
            }
            return false;
        }

        private bool abilityToggleStormlight() {
            if (ability.def == StormlightModDefs.whtwl_BreathStormlight) {
                ability.Activate(pawn, pawn);
                return true;
            }
            return false;
        }
        private bool abilitySummonShardblade() {
            if (ability.def == StormlightModDefs.whtwl_SummonShardblade) {
                ability.Activate(pawn, pawn);
                return true;
            }
            return false;
        }

        private bool abilityLashingUpward() {
            if (ability.def == StormlightModDefs.whtwl_LashingUpward) {

                TargetingParameters tp = new TargetingParameters {
                    canTargetPawns = true,
                    canTargetAnimals = false,
                    canTargetItems = false,
                    canTargetBuildings = false,
                    canTargetLocations = false,
                    validator = (TargetInfo info) => {
                        return info.IsValid &&
                               pawn != null &&
                               pawn.Spawned &&
                               pawn.Position.InHorDistOf(info.Cell, ability.verb.EffectiveRange);
                    }
                };
                StartCustomTargeting(pawn, ability.verb.EffectiveRange, tp);
                return true;
            }
            return false;
        }

        private bool abilityWindRunnerFlight() {
            if (ability.def == StormlightModDefs.whtwl_WindRunnerFlight) {
                float cost = pawn.GetAbilityComp<CompAbilityEffect_AbilityWindRunnerFlight>(StormlightModDefs.whtwl_WindRunnerFlight.defName).Props.stormLightCost;
                float distance = pawn.GetComp<CompStormlight>().Stormlight / cost;
                TargetingParameters tp = new TargetingParameters {
                    canTargetPawns = true,
                    canTargetAnimals = true,
                    canTargetItems = true,
                    canTargetBuildings = true,
                    canTargetLocations = true,
                    mustBeSelectable = false,
                    validator = (TargetInfo info) => {
                        return info.IsValid &&
                        pawn != null &&
                               pawn.Spawned &&
                               pawn.Position.InHorDistOf(info.Cell, distance);
                    }
                };
                StartCustomTargeting(pawn, distance, tp);
                return true;
            }
            return false;
        }


        public void StartCustomTargeting(Pawn caster, float maxDistance, TargetingParameters tp) {

            //var tp = new TargetingParameters {
            //    canTargetPawns = true,
            //    canTargetItems = true,
            //    canTargetLocations = true,
            //    validator = null // passing a separate 'targetValidator' argument below
            //};

            // The main action to do once a valid target is chosen
            Action<LocalTargetInfo> mainAction = (LocalTargetInfo chosenTarget) => {
                ability.Activate(chosenTarget, chosenTarget);

            };

            // This runs each frame to highlight the hovered cell/thing
            Action<LocalTargetInfo> highlightAction = (LocalTargetInfo info) => {
                // If it's valid, highlight it in green
                if (info.IsValid)
                    GenDraw.DrawTargetHighlight(info);
            };

            // This function checks if the user is allowed to pick 'info'
            // Return 'true' if valid, 'false' if not
            Func<LocalTargetInfo, bool> targetValidator = (LocalTargetInfo info) => {
                // Must be valid and within maxDistance
                return info.IsValid &&
                       caster.Spawned &&
                       caster.Position.InHorDistOf(info.Cell, maxDistance);
            };

            // Called each GUI frame after the default crosshair is drawn
            // e.g. you can add a label if out of range
            Action<LocalTargetInfo> onGuiAction = (LocalTargetInfo info) => {
                if (info.IsValid && !targetValidator(info)) {
                    Widgets.MouseAttachedLabel("Out of range");
                }
            };

            // Called each frame. We’ll draw a radius ring to show the cast range
            Action<LocalTargetInfo> onUpdateAction = (LocalTargetInfo info) => {
                GlowCircleRenderer.DrawCustomCircle(caster, maxDistance, Color.cyan);
            };

            Find.Targeter.BeginTargeting(
                targetParams: tp,
                action: mainAction,
                highlightAction: highlightAction,
                targetValidator: targetValidator,
                caster: caster,
                actionWhenFinished: null,     // optional cleanup
                mouseAttachment: null,        // or use a custom icon
                playSoundOnAction: true,
                onGuiAction: onGuiAction,
                onUpdateAction: onUpdateAction
            );
        }
        public void StartCustomTargeting(Pawn caster, float maxDistance, TargetingParameters tp, Color color) {


            // The main action to do once a valid target is chosen
            Action<LocalTargetInfo> mainAction = (LocalTargetInfo chosenTarget) => {
                Vector3 mouseMapPos = UI.MouseMapPosition();
                IntVec3 centerCell = mouseMapPos.ToIntVec3();
                List<Plant> plantsInRadius = new List<Plant>();

                foreach (IntVec3 cell in GenRadial.RadialCellsAround(centerCell, maxDistance, true)) {
                    if (cell.InBounds(pawn.Map)) {
                        foreach (Thing thing in cell.GetThingList(pawn.Map)) {
                            // Check if the Thing is a Plant
                            if (thing is Plant plant) {
                                ability.Activate(plant, plant);
                            }
                        }
                    }
                }

            };

            // This runs each frame to highlight the hovered cell/thing
            Action<LocalTargetInfo> highlightAction = (LocalTargetInfo info) => {
                // If it's valid, highlight it in green
                if (info.IsValid)
                    GenDraw.DrawTargetHighlight(info);
            };

            // This function checks if the user is allowed to pick 'info'
            // Return 'true' if valid, 'false' if not
            Func<LocalTargetInfo, bool> targetValidator = (LocalTargetInfo info) => {
                // Must be valid and within maxDistance
                return info.IsValid &&
                       caster.Spawned &&
                       caster.Position.InHorDistOf(info.Cell, maxDistance);
            };

            // Called each GUI frame after the default crosshair is drawn
            // e.g. you can add a label if out of range
            Action<LocalTargetInfo> onGuiAction = (LocalTargetInfo info) => {
                if (info.IsValid && !targetValidator(info)) {
                    Widgets.MouseAttachedLabel("Out of range");
                }
            };

            // Called each frame. We’ll draw a radius ring to show the cast range
            Action<LocalTargetInfo> onUpdateAction = (LocalTargetInfo info) => {
                Vector3 mousePos = UI.MouseMapPosition();
                double distance = Math.Sqrt(Math.Pow(mousePos.x - caster.Position.x, 2) + Math.Pow(mousePos.z - caster.Position.z, 2));
                if (distance - 0.5f > maxDistance)
                    color = Color.red;
                else {
                    color = Color.green;
                }
                GlowCircleRenderer.DrawCustomCircle(UI.MouseMapPosition(), maxDistance, color);
            };

            Find.Targeter.BeginTargeting(
                targetParams: tp,
                action: mainAction,
                highlightAction: highlightAction,
                targetValidator: targetValidator,
                caster: caster,
                actionWhenFinished: null,     // optional cleanup
                mouseAttachment: null,        // or use a custom icon
                playSoundOnAction: true,
                onGuiAction: onGuiAction,
                onUpdateAction: onUpdateAction
            );
        }

        public override bool InheritInteractionsFrom(Gizmo other) {
            return false;
        }
    }
}
