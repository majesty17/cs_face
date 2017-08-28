using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace cs_face
{
    public class WinAPI
    {
        private static class NativeMethods
        {
            internal const uint GW_OWNER = 4;

            internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            internal static extern int GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            internal static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            internal static extern bool IsWindowVisible(IntPtr hWnd);
        }
        public static IntPtr GetMainWindowHandle(int processId)
        {
            IntPtr MainWindowHandle = IntPtr.Zero;

            NativeMethods.EnumWindows(new NativeMethods.EnumWindowsProc((hWnd, lParam) =>
            {
                IntPtr PID;
                NativeMethods.GetWindowThreadProcessId(hWnd, out PID);

                if (PID == lParam &&
                    NativeMethods.IsWindowVisible(hWnd) &&
                    NativeMethods.GetWindow(hWnd, NativeMethods.GW_OWNER) == IntPtr.Zero)
                {
                    MainWindowHandle = hWnd;
                    return false;
                }

                return true;

            }), new IntPtr(processId));

            return MainWindowHandle;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;                             //最左坐标
            public int Top;                             //最上坐标
            public int Right;                           //最右坐标
            public int Bottom;                        //最下坐标
        }


        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public static void paste() {
            keybd_event((byte)Keys.ControlKey, 0, 0, 0);  //按下
            keybd_event((byte)Keys.V, 0, 0, 0);                    //按下
            keybd_event((byte)Keys.V, 0, 2, 0);
            keybd_event((byte)Keys.ControlKey, 0, 2, 0);  //松开
        }
        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();
    } 
}
