using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Ogur.Clicker.Core.Services;

namespace Ogur.Clicker.Infrastructure.Services;

public class MouseHookService : IMouseHookService, IDisposable
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_XBUTTONDOWN = 0x020B;

    private const int VK_LBUTTON = 0x01;
    private const int VK_RBUTTON = 0x02;
    private const int VK_MBUTTON = 0x04;
    private const int VK_XBUTTON1 = 0x05;
    private const int VK_XBUTTON2 = 0x06;

    private IntPtr _hookId = IntPtr.Zero;
    private LowLevelMouseProc? _proc;

    public event EventHandler<Point>? LeftButtonClicked;
    public event EventHandler<int>? MouseButtonClicked;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public void StartListening()
    {
        if (_hookId != IntPtr.Zero)
            return;

        _proc = HookCallback;
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        _hookId = SetWindowsHookEx(WH_MOUSE_LL, _proc, GetModuleHandle(curModule?.ModuleName ?? ""), 0);
    }

    public void StopListening()
    {
        
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            var point = new Point(hookStruct.pt.x, hookStruct.pt.y);

            if (wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                LeftButtonClicked?.Invoke(this, point);
                MouseButtonClicked?.Invoke(this, VK_LBUTTON);
            }
            else if (wParam == (IntPtr)WM_RBUTTONDOWN)
            {
                MouseButtonClicked?.Invoke(this, VK_RBUTTON);
            }
            else if (wParam == (IntPtr)WM_MBUTTONDOWN)
            {
                MouseButtonClicked?.Invoke(this, VK_MBUTTON);
            }
            else if (wParam == (IntPtr)WM_XBUTTONDOWN)
            {
                var xButton = (hookStruct.mouseData >> 16) & 0xFFFF;
            
                if (xButton == 1)
                    MouseButtonClicked?.Invoke(this, VK_XBUTTON1); // MB4
                else if (xButton == 2)
                    MouseButtonClicked?.Invoke(this, VK_XBUTTON2); // MB5
            }
        }
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        StopListening();
    }
}