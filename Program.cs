using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DXLinkFormatter {
    static class Program {
        private static WinAPI.LowLevelKeyboardProc _proc;
        private static IntPtr _hookID = IntPtr.Zero;

        public static FormClipboardRecords ClipboardRecordsForm { get; set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            ClipboardRecordsForm = new FormClipboardRecords();
            
            Form1 form = new Form1();
            form.Left = Screen.PrimaryScreen.WorkingArea.Width - form.Width;
            form.Top = Screen.PrimaryScreen.WorkingArea.Height - form.Height;

            _proc = HookCallback;
            _hookID = SetHook(_proc);

            Application.Run();
        }

        private static IntPtr SetHook(WinAPI.LowLevelKeyboardProc proc) {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                return WinAPI.SetWindowsHookEx(WinAPI.WH_KEYBOARD_LL, proc,
                    WinAPI.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && wParam == (IntPtr)WinAPI.WM_KEYDOWN) {
                int vkCode = Marshal.ReadInt32(lParam);

                if ((Keys)vkCode == Keys.M) {
                    bool ctrl = (Form.ModifierKeys & Keys.Control) == Keys.Control;
                    bool shift = (Form.ModifierKeys & Keys.Shift) == Keys.Shift;

                    if (ctrl && shift) {
                        ClipboardRecordsForm.Show();
                    }
                }
            }

            return WinAPI.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}