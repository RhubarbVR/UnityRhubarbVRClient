using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using RNumerics;
using UnityEngine.XR;

public class UnityInput : IRInput
{
    public UnityInput(EngineRunner engineRunner)
    {
        EngineRunner = engineRunner;
    }

    public string TypeDelta => Input.inputString;

    public class UnityHead : IRHead
    {
        public EngineRunner EngineRunner { get; }

        public Vector3f Position
        {
            get
            {
                return HeadMatrix.Translation;
            }
        }

        public Matrix HeadMatrix
        {
            get
            {
                var rot = EngineRunner.UserHead.transform.rotation;
                var pos = EngineRunner.UserHead.transform.position;
                var scale = EngineRunner.UserHead.transform.lossyScale;
                return Matrix.TRS(new Vector3f(pos.x, pos.y, -pos.z), new Quaternionf(-rot.x, -rot.y, rot.z, rot.w), new Vector3f(scale.x, scale.y, scale.z));
            }
        }

        public UnityHead(EngineRunner engineRunner)
        {
            EngineRunner = engineRunner;
        }
    }

    public IRHead Head => new UnityHead(EngineRunner);

    public class UMouse : IRMouse
    {
        public Vector2f ScrollChange
        {
            get
            {
                var scroll = Input.mouseScrollDelta;
                return new Vector2f(scroll.x, scroll.y);
            }
        }

        public Vector2f PosChange
        {
            get
            {
                var scroll = EngineRunner.MouseDelta;
                return new Vector2f(scroll.x, scroll.y);
            }
        }

        public bool HideMouse { get => Cursor.visible; set => EngineRunner.RunonMainThread(() => Cursor.visible = value); }
        public bool CenterMouse { get => Cursor.lockState == CursorLockMode.Locked; set { EngineRunner.RunonMainThread(() => { if (value) { Cursor.lockState = CursorLockMode.Locked; } else { Cursor.lockState = CursorLockMode.None; } }); } }

        public EngineRunner EngineRunner { get; }

        public UMouse(EngineRunner engineRunner)
        {
            EngineRunner = engineRunner;
        }
    }

    public IRMouse Mouse => new UMouse(EngineRunner);

    public EngineRunner EngineRunner { get; }

    public class UnityController : IRController
    {
        public UnityController(Handed handed, EngineRunner engineRunner)
        {
            Handed = handed;
            EngineRunner = engineRunner;
        }

        public Handed Handed { get; }
        public EngineRunner EngineRunner { get; }

        public float Trigger 
        {
            get
            {
                EngineRunner.left.TryGetFeatureValue(CommonUsages.trigger, out var val_L);
                EngineRunner.right.TryGetFeatureValue(CommonUsages.trigger, out var val_R);
                switch (Handed)
                {
                    case Handed.Left:
                        return val_L;
                    case Handed.Right:
                        return val_R;
                    case Handed.Max:
                        return (val_L + val_R)/2;   
                    default:
                        break;
                }
                return 0;
            }
        }

        public float Grip
        {
            get
            {
                EngineRunner.left.TryGetFeatureValue(CommonUsages.grip, out var val_L);
                EngineRunner.right.TryGetFeatureValue(CommonUsages.grip, out var val_R);
                switch (Handed)
                {
                    case Handed.Left:
                        return val_L;
                    case Handed.Right:
                        return val_R;
                    case Handed.Max:
                        return (val_L + val_R) / 2;
                    default:
                        break;
                }
                return 0;
            }
        }

        public class VRKeyPress : IKeyPress
        {
            public VRKeyPress(InputFeatureUsage<bool> key, Handed handed, EngineRunner engineRunner)
            {
                Key = key;
                Handed = handed;
                EngineRunner = engineRunner;
            }

            public InputFeatureUsage<bool> Key { get; }
            public Handed Handed { get; }
            public EngineRunner EngineRunner { get; }

            public bool IsActive()
            {
                EngineRunner.left.TryGetFeatureValue(Key, out var val_L);
                EngineRunner.right.TryGetFeatureValue(Key, out var val_R);
                switch (Handed)
                {
                    case Handed.Left:
                        return val_L;
                    case Handed.Right:
                        return val_R;
                    case Handed.Max:
                        return val_L | val_R;
                    default:
                        break;
                }
                return false;
            }

