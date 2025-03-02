using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using System.Reflection;
using RimWorld.QuestGen;
using System.Linq;

namespace StormlightMod {

    public static class AbilityExtensions {
        public static T GetAbilityComp<T>(this Pawn pawn, string abilityDefName) where T : CompAbilityEffect {
            if (pawn.abilities == null) return null;

            Ability ability = pawn.abilities.GetAbility(DefDatabase<AbilityDef>.GetNamed(abilityDefName));
            return ability?.comps?.OfType<T>().FirstOrDefault();
        }
    }

    public class CompShardblade : ThingComp {
        public CompProperties_Shardblade Props => props as CompProperties_Shardblade;
        private Pawn swordOwner = null;
        public override void PostExposeData() {
            base.PostExposeData();
        }
        private bool isSpawned = false;
        public bool isBonded(Pawn pawn) {
            return swordOwner == pawn;
        }

        private void handleSwordAbility(Pawn pawn, CompAbilityEffect_SpawnEquipment abilityComp) {
            if (abilityComp == null) {
                pawn.abilities.GainAbility(StormlightModDefs.whtwl_SummonShardblade);
                //ThisFilterList.Find(def => selPawn.Map.listerThings.ThingsOfDef(def).Any());
                //Trait trait = pawn.story.traits.allTraits.Find(t => StormlightModDefs.whtwl_Radiant_Traits.Any()); 
                //Trait trait = pawn.story.traits.allTraits.FirstOrDefault(t => StormlightModUtilities.RadiantTraits.Contains(t.def));
                Trait trait = StormlightUtilities.GetRadiantTrait(pawn); 

                if (trait == null) { //radiants does not get this ability
                    pawn.abilities.GainAbility(StormlightModDefs.whtwl_UnbondBlade);
                }
            }
        }

        public void bondWithPawn(Pawn pawn, bool isBladeSpawned) {
            swordOwner = pawn;
            ThingWithComps blade = this.parent as ThingWithComps;
            CompAbilityEffect_SpawnEquipment abilityComp = pawn.GetAbilityComp<CompAbilityEffect_SpawnEquipment>(StormlightModDefs.whtwl_SummonShardblade.defName);
            handleSwordAbility(pawn, abilityComp);
            if (abilityComp == null) {
                Log.Message("Abilitycomp still null!");
                abilityComp = pawn.GetAbilityComp<CompAbilityEffect_SpawnEquipment>(StormlightModDefs.whtwl_SummonShardblade.defName);
            }
            abilityComp.bladeObject = blade;
            isSpawned = isBladeSpawned;
        }


        private static void removeRadiantStuff(Pawn pawn) {
            if (pawn?.story?.traits == null) {
                Log.Error("[stormlight mod] Pawn has no traits system!");
                return;
            }

            //Trait trait = pawn.story.traits.allTraits.FirstOrDefault(t => t.def == StormlightModDefs.whtwl_Radiant);
            //Trait trait = pawn.story.traits.allTraits.Find(t => StormlightModDefs.whtwl_Radiant_Traits.Any());
            //Trait trait = pawn.story.traits.allTraits.FirstOrDefault(t => StormlightModUtilities.RadiantTraits.Contains(t.def));
            Trait trait = StormlightUtilities.GetRadiantTrait(pawn);

            if (trait != null) {
                pawn.story.traits.allTraits.Remove(trait);
                Log.Message($"[stormlight mod] {pawn.Name} broke an oath and lost its Radiant trait.");

                CompStormlight comp = pawn.GetComp<CompStormlight>();
                if (comp != null) {
                    pawn.AllComps.Remove(comp);
                }

                CompGlower glowComp = pawn.GetComp<CompGlower>();
                if (glowComp != null) {
                    pawn.AllComps.Remove(glowComp);
                }
            }
            pawn.abilities.RemoveAbility(StormlightModDefs.whtwl_SummonShardblade);
            pawn.abilities.RemoveAbility(StormlightModDefs.whtwl_UnbondBlade); 
        }


        public void severBond(Pawn pawn) {
            if (swordOwner == pawn) {
                removeRadiantStuff(pawn);
                swordOwner = null;
            }
        }
        public bool isBladeSpawned() {
            return isSpawned;
        }
        public void summon() {
            if (swordOwner == null || isSpawned == true) {
                Log.Message("sword owner was null or blade was already summoned");
                return;
            }
            CompAbilityEffect_SpawnEquipment abilityComp = swordOwner.GetAbilityComp<CompAbilityEffect_SpawnEquipment>(StormlightModDefs.whtwl_SummonShardblade.defName);
            if (abilityComp != null && abilityComp.bladeObject != null) {
                swordOwner.equipment.AddEquipment(abilityComp.bladeObject);
                isSpawned = true;
                Log.Message("sword spawned");
            }
            else {
                if (abilityComp == null)
                    Log.Message("abilityComp is null");
                else {
                    Log.Message("abilityComp.bladeObject is null");
                }

            }
        }

        public void dismissBlade(Pawn pawn) {
            ThingWithComps droppedWeapon;
            pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out droppedWeapon, pawn.Position, forbid: false);
            isSpawned = false;
            //dismissal vs dropping is handled by harmony patch in ShardbladePatches.cs
        }

    }
}
namespace StormlightMod {
    public class CompProperties_Shardblade : CompProperties {
        public CompProperties_Shardblade() {
            this.compClass = typeof(CompShardblade);
        }
    }
}

