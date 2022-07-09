using RhuEngine.Components;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RNumerics;

namespace Assets.UnityBindings
{
    public class TextRender : RenderLinkBase<WorldText>
    {
        public override void Init()
        {
        }

        public override void Remove()
        {
        }

        public override void Render()
        {
            RenderingComponent.textRender.Render(Matrix.S(0.1f), RenderingComponent.Entity.GlobalTrans, RenderLayer.Text);
        }

        public override void Started()
        {
        }

        public override void Stopped()
        {
        }
    }

}
