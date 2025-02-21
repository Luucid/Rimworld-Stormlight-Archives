using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;

namespace StormlightMod {
    public class CompStormlight : ThingComp {
        private float m_CurrentStormlight;
        public CompProperties_Stormlight Props => (CompProperties_Stormlight)props;
        public CompGlower GlowerComp => parent.GetComp<CompGlower>();
        public bool HasStormlight => m_CurrentStormlight > 0;
        public float Stormlight => m_CurrentStormlight;
        public int StackCount => parent.stackCount;
        public float MaxStormlightPerItem => Props.maxStormlight;
        public float CurrentMaxStormlight = 0;
        public bool m_BreathStormlight = false;

        private int tick = 0;

        // Called after loading or on spawn
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look(ref m_CurrentStormlight, "currentStormlight", 0f);
            Scribe_Values.Look(ref m_BreathStormlight, "breathStormlight", false);
        }

        public void toggleBreathStormlight() {
            m_BreathStormlight = !m_BreathStormlight;
        }

        public override void CompTick() {
          
            if (tick == 0) {
                base.CompTick();
                drainStormLight();
                if (parent is Pawn pawn && pawn.RaceProps.Humanlike) {
                    handleRadiantStuff(pawn);
                }
                CurrentMaxStormlight = MaxStormlightPerItem * StackCount;
            }
            handleGlow();
            tick = (tick + 1) % 50;
        }


        // This method adds additional text to the inspect pane.
        public override string CompInspectStringExtra() {
            // You can format the stormlight value as you like.
            return "Stormlight: " + m_CurrentStormlight.ToString("F0") + " / " + CurrentMaxStormlight.ToString("F0");
        }

        private void fixGlowIssue() {
            if (parent.Map != null) {
                parent.Map.glowGrid.DeRegisterGlower(GlowerComp);
                parent.Map.glowGrid.RegisterGlower(GlowerComp);
            }
        }
        public void handleGlow() {
            if (parent is Pawn pawn && pawn.Spawned) {
                if (!StormlightMod.settings.enablePawnGlow) {
                    GlowerComp.Props.glowRadius = 0;
                    GlowerComp.Props.overlightRadius = 0;
                    parent.Map.glowGrid.DeRegisterGlower(GlowerComp);
                    return;
                }
            }
            if (GlowerComp != null) {

                if (m_CurrentStormlight == 0) {
                    GlowerComp.Props.glowRadius = 0;
                }
                else if (m_CurrentStormlight < 10.0f) {
                    GlowerComp.Props.glowRadius = 2.0f;
                }
                else {
                    GlowerComp.Props.glowRadius = 5.0f;
                }
                fixGlowIssue();
            }
        }


        private void radiantHeal(Pawn pawn) {
            // HEAL MISSING PARTS
            var missingParts = pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().OrderByDescending(h => h.Severity).ToList();
            foreach (var injury in missingParts) {
                float cost = injury.Severity * 6f;  // More severe wounds cost more stormlight
                if (m_CurrentStormlight < cost)
                    break;

                injury.Heal(10.0f);
                m_CurrentStormlight -= cost;
                RadiantUtility.GiveRadiantXP(pawn, 2f);
            }

            // HEAL INJURIES
            var injuries = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().OrderByDescending(h => h.Severity).ToList();
            foreach (var injury in injuries) {
                float cost = injury.Severity * 3f;  // More severe wounds cost more stormlight
                if (m_CurrentStormlight < cost)
                    break;

                injury.Heal(10.0f);
                m_CurrentStormlight -= cost;
                RadiantUtility.GiveRadiantXP(pawn, 0.5f);
            }
        }



