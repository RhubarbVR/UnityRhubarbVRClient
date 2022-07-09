using RhuEngine.WorldObjects.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UnityBindings
{
    public class EntityConnect : IDisposable
    {
        public GameObject gameObject;

        public Entity Target;
        public Entity Parret;

        public EntityConnect(Transform transform, Entity target, Entity parret = null)
        {
            gameObject = new GameObject(target?.Name ?? null);
            gameObject.transform.parent = transform;
            Target = target;
            Parret = parret;
            if(target is not null)
            {
                target.GlobalTransformChange += Target_GlobalTransformChange;
            }
            parret.GlobalTransformChange += Target_GlobalTransformChange;
            UpdatePos = true;
        }

        private void Target_GlobalTransformChange(Entity arg1, bool arg2)
        {
            UpdatePos = true;
        }

        public void Dispose()
        {
            GameObject.Destroy(gameObject);
        }

        public bool UpdatePos { get; set; } = true;

        public void Render()
        {
            if (UpdatePos)
            {
                EntityHelpers.SetPosFromEntity(gameObject.transform, Target ?? Parret, Parret);
                UpdatePos = false;
            }
        }
    }
}
