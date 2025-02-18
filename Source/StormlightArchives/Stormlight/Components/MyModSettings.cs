using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Verse;
using UnityEngine;


namespace StormLight {
    namespace MyExampleMod {
        public class ExampleSettings : ModSettings {
            /// <summary>
            /// The three settings our mod has.
            /// </summary>
            public bool enableHighstormPushing;

            /// <summary>
            /// The part that writes our settings to file. Note that saving is by ref.
            /// </summary>
            public override void ExposeData() {
                Scribe_Values.Look(ref enableHighstormPushing, "enableHighstormPushing", true);
                base.ExposeData();
            }
        }

        public class ExampleMod : Mod {

            static public ExampleSettings settings;


            public ExampleMod(ModContentPack content) : base(content) {
                settings = GetSettings<ExampleSettings>();
            }
            public override void DoSettingsWindowContents(Rect inRect) {
                Listing_Standard listingStandard = new Listing_Standard();
                listingStandard.Begin(inRect);
                listingStandard.CheckboxLabeled("Highstorms will move items and pawns", ref settings.enableHighstormPushing, "expensive stuff");
                //settings.exampleFloat = listingStandard.Slider(settings.exampleFloat, 100f, 300f);
                listingStandard.End();
                base.DoSettingsWindowContents(inRect);
            }

            public override string SettingsCategory() {
                return "StormlightArchives";
            }
        }
    }
}
