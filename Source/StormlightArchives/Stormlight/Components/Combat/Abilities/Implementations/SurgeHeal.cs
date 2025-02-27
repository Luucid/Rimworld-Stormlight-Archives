using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace StormlightMod {

    /// Surge regrowth plants
    public class CompProperties_AbilitySurgeHeal : CompProperties_AbilityEffect {
        public float stormLightCost;

        public CompProperties_AbilitySurgeHeal() {
            this.compClass = typeof(CompAbilityEffect_SurgeHeal);
        }
    }
    public class CompAbilityEffect_SurgeHeal : CompAbilityEffect {
        public new CompProperties_AbilitySurgeHeal Props => this.props as CompProperties_AbilitySurgeHeal;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            // 1) Validate target
            if (!target.IsValid || target.Thing == null || !target.Thing.Spawned) {
                Log.Warning("[LashUpward] Invalid target.");
                return;
            }

            Pawn caster = this.parent.pawn as Pawn;
            if (caster == null) {
                return;
            }

            if (caster.GetComp<CompStormlight>() == null) {
                Log.Message($"[heal] null!");
                return;
            }

            // 3) heal target
            healFunction(target.Thing);
        }



        private void healFunction(Thing targetThing) {
            Map map = targetThing.Map;

            Pawn targetPawn = targetThing as Pawn;
            if (targetPawn == null) {
                return;
            }

            Log.Message($"Target: {targetPawn.Name}");

            radiantHeal(targetPawn);
        }


        private void radiantHeal(Pawn pawn) {
            Pawn caster = this.parent.pawn as Pawn;
            CompStormlight casterComp = caster.GetComp<CompStormlight>();
            if (casterComp != null) {
                
                //add check for being stage 3 knight radiant!
                // HEAL MISSING PARTS
                var missingParts = pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().OrderByDescending(h => h.Severity).ToList();
                foreach (var injury in missingParts) {
                    float cost = 175f;  // More severe wounds cost more stormlight
                    if (casterComp.Stormlight < cost)
                        break;
                    pawn.health.hediffSet.hediffs.Remove(injury);
                    casterComp.drawStormlight(cost);

                    RadiantUtility.GiveRadiantXP(caster, 20f);
                }


                // HEAL ADDICTIONS
                var addictions = pawn.health.hediffSet.hediffs.OfType<Hediff_Addiction>().OrderByDescending(h => h.Severity).ToList();
                foreach (var addiction in addictions) {
                    float cost = 500f;
                    if (casterComp.Stormlight < cost)
                        break;
                    pawn.health.hediffSet.hediffs.Remove(addiction);
                    casterComp.drawStormlight(cost);

                    RadiantUtility.GiveRadiantXP(caster, 12f);
                }

                // HEAL INJURIES
                var injuries = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().OrderByDescending(h => h.Severity).ToList();
                foreach (var injury in injuries) {
                    float cost = injury.Severity * 10f;  // More severe wounds cost more stormlight
                    if (casterComp.Stormlight < cost)
                        break;

                    injury.Heal(10.0f);
                    casterComp.drawStormlight(cost);
                    RadiantUtility.GiveRadiantXP(caster, 0.5f);
                }
            }
        }
    }




}
