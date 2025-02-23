using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using Verse.AI;
using Unity.Jobs;

namespace StormlightMod {
    public class StormlightLamps : ThingComp {
        public CompProperties_SphereLamp Props => (CompProperties_SphereLamp)props;
        public CompGlower GlowerComp => parent.GetComp<CompGlower>();
        public List<Thing> storedSpheres = new List<Thing>(); //  List of sphere stacks inside the pouch
        bool lightEnabled = true;
        bool initSphereAdded = false;
        private float m_CurrentStormlight = 0f;
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Collections.Look(ref storedSpheres, "storedSpheres", LookMode.Deep);
        }

        public bool IsFull() {
            ThingWithComps sphere = storedSpheres.ElementAt(0) as ThingWithComps;
            CompStormlight comp = sphere.GetComp<CompStormlight>();
            return storedSpheres.Count() >= Props.maxCapacity && comp.HasStormlight;
        }


        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {
            //foreach (FloatMenuOption option in base.CompFloatMenuOptions(selPawn))
            //    yield return option;

            //// Ensure the pawn has a sphere to add
            //bool hasSphere = selPawn.inventory.innerContainer.Any(s =>
            //    Props.allowedSpheres.Contains(s.def));
            yield return new FloatMenuOption("Add Stormlight Sphere", () => {
                Job job = JobMaker.MakeJob(StormlightModDefs.whtwl_RefuelSphereLamp, parent);
                if (!job.TryMakePreToilReservations(selPawn, errorOnFailed: true)) {
                    Log.Message("It failed");
                }
                else {
                    selPawn.jobs.TryTakeOrderedJob(job);
                }
            });
        }


        public override void CompTick() {
            if (storedSpheres.Count <= 0 && initSphereAdded == false) {
                Thing sphere = ThingMaker.MakeThing(ThingDef.Named("Sphere_Emerald"));
                ThingWithComps sphereComp = sphere as ThingWithComps;

                CompStormlight comp = sphereComp.GetComp<CompStormlight>();
                if (comp != null) {
                    comp.Initialize(new CompProperties_Stormlight {
                        maxStormlight = 1000f,
                        drainRate = 0.01f
                    });
                }

                AddSphereToLamp(sphereComp);
                initSphereAdded = true;
            }
            CheckStormlightFuel();
        }

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

        public void CheckStormlightFuel() {
            float total = 0;
            foreach (ThingWithComps sphere in storedSpheres) {
                CompStormlight comp = sphere.GetComp<CompStormlight>();
                comp.CompTick();
                if (comp != null) {
                    total += comp.Stormlight;
                }
            }
            m_CurrentStormlight = total;
            toggleGlow(total > 0f);
        }

        public override string CompInspectStringExtra() {
            return "Stormlight: " + m_CurrentStormlight.ToString("F0");
        }

        public void InfuseStormlight(float amount) {
            foreach (ThingWithComps sphere in storedSpheres) {
                CompStormlight comp = sphere.GetComp<CompStormlight>();
                if (comp != null) {
                    comp.infuseStormlight(amount);
                }
            }
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
            GlowerComp.GlowColor = sphere.GetComp<CompStormlight>().GlowerComp.GlowColor;
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
            }
        }

        public void RemoveSphereFromLamp(int index) {
            if (index < 0 || index >= storedSpheres.Count) {
                return;
            }

            ThingWithComps sphere = storedSpheres[index] as ThingWithComps;

            storedSpheres.RemoveAt(index);

            if (sphere != null && parent.Map != null) {
                IntVec3 dropPosition = parent.Position;
                GenPlace.TryPlaceThing(sphere, dropPosition, parent.Map, ThingPlaceMode.Near);
            }

        }
        private void toggleGlow(bool toggleOn) {
            if (parent.Map != null && lightEnabled != toggleOn) {
                lightEnabled = toggleOn;
                if (toggleOn) {
                    parent.Map.glowGrid.RegisterGlower(GlowerComp);
                }
                else {
                    parent.Map.glowGrid.DeRegisterGlower(GlowerComp);
                }
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

