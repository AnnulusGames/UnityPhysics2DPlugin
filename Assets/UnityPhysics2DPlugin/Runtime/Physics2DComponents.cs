using Unity.Entities;
using Unity.Mathematics;

namespace UnityPhysics2DPlugin
{
    public struct Physics2DTag : IComponentData { }

    internal struct Physics2DTempData : IComponentData
    {
        public float PositionZ;
        public quaternion Rotation;
    }
}
