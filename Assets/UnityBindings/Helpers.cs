using RhuEngine.WorldObjects.ECS;
using RNumerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public static class EntityHelpers
    {
        public static void SetPosFromEntity(Transform transform, Entity entity, Entity reletive = null)
        {
            Matrix m = Matrix.Identity;
            if (reletive is not null)
            {
                if (entity.Depth <= 100)
                {
                    m = reletive.GlobalToLocal(entity.GlobalTrans);
                }
            }
            else
            {
                if (entity.Depth <= 100)
                {
                    m = entity.GlobalTrans;
                }

            };
            var pos = m.Translation;
            var rot = m.Rotation;
            var scale = m.Scale;
            transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
            transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
            transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, float.IsNaN(scale.z) ? 0 : scale.z);
        }
    }

}
