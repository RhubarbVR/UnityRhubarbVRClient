using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;
using RhuEngine.Linker;
public class ControllerModelLoader : MonoBehaviour
{
    public GameObject loadedContoller;

    public TrackedPoseDriver trackedPoseDriver;
    // Start is called before the first frame update
    void Start()
    {
        InputDevices.deviceConnected += InputDevices_Update;
        InputDevices.deviceDisconnected += InputDevices_Remove;
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevices(inputDevices);
        foreach (var item in inputDevices)
        {
            InputDevices_Update(item);
        }
    }


    KnownControllers Controller = KnownControllers.Unknown;
    InputDevice inputDevice;
    private void InputDevices_Remove(InputDevice obj)
    {
        if(obj == inputDevice)
        {
            if(obj == null)
            {
                return;
            }
            Destroy(loadedContoller);
        }
    }
    private void InputDevices_Update(InputDevice obj)
    {
        if((obj.characteristics & InputDeviceCharacteristics.Controller) == InputDeviceCharacteristics.None)
        {
            return;
        }
        if (trackedPoseDriver.poseSource == TrackedPoseDriver.TrackedPose.LeftPose)
        {
            if ((obj.characteristics & InputDeviceCharacteristics.Left) == InputDeviceCharacteristics.None)
            {
                return;
            }
        }
        if (trackedPoseDriver.poseSource == TrackedPoseDriver.TrackedPose.RightPose)
        {
            if ((obj.characteristics & InputDeviceCharacteristics.Right) == InputDeviceCharacteristics.None)
            {
                return;
            }
        }
        if(obj == inputDevice)
        {
            return;
        }
        var type = UnityInput.UnityController.GetKnownController(obj.name);
        if (Controller == type)
        {
            return;
        }
        if (loadedContoller is not null)
        {
            Destroy(loadedContoller);
        }
        string prefabPath = type switch
        {
            KnownControllers.Vive => "ViveController",
            KnownControllers.Index => "IndexController",
            KnownControllers.Touch => "TouchController",
            KnownControllers.Cosmos => "CosmosController",
            KnownControllers.HPReverb => "HPReverbController",
            KnownControllers.WindowsMR => "WindowsMRController",
            KnownControllers.Etee => "EteeController",
            KnownControllers.Khronos => "KhronosController",
            _ => "XRController",
        };
        var contoler = (trackedPoseDriver.poseSource == TrackedPoseDriver.TrackedPose.RightPose)? "Right":"Left";
        loadedContoller = Instantiate(Resources.Load("Assets\\Prefabs\\"+prefabPath + contoler, typeof(GameObject))?? Resources.Load("Assets\\Prefabs\\" + "XRController" + contoler, typeof(GameObject))) as GameObject;
        loadedContoller.transform.parent = transform;
        loadedContoller.transform.localPosition = new Vector3(0,0,0);
        loadedContoller.transform.localScale = Vector3.one;
        loadedContoller.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
