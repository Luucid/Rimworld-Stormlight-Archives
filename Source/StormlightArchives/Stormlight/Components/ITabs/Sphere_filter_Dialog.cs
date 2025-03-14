using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace StormlightMod {
    public class Dialog_SphereFilter : Window {
        private StormlightLamps lamp;
        private Vector2 scrollPosition;
        // You can adjust the size of the window as needed.
        public override Vector2 InitialSize => new Vector2(400f, 500f);

        public Dialog_SphereFilter(StormlightLamps lamp) {
            this.lamp = lamp;
            forcePause = false;
            //absorbInputAroundWindow = true;
        }

        private void addCheckboxSpheres(int i, Rect viewRect) {
            ThingDef sphereDef = lamp.Props.allowedSpheres[i];
            Rect checkboxRect = new Rect(0, i * 30, viewRect.width, 30);

            // Determine if this sphere is currently enabled in the filter.
            bool currentlyAllowed = lamp.ThisFilterList.Contains(sphereDef);

            // Copy the state into a local variable.
            bool flag = currentlyAllowed;

            // Pass the local variable by reference.
            Widgets.CheckboxLabeled(checkboxRect, sphereDef.label, ref flag);

            // If the checkbox value has changed, update the filter list accordingly.
            if (flag != currentlyAllowed) {
                if (flag) {
                    lamp.ThisFilterList.Add(sphereDef);
                }
                else {
                    lamp.ThisFilterList.Remove(sphereDef);
                }
            }
        }
        private void addCheckboxGemSize(int i, Rect viewRect, CompGemSphere.GemSize size) {
            Rect checkboxRect = new Rect(0, i * 30, viewRect.width, 30);
            // Determine if this sphere is currently enabled in the filter.
            bool currentlyAllowed = lamp.SizeFilterList.Contains(size);

            // Copy the state into a local variable.
            bool flag = currentlyAllowed;

            // Pass the local variable by reference.
            Widgets.CheckboxLabeled(checkboxRect, size.ToString(), ref flag);

            // If the checkbox value has changed, update the filter list accordingly.
            if (flag != currentlyAllowed) {
                if (flag) {
                    lamp.SizeFilterList.Add(size);
                }
                else {
                    lamp.SizeFilterList.Remove(size);
                }
            }
        }

        public override void DoWindowContents(Rect inRect) {
            // Draw a label for instructions.
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30), "Select allowed sphere types:");

            // Define a scrollable area.
            Rect outRect = new Rect(inRect.x, inRect.y + 30, inRect.width, inRect.height + 165);
            Rect viewRect = new Rect(0, 0, inRect.width - 16, 300);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            // Iterate through all allowed spheres from Props and create a checkbox for each.
            for (int i = 0; i < lamp.Props.allowedSpheres.Count; i++) {
                addCheckboxSpheres(i, viewRect); 
            }
            addCheckboxGemSize(6, viewRect, CompGemSphere.GemSize.Chip);
            addCheckboxGemSize(7, viewRect, CompGemSphere.GemSize.Mark);
            addCheckboxGemSize(8, viewRect, CompGemSphere.GemSize.Broam);

            Rect sliderRect = new Rect(0, lamp.Props.allowedSpheres.Count * 30, viewRect.width, 30);
            Widgets.HorizontalSlider(sliderRect, ref this.lamp.stormlightThresholdForRefuel, new FloatRange(0f, 1000f), label: $"Minimum Stormlight threshold: {this.lamp.stormlightThresholdForRefuel}", roundTo: 1);
            Widgets.EndScrollView();

            // Optionally, add a close button.
            if (Widgets.ButtonText(new Rect(inRect.width - 100, inRect.height - 35, 100, 30), "Close")) {
                Close();
            }
        }
    }
}
