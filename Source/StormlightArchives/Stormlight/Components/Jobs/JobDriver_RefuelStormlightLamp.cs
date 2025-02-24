using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;

namespace StormlightMod {

    public class Toils_Resphere_Lamp {
        public static Toil SwapInNewSpheres(Pawn pawn, Thing lamp, Thing sphere) {
            Toil toil = ToilMaker.MakeToil("SwapInNewSpheres");
            toil.initAction = delegate {
                Job curJob = toil.actor.CurJob;
                StormlightLamps lampComp = lamp.TryGetComp<StormlightLamps>();
                if (lampComp != null) {
                    ThingWithComps removedSphere = lampComp.RemoveSphereFromLamp(0, false);
                    lampComp.AddSphereToLamp(sphere as ThingWithComps);
                    CompSpherePouch spherePouch = CompSpherePouch.GetWornSpherePouch(pawn);

                    if (spherePouch != null) {
                        spherePouch.removeSpheresFromRemoveList();
                        spherePouch.AddSphereToPouch(removedSphere);
                        Log.Message("Sphere is not null!");
                    }
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
    }

    public class JobDriver_AddSphereToLamp : JobDriver_Refuel {
        private const TargetIndex LampIndex = TargetIndex.A;
        private const TargetIndex SphereIndex = TargetIndex.B;

        public const int RespheringDuration = 240;

        protected StormlightLamps LampComp => Lamp.TryGetComp<StormlightLamps>();

        protected Thing Lamp => job.GetTarget(LampIndex).Thing;
        protected Thing Sphere => job.GetTarget(SphereIndex).Thing;



        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            Log.Message("TryMakePreToilReservations");
            return pawn.Reserve(Lamp, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Sphere, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedNullOrForbidden(LampIndex);
            AddEndCondition(() => (!LampComp.IsFull()) ? JobCondition.Ongoing : JobCondition.Succeeded);
            AddFailCondition(() => Sphere == null);
            //AddFailCondition(() => !LampComp.allowAutoRefuel && !job.playerForced);
            yield return Toils_General.DoAtomic(delegate {
                job.count = LampComp.GetDunSphereCount();
            });

            //if pawn does not have spheres in inventory or pouch -> DO section A
            //section A BEGIN//
            //Toil reserveFuel = Toils_Reserve.Reserve(SphereIndex);
            //yield return reserveFuel;

            //yield return Toils_Goto.GotoThing(SphereIndex, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(SphereIndex).FailOnSomeonePhysicallyInteracting(SphereIndex);
            //yield return Toils_Haul.StartCarryThing(SphereIndex, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(SphereIndex);
            //yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, SphereIndex, TargetIndex.None, takeFromValidStorage: true);
            //section A END//

            yield return Toils_Goto.GotoThing(LampIndex, PathEndMode.Touch);
            yield return Toils_General.Wait(RespheringDuration).FailOnDestroyedNullOrForbidden(SphereIndex).FailOnDestroyedNullOrForbidden(LampIndex)
                .FailOnCannotTouch(LampIndex, PathEndMode.Touch)
                .WithProgressBarToilDelay(LampIndex);
            yield return Toils_Resphere_Lamp.SwapInNewSpheres(pawn, Lamp, Sphere);  //custom toil
        }
    }
}
