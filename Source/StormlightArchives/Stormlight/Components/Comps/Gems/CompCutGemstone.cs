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
        public int gemstoneQuality;
        public int gemstoneSize;
        public int maximumGemstoneSize = 20;

        public override void Initialize(CompProperties props) {
            base.Initialize(props);
            stormlightComp = parent.GetComp<CompStormlight>();
            List<int> sizeList = new List<int>() { 1, 5, 20 };
            sizeList.RemoveAll(n => n > maximumGemstoneSize);
            gemstoneQuality = StormlightUtilities.RollTheDice(1, 5);
            gemstoneSize = StormlightUtilities.RollForRandomIntFromList(sizeList);   //make better roller later with lower prob for bigger size

            if (stormlightComp != null) {
                stormlightComp.StormlightContainerQuality = gemstoneQuality;
                stormlightComp.StormlightContainerSize = gemstoneSize;
                stormlightComp.calculateMaximumGlowRadius(gemstoneQuality, gemstoneSize);
                Log.Message($"gemstoneQuality: {gemstoneQuality}, gemstoneSize: {gemstoneSize}");
            }
            else {
                Log.Error("CompRawGemstone requires CompStormlight, but none was found on parent.");
            }
        }

        public override bool AllowStackWith(Thing other) {
            CompCutGemstone comp = other.TryGetComp<CompCutGemstone>();
            if (comp != null) {
                if (comp.gemstoneQuality != this.gemstoneQuality || comp.gemstoneSize != this.gemstoneSize) {
                    return false;
                }
            }
            return true;
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

        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            if (StormlightMod.settings.devOptionAutofillSpheres && stormlightComp != null) {

                yield return new Command_Action {
                    defaultLabel = "Fill gem with 10 stormlight",
                    defaultDesc = "Debug/Dev feature.",
                    icon = TexCommand.DesirePower,
                    action = () => {
                        stormlightComp.infuseStormlight(10f);
                    }
                };

                yield return new Command_Action {
                    defaultLabel = "Fill gem with 100 stormlight",
                    defaultDesc = "Debug/Dev feature.",
                    icon = TexCommand.DesirePower,
                    action = () => {
                        stormlightComp.infuseStormlight(100f);
                    }
                };

                yield return new Command_Action {
                    defaultLabel = "Fill gem with maximum stormlight",
                    defaultDesc = "Debug/Dev feature.",
                    icon = TexCommand.DesirePower,
                    action = () => {
                        stormlightComp.infuseStormlight(stormlightComp.CurrentMaxStormlight);
                    }
                };
            }
            yield break;
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
