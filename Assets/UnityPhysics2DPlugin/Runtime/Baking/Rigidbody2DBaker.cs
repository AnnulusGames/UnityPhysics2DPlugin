using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.GraphicsIntegration;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;

namespace UnityPhysics2DPlugin.Baking
{
    public class Rigidbody2DBaker : Baker<Rigidbody2D>
    {
        static readonly List<Collider2D> colliderComponents = new();

        public override void Bake(Rigidbody2D authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<Physics2DTag>(entity);
            AddComponent<Physics2DTempData>(entity);

            var bakingData = new RigidbodyBakingData
            {
                isKinematic = authoring.isKinematic,
                mass = authoring.mass,
                automaticCenterOfMass = false,
                centerOfMass = new float3(authoring.centerOfMass.x, authoring.centerOfMass.y, 0f),
                automaticInertiaTensor = true
            };
            AddComponent(entity, bakingData);

            AddSharedComponent(entity, new PhysicsWorldIndex(UnityPhysics2D.PhysicsWorldIndex));

            var bodyTransform = GetComponent<Transform>();

            var motionType = authoring.isKinematic ? BodyMotionType.Kinematic : BodyMotionType.Dynamic;
            var hasInterpolation = authoring.interpolation != RigidbodyInterpolation2D.None;
            PostProcessTransform(bodyTransform, motionType);

            AddColliders(entity, authoring);
            if (authoring.sharedMaterial != null) DependsOn(authoring.sharedMaterial);

            if (IsStatic() || authoring.bodyType == RigidbodyType2D.Static) return;

            if (hasInterpolation)
            {
                AddComponent(entity, new PhysicsGraphicalSmoothing());

                if (authoring.interpolation == RigidbodyInterpolation2D.Interpolate)
                {
                    AddComponent(entity, new PhysicsGraphicalInterpolationBuffer
                    {
                        PreviousTransform = Math.DecomposeRigidBodyTransform(bodyTransform.localToWorldMatrix)
                    });
                }
            }

            AddComponent(entity, new PhysicsVelocity());

            if (!authoring.isKinematic)
            {
                AddComponent(entity, new PhysicsDamping
                {
                    Linear = authoring.drag,
                    Angular = authoring.angularDrag
                });
                AddComponent(entity, new PhysicsGravityFactor { Value = authoring.gravityScale });
            }
            else
            {
                AddComponent(entity, new PhysicsGravityFactor { Value = authoring.gravityScale });
            }
        }

        void AddColliders(Entity entity, Rigidbody2D authoring)
        {
            GetComponentsInChildren(colliderComponents);
            if (colliderComponents.Count == 0) return;

            if (colliderComponents.Count == 1 && colliderComponents[0].gameObject == authoring.gameObject)
            {
                if (!colliderComponents[0].enabled) return;

                if (ColliderBakingHelper.TryCreateCollider(colliderComponents[0], ColliderScalingMode.Local, 0f, out var physicsCollider))
                {
                    AddComponent(entity, new PhysicsCollider() { Value = physicsCollider });
                    AddComponent(entity, authoring.isKinematic ?
                        PhysicsMass.CreateKinematic(physicsCollider.Value.MassProperties) :
                        PhysicsMass.CreateDynamic(physicsCollider.Value.MassProperties, authoring.mass)
                    );
                }
                return;
            }

            var childColliders = new NativeList<CompoundCollider.ColliderBlobInstance>(colliderComponents.Count, Allocator.Temp);
            foreach (var childCollider in colliderComponents)
            {
                if (!childCollider.enabled) continue;

                if (ColliderBakingHelper.TryCreateCollider(childCollider, ColliderScalingMode.Local, authoring.transform.position.z - childCollider.transform.position.z, out var physicsCollider))
                {
                    var childEntity = GetEntity(childCollider.gameObject, TransformUsageFlags.Dynamic);

                    childColliders.Add(new CompoundCollider.ColliderBlobInstance()
                    {
                        Collider = physicsCollider,
                        CompoundFromChild = ColliderBakingHelper.GetCompoundFromChild(childCollider.transform, authoring.transform),
                        Entity = childEntity
                    });
                }
            }

            if (childColliders.Length > 0)
            {
                var compoundCollider = CompoundCollider.Create(childColliders.ToArray(Allocator.Temp));
                AddComponent(entity, new PhysicsCollider() { Value = compoundCollider });
                AddComponent(entity, authoring.isKinematic ?
                    PhysicsMass.CreateKinematic(compoundCollider.Value.MassProperties) :
                    PhysicsMass.CreateDynamic(compoundCollider.Value.MassProperties, authoring.mass)
                );
            }
            else
            {
                var massProperties = MassProperties.UnitSphere;
                AddComponent(entity, authoring.isKinematic ?
                    PhysicsMass.CreateKinematic(massProperties) :
                    PhysicsMass.CreateDynamic(massProperties, authoring.mass)
                );
            }
        }

        bool NeedsPostProcessTransform(Transform worldTransform, bool gameObjectStatic, BodyMotionType motionType, out PhysicsPostProcessData data)
        {
            Transform transformParent = worldTransform.parent;
            bool haveParentEntity = transformParent != null;
            bool haveBakedTransform = gameObjectStatic;
            bool hasNonIdentityScale = HasNonIdentityScale(worldTransform);
            bool unparent = motionType != BodyMotionType.Static || hasNonIdentityScale || !haveParentEntity || haveBakedTransform;

            data = default;
            if (unparent)
            {
                data = new PhysicsPostProcessData()
                {
                    LocalToWorldMatrix = worldTransform.localToWorldMatrix,
                    LossyScale = worldTransform.lossyScale
                };
            }
            return unparent;
        }

        bool HasNonIdentityScale(Transform bodyTransform)
        {
            return math.lengthsq((float3)bodyTransform.lossyScale - new float3(1f)) > 0f;
        }

        void PostProcessTransform(Transform bodyTransform, BodyMotionType motionType = BodyMotionType.Static)
        {
            if (NeedsPostProcessTransform(bodyTransform, IsStatic(), motionType, out PhysicsPostProcessData data))
            {
                var entity = GetEntity(TransformUsageFlags.ManualOverride);
                AddComponent(entity, new LocalToWorld { Value = bodyTransform.localToWorldMatrix });

                if (HasNonIdentityScale(bodyTransform))
                {
                    var compositeScale = float4x4.Scale(bodyTransform.localScale);
                    AddComponent(entity, new PostTransformMatrix { Value = compositeScale });
                }
                var uniformScale = 1.0f;
                LocalTransform transform = LocalTransform.FromPositionRotationScale(bodyTransform.localPosition, bodyTransform.localRotation, uniformScale);

                AddComponent(entity, transform);
                AddComponent(entity, data);
            }
        }
    }
}