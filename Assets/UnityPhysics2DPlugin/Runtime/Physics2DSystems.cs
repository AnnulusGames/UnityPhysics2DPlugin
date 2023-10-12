using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace UnityPhysics2DPlugin
{
    [BurstCompile]
    [UpdateInGroup(typeof(Physics2DSystemGroup), OrderFirst = true)]
    internal partial struct BeginPhysics2DSimulationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job1 = new AdjustTransformJob();
            var job2 = new AdjustMassJob();
            var job3 = new AdjustVelocityJob();
            job1.ScheduleParallel();
            job2.ScheduleParallel();
            job3.ScheduleParallel();
            state.CompleteDependency();
        }

        [BurstCompile]
        partial struct AdjustTransformJob : IJobEntity
        {
            public void Execute(in Physics2DTag tag, ref LocalTransform transform, ref Physics2DTempData temp)
            {
                temp.PositionZ = transform.Position.z;
                temp.Rotation = transform.Rotation;
                transform.Position = new float3(transform.Position.x, transform.Position.y, 0f);
                transform.Rotation = MathHelper.ToQuaternion(new float3(0f, 0f, MathHelper.ToEulerAngles(transform.Rotation).z));
            }
        }

        [BurstCompile]
        partial struct AdjustMassJob : IJobEntity
        {
            public void Execute(in Physics2DTag tag, ref PhysicsMass mass)
            {
                mass.Transform.pos.z = 0f;
                mass.Transform.rot = MathHelper.ToQuaternion(new float3(0f, 0f, MathHelper.ToEulerAngles(mass.Transform.rot).z));
            }
        }

        [BurstCompile]
        partial struct AdjustVelocityJob : IJobEntity
        {
            public void Execute(in Physics2DTag tag, ref PhysicsVelocity velocity)
            {
                velocity.Linear.z = 0f;
                velocity.Angular.x = 0f;
                velocity.Angular.y = 0f;
            }
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(Physics2DSystemGroup), OrderLast = true)]
    internal partial struct EndPhysics2DSimulationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job1 = new AdjustTransformJob();
            var job2 = new AdjustVelocityJob();
            job1.ScheduleParallel();
            job2.ScheduleParallel();
            state.CompleteDependency();
        }

        [BurstCompile]
        partial struct AdjustTransformJob : IJobEntity
        {
            public void Execute(in Physics2DTag tag, ref LocalTransform transform, ref Physics2DTempData temp)
            {
                transform.Position = new float3(transform.Position.x, transform.Position.y, temp.PositionZ);

                var eulerAngles = MathHelper.ToEulerAngles(temp.Rotation);
                eulerAngles.z = MathHelper.ToEulerAngles(transform.Rotation).z;
                transform.Rotation = MathHelper.ToQuaternion(eulerAngles);
            }
        }

        [BurstCompile]
        partial struct AdjustVelocityJob : IJobEntity
        {
            public void Execute(in Physics2DTag tag, ref PhysicsVelocity velocity)
            {
                velocity.Linear.z = 0f;
                velocity.Angular.x = 0f;
                velocity.Angular.y = 0f;
            }
        }
    }
}