            public bool IsJustActive()
            {
                //todo make only run at start of frame
                EngineRunner.left.TryGetFeatureValue(Key, out var val_L);
                EngineRunner.right.TryGetFeatureValue(Key, out var val_R);
                switch (Handed)
                {
                    case Handed.Left:
                        return val_L;
                    case Handed.Right:
                        return val_R;
                    case Handed.Max:
                        return val_L | val_R;
                    default:
                        break;
                }
                return false;
            }
        }


        public IKeyPress StickClick => new VRKeyPress(CommonUsages.primary2DAxisClick, Handed, EngineRunner);

        public IKeyPress X1 => new VRKeyPress(CommonUsages.primaryButton, Handed, EngineRunner);

        public IKeyPress X2 => new VRKeyPress(CommonUsages.secondaryButton, Handed, EngineRunner);

        public class VRStick : IRStick
        {
            public VRStick(Handed handed, EngineRunner engineRunner)
            {
                Handed = handed;
                EngineRunner = engineRunner;
            }

            public Handed Handed { get; }
            public EngineRunner EngineRunner { get; }

            public Vector2f YX
            {
                get
                {
                    EngineRunner.left.TryGetFeatureValue(CommonUsages.primary2DAxis, out var val_L_e);
                    var val_L = new Vector2f(val_L_e.x, val_L_e.y);
                    EngineRunner.right.TryGetFeatureValue(CommonUsages.primary2DAxis, out var val_R_e);
                    var val_R = new Vector2f(val_R_e.x, val_R_e.y);
                    switch (Handed)
                    {
                        case Handed.Left:
                            return val_L;
                        case Handed.Right:
                            return val_R;
                        case Handed.Max:
                            return (val_L + val_R) / 2;
                        default:
                            break;
                    }
                    return Vector2f.Zero;
                }
            }
        }

        public IRStick Stick => new VRStick(Handed, EngineRunner);

        public string Model
        {
            get
            {
                var val_L = EngineRunner.left.name;
                var val_R = EngineRunner.right.name;
                switch (Handed)
                {
                    case Handed.Left:
                        return val_L;
                    case Handed.Right:
                        return val_R;
                    case Handed.Max:
                        return ("L" + val_L + " R" + val_R);
                    default:
                        break;
                }
                return null;
            }
        }

        public static KnownControllers GetKnownController(string name)
        {
            name = name.ToLower();
            if (name.Contains("vive"))
            {
                return KnownControllers.Vive;
            }
            if (name.Contains("touch"))
            {
                return KnownControllers.Touch;
            }
            if (name.Contains("index"))
            {
                return KnownControllers.Index;
            }
            if (name.Contains("cosmos"))
            {
                return KnownControllers.Cosmos;
            }
            if (name.Contains("generic"))
            {
                return KnownControllers.GenericXR;
            }
            if (name.Contains("khronos"))
            {
                return KnownControllers.Khronos;
            }
            if (name.Contains("hand"))
            {
                return KnownControllers.MicrosoftHand;
            }
            if (name.Contains("reverb"))
            {
                return KnownControllers.HPReverb;
            }
            if (name.Contains("etee"))
            {
                return KnownControllers.Etee;
            }
            return KnownControllers.Unknown;
        } 

        public KnownControllers ModelEnum
        {
            get
            {
                var val_L = GetKnownController(EngineRunner.left.name);
                var val_R = GetKnownController(EngineRunner.right.name);
                switch (Handed)
                {
                    case Handed.Left:
                        return val_L;
                    case Handed.Right:
                        return val_R;
                    case Handed.Max:
                        return val_L & val_R;
                    default:
                        break;
                }
                return KnownControllers.Unknown;
            }
        }


        public float BatteryPercentage
        {
            get
            {
                EngineRunner.left.TryGetFeatureValue(CommonUsages.batteryLevel, out var val_L);
                EngineRunner.right.TryGetFeatureValue(CommonUsages.batteryLevel, out var val_R);
                switch (Handed)
                {
                    case Handed.Left:
                        return val_L;
                    case Handed.Right:
                        return val_R;
                    case Handed.Max:
                        return (val_L + val_R) / 2;
                    default:
                        break;
                }
                return 0;
            }
        }
    }

