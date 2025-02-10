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

        public bool isBonded() { return Props.owner != null; }
        public void bondWithPawn(ref Pawn pawn) {
            Props.owner = pawn;
            Log.Message($"Shard bonded with {pawn.Name}"); 
        }
        public void severBond() {
            Props.owner = null;
        }

        public void summon(ref ThingWithComps thisBlade) {
            if(thisBladeThing == null)
                thisBladeThing = thisBlade;
            Props.owner.equipment.AddEquipment(thisBladeThing);
        }

        public void createBlade(ref Pawn pawn) {
            ThingDef stuffDef = DefDatabase<ThingDef>.GetNamed("ShardMaterial", true);
            ThingWithComps equipment = (ThingWithComps)ThingMaker.MakeThing(Props.thingDef, stuffDef);
            Props.thisBladeThing = equipment.Primary.GetComp<CompShardblade>();
            Log.Message($"Radiant {pawn.Name} created his shard blade!");
            if (blade.isBonded() == false) {
                blade.bondWithPawn(ref pawn);
            }
        }
    }
}
namespace StormlightMod {
    public class CompProperties_Shardblade : CompProperties {
        public bool isSpawned = false;
        public Pawn owner = null;
        public ThingWithComps thisBladeThing = null;
        public CompProperties_Shardblade() {
            this.compClass = typeof(CompShardblade);
        }
    }
}

