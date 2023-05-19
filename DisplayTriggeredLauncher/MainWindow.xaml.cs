using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Display;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DisplayTriggeredLauncher
{
    enum DBT
    {
        DBT_DEVNODES_CHANGED = 0x0007,
    }

    public sealed partial class MainWindow : Window
    {
        private NativeMethods.WndProc newWndProc = null;
        private IntPtr oldWndProc = IntPtr.Zero;

        public MainWindow()
        {
            this.InitializeComponent();
            SubClassing();
        }

        private void SubClassing()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            newWndProc = new NativeMethods.WndProc(NewWndProc);
            if (System.Environment.Is64BitProcess)
            {
                oldWndProc = NativeMethods.SetWindowLongPtr(hwnd, PInvoke.User32.WindowLongIndexFlags.GWL_WNDPROC, newWndProc);
            }
            else
            {
                oldWndProc = NativeMethods.SetWindowLong(hwnd, PInvoke.User32.WindowLongIndexFlags.GWL_WNDPROC, newWndProc);
            }
        }

        private IntPtr NewWndProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam)
        {
            switch (Msg)
            {
                case PInvoke.User32.WindowMessage.WM_DEVICECHANGE:
                    {
                        var e = wParam.ToInt32();
                        if (e == (int)DBT.DBT_DEVNODES_CHANGED)
                        {
                            static async System.Threading.Tasks.Task DetectDisplay()
                            {
                                var deviceInformations = await DeviceInformation.FindAllAsync(DisplayMonitor.GetDeviceSelector());
                                Debug.WriteLine($"Detected {deviceInformations.Count} Displays");
                                foreach (DeviceInformation device in deviceInformations)
                                {
                                    DisplayMonitor displayMonitor = await DisplayMonitor.FromInterfaceIdAsync(device.Id);
                                    Debug.WriteLine("============================================");
                                    Debug.WriteLine("DisplayName: " + displayMonitor.DisplayName);
                                    Debug.WriteLine("ConnectionKind: " + displayMonitor.ConnectionKind);
                                    Debug.WriteLine("============================================");
                                }
                            }
                            DetectDisplay();
                        }
                        break;
                    }
            }

            return NativeMethods.CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
        }
    }
}
