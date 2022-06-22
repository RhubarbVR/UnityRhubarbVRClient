using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using System.IO;
using UnityEngine.XR;
using RNumerics;
using System.Threading;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.XR.Management;
using System.Text;

public class EngineRunner : MonoBehaviour
{

    [ThreadStatic]
    public static bool IsMainThread = false;

    public SafeList<Action> runonmainthread = new SafeList<Action>();

    public static EngineRunner _;


    private static Vector2f LastMousePos;
    public static Vector2f MouseDelta;

    public GameObject UserRoot;

    public GameObject UserHead;

    public Camera Camera;

    public GameObject CameraOffset;

    public GameObject Root;
    public void RunonMainThread(Action action)
    {
        if (IsMainThread)
        {
            action();
            return;
        }
        runonmainthread.SafeAdd(action);
    }

    public GameObject LeftController;

    public GameObject RightController;

    public Shader Unlit;
    public Shader UnlitTransparentAdditive;
    public Shader UnlitTransparentBlend;
    public Shader TextShader;


    public InputDevice left;

    public InputDevice right;

    public bool isHardwarePresent()
    {
        InputDevices.deviceConnected += InputDevices_Update;
        InputDevices.deviceDisconnected += InputDevices_Update;
        InputDevices_Update(new InputDevice { });
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            return true;
        }
        return false;
    }


    public Engine engine;
    public OutputCapture cap;
    public UnityEngineLink link;

    public bool isVR = false;

    public bool IsVREnabled = false;

    IEnumerator Start()
    {
        XRSettings.enabled = IsVREnabled;
        IsMainThread = true;
        _ = this;
        yield return null;

        var xrLoader = XRGeneralSettings.Instance.Manager.activeLoader;
        var xrDisplay = xrLoader.GetLoadedSubsystem<XRDisplaySubsystem>();
        if (!IsVREnabled)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            xrDisplay.Stop();
        }
        var platform = Application.platform;
        if(platform == RuntimePlatform.Android)
        {
            Debug.Log("Is on Android");
        }
        isVR = isHardwarePresent() && IsVREnabled;
        if (!isVR)
        {
            Debug.Log("Starting RuhbarbVR ScreenMode");
            CameraOffset.transform.localPosition = Vector3.zero;
            LeftController.SetActive(false);
            RightController.SetActive(false);
        }
        else
        {
            Debug.Log("Starting RuhbarbVR VRMode");
        }
        Debug.Log("Graphics Memory Size: " + SystemInfo.graphicsMemorySize);
        cap = new OutputCapture();
        link = new UnityEngineLink(this);
        var args = Environment.GetCommandLineArgs();
        var appPath = Path.GetDirectoryName(Application.dataPath);
        engine = new Engine(link, args, cap, appPath);
        engine.Init();
    }

    public interface IRemoveLater
    {
        public void Remove();


        public bool UsedThisFrame { get; set; }
    }

    public class TempMesh:IRemoveLater
    {
        public bool UsedThisFrame { get; set; } = true;

        public GameObject gameObject;

        public MeshFilter meshfilter;

        public MeshRenderer meshRenderer;

        Material targetMit;


        public void Reload(Mesh mesh, Material target, Matrix p)
        {
            if(targetMit != target)
            {
                targetMit = target;
            }
            UsedThisFrame = true;
            meshRenderer.material = target;
            meshfilter.mesh = mesh;
            var pos = p.Translation;
            var rot = p.Rotation;
            var scale = p.Scale;
            gameObject.transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
            gameObject.transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
            gameObject.transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, float.IsNaN(scale.z) ? 0 : scale.z);
        }

        public TempMesh(string id, Mesh mesh, Material target, Matrix p)
        {
            targetMit = target;
            gameObject = new GameObject("TempMesh" + id);
            gameObject.transform.parent = EngineRunner._.Root.transform;
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = target;
            meshfilter = gameObject.AddComponent<MeshFilter>();
            meshfilter.mesh = mesh;
            var pos = p.Translation;
            var rot = p.Rotation;
            var scale = p.Scale;
            gameObject.transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
            gameObject.transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
            gameObject.transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, (float.IsNaN(scale.z) ? 0 : scale.z));
        }

        public void Remove()
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }

    public Dictionary<string, IRemoveLater> tempObjects = new();

    public void Draw(string id, Mesh mesh, Material target, Matrix p)
    {
        if(target is null)
        {
            return;
        }
        id = "Mesh." + id;
        if (!tempObjects.ContainsKey(id))
        {
            tempObjects.Add(id, new TempMesh(id, mesh, target, p));
        }
        else
        {
            ((TempMesh)tempObjects[id]).Reload(mesh, target, p);
        }
    }
    void MainThreadUpdate()
    {
        runonmainthread.SafeOperation((list) =>
        {
            foreach (var item in list)
            {
                item.Invoke();
            }
            list.Clear();
        });
    }

    public float speedMultply = 200f;
    public Texture LoadingTexture;

    public void InputDevices_Update(InputDevice inputDevice)
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
        Debug.Log($"Devices Update Count:{devices.Count}");
        for (int i = 0; i < devices.Count; i++)
        {
            Debug.Log($"Devices {i} Name:{devices[i].name} Manufacturer:{devices[i].manufacturer} Manufacturer:{devices[i].characteristics}");
            var charictes = devices[i].characteristics;
            if ((charictes & InputDeviceCharacteristics.Left) != InputDeviceCharacteristics.None)
            {
                if ((charictes & InputDeviceCharacteristics.Controller) != InputDeviceCharacteristics.None)
                {
                    left = devices[i];
                }
            }
            if ((charictes & InputDeviceCharacteristics.Right) != InputDeviceCharacteristics.None)
            {
                if ((charictes & InputDeviceCharacteristics.Controller) != InputDeviceCharacteristics.None)
                {
                    right = devices[i];
                }
            }
        }
    }

    void Update()
    {
        if(engine is null)
        {
            return;
        }
        MainThreadUpdate();
        var sens = speedMultply * Time.deltaTime;
        MouseDelta = new Vector2f(Input.GetAxis("Mouse X") * sens, -Input.GetAxis("Mouse Y") * sens);
        foreach (var item in tempObjects)
        {
            item.Value.UsedThisFrame = false;
        }
        MainThreadUpdate();
        engine.Step();
        MainThreadUpdate();
        var removethisframe = new List<string>();
        foreach (var item in tempObjects)
        {
            if (!item.Value.UsedThisFrame)
            {
                removethisframe.Add(item.Key);
                item.Value.Remove();
            }
        }
        foreach (var item in removethisframe)
        {
            tempObjects.Remove(item);
        }
        MainThreadUpdate();
    }
}
