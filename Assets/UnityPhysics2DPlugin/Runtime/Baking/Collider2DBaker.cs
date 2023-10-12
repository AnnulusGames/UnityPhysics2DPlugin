using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace UnityPhysics2DPlugin.Baking
{
    public abstract class Collider2DBakerBase<TCollider> : Baker<TCollider> where TCollider : Collider2D
    {
        static readonly List<Collider2D> colliderComponents = new();

        public override void Bake(TCollider authoring)
        {
            if (authoring.attachedRigidbody != null) return;

            GetComponentsInParent(colliderComponents);
            if (colliderComponents.Where(x => x.gameObject != authoring.gameObject).Count() > 0) return;

            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<Physics2DTag>(entity);
            AddComponent<Physics2DTempData>(entity);

            AddColliders(entity, authoring);
            if (authoring.sharedMaterial != null) DependsOn(authoring.sharedMaterial);
            
            AddSharedComponent(entity, new PhysicsWorldIndex(UnityPhysics2D.PhysicsWorldIndex));
        }

        void AddColliders(Entity entity, TCollider authoring)
        {
            GetComponentsInChildren(colliderComponents);

            if (colliderComponents.Count == 0 && colliderComponents[0].gameObject == authoring.gameObject)
            {
                if (!colliderComponents[0].enabled) return;

                if (ColliderBakingHelper.TryCreateCollider(colliderComponents[0], ColliderScalingMode.Local, 0f, out var physicsCollider))
                {
                    AddComponent(entity, new PhysicsCollider() { Value = physicsCollider });
                }
                return;
            }

            var childColliders = new NativeList<CompoundCollider.ColliderBlobInstance>(colliderComponents.Count, Allocator.Temp);
            foreach (var childCollider in colliderComponents)
            {
                if (!childCollider.enabled) continue;

                if (ColliderBakingHelper.TryCreateCollider(childCollider, ColliderScalingMode.Global, authoring.transform.position.z - childCollider.transform.position.z, out var physicsCollider))
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
            }
        }
    }

    public class BoxCollider2DBaker : Collider2DBakerBase<BoxCollider2D> { }
    public class CircleCollider2DBaker : Collider2DBakerBase<CircleCollider2D> { }
    public class CapsuleCollider2DBaker : Collider2DBakerBase<CapsuleCollider2D> { }
}