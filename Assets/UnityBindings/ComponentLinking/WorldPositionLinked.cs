using RhuEngine.Linker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine.WorldObjects.ECS;
using UnityEngine;

namespace Assets.UnityBindings
{
    public abstract class WorldPositionLinked<T, T2, T3> : WorldPositionLinked<T, T2> where T : RenderingComponent, new() where T2 : UnityEngine.Component where T3 : UnityEngine.Component
    {
        public T3 UnityComponent2;

        public abstract void ContInit();

        public override void ContinueInit()
        {
            UnityComponent2 = gameObject.AddComponent<T3>();
            ContInit();
        }
    }

    public abstract class WorldPositionLinked<T, T2>:WorldPositionLinked<T> where T : RenderingComponent, new() where T2 : UnityEngine.Component
    {
        public T2 UnityComponent;

        public override void StartContinueInit()
        {
            UnityComponent = gameObject.AddComponent<T2>();
            ContinueInit();
        }
        public abstract void ContinueInit();
    }


    public abstract class WorldPositionLinked<T> : RenderLinkBase<T> where T : RenderingComponent, new() 
    {
        public GameObject gameObject;


        public abstract string ObjectName { get; }

        public override void Init()
        {
            EngineRunner._.RunonMainThread(() =>
            {
                gameObject = new GameObject(ObjectName);
                gameObject.transform.parent = EngineRunner._.Root.transform;
                RenderingComponent.Entity.GlobalTransformChange += Entity_GlobalTransformChange;
                StartContinueInit();
                UpdatePosThisFrame = true;
            });
        }
        public abstract void StartContinueInit();

        public override void Remove()
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityEngine.Object.Destroy(gameObject);
            });
        }

        public override void Started()
        {
            EngineRunner._.RunonMainThread(() =>
            {
                gameObject?.SetActive(true);
            });
        }

        public override void Stopped()
        {
            EngineRunner._.RunonMainThread(() =>
            {
                gameObject?.SetActive(false);
            });
        }

        public bool UpdatePosThisFrame { get; private set; } = true;

        private void Entity_GlobalTransformChange(Entity obj, bool data)
        {
            UpdatePosThisFrame = true;
        }

        public override void Render()
        {
            if (UpdatePosThisFrame)
            {
                EntityHelpers.SetPosFromEntity(gameObject.transform, RenderingComponent.Entity);
                UpdatePosThisFrame = false;
            }
        }

    }
}
