using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;

namespace StormlightMod {
    public class JobDriver_AddSphereToLamp : JobDriver {
        private const TargetIndex RefuelableInd = TargetIndex.A;

        private const TargetIndex FuelInd = TargetIndex.B;

        public const int RefuelingDuration = 240;

        protected Thing Lamp => job.GetTarget(TargetIndex.A).Thing;

        protected StormlightLamps LampComp => Lamp.TryGetComp<StormlightLamps>();

        protected Thing Sphere => job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            //if (pawn.Reserve(Lamp, job, 1, -1, null, errorOnFailed)) {
            //    return pawn.Reserve(Sphere, job, 1, -1, null, errorOnFailed);
            //}
            return pawn.Reserve(Lamp, job, 1, -1, null, errorOnFailed);
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            AddEndCondition(() => (!LampComp.IsFull()) ? JobCondition.Ongoing : JobCondition.Succeeded);
            //AddFailCondition(() => !job.playerForced && !LampComp.ShouldAutoRefuelNowIgnoringFuelPct);
            //AddFailCondition(() => !LampComp.allowAutoRefuel && !job.playerForced);
            yield return Toils_General.DoAtomic(delegate {
                //job.count = LampComp.GetFuelCountToFullyRefuel();
                job.count = 1;
            });
            Toil reserveFuel = Toils_Reserve.Reserve(TargetIndex.B);
            yield return reserveFuel;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None, takeFromValidStorage: true);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.A);
            //yield return Toils_Refuel.FinalizeRefueling(TargetIndex.A, TargetIndex.B);
        }
    }
}
