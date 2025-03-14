using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Verse;
using UnityEngine;


namespace StormlightMod {

    public class StormlightModSettings : ModSettings {
        public bool enableHighstormPushing;
        public bool enablePawnGlow;
        public bool devOptionAutofillSpheres;
        public float bondChanceMultiplier;
        public string bondChanceMultiplierBuffer;

        public override void ExposeData() {
            Scribe_Values.Look(ref enableHighstormPushing, "enableHighstormPushing", true); 
            Scribe_Values.Look(ref enablePawnGlow, "enablePawnGlow", true); 
            Scribe_Values.Look(ref devOptionAutofillSpheres, "devOptionAutofillSpheres", false); 
            Scribe_Values.Look(ref bondChanceMultiplier, "bondChanceMultiplier", 1);
            base.ExposeData();
        }
    }

    public class StormlightMod : Mod {

        static public StormlightModSettings settings;


        public StormlightMod(ModContentPack content) : base(content) {
            settings = GetSettings<StormlightModSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect) {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Highstorms will move items and pawns", ref settings.enableHighstormPushing, "expensive stuff");
            listingStandard.CheckboxLabeled("Pawns will glow when infused with stormlight", ref settings.enablePawnGlow, "expensive stuff");
            listingStandard.CheckboxLabeled("DEV OPTION: Always fill spheres", ref settings.devOptionAutofillSpheres, "will automatically always fill spheres when set to true");
            listingStandard.TextFieldNumericLabeled<float>("bond chance multiplier", ref settings.bondChanceMultiplier, ref settings.bondChanceMultiplierBuffer, 1, 100);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() {
            return "Stormlight Archives";
        }
    }

}
