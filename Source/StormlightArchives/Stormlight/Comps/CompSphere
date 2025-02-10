using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
namespace StormlightMod {
    public class CompSphere : CompStormLight {
        public CompProperties_Sphere Props => (CompProperties_Sphere)props;
        private int tick = 0;

        // Called after loading or on spawn
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look(ref m_CurrentStormlight, "currentStormlight", 0f);
        }


        public override void CompTick() {

            if (tick == 0) {
                base.CompTick();
                Props.stormlightComp.handleGlow();
            }
            tick = (tick + 1) % 50;
        }


        // This method adds additional text to the inspect pane.
        public override string CompInspectStringExtra() {
            // You can format the stormlight value as you like.
            return "Stormlight: " + m_CurrentStormlight.ToString("F0") + " / " + CurrentMaxStormlight.ToString("F0");
        }

      
    }
}

namespace StormlightMod {
    public class CompProperties_Sphere : CompProperties {
        public CompStormlight stormlightComp; // = createStormlightComp;
        public CompProperties_Sphere() {
            this.compClass = typeof(CompSphere);
        }
    }
}
