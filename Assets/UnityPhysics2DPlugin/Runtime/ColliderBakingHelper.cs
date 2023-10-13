using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Material = Unity.Physics.Material;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using BoxCollider = Unity.Physics.BoxCollider;
using CapsuleCollider = Unity.Physics.CapsuleCollider;

namespace UnityPhysics2DPlugin
{
    internal enum ColliderScalingMode
    {
        None,
        Local,
        Global
    }

    internal static class ColliderBakingHelper
    {
        public static RigidTransform GetCompoundFromChild(Transform shape, Transform body)
        {
            var worldFromBody = Math.DecomposeRigidBodyTransform(body.transform.localToWorldMatrix);
            var worldFromShape = Math.DecomposeRigidBodyTransform(shape.transform.localToWorldMatrix);
            return math.mul(math.inverse(worldFromBody), worldFromShape);
        }

        public static Material ProduceMaterial(Collider2D collider)
        {
            var material = new Material();
            if (collider.isTrigger)
            {
                material.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
            }

            material.Friction = collider.friction;
            material.Restitution = collider.bounciness;
            material.FrictionCombinePolicy = Material.CombinePolicy.ArithmeticMean;
            material.RestitutionCombinePolicy = Material.CombinePolicy.ArithmeticMean;

            return material;
        }

        public static CollisionFilter ProduceCollisionFilter(Collider2D collider)
        {
            var layer = collider.gameObject.layer;
            var filter = new CollisionFilter { BelongsTo = (uint)(1 << collider.gameObject.layer) };
            for (var i = 0; i < 32; ++i)
                filter.CollidesWith |= (uint)(Physics2D.GetIgnoreLayerCollision(layer, i) ? 0 : 1 << i);
            return filter;
        }

        public static bool TryCreateCollider(Collider2D src, ColliderScalingMode scalingMode, float positionZ, out BlobAssetReference<Collider> result)
        {
            switch (src)
            {
                case CircleCollider2D circleCollider:
                    result = CreateCircleCollider(circleCollider, scalingMode, positionZ);
                    return true;
                case BoxCollider2D boxCollider:
                    result = CreateBoxCollider(boxCollider, scalingMode, positionZ);
                    return true;
                case CapsuleCollider2D capsuleCollider:
                    result = CreateCapsuleCollider(capsuleCollider, scalingMode, positionZ);
                    return true;
                default:
                    Debug.LogWarning($"Collider type:{src.GetType().Name} is not supported.");
                    result = default;
                    return false;
            }
        }

        public static BlobAssetReference<Collider> CreateCircleCollider(CircleCollider2D src, ColliderScalingMode scalingMode, float positionZ)
        {
            float scale = 1f;
            switch (scalingMode)
            {
                case ColliderScalingMode.None:
                    break;
                case ColliderScalingMode.Local:
                    scale = math.max(src.transform.localScale.x, src.transform.localScale.y);
                    break;
                case ColliderScalingMode.Global:
                    scale = math.max(src.transform.lossyScale.x, src.transform.lossyScale.y);
                    break;
            }

            var geometry = new SphereGeometry()
            {
                Center = new float3(src.offset.x, src.offset.y, positionZ),
                Radius = src.radius * scale
            };
            return SphereCollider.Create(geometry, ProduceCollisionFilter(src), ProduceMaterial(src));
        }

        public static BlobAssetReference<Collider> CreateBoxCollider(BoxCollider2D src, ColliderScalingMode scalingMode, float positionZ)
        {
            float2 scale = new(1f, 1f);
            switch (scalingMode)
            {
                case ColliderScalingMode.None:
                    break;
                case ColliderScalingMode.Local:
                    scale = new(src.transform.localScale.x, src.transform.localScale.y);
                    break;
                case ColliderScalingMode.Global:
                    scale = new(src.transform.lossyScale.x, src.transform.lossyScale.y);
                    break;
            }

            var geometry = new BoxGeometry()
            {
                Size = new float3(
                    src.size.x * scale.x,
                    src.size.y * scale.y,
                    1f
                ),
                Center = new float3(src.offset.x, src.offset.y, positionZ),
                Orientation = quaternion.identity,
                BevelRadius = 0f
            };
            return BoxCollider.Create(geometry, ProduceCollisionFilter(src), ProduceMaterial(src));
        }

        public static BlobAssetReference<Collider> CreateCapsuleCollider(CapsuleCollider2D src, ColliderScalingMode scalingMode, float positionZ)
        {
            float2 scale = new(1f, 1f);
            switch (scalingMode)
            {
                case ColliderScalingMode.None:
                    break;
                case ColliderScalingMode.Local:
                    scale = new(src.transform.localScale.x, src.transform.localScale.y);
                    break;
                case ColliderScalingMode.Global:
                    scale = new(src.transform.lossyScale.x, src.transform.lossyScale.y);
                    break;
            }

            CapsuleGeometry geometry;
            switch (src.direction)
            {
                default:
                case CapsuleDirection2D.Vertical:
                    geometry = new CapsuleGeometry()
                    {
                        Radius = src.size.x * 0.5f * scale.x,
                        Vertex0 = new float3(
                            src.offset.x,
                            (-src.size.y * 0.5f + src.size.x * 0.5f) * scale.y + src.offset.y,
                            positionZ
                        ),
                        Vertex1 = new float3(
                            src.offset.x,
                            (src.size.y * 0.5f - src.size.x * 0.5f) * scale.y + src.offset.y,
                            positionZ
                        )
                    };
                    break;
                case CapsuleDirection2D.Horizontal:
                    geometry = new CapsuleGeometry()
                    {
                        Radius = src.size.y * 0.5f * scale.y,
                        Vertex0 = new float3(
                            (-src.size.x * 0.5f + src.size.y * 0.5f) * scale.x + src.offset.x,
                            src.offset.y,
                            positionZ
                        ),
                        Vertex1 = new float3(
                            (src.size.x * 0.5f - src.size.y * 0.5f + src.offset.x) * scale.x,
                            src.offset.y,
                            positionZ
                        ),
                    };
                    break;
            }
            return CapsuleCollider.Create(geometry, ProduceCollisionFilter(src), ProduceMaterial(src));
        }
    }
}