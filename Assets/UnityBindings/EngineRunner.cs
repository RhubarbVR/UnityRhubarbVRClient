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
using UnityEngine.Events;
using B83.Win32;

public class EngineRunner : MonoBehaviour
{

    public GameObject TouchCanvas;

    public FixedJoystick Left;

    public FixedJoystick Right;

    public void ClickScreen()
    {
        RInput.ingectedkeys.Add(Key.MouseRight, new RInput.InjectKey(true, true));
        RWorld.ExecuteOnEndOfFrame(() =>
        {
            RInput.ingectedkeys.Remove(Key.MouseRight);
        });
    }
    public void DashOpen()
    {
        RInput.ingectedkeys.Add(Key.Ctrl, new RInput.InjectKey(true, true));
        RInput.ingectedkeys.Add(Key.Space, new RInput.InjectKey(true, true));
        RWorld.ExecuteOnEndOfFrame(() =>
        {
            RInput.ingectedkeys.Remove(Key.Ctrl);
            RInput.ingectedkeys.Remove(Key.Space);
        });
    }

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
    public Shader PBRShader;
    public Shader TwoSidedUnlit;
    public Shader TwoSidedUnlitTransparentAdditive;
    public Shader TwoSidedUnlitTransparentBlend;
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

    public bool isInVR = false;

    public bool StartInVR = false;

    public void ChangeVR(bool value)
    {
        RunonMainThread(() =>
        {
            if (value)
            {
                try
                {
                    Debug.Log("Starting VR");
                    XRSettings.enabled = true;
                    XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
                    XRGeneralSettings.Instance.Manager.StartSubsystems();
                    var xrLoader = XRGeneralSettings.Instance.Manager.activeLoader;
                    var xrDisplay = xrLoader.GetLoadedSubsystem<XRDisplaySubsystem>();
                    xrDisplay?.Start();
                    Debug.Log("Started VR System");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to Start VR Error: {e}");
                }
            }
            else
            {
                try
                {
                    Debug.Log("Stopping VR");
                    XRSettings.enabled = false;
                    XRGeneralSettings.Instance.Manager.StopSubsystems();
                    XRGeneralSettings.Instance.Manager.DeinitializeLoader();
                    var xrLoader = XRGeneralSettings.Instance.Manager?.activeLoader;
                    var xrDisplay = xrLoader?.GetLoadedSubsystem<XRDisplaySubsystem>();
                    xrDisplay?.Stop();
                    Debug.Log("Stopped VR");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to Stop VR Error: {e}");
                }
            }
            VRUpdate();
        });
    }

    public void VRUpdate()
    {
        if (XRSettings.enabled)
        {
            isInVR = isHardwarePresent();
            if (isInVR)
            {
                Debug.Log("Had VR Headset");
            }
            else
            {
                Debug.Log("Didn't Detect VR Headset");
            }
        }
        else
        {
            isInVR = false;
        }
        if (isInVR)
        {
            LeftController.SetActive(true);
            RightController.SetActive(true);
        }
        else
        {
            UserHead.transform.localPosition = Vector3.zero;
            UserHead.transform.localRotation = Quaternion.identity;
            UserHead.transform.localScale = Vector3.one;
            CameraOffset.transform.localPosition = Vector3.zero;
            LeftController.SetActive(false);
            RightController.SetActive(false);
            try
            {
                engine.MainSettings.RenderSettings.RenderSettingsChange?.Invoke();
            }
            catch { }
        }
        link.ChangeVRState(isInVR);
    }

    public void OnDragAnDrop(List<string> files, POINT aDropPoint)
    {
#if UNITY_EDITOR
        RLog.Info($"Files Droped {files.Count}");
#endif
        engine?.DragAndDropAction(files);
    }

    IEnumerator Start()
    {
        if (!IsMainThread)
        {
#if !UNITY_EDITOR
            Thread.CurrentThread.Name = "Unity Main Thread";
#endif
            Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
            IsMainThread = true;
            _ = this;
        }
        var platform = Application.platform;
        if (platform == RuntimePlatform.Android)
        {
            Debug.Log("Is on Android");
            ChangeVR(true);
            if (isInVR)
            {
                Destroy(TouchCanvas);
            }
        }
        else
        {
            Destroy(TouchCanvas);
        }
        if (Assimp.AssimpUnity.IsAssimpAvailable)
        {
            Debug.Log("Assimp is Available");
        }
        else
        {
            Debug.Log("Assimp is not Available");
        }

        if (!isInVR)
        {
            Debug.Log("Starting RuhbarbVR ScreenMode");
        }
        else
        {
            Debug.Log("Starting RuhbarbVR VRMode");
        }
        Debug.Log("Graphics Memory Size: " + SystemInfo.graphicsMemorySize);
        cap = new OutputCapture();
        link = new UnityEngineLink(this);
        link.LiveVRChange = platform.ToString().Contains("Windows");
        try
        {
            UnityDragAndDropHook.InstallHook();
            UnityDragAndDropHook.OnDroppedFiles += OnDragAnDrop;
        }
        catch
        {
            RLog.Err("Failed to add Drag And Drop Hook");
        }
        var args = new List<string>(Environment.GetCommandLineArgs());
        if (!StartInVR)
        {
            args.Add("--no-vr");
        }
        var appPath = Path.GetDirectoryName(Application.dataPath);
        Debug.Log("App Path: " + appPath);
        yield return null;
        engine = new Engine(link, args.ToArray(), cap, appPath);
        yield return null;
        engine.OnCloseEngine += () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
            ProcessCleanup();
        };
        engine.Init();
    }

    private bool IsDisposeing { set; get; }

    private void ProcessCleanup()
    {
        try
        {
            if (!IsDisposeing)
            {
                IsDisposeing = true;
                Debug.Log("Rhubarb CleanUp Started");
                engine.IsCloseing = true;
                engine.Dispose();
            }
        }
        catch
        {
            Debug.Log("Failed to start Rhubarb CleanUp");
        }
    }

    void OnDestroy()
    {
        Debug.Log("Destroy");
        ProcessCleanup();
    }

    public interface IRemoveLater
    {
        public void Remove();


        public bool UsedThisFrame { get; set; }
    }


    public void Draw(string id, Mesh mesh, Material target, Matrix p, RenderLayer layer)
    {
        if (target is null)
        {
            return;
        }
        Graphics.DrawMesh(mesh, Matrix4x4.Scale(new Vector3(1, 1, -1)) * new Matrix4x4
        {
            m00 = p.m.M11,
            m01 = p.m.M21,
            m02 = p.m.M31,
            m03 = p.m.M41,
            m10 = p.m.M12,
            m11 = p.m.M22,
            m12 = p.m.M32,
            m13 = p.m.M42,
            m20 = p.m.M13,
            m21 = p.m.M23,
            m22 = p.m.M33,
            m23 = p.m.M43,
            m30 = p.m.M14,
            m31 = p.m.M24,
            m32 = p.m.M34,
            m33 = p.m.M44,
        }, target, (int)layer,null,0,null,false,false,false);
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

    public event Action<bool> OnVRChange;

    public void InputDevices_Update(InputDevice inputDevice)
    {
        Camera.cullingMask = (int)RenderLayer.MainCam;
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
        MainThreadUpdate();
        if (engine is null)
        {
            return;
        }
        var calculatedDelta = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();
        calculatedDelta += new Vector2(Right.Vertical, Right.Horizontal);
        MouseDelta = new Vector2f(calculatedDelta.x * speedMultply, calculatedDelta.y * -speedMultply);
        engine.Step();
    }
}
