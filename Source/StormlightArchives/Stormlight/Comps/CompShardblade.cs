using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using System.Reflection;

namespace StormlightMod {
    public class CompShardblade : ThingComp {
        public CompProperties_Shardblade Props => (CompProperties_Shardblade)props;
        public override void PostExposeData() {
            base.PostExposeData();
        }

        public bool isSpawned = false;
        public Pawn owner = null;
        public ThingWithComps bladeThing = null;

        public bool isBonded() 
            {
            Log.Message($"isBonded check for {owner?.Name}");
            return owner != null; 
        }
        public void bondWithPawn(ref Pawn pawn) {
            owner = pawn;
            Log.Message($"Shard bonded with {pawn.Name}"); 
        }

        public void severBond() {
            owner = null;
        }

        public void summon() {
            if(bladeThing == null)
            {
                Log.Message("bladeThing is NULL");
                return;
            }
            Log.Message($"sword ID: {bladeThing.GetHashCode()}");

            owner.equipment.AddEquipment(bladeThing);
            isSpawned = true;
        }
        
        public void dismissBlade()
        {
            ThingWithComps droppedWeapon;
           owner.equipment.TryDropEquipment(owner.equipment.Primary, out droppedWeapon, owner.Position, forbid: false); 
            isSpawned = false;
            //dismissal vs dropping is handled by harmony patch in ShardbladePatches.cs
        }

        public void createBlade(ref Pawn pawn) {
            ThingDef stuffDef = DefDatabase<ThingDef>.GetNamed("ShardMaterial", true);
            ThingDef shardThing = DefDatabase<ThingDef>.GetNamed("MeleeWeapon_Shardblade", true);
            bladeThing  = (ThingWithComps)ThingMaker.MakeThing(shardThing, stuffDef);
            Log.Message($"Radiant {pawn.Name} created his shard blade!");

            bladeThing.AllComps.Add(this);
            this.parent = bladeThing; 
           
            if (isBonded() == false) {
               bondWithPawn(ref pawn);
            }
        }
    }
}
namespace StormlightMod {
    public class CompProperties_Shardblade : CompProperties {
        public bool initiated;
        public CompProperties_Shardblade() {
            this.compClass = typeof(CompShardblade);
        }
    }
}

