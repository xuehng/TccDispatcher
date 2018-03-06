using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows;

namespace renstech.NET.SupernovaDispatcher.Utils
{
    internal class OnScreenKeyboard
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

        private const string OnScreenKeyboardExe = "osk.exe";

        public static Process StartOsk(Window parentWnd)
        {
            //if (RestoreWindow())
            //{
            //    return null;
            //}

            //IntPtr ptr = new IntPtr(); ;
            //bool sucessfullyDisabledWow64Redirect = false;

            //// Disable x64 directory virtualization if we're on x64,
            //// otherwise keyboard launch will fail.
            //if (System.Environment.Is64BitOperatingSystem)
            //{
            //    sucessfullyDisabledWow64Redirect =
            //        Wow64DisableWow64FsRedirection(ref ptr);
            //}
            
            Process myProcess = null;
            try
            {
                myProcess = new Process();
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = OnScreenKeyboardExe;
                myProcess.Start();
            }
            catch (System.Exception ex)
            {
                return null;
            }

            //ProcessStartInfo psi = new ProcessStartInfo();
            //psi.FileName = OnScreenKeyboardExe;
            // We must use ShellExecute to start osk from the current thread
            // with psi.UseShellExecute = false the CreateProcessWithLogon API 
            // would be used which handles process creation on a separate thread 
            // where the above call to Wow64DisableWow64FsRedirection would not 
            // have any effect.
            //
            //psi.UseShellExecute = true;
            //Process.Start(psi);

            // Re-enable directory virtualisation if it was disabled.

            //if (System.Environment.Is64BitOperatingSystem)
            //    if (sucessfullyDisabledWow64Redirect)
            //        Wow64RevertWow64FsRedirection(ptr);

            //if (parentWnd != null)
            //    parentWnd.Activate();

            return myProcess;
        }

        private const int SW_HIDE = 0;
        private const int SW_RESTORE = 9;

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);
        
        public static bool IsMinimized(IntPtr hWnd)
        {
            WINDOWINFO wi = new WINDOWINFO();
            GetWindowInfo(hWnd, ref wi);
            return (wi.dwStyle & 0x20000000) != 0;
        }

        [DllImport("User32")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        private static bool RestoreWindow()
        {
            Process[] processRunning = Process.GetProcesses();
            foreach (Process pr in processRunning)
            {
                if (pr.ProcessName == "osk")
                {
                    IntPtr hWnd = pr.MainWindowHandle;
                    if (IsMinimized(hWnd))
                        ShowWindow(hWnd, SW_RESTORE);
                    return true;
                }
            }
            return false;
        }
    }
}