    public IRController Controller(Handed handed)
    {
        return new UnityController(handed,EngineRunner);
    }

    public class UnityHand: IRHand
    {
        public UnityHand(Handed handed, EngineRunner engineRunner)
        {
            Handed = handed;
            EngineRunner = engineRunner;
        }

        public Matrix Wrist
        {
            get
            {
                if(Handed == Handed.Left)
                {
                    var rot = EngineRunner.LeftController.transform.rotation;
                    var pos = EngineRunner.LeftController.transform.position;
                    var scale = EngineRunner.LeftController.transform.lossyScale;
                    return Matrix.TRS(new Vector3f(pos.x, pos.y, -pos.z), new Quaternionf(-rot.x, -rot.y, rot.z, rot.w), new Vector3f(scale.x, scale.y, scale.z));
                }
                else
                {
                    var rot = EngineRunner.RightController.transform.rotation;
                    var pos = EngineRunner.RightController.transform.position;
                    var scale = EngineRunner.RightController.transform.lossyScale;
                    return Matrix.TRS(new Vector3f(pos.x, pos.y, -pos.z), new Quaternionf(-rot.x, -rot.y, rot.z, rot.w), new Vector3f(scale.x, scale.y, scale.z));
                }
            }
        }

        public Handed Handed { get; }
        public EngineRunner EngineRunner { get; }
    }

    public IRHand Hand(Handed value)
    {
        return new UnityHand(value, EngineRunner);
    }

    public class KeyClick : IKeyPress
    {
        public KeyCode codetwo;

