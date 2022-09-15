using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UnityBindings
{
    public class ArmatureRenderLink : WorldPositionLinked<Armature>
    {
        public EntityConnect[] childs = new EntityConnect[0];

        public override string ObjectName => "Armature";

        public event Action ChildrenReload;

        public override void StartContinueInit()
        {
            LinkedComp.ArmatureEntitys.Changed += ArmatureEntitys_Changed;
            ArmatureEntitys_Changed(null);
        }

        public override void Render()
        {
            base.Render();
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i].Render();
            }
        }

        private void ArmatureEntitys_Changed(IChangeable obj)
        {
            RUpdateManager.ExecuteOnStartOfFrame(this, () =>
            {
                EngineRunner._.RunonMainThread(() =>
                {
                    for (int i = 0; i < childs.Length; i++)
                    {
                        childs[i]?.Dispose();
                    }
                    childs = new EntityConnect[LinkedComp.ArmatureEntitys.Count];
                    for (int i = 0; i < childs.Length; i++)
                    {
                        var target = LinkedComp.ArmatureEntitys[i].Target;
                        childs[i] = new EntityConnect(gameObject.transform, target, LinkedComp.Entity);
                    }
                    ChildrenReload?.Invoke();
                });
            });
        }

        public override void Remove()
        {
            EngineRunner._.RunonMainThread(() =>
            {
                for (int i = 0; i < childs.Length; i++)
                {
                    childs[i]?.Dispose();
                }
            });
            base.Remove();
        }
    }
}
