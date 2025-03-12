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
        public bool inheritGemstone = false;
        public int inheritGemstoneQuality = 1;
        public int inheritGemstoneSize = 1;

        public override void PostPostMake() {
            base.PostPostMake();
        }
        public override void Initialize(CompProperties props) {
            base.Initialize(props);
            stormlightComp = parent.GetComp<CompStormlight>();
            if (inheritGemstone == false) {
                List<int> sizeList = new List<int>() { 1, 5, 20 };
                List<int> qualityList = new List<int>() { 1, 2, 3, 4, 5 };

                gemstoneQuality = StormlightUtilities.RollForRandomIntFromList(qualityList);   //make better roller later with lower prob for bigger size
                gemstoneSize = StormlightUtilities.RollForRandomIntFromList(sizeList);         //make better roller later with lower prob for bigger size
            }
            else {
                gemstoneQuality = inheritGemstoneQuality;
                gemstoneSize = inheritGemstoneSize;
            }
            if (stormlightComp != null) {
                stormlightComp.StormlightContainerQuality = gemstoneQuality;
                stormlightComp.StormlightContainerSize = gemstoneSize;
            }
        }

        public override string TransformLabel(string label) {
            string sizeLabel = "";
            string qualityLabel = "";
            switch (gemstoneSize) {
                case 1:
                    sizeLabel = " chip";
                    break;
                case 5:
                    sizeLabel = " mark";
                    break;
                case 20:
                    sizeLabel = " broam";
                    break;
                default:
                    break;
            }
            switch (gemstoneQuality) {
                case 1:
                    qualityLabel = "flawed ";
                    break;
                case 2:
                    qualityLabel = "imperfect ";
                    break;
                case 3:
                    qualityLabel = "standard ";
                    break;
                case 4:
                    qualityLabel = "flawless ";
                    break;
                case 5:
                    qualityLabel = "perfect ";
                    break;

                default:
                    break;
            }
            return qualityLabel + label + sizeLabel;
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
