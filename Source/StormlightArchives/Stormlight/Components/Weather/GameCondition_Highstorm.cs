using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using static HarmonyLib.Code;

namespace StormlightMod {
    public class HighstormExtension : DefModExtension {
        public float windStrength;
        public string windDirection;
        public int stormDuration;
    }

    public class GameCondition_Highstorm : GameCondition {

        private Random m_Rand = new Random();
        public override void End() {
            base.End();

            if (SingleMap != null) {
                //Log.Message("Highstorm ended. Forcing weather to clear.");
                SingleMap.weatherManager.TransitionTo(WeatherDefOf.Clear);
            }
        }


        public override void GameConditionTick() {
            base.GameConditionTick();
            var ext = def.GetModExtension<HighstormExtension>();
            if (ext == null) return;

            if (Find.TickManager.TicksGame % 8 == 0) {
                tryToInfuseThings();
                if (StormlightMod.settings.enableHighstormPushing) {
                    moveItem();
                }
            }
        }

        public void tryToInfuseThings() {
            List<Thing> things = this.SingleMap.listerThings.ThingsInGroup(ThingRequestGroup.Everything);
            foreach (Thing thing in things) {
                if (thing.Position.Roofed(thing.Map))
                    continue;

                if (thing.TryGetComp<CompStormlight>() is CompStormlight stormlightComp) {
                    stormlightComp.infuseStormlight(5f);
                }
                else if (thing.def == StormlightModDefs.whtwl_Apparel_SpherePouch && !thing.Position.Roofed(thing.Map)) {
                    var pouch = thing.TryGetComp<CompSpherePouch>();
                    if (pouch != null) {
                        pouch.InfuseStormlight(5f);
                    }
                }
                else if (thing.def == StormlightModDefs.whtwl_SphereLamp_Wall && !thing.Position.Roofed(thing.Map)) {
                    var lamp = thing.TryGetComp<StormlightLamps>();
                    if (lamp != null) {
                        lamp.InfuseStormlight(5f);
                    }
                }
            }
        }
        public void destoyIfCollide(Thing item, Map itemMap, IntVec3 newPos) {
            List<Thing> thingsHere = itemMap.thingGrid.ThingsListAtFast(newPos).ListFullCopy();
            foreach (Thing thing in thingsHere) {
                if (thing is Plant plant && plant.def.plant.harvestedThingDef != null) {
                    // Harvest yield
                    int woodCount = 5;
                    Thing wood = ThingMaker.MakeThing(plant.def.plant.harvestedThingDef);
                    wood.stackCount = woodCount;

                    // Destroy the plant
                    plant.Destroy(DestroyMode.Vanish);

                    // Place wood
                    GenPlace.TryPlaceThing(wood, newPos, itemMap, ThingPlaceMode.Near);
                }
                else if (thing is Building building) {

                    if (building.Stuff != null && building.Stuff.stuffProps.categories?.Contains(StuffCategoryDefOf.Woody) == true) {
                        //Log.Message($"Damaging wooden wall: {building.def.defName}");
                        building.TakeDamage(new DamageInfo(DamageDefOf.Blunt,
                                                           10, // amount
                                                           0, // armorPenetration
                                                           -1, // angle
                                                           item, // instigator
                                                           null, // hitPart
                                                           null, // weapon
                                                           DamageInfo.SourceCategory.ThingOrUnknown));
                    }
                }
                else if (thing is Pawn pawn) {
                    if (pawn.Dead == false) {
                        DamageInfo damage = new DamageInfo(DamageDefOf.Blunt,
                                                           10, // Damage amount
                                                           0,  // Armor penetration
                                                           -1, // Angle
                                                           item, // Instigator (this tells the log what hit the pawn)
                                                           null, // Hit part (null = random)
                                                           null, // Weapon (not relevant here)
                                                           DamageInfo.SourceCategory.ThingOrUnknown);



                        // Apply damage
                        pawn.TakeDamage(damage);
                    }
                }


            }
        }
        public void moveItem() {


            // Copy the list to avoid modifying during enumeration
            List<Thing> items = this.SingleMap.listerThings
                .ThingsInGroup(ThingRequestGroup.HaulableEver)
                .ListFullCopy();

            foreach (Thing item in items) {
                if (!item.Spawned) continue;

                Map itemMap = item.Map;
                // if itemMap == null, skip
                if (itemMap == null) continue;

                // push logic
                IntVec3 newPos = item.Position + (IntVec3.West * m_Rand.Next(1, 2));
                destoyIfCollide(item, itemMap, newPos);

                if (!newPos.Walkable(itemMap))
                    continue;

                if (newPos.DistanceToEdge(itemMap) == 0) {
                    item.DeSpawn(DestroyMode.Vanish);
                }
                else {
                    item.Position = newPos;
                }
            }
        }
    }
}
