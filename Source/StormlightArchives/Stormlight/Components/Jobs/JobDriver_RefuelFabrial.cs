using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;
using System;

namespace StormlightMod {

    public class Toils_Refuel_Fabrial {
        public static Toil SwapInNewGemstone(TargetIndex gemInd, TargetIndex fabrialInd) {
            Toil toil = ToilMaker.MakeToil("SwapInNewGemstone");
            toil.initAction = delegate {
                Pawn pawn = toil.actor;
                Thing fabrial = pawn.CurJob.GetTarget(fabrialInd).Thing;
                Thing gemstone = pawn.CurJob.GetTarget(gemInd).Thing;


                if (isHeatrial(fabrial)) 
                {
                    doRemoveToil(fabrial.TryGetComp<CompHeatrial>());
                    doReplaceToil(fabrial.TryGetComp<CompHeatrial>(), gemstone);
                }
                else if (isSprenTrapper(fabrial)) 
                {
                    doRemoveToil(fabrial.TryGetComp<CompSprenTrapper>());
                    doReplaceToil(fabrial.TryGetComp<CompSprenTrapper>(), gemstone);
                }
                else if (isGenericFabrial(fabrial)) {
                    doRemoveToil(fabrial.TryGetComp<CompBasicBuildingFabrial>());
                    doReplaceToil(fabrial.TryGetComp<CompBasicBuildingFabrial>(), gemstone);
                }

            };

            toil.AddFinishAction(delegate {
                Thing carriedThing = toil.actor.carryTracker.CarriedThing;
                if (carriedThing != null) {
                    toil.actor.carryTracker.TryDropCarriedThing(toil.actor.Position, ThingPlaceMode.Near, out Thing droppedThing);
                    if (droppedThing != null && droppedThing.Spawned) {
                        droppedThing.DeSpawn();
                    }
                }
            });

            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil RemoveGemstone(TargetIndex fabrialInd) {
            Toil toil = ToilMaker.MakeToil("RemoveGemstone");
            toil.initAction = delegate {
                Pawn pawn = toil.actor;
                Thing fabrial = pawn.CurJob.GetTarget(fabrialInd).Thing;

                if (isHeatrial(fabrial)) {
                    doRemoveToil(fabrial.TryGetComp<CompHeatrial>());
                }
                else if (isSprenTrapper(fabrial)) {
                    doRemoveToil(fabrial.TryGetComp<CompSprenTrapper>());
                }
                else if (isGenericFabrial(fabrial)) {
                    doRemoveToil(fabrial.TryGetComp<CompBasicBuildingFabrial>());
                }

            };

            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }


        static void doReplaceToil<T>(T fabComp, Thing gemstone) where T : IGemstoneHandler
        { 
            fabComp.AddGemstone(gemstone as ThingWithComps);
        }

        static void doRemoveToil<T>(T fabComp) where T : IGemstoneHandler {
            fabComp.RemoveGemstone();
        }

        static bool isHeatrial(Thing fabrial) {
            CompHeatrial fabComp = fabrial.TryGetComp<CompHeatrial>();
            return fabComp != null;
        }
        static bool isSprenTrapper(Thing fabrial) {
            CompSprenTrapper fabComp = fabrial.TryGetComp<CompSprenTrapper>();
            return fabComp != null;
        }
        static bool isGenericFabrial(Thing fabrial) {
            CompBasicBuildingFabrial fabComp = fabrial.TryGetComp<CompBasicBuildingFabrial>();
            return fabComp != null;
        }
    }

    public class JobDriver_AddGemToFabrial : JobDriver {
        private const TargetIndex FabrialIndex = TargetIndex.A;
        private const TargetIndex GemIndex = TargetIndex.B;
        public const int ReGemmingDuration = 240;


        protected Thing Fabrial => job.GetTarget(FabrialIndex).Thing;
        protected Thing Gemstone => job.GetTarget(GemIndex).Thing;



        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            Log.Message("TryMakePreToilReservations");
            return pawn.Reserve(Fabrial, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Gemstone, job, 1, -1, null, errorOnFailed);
        }



        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedNullOrForbidden(FabrialIndex);

            AddFailCondition(() => Fabrial == null);
            AddFailCondition(() => Gemstone == null);

            yield return Toils_General.DoAtomic(delegate {
                job.count = 1;
            });

            yield return Toils_Goto.GotoThing(GemIndex, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(GemIndex).FailOnSomeonePhysicallyInteracting(GemIndex);
            yield return Toils_Haul.StartCarryThing(GemIndex, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(GemIndex);

            yield return Toils_Goto.GotoThing(FabrialIndex, PathEndMode.Touch);
            yield return Toils_General.Wait(ReGemmingDuration).FailOnDestroyedNullOrForbidden(GemIndex).FailOnDestroyedNullOrForbidden(FabrialIndex)
                .FailOnCannotTouch(FabrialIndex, PathEndMode.Touch)
                .WithProgressBarToilDelay(FabrialIndex);
            yield return Toils_Refuel_Fabrial.SwapInNewGemstone(GemIndex, FabrialIndex);  //custom toil
        }
    }


    public class JobDriver_RemoveGemFromFabrial : JobDriver {
        private const TargetIndex FabrialIndex = TargetIndex.A;
        public const int JobDuration = 75;


        protected Thing Fabrial => job.GetTarget(FabrialIndex).Thing;



        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            Log.Message("TryMakePreToilReservations");
            return pawn.Reserve(Fabrial, job, 1, -1, null, errorOnFailed);
        }



        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedNullOrForbidden(FabrialIndex);

            AddFailCondition(() => Fabrial == null);

            yield return Toils_General.DoAtomic(delegate {
                job.count = 1;
            });

            yield return Toils_Goto.GotoThing(FabrialIndex, PathEndMode.Touch);
            yield return Toils_General.Wait(JobDuration).FailOnDestroyedNullOrForbidden(FabrialIndex) 
                .FailOnCannotTouch(FabrialIndex, PathEndMode.Touch)
                .WithProgressBarToilDelay(FabrialIndex);
            yield return Toils_Refuel_Fabrial.RemoveGemstone(FabrialIndex);  //custom toil
        }
    }
}
