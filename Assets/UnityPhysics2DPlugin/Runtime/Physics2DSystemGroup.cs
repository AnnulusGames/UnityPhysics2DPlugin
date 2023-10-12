using Unity.Entities;
using Unity.Physics.Systems;

namespace UnityPhysics2DPlugin
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class Physics2DSystemGroup : CustomPhysicsSystemGroup
    {
        public Physics2DSystemGroup() : base(UnityPhysics2D.PhysicsWorldIndex, true) { }
    }
}
