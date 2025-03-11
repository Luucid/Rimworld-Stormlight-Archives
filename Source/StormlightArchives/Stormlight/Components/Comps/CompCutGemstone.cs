using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

namespace StormlightMod {
    public class CompCutGemstone : ThingComp {
        public CompProperties_CutGemstone GemstoneProps => (CompProperties_CutGemstone)props;
        public CompProperties_Stormlight StormlightProps => (CompProperties_Stormlight)props;
        private CompStormlight stormlightComp;
        private int gemstoneQuality;
        private int gemstoneSize;

        public override void Initialize(CompProperties props) {
            base.Initialize(props);
            stormlightComp = parent.GetComp<CompStormlight>();

            gemstoneQuality = StormlightUtilities.RollTheDice(1, 5); //make better roller later with lower prob for higher quality
            gemstoneSize = StormlightUtilities.RollTheDice(1, 3);    //make better roller later with lower prob for bigger size

            if (stormlightComp != null) {
                stormlightComp.StormlightContainerQuality = gemstoneQuality;
                stormlightComp.StormlightContainerSize = gemstoneSize;
                Log.Message($"gemstoneQuality: {gemstoneQuality}, gemstoneSize: {gemstoneSize}");
            }
            else {
                Log.Error("CompRawGemstone requires CompStormlight, but none was found on parent.");
            }
        }

        // Called after loading or on spawn
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look(ref gemstoneQuality, "gemstoneQuality", 1);
            Scribe_Values.Look(ref gemstoneSize, "gemstoneSize", 1);
        }

        public override void CompTick() {
            base.CompTick();
        }
    }
}

namespace StormlightMod {
    public class CompProperties_CutGemstone : CompProperties {
        public CompProperties_CutGemstone() {
            this.compClass = typeof(CompCutGemstone);
        }
    }
}
