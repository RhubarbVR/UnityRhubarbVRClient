using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuSettings;
using RhuEngine.Settings;

public class UnityRenderSettings : RenderSettingsBase
{
    [SettingsField("Fov")]
    public float Fov = 60f;

    public UnityRenderSettings()
    {
        RenderSettingsChange = () =>
        {
            EngineRunner._.RunonMainThread(() =>
            {
                var cam = EngineRunner._.Camera;
                cam.fieldOfView = Fov;
            });
        };
    }
}