        public KeyCode code;
        public KeyClick(Key key)
        {
            switch (key)
            {
                case RhuEngine.Linker.Key.None:
                    code = KeyCode.None;
                    break;
                case RhuEngine.Linker.Key.MouseLeft:
                    code = KeyCode.Mouse0;
                    break;
                case RhuEngine.Linker.Key.MouseRight:
                    code = KeyCode.Mouse1;
                    break;
                case RhuEngine.Linker.Key.MouseCenter:
                    code = KeyCode.Mouse2;
                    break;
                case RhuEngine.Linker.Key.MouseForward:
                    code = KeyCode.Mouse3;
                    break;
                case RhuEngine.Linker.Key.MouseBack:
                    code = KeyCode.Mouse4;
                    break;
                case RhuEngine.Linker.Key.Backspace:
                    code = KeyCode.Backspace;
                    break;
                case RhuEngine.Linker.Key.Tab:
                    code = KeyCode.Tab;
                    break;
                case RhuEngine.Linker.Key.Return:
                    code = KeyCode.Return;
                    break;
                case RhuEngine.Linker.Key.Shift:
                    code = KeyCode.LeftShift;
                    codetwo = KeyCode.RightShift;
                    break;
                case RhuEngine.Linker.Key.Ctrl:
                    code = KeyCode.LeftControl;
                    codetwo = KeyCode.RightControl;
                    break;
                case RhuEngine.Linker.Key.Alt:
                    code = KeyCode.LeftAlt;
                    codetwo = KeyCode.RightAlt;
                    break;
                case RhuEngine.Linker.Key.CapsLock:
                    code = KeyCode.CapsLock;
                    break;
                case RhuEngine.Linker.Key.Esc:
                    code = KeyCode.Escape;
                    break;
                case RhuEngine.Linker.Key.Space:
                    code = KeyCode.Space;
                    break;
                case RhuEngine.Linker.Key.End:
                    code = KeyCode.End;
                    break;
                case RhuEngine.Linker.Key.Home:
                    code = KeyCode.Home;
                    break;
                case RhuEngine.Linker.Key.Left:
                    code = KeyCode.LeftArrow;
                    break;
                case RhuEngine.Linker.Key.Right:
                    code = KeyCode.RightArrow;
                    break;
                case RhuEngine.Linker.Key.Up:
                    code = KeyCode.UpArrow;
                    break;
                case RhuEngine.Linker.Key.Down:
                    code = KeyCode.DownArrow;
                    break;
                case RhuEngine.Linker.Key.PageUp:
                    code = KeyCode.PageUp;
                    break;
                case RhuEngine.Linker.Key.PageDown:
                    code = KeyCode.PageDown;
                    break;
                case RhuEngine.Linker.Key.Printscreen:
                    code = KeyCode.Print;
                    break;
                case RhuEngine.Linker.Key.Insert:
                    code = KeyCode.Insert;
                    break;
                case RhuEngine.Linker.Key.Del:
                    code = KeyCode.Delete;
                    break;
                case RhuEngine.Linker.Key.N0:
                    code = KeyCode.Alpha0;
                    break;
                case RhuEngine.Linker.Key.N1:
                    code = KeyCode.Alpha1;
                    break;
                case RhuEngine.Linker.Key.N2:
                    code = KeyCode.Alpha2;
                    break;
                case RhuEngine.Linker.Key.N3:
                    code = KeyCode.Alpha3;
                    break;
                case RhuEngine.Linker.Key.N4:
                    code = KeyCode.Alpha4;
                    break;
                case RhuEngine.Linker.Key.N5:
                    code = KeyCode.Alpha5;
                    break;
                case RhuEngine.Linker.Key.N6:
                    code = KeyCode.Alpha6;
                    break;
                case RhuEngine.Linker.Key.N7:
                    code = KeyCode.Alpha7;
                    break;
                case RhuEngine.Linker.Key.N8:
                    code = KeyCode.Alpha8;
                    break;
                case RhuEngine.Linker.Key.N9:
                    code = KeyCode.Alpha9;
                    break;
                case RhuEngine.Linker.Key.A:
                    code = KeyCode.A;
                    break;
                case RhuEngine.Linker.Key.B:
                    code = KeyCode.B;
                    break;
                case RhuEngine.Linker.Key.C:
                    code = KeyCode.C;
                    break;
                case RhuEngine.Linker.Key.D:
                    code = KeyCode.D;
                    break;
                case RhuEngine.Linker.Key.E:
                    code = KeyCode.E;
                    break;
                case RhuEngine.Linker.Key.F:
                    code = KeyCode.F;
                    break;
                case RhuEngine.Linker.Key.G:
                    code = KeyCode.G;
                    break;
                case RhuEngine.Linker.Key.H:
                    code = KeyCode.H;
                    break;
                case RhuEngine.Linker.Key.I:
                    code = KeyCode.I;
                    break;
                case RhuEngine.Linker.Key.J:
                    code = KeyCode.J;
                    break;
                case RhuEngine.Linker.Key.K:
                    code = KeyCode.K;
                    break;
                case RhuEngine.Linker.Key.L:
                    code = KeyCode.L;
                    break;
                case RhuEngine.Linker.Key.M:
                    code = KeyCode.M;
                    break;
                case RhuEngine.Linker.Key.N:
                    code = KeyCode.N;
                    break;
                case RhuEngine.Linker.Key.O:
                    code = KeyCode.O;
                    break;
                case RhuEngine.Linker.Key.P:
                    code = KeyCode.P;
                    break;
                case RhuEngine.Linker.Key.Q:
                    code = KeyCode.Q;
                    break;
                case RhuEngine.Linker.Key.R:
                    code = KeyCode.R;
                    break;
                case RhuEngine.Linker.Key.S:
                    code = KeyCode.S;
                    break;
                case RhuEngine.Linker.Key.T:
                    code = KeyCode.T;
                    break;
                case RhuEngine.Linker.Key.U:
                    code = KeyCode.U;
                    break;
                case RhuEngine.Linker.Key.V:
                    code = KeyCode.V;
                    break;
                case RhuEngine.Linker.Key.W:
                    code = KeyCode.W;
                    break;
                case RhuEngine.Linker.Key.X:
                    code = KeyCode.X;
                    break;
                case RhuEngine.Linker.Key.Y:
                    code = KeyCode.Y;
                    break;
                case RhuEngine.Linker.Key.Z:
                    code = KeyCode.Z;
                    break;
                case RhuEngine.Linker.Key.Num0:
                    code = KeyCode.Keypad0;
                    break;
                case RhuEngine.Linker.Key.Num1:
                    code = KeyCode.Keypad1;
                    break;
                case RhuEngine.Linker.Key.Num2:
                    code = KeyCode.Keypad2;
                    break;
                case RhuEngine.Linker.Key.Num3:
                    code = KeyCode.Keypad3;
                    break;
                case RhuEngine.Linker.Key.Num4:
                    code = KeyCode.Keypad4;
                    break;
                case RhuEngine.Linker.Key.Num5:
                    code = KeyCode.Keypad5;
                    break;
                case RhuEngine.Linker.Key.Num6:
                    code = KeyCode.Keypad6;
                    break;
                case RhuEngine.Linker.Key.Num7:
                    code = KeyCode.Keypad7;
                    break;
                case RhuEngine.Linker.Key.Num8:
                    code = KeyCode.Keypad8;
                    break;
                case RhuEngine.Linker.Key.Num9:
                    code = KeyCode.Keypad9;
                    break;
                case RhuEngine.Linker.Key.F1:
                    code = KeyCode.F1;
                    break;
                case RhuEngine.Linker.Key.F2:
                    code = KeyCode.F2;
                    break;
                case RhuEngine.Linker.Key.F3:
                    code = KeyCode.F3;
                    break;
                case RhuEngine.Linker.Key.F4:
                    code = KeyCode.F4;
                    break;
                case RhuEngine.Linker.Key.F5:
                    code = KeyCode.F5;
                    break;
                case RhuEngine.Linker.Key.F6:
                    code = KeyCode.F6;
                    break;
                case RhuEngine.Linker.Key.F7:
                    code = KeyCode.F7;
                    break;
                case RhuEngine.Linker.Key.F8:
                    code = KeyCode.F8;
                    break;
                case RhuEngine.Linker.Key.F9:
                    code = KeyCode.F9;
                    break;
                case RhuEngine.Linker.Key.F10:
                    code = KeyCode.F10;
                    break;
                case RhuEngine.Linker.Key.F11:
                    code = KeyCode.F11;
                    break;
                case RhuEngine.Linker.Key.F12:
                    code = KeyCode.F12;
                    break;
                case RhuEngine.Linker.Key.Comma:
                    code = KeyCode.Comma;
                    break;
                case RhuEngine.Linker.Key.Period:
                    code = KeyCode.Period;
                    break;
                case RhuEngine.Linker.Key.SlashFwd:
                    code = KeyCode.Slash;
                    break;
                case RhuEngine.Linker.Key.SlashBack:
                    code = KeyCode.Backslash;
                    break;
                case RhuEngine.Linker.Key.Semicolon:
                    code = KeyCode.Semicolon;
                    break;
                case RhuEngine.Linker.Key.Apostrophe:
                    code = KeyCode.Comma;
                    break;
                case RhuEngine.Linker.Key.BracketOpen:
                    code = KeyCode.LeftBracket;
                    break;
                case RhuEngine.Linker.Key.BracketClose:
                    code = KeyCode.RightBracket;
                    break;
                case RhuEngine.Linker.Key.Minus:
                    code = KeyCode.Minus;
                    break;
                case RhuEngine.Linker.Key.Equals:
                    code = KeyCode.Equals;
                    break;
                case RhuEngine.Linker.Key.Backtick:
                    code = KeyCode.BackQuote;
                    break;
                case RhuEngine.Linker.Key.LCmd:
                    code = KeyCode.LeftCommand;
                    break;
                case RhuEngine.Linker.Key.RCmd:
                    code = KeyCode.RightCommand;
                    break;
                case RhuEngine.Linker.Key.Multiply:
                    code = KeyCode.KeypadMultiply;
                    break;
                case RhuEngine.Linker.Key.Add:
                    code = KeyCode.KeypadPlus;
                    break;
                case RhuEngine.Linker.Key.Subtract:
                    code = KeyCode.KeypadMinus;
                    break;
                case RhuEngine.Linker.Key.Decimal:
                    code = KeyCode.KeypadPeriod;
                    break;
                case RhuEngine.Linker.Key.Divide:
                    code = KeyCode.KeypadDivide;
                    break;
                case RhuEngine.Linker.Key.MAX:
                    break;
                default:
                    break;
            }
        }

        public bool IsActive()
        {
            return Input.GetKey(code) || Input.GetKey(codetwo);
        }

        public bool IsJustActive()
        {
            return Input.GetKeyDown(code) || Input.GetKeyDown(codetwo);
        }
    }

    public IKeyPress Key(Key secondKey)
    {
        return new KeyClick(secondKey);
    }
}
