using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

namespace StormlightMod {
    public class CompRawGemstone : ThingComp {
        public CompProperties_RawGemstone GemstoneProps => (CompProperties_RawGemstone)props;
        public CompProperties_Stormlight StormlightProps => (CompProperties_Stormlight)props;
        private CompStormlight stormlightComp;

        public override void Initialize(CompProperties props) {
            base.Initialize(props);
            stormlightComp = parent.GetComp<CompStormlight>();
        }

        // Called after loading or on spawn
        public override void PostExposeData() {
            base.PostExposeData();
        }

        public override void CompTick() {
            base.CompTick();
        }
    }
}

namespace StormlightMod {
    public class CompProperties_RawGemstone : CompProperties {
        public int spawnChance;
        public CompProperties_RawGemstone() {
            this.compClass = typeof(CompRawGemstone);
        }
    }
}
