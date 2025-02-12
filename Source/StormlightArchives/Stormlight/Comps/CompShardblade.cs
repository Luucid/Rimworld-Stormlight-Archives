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
        public void bondWithPawn(Pawn pawn) {
            swordOwner = pawn;
            ThingWithComps blade = this.parent as ThingWithComps;
            CompAbilityEffect_SpawnEquipment abilityComp = pawn.GetAbilityComp<CompAbilityEffect_SpawnEquipment>("SummonShardblade");
            if (abilityComp != null) {
                abilityComp.bladeObject = blade;
                if (blade != null && pawn != null) {
                    Log.Message($"{pawn.Name} bonded with {blade.GetHashCode()}");
                }
            }
            //Ability summonAbility = swordOwner.abilities.GetAbility(DefDatabase<AbilityDef>.GetNamed("SummonShardblade"));
            //if (summonAbility != null) {
            //    CompAbilityEffect_SpawnEquipment comp = summonAbility.EffectComps.
            //}
        }

        public void severBond(Pawn pawn) {
            if (swordOwner == pawn)
                swordOwner = null;
        }
        public bool isBladeSpawned() {
            return isSpawned;
        }
        public void summon() {
            if (swordOwner == null) {
                Log.Message("sword owner was null");
                return;
            }
            CompAbilityEffect_SpawnEquipment abilityComp = swordOwner.GetAbilityComp<CompAbilityEffect_SpawnEquipment>("SummonShardblade");
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

