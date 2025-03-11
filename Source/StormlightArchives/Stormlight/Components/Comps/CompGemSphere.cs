using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection.Emit;
using System.Security.Cryptography;

namespace StormlightMod {
    public class CompGemSphere : ThingComp {
        public CompProperties_GemSphere GemstoneProps => (CompProperties_GemSphere)props;
        public CompProperties_Stormlight StormlightProps => (CompProperties_Stormlight)props;
        private CompStormlight stormlightComp;
        private int gemstoneQuality;
        private int gemstoneSize;

        public override void PostPostMake() {
            base.PostPostMake();
        }
        public override void Initialize(CompProperties props) {
            base.Initialize(props);
            stormlightComp = parent.GetComp<CompStormlight>();
            List<int> sizeList = new List<int>() { 1, 5, 20 };
            gemstoneQuality = StormlightUtilities.RollTheDice(1, 5);
            gemstoneSize = StormlightUtilities.RollForRandomIntFromList(sizeList);   //make better roller later with lower prob for bigger size

            if (stormlightComp != null) {
                stormlightComp.StormlightContainerQuality = gemstoneQuality;
                stormlightComp.StormlightContainerSize = gemstoneSize;
            }
        }

        public override string TransformLabel(string label) {
            switch (gemstoneSize) {
                case 1:
                    return label + " chip";
                case 5:
                    return label + " mark";
                case 20:
                    return label + " broam";
                default:
                    return label;
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
    public class CompProperties_GemSphere : CompProperties {
        public CompProperties_GemSphere() {
            this.compClass = typeof(CompGemSphere);
        }
    }
}
