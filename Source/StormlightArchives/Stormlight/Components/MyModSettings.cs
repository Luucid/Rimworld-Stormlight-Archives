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
        public float bondChance;

        public override void ExposeData() {
            Scribe_Values.Look(ref enableHighstormPushing, "enableHighstormPushing", true); 
            Scribe_Values.Look(ref enablePawnGlow, "enablePawnGlow", true); 
            Scribe_Values.Look(ref bondChance, "bondChance", 1000); //fixthis
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
            //listingStandard.TextFieldNumericLabeled("change to bond")
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() {
            return "Stormlight Archives";
        }
    }

}
