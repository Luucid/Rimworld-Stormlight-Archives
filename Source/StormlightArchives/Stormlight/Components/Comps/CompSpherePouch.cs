using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace StormlightMod {
    public class CompSpherePouch : ThingComp {
        public List<Thing> storedSpheres = new List<Thing>(); //  List of sphere stacks inside the pouch

        public CompProperties_SpherePouch Props => (CompProperties_SpherePouch)props;

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Collections.Look(ref storedSpheres, "storedSpheres", LookMode.Deep);
        }

        //  Get total Stormlight from all stored spheres
        public float GetTotalStoredStormlight() {
            float total = 0;
            foreach (ThingWithComps sphere in storedSpheres) {
                CompStormlight comp = sphere.GetComp<CompStormlight>();
                if (comp != null) {
                    total += comp.Stormlight;
                }
            }
            return total;
        }


        public void InfuseStormlight(float amount) {
            foreach (ThingWithComps sphere in storedSpheres) {
                CompStormlight comp = sphere.GetComp<CompStormlight>();
                if (comp != null) {
                    comp.infuseStormlight(amount);
                }
            }
        }

        //  Absorb Stormlight from pouch
        public float DrawStormlight(float amount) {
            float absorbed = 0;

            foreach (ThingWithComps sphere in storedSpheres) {
                CompStormlight comp = sphere.GetComp<CompStormlight>();
                if (comp != null && comp.HasStormlight) {
                    float drawn = comp.drawStormlight(amount - absorbed);
                    absorbed += drawn;
                    if (absorbed >= amount) break; // Stop once fully absorbed
                }
            }

            return absorbed;
        }

        public void RemoveSphereFromPouch(int index, Pawn pawn) {
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

        public bool AddSphereToPouch(ThingWithComps sphere) {  
            if (sphere == null) {
                return false;
            }

            if (storedSpheres.Count >= Props.maxCapacity) { // Adjust max capacity as needed
                return false;
            }

            storedSpheres.Add(sphere);
            return true;
        }
    }
}
namespace StormlightMod {
    public class CompProperties_SpherePouch : CompProperties {
        public int maxCapacity;
        public List<ThingDef> allowedSpheres;

        public CompProperties_SpherePouch() {
            this.compClass = typeof(CompSpherePouch);
        }
    }
}

