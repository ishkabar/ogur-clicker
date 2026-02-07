using System.Drawing;
using System.Runtime.InteropServices;
using Ogur.Clicker.Core.Models;
using Ogur.Clicker.Core.Services;

namespace Ogur.Clicker.Infrastructure.Services;

public class MouseService : IMouseService
{
    private MouseInputMethod _currentMethod = MouseInputMethod.SendInput;

    public MouseInputMethod CurrentMethod => _currentMethod;

    #region Win32 API Imports

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(POINT point);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

    #endregion

    #region Constants

    private const int INPUT_MOUSE = 0;
    private const int INPUT_HARDWARE = 2;

    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

    private const uint WM_LBUTTONDOWN = 0x0201;
    private const uint WM_LBUTTONUP = 0x0202;
    private const uint WM_RBUTTONDOWN = 0x0204;
    private const uint WM_RBUTTONUP = 0x0205;
    private const uint WM_MBUTTONDOWN = 0x0207;
    private const uint WM_MBUTTONUP = 0x0208;

    private const uint MK_LBUTTON = 0x0001;
    private const uint MK_RBUTTON = 0x0002;
    private const uint MK_MBUTTON = 0x0010;

    #endregion

    #region Structures

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public int type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    #endregion

    public bool TryClick(ClickAction action)
    {
        return TryClick(action, _currentMethod);
    }


    public bool TryClick(ClickAction action, MouseInputMethod method)
    {
        _currentMethod = method;

        try
        {
            // Move cursor to position first
            if (!SetCursorPos(action.X, action.Y))
            {
                return false;
            }

            // Add small delay to simulate human behavior
            //Thread.Sleep(Random.Shared.Next(5, 15));
            Thread.Sleep(10);

            bool result = method switch
            {
                MouseInputMethod.SendInput => ClickUsingSendInput(action, false),
                MouseInputMethod.SendInputHardware => ClickUsingSendInput(action, true),
                MouseInputMethod.PostMessage => ClickUsingPostMessage(action),
                MouseInputMethod.DirectInput => ClickUsingMouseEvent(action),
                _ => false
            };

            return result;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public Point GetCurrentPosition()
    {
        GetCursorPos(out POINT point);

        return new Point(point.X, point.Y);
    }

    #region Click Implementation Methods

    private bool ClickUsingSendInput(ClickAction action, bool useHardwareFlag)
    {
        var inputs = new INPUT[2];

        // Get button flags
        var (downFlag, upFlag) = GetMouseEventFlags(action.Button);

        // Mouse down
        inputs[0] = new INPUT
        {
            type = useHardwareFlag ? INPUT_HARDWARE : INPUT_MOUSE,
            u = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = action.X,
                    dy = action.Y,
                    mouseData = 0,
                    dwFlags = downFlag,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        // Mouse up
        inputs[1] = new INPUT
        {
            type = useHardwareFlag ? INPUT_HARDWARE : INPUT_MOUSE,
            u = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = action.X,
                    dy = action.Y,
                    mouseData = 0,
                    dwFlags = upFlag,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        uint result = SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        return result == 2;
    }

    private bool ClickUsingPostMessage(ClickAction action)
    {
        // Get window handle at click position
        var point = new POINT { X = action.X, Y = action.Y };
        IntPtr hWnd = WindowFromPoint(point);


        if (hWnd == IntPtr.Zero)
        {
            hWnd = GetForegroundWindow();
        }

        if (hWnd == IntPtr.Zero)
        {
            return false;
        }

        // Convert screen coordinates to client coordinates
        var lParam = MakeLParam(action.X, action.Y);
        var wParam = GetMouseButtonParam(action.Button);

        // Get message constants
        var (downMsg, upMsg) = GetWindowMessageConstants(action.Button);

        // Send down and up messages
        bool downResult = PostMessage(hWnd, downMsg, wParam, lParam);
        //Thread.Sleep(Random.Shared.Next(10, 30));
        bool upResult = PostMessage(hWnd, upMsg, IntPtr.Zero, lParam);

        return downResult && upResult;
    }

    private bool ClickUsingMouseEvent(ClickAction action)
    {
        var (downFlag, upFlag) = GetMouseEventFlags(action.Button);

        mouse_event(downFlag, action.X, action.Y, 0, 0);
        //Thread.Sleep(Random.Shared.Next(10, 30));
        mouse_event(upFlag, action.X, action.Y, 0, 0);

        return true;
    }

    public void SetCursorPosition(int x, int y)
    {
        SetCursorPos(x, y);
    }

    #endregion

    #region Helper Methods

    private (uint down, uint up) GetMouseEventFlags(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => (MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP),
            MouseButton.Right => (MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP),
            MouseButton.Middle => (MOUSEEVENTF_MIDDLEDOWN, MOUSEEVENTF_MIDDLEUP),
            _ => (MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP)
        };
    }

    private (uint down, uint up) GetWindowMessageConstants(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => (WM_LBUTTONDOWN, WM_LBUTTONUP),
            MouseButton.Right => (WM_RBUTTONDOWN, WM_RBUTTONUP),
            MouseButton.Middle => (WM_MBUTTONDOWN, WM_MBUTTONUP),
            _ => (WM_LBUTTONDOWN, WM_LBUTTONUP)
        };
    }

    private IntPtr GetMouseButtonParam(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => new IntPtr(MK_LBUTTON),
            MouseButton.Right => new IntPtr(MK_RBUTTON),
            MouseButton.Middle => new IntPtr(MK_MBUTTON),
            _ => new IntPtr(MK_LBUTTON)
        };
    }

    private IntPtr MakeLParam(int x, int y)
    {
        return (IntPtr)((y << 16) | (x & 0xFFFF));
    }

    #endregion
}