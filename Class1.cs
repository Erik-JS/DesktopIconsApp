using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopIconsApp
{
    class Class1
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        static extern IntPtr GetShellWindow();


        public static IntPtr GetSysListView32()
        {
            IntPtr handleProgman, handleShelldll, handleSysListView32;
            handleProgman = FindWindow("Progman", null);
            handleShelldll = FindWindowEx(handleProgman, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (handleShelldll == IntPtr.Zero)
            {
                EnumWindows(delegate (IntPtr hWnd, IntPtr lParam) {
                    if (GetClassNameString(hWnd) == "WorkerW")
                    {
                        handleShelldll = FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                        if (handleShelldll != IntPtr.Zero)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                , IntPtr.Zero);
            }
            handleSysListView32 = FindWindowEx(handleShelldll, IntPtr.Zero, "SysListView32", null);
            return handleSysListView32;
        }

        public static int GetDesktopItemCount(IntPtr desktopListViewHandle)
        {
            IntPtr iconccount = SendMessage(desktopListViewHandle, MessageConst.LVM_GETITEMCOUNT, 0, 0);
            return iconccount.ToInt32();
        }

        public static string GetClassNameString(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(128);
            int a = GetClassName(hWnd, sb, 128);
            if (a == 0) return "";
            return sb.ToString();
        }

        public static void LogText(string text)
        {
            Form1.mainForm.Invoke((MethodInvoker)delegate
            {
                Form1.listbox.Items.Add(text);
            });
        }

    }
}