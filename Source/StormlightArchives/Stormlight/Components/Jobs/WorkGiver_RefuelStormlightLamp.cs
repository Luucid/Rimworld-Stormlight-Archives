using Verse;
using RimWorld;
using System.Linq;
using Verse.AI;
using static HarmonyLib.Code;

namespace StormlightMod {


  
    public class WorkGiver_AddSphereToLamp : WorkGiver_Scanner {
        //public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("SphereLamp"));

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Refuelable);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public virtual JobDef JobStandard => JobDefOf.Refuel;

        public virtual JobDef JobAtomic => JobDefOf.RefuelAtomic;

        public virtual bool CanRefuelThing(Thing t) {
            return !(t is Building_Turret);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false) {
            if (CanRefuelThing(t)) {
                return RefuelWorkGiverUtility.CanRefuel(pawn, t, forced);
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) {
            //LOG HERE
            return RefuelWorkGiverUtility.RefuelJob(pawn, t, forced, JobStandard, JobAtomic);
        }
    }
}