        private void radiantAbsorbStormlight(Pawn pawn) {
            if (pawn == null || !pawn.Spawned || !pawn.RaceProps.Humanlike)
                return;

            float absorbAmount = 25f; // How much Stormlight is drawn per tick
            float maxDrawDistance = 3.0f; // Range to absorb from nearby spheres

            if ((MaxStormlightPerItem - Stormlight) < absorbAmount)
                absorbAmount = (MaxStormlightPerItem - Stormlight);

            if (m_BreathStormlight == false)
                return;

            //  Absorb Stormlight from pouch
            CompSpherePouch pouchComp = pawn.apparel?.WornApparel?.Find(a => a.GetComp<CompSpherePouch>() != null)?.GetComp<CompSpherePouch>();
            if (pouchComp != null && pouchComp.GetTotalStoredStormlight() > 0) {
                float drawn = pouchComp.DrawStormlight(absorbAmount);
                infuseStormlight(drawn);
                RadiantUtility.GiveRadiantXP(pawn, 0.1f);
                return;
            }


            //  Absorb Stormlight from inventory
            if (pawn.inventory != null) {
                foreach (Thing item in pawn.inventory.innerContainer) {
                    ThingWithComps thingWithComps = item as ThingWithComps;
                    if (thingWithComps == null) continue; // Skip items without comps

                    CompStormlight sphereComp = thingWithComps.GetComp<CompStormlight>();
                    if (sphereComp != null && sphereComp.Stormlight > 0) {
                        float drawn = Math.Min(absorbAmount, sphereComp.Stormlight);
                        sphereComp.infuseStormlight(-drawn); // Remove from sphere
                        infuseStormlight(drawn); // Add to Radiant
                        RadiantUtility.GiveRadiantXP(pawn, 0.1f);
                        return; // Absorb from only one at a time
                    }
                }
            }

            //  Absorb Stormlight from nearby spheres
            List<Thing> nearbyThings = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Everything);
            foreach (Thing thing in nearbyThings) {
                if (thing.def.defName.StartsWith("Sphere_") && !thing.Position.Roofed(thing.Map)) {
                    var stormlightComp = thing.TryGetComp<CompStormlight>();
                    if (stormlightComp != null) {
                        float drawnLight = stormlightComp.drawStormlight(absorbAmount);
                        infuseStormlight(drawnLight); 
                        RadiantUtility.GiveRadiantXP(pawn, 0.1f);
                        return; 
                    }
                }
                else if (thing.def.defName.Equals("Apparel_SpherePouch") && !thing.Position.Roofed(thing.Map)) {
                    var pouch = thing.TryGetComp<CompSpherePouch>();
                    if (pouch != null) {
                        absorbAmount = Math.Min(absorbAmount, pouch.GetTotalStoredStormlight()); 
                        float drawnLight = pouch.DrawStormlight(absorbAmount); 
                        infuseStormlight(drawnLight);
                        RadiantUtility.GiveRadiantXP(pawn, 0.1f);
                        return;
                    }
                }
                else if (thing.def.defName.Equals("SphereLamp") && !thing.Position.Roofed(thing.Map)) {
                    var lamp = thing.TryGetComp<StormlightLamps>();
                    if (lamp != null) {
                        float drawnLight = lamp.DrawStormlight(absorbAmount);
                        infuseStormlight(drawnLight);
                        RadiantUtility.GiveRadiantXP(pawn, 0.1f);
                        return;
                    }
                }
            }
        }



        private void handleRadiantStuff(Pawn pawn) {
            radiantHeal(pawn);
            radiantAbsorbStormlight(pawn);
        }



        private void drainStormLight() {
            if (m_CurrentStormlight > 0) {
                m_CurrentStormlight -= Props.drainRate;
                if (m_CurrentStormlight < 0)
                    m_CurrentStormlight = 0;
            }
        }

        public float drawStormlight(float amount) {
            float drawnAmount = amount;
            if (m_CurrentStormlight > 0) {
                m_CurrentStormlight -= amount;
                if (m_CurrentStormlight < 0) {
                    drawnAmount += m_CurrentStormlight;
                    m_CurrentStormlight = 0;
                }
            }
            else { drawnAmount = 0f; }
            return drawnAmount;
        }

        // Infuse from code when highstorm is active
        public void infuseStormlight(float amount) {
            m_CurrentStormlight += amount;
            if (m_CurrentStormlight > MaxStormlightPerItem)
                m_CurrentStormlight = MaxStormlightPerItem;
        }
    }
}

namespace StormlightMod {
    public class CompProperties_Stormlight : CompProperties {
        public float maxStormlight;
        public float drainRate;

        public CompProperties_Stormlight() {
            this.compClass = typeof(CompStormlight);
        }
    }
}
