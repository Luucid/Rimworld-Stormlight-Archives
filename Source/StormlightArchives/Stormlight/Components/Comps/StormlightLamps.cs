using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

namespace StormlightMod {
    public class StormlightLamps : ThingComp {
        public CompProperties_SphereLamp Props => (CompProperties_SphereLamp)props;
        public CompGlower GlowerComp => parent.GetComp<CompGlower>();
        public List<Thing> storedSpheres = new List<Thing>(); //  List of sphere stacks inside the pouch
        float totalStormlight = 0;

        // Called after loading or on spawn
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Collections.Look(ref storedSpheres, "storedSpheres", LookMode.Deep);
        }



        public override void CompTick() {
            if (storedSpheres.Count <= 0) {
                Thing sphere = ThingMaker.MakeThing(ThingDef.Named("Sphere_Emerald"));
                AddSphereToLamp(sphere as ThingWithComps);
            }
            totalStormlight = CheckStormlightFuel();
            //Log.Message($"light{totalStormlight}");
            //if (totalStormlight <= 0) {
            //    GlowerComp.ReceiveCompSignal("PowerTurnedOff");
            //    Log.Message("PowerTurnedOff");
            //}
            //else {
            //    Log.Message("PowerTurnedOn");
            //    GlowerComp.ReceiveCompSignal("PowerTurnedOn");
            //}
            //    GlowerComp.UpdateLit(parent.Map); 
        }

        public float CheckStormlightFuel() {
            float total = 0;
            foreach (ThingWithComps sphere in storedSpheres) {
                CompStormlight comp = sphere.GetComp<CompStormlight>();
                if (comp != null) {
                    total += comp.Stormlight;
                    comp.handleGlow();
                }
            }
            return total;
        }


        public bool AddSphereToLamp(ThingWithComps sphere) {
            if (sphere == null) {
                return false;
            }

            if (storedSpheres.Count >= Props.maxCapacity) { // Adjust max capacity as needed
                return false;
            }
            Log.Message("added sphere");
            storedSpheres.Add(sphere);
            return true;
        }

        public void RemoveSphereFromLamp(int index, Pawn pawn) {
            if (index < 0 || index >= storedSpheres.Count) {
                return;
            }

            ThingWithComps sphere = storedSpheres[index] as ThingWithComps;

            storedSpheres.RemoveAt(index);

            if (sphere != null && pawn != null && pawn.Map != null) {
                IntVec3 dropPosition = pawn.Position; // Drop at the pawn's current position
                GenPlace.TryPlaceThing(sphere, dropPosition, pawn.Map, ThingPlaceMode.Near);
                //Log.Message($"[StormlightMod] Removed sphere: {sphere.LabelCap} from pouch and placed it at {dropPosition}.");
            }

        }

    }

    public class CompProperties_SphereLamp : CompProperties {
        public int maxCapacity;
        public List<ThingDef> allowedSpheres;

        public CompProperties_SphereLamp() {
            this.compClass = typeof(StormlightLamps);
        }
    }
}

