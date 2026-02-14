// Ogur.Clicker.Infrastructure/Services/KeyboardService.cs
using System.Runtime.InteropServices;
using Ogur.Clicker.Core.Models;
using Ogur.Clicker.Core.Services;

namespace Ogur.Clicker.Infrastructure.Services;

public class KeyboardService : IKeyboardService
{
    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private const int INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYDOWN = 0x0000;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_SCANCODE = 0x0008;
    private const uint WM_KEYDOWN = 0x0100;
    private const uint WM_KEYUP = 0x0101;

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
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
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

    public async Task PressKeyAsync(int virtualKey, int pressCount, int delayMs, KeyboardInputMethod method)
    {
        for (int i = 0; i < pressCount; i++)
        {
            PressKey(virtualKey, method);

            if (i < pressCount - 1)
                await Task.Delay(delayMs);
        }
    }

    public void PressKey(int virtualKey, KeyboardInputMethod method, int holdTimeMs = 10)
    {
        KeyDown(virtualKey, method);
        Thread.Sleep(holdTimeMs);
        KeyUp(virtualKey, method);
    }

    public void KeyDown(int virtualKey, KeyboardInputMethod method)
    {
        switch (method)
        {
            case KeyboardInputMethod.SendInput:
                SendInputKeyDown(virtualKey, false);
                break;
            case KeyboardInputMethod.SendInputHardware:
                SendInputKeyDown(virtualKey, true);
                break;
            case KeyboardInputMethod.PostMessage:
                PostMessageKey(virtualKey, true);
                break;
            case KeyboardInputMethod.DirectInput:
                DirectInputKey(virtualKey, true);
                break;
        }
    }

    public void KeyUp(int virtualKey, KeyboardInputMethod method)
    {
        switch (method)
        {
            case KeyboardInputMethod.SendInput:
                SendInputKeyUp(virtualKey, false);
                break;
            case KeyboardInputMethod.SendInputHardware:
                SendInputKeyUp(virtualKey, true);
                break;
            case KeyboardInputMethod.PostMessage:
                PostMessageKey(virtualKey, false);
                break;
            case KeyboardInputMethod.DirectInput:
                DirectInputKey(virtualKey, false);
                break;
        }
    }

    private void SendInputKeyDown(int virtualKey, bool useScanCode)
    {
        var input = new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = useScanCode ? (ushort)0 : (ushort)virtualKey,
                    wScan = useScanCode ? (ushort)MapVirtualKey((uint)virtualKey, 0) : (ushort)0,
                    dwFlags = useScanCode ? KEYEVENTF_SCANCODE : KEYEVENTF_KEYDOWN,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
    }

    private void SendInputKeyUp(int virtualKey, bool useScanCode)
    {
        var input = new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = useScanCode ? (ushort)0 : (ushort)virtualKey,
                    wScan = useScanCode ? (ushort)MapVirtualKey((uint)virtualKey, 0) : (ushort)0,
                    dwFlags = useScanCode ? (KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP) : KEYEVENTF_KEYUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
    }

    private void PostMessageKey(int virtualKey, bool isDown)
    {
        var hwnd = GetForegroundWindow();
        PostMessage(hwnd, isDown ? WM_KEYDOWN : WM_KEYUP, (IntPtr)virtualKey, IntPtr.Zero);
    }

    private void DirectInputKey(int virtualKey, bool isDown)
    {
        keybd_event((byte)virtualKey, 0, isDown ? KEYEVENTF_KEYDOWN : KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);
}