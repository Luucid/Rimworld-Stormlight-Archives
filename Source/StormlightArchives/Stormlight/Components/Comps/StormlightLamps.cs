﻿using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using Verse.AI;
using Unity.Jobs;
using UnityEngine;

namespace StormlightMod {
    public class StormlightLamps : ThingComp {
        public CompProperties_SphereLamp Props => (CompProperties_SphereLamp)props;
        public CompGlower GlowerComp => parent.GetComp<CompGlower>();
        public List<Thing> storedSpheres = new List<Thing>(); //  List of sphere stacks inside the pouch
        public List<ThingDef> ThisFilterList;


        bool lightEnabled = true;
        bool initSphereAdded = false;
        public bool Empty => storedSpheres.Count == 0;
        private float m_CurrentStormlight = 0f;

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
            // Initialize ThisFilterList only after props is guaranteed to be available.
            if (ThisFilterList == null && Props.allowedSpheres != null) {
                ThisFilterList = Props.allowedSpheres.ToList();
            }
        }
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Collections.Look(ref storedSpheres, "storedSpheres", LookMode.Deep);
        }

        public bool IsFull() {
            if (Empty)
                return false;
            ThingWithComps sphere = storedSpheres.ElementAt(0) as ThingWithComps;
            CompStormlight comp = sphere.GetComp<CompStormlight>();
            return storedSpheres.Count() >= Props.maxCapacity && comp.HasStormlight;
        }

        public int GetDunSphereCount() {
            return storedSpheres.Count(); //make more advanced later
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra()) {
                yield return gizmo;
            }

            yield return new Command_Action {
                defaultLabel = "Set Sphere Filters",
                defaultDesc = "Click to choose which spheres are allowed in this lamp.",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/SomeIcon"), 
                action = () =>
                {
                    // Opens the custom dialog for filtering spheres.
                    Find.WindowStack.Add(new Dialog_SphereFilter(this));
                }
            };
            Log.Message($"filter: {ThisFilterList}");
        }


        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {
            Thing sphere = null;
            CompSpherePouch spherePouch = CompSpherePouch.GetWornSpherePouch(selPawn);
            ThingDef matchingSphereDef = ThisFilterList.Find(def => selPawn.Map.listerThings.ThingsOfDef(def).Any());

            if (spherePouch != null && !spherePouch.Empty) {
                sphere = spherePouch.GetSphereWithMostStormlight(true);
            }
            else if (matchingSphereDef != null && GenClosest.ClosestThing_Global(selPawn.Position, selPawn.Map.listerThings.ThingsOfDef(matchingSphereDef), 500f) != null) {
                sphere = GenClosest.ClosestThing_Global(selPawn.Position, selPawn.Map.listerThings.ThingsOfDef(matchingSphereDef), 500f);
            }

            if (sphere != null) {

                yield return new FloatMenuOption("Replace sphere", () => {
                    Job job = JobMaker.MakeJob(StormlightModDefs.whtwl_RefuelSphereLamp, parent, sphere);
                    if (!job.TryMakePreToilReservations(selPawn, errorOnFailed: true)) {
                        Log.Message("It failed");
                    }
                    else {
                        selPawn.jobs.TryTakeOrderedJob(job);
                    }
                });
            }
            yield return new FloatMenuOption("Remove Sphere", () => {
                if (storedSpheres.Count() > 0) { RemoveSphereFromLamp(0); }; //make job later
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
            Log.Message("absorb stuff");
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
            string sphereName = "No sphere in lamp.";
            if (storedSpheres.Count > 0) {
                ThingWithComps sphere = storedSpheres.First() as ThingWithComps;
                sphereName = sphere.def.label + "(" + m_CurrentStormlight.ToString("F0") + ")";
            }
            return sphereName;
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

        public ThingWithComps RemoveSphereFromLamp(int index, bool dropOnGround = true) {
            if (index < 0 || index >= storedSpheres.Count) {
                return null;
            }

            ThingWithComps sphere = storedSpheres[index] as ThingWithComps;

            storedSpheres.RemoveAt(index);

            Log.Message($"removed sphere, size of lamp content: {storedSpheres.Count}");
            if (dropOnGround && sphere != null && parent.Map != null) {
                IntVec3 dropPosition = parent.Position;
                GenPlace.TryPlaceThing(sphere, dropPosition, parent.Map, ThingPlaceMode.Near);
            }
            return sphere;
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

