using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
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

        private int tick = 0;

        // Called after loading or on spawn
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look(ref m_CurrentStormlight, "currentStormlight", 0f);
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
            handleSphereGlow();
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
        private void handleSphereGlow() {
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

        private void radiantGlow(Pawn pawn) {

            if (!HasStormlight || pawn.health == null) return;
            if (GlowerComp != null) {
                Log.Message($"Current radiant glow radius: {GlowerComp.Props.glowRadius}");
                if (m_CurrentStormlight == 0) {
                    GlowerComp.Props.glowRadius = 0;
                }
                else {
                    GlowerComp.Props.glowRadius = m_CurrentStormlight * 0.01f;
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

                injury.Heal(10.0f);  // Heal by 1 point
                m_CurrentStormlight -= cost;
            }

            // HEAL INJURIES
            var injuries = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().OrderByDescending(h => h.Severity).ToList();
            foreach (var injury in injuries) {
                float cost = injury.Severity * 3f;  // More severe wounds cost more stormlight
                if (m_CurrentStormlight < cost)
                    break;

                injury.Heal(10.0f);  // Heal by 1 point
                m_CurrentStormlight -= cost;
            }
        }



        private void radiantAbsorbStormlight(Pawn pawn) {
            if (pawn == null || !pawn.Spawned || !pawn.RaceProps.Humanlike)
                return;

            float absorbAmount = 5f; // How much Stormlight is drawn per tick
            float maxDrawDistance = 3.0f; // Range to absorb from nearby spheres

            //  Absorb Stormlight from pouch
            CompSpherePouch pouchComp = pawn.apparel?.WornApparel?.Find(a => a.GetComp<CompSpherePouch>() != null)?.GetComp<CompSpherePouch>();
            if (pouchComp != null && pouchComp.GetTotalStoredStormlight() > 0) {
                float drawn = pouchComp.DrawStormlight(absorbAmount);
                infuseStormlight(drawn);
                Log.Message($"{pawn.Name} absorbed {drawn} Stormlight from a pouch! Remaining in pouch: {pouchComp.GetTotalStoredStormlight()}");
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
                        Log.Message($"{pawn.Name} absorbed {drawn} Stormlight from an inventory sphere!");
                        return; // Absorb from only one at a time
                    }
                }
            }

            //  Absorb Stormlight from nearby spheres
            List<Thing> nearbyThings = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
            foreach (Thing thing in nearbyThings) {
                if (thing.Position.DistanceTo(pawn.Position) <= maxDrawDistance) {
                    ThingWithComps thingWithComps = thing as ThingWithComps;
                    if (thingWithComps == null) continue; // Skip non-comp objects

                    CompStormlight sphereComp = thingWithComps.GetComp<CompStormlight>();
                    if (sphereComp != null && sphereComp.Stormlight > 0) {
                        float drawn = Math.Min(absorbAmount, sphereComp.Stormlight);
                        sphereComp.infuseStormlight(-drawn); // Remove from sphere
                        infuseStormlight(drawn); // Add to Radiant
                        return; // Absorb from only one at a time
                    }
                }
            }
        }



        private void handleRadiantStuff(Pawn pawn) {
            radiantGlow(pawn);
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
            Log.Message($"infused spere, {m_CurrentStormlight}/{CurrentMaxStormlight}");
            m_CurrentStormlight += amount;
            if (m_CurrentStormlight > CurrentMaxStormlight)
                m_CurrentStormlight = CurrentMaxStormlight;
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
