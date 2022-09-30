using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UnityBindings
{
    public class UnitySound : WorldPositionLinked<RhuEngine.Components.SoundSource, UnityEngine.AudioSource>
    {
        public override string ObjectName => "Sound"; 

        public override void ContinueInit()
        {
        
        }
    }

}
