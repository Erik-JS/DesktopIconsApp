﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DesktopIconsApp.MemoryStuff;

namespace DesktopIconsApp
{
    class Class1
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

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
            IntPtr iconccount = SendMessage(desktopListViewHandle, MessageConst.LVM_GETITEMCOUNT, 0, IntPtr.Zero);
            return iconccount.ToInt32();
        }

        public static string GetClassNameString(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(128);
            int a = GetClassName(hWnd, sb, 128);
            if (a == 0) return "";
            return sb.ToString();
        }

        public static List<string> GetDesktopItemTextList()
        {
            List<string> lstItems = new List<string>();

            IntPtr handleListView = GetSysListView32();
            int itemCount = GetDesktopItemCount(handleListView);

            GetWindowThreadProcessId(handleListView, out uint pid);

            IntPtr handleX = OpenProcess(ProcessAccessFlags.All, false, pid);
            if(handleX==IntPtr.Zero)
            {
                LogText("*** OpenProcess failed ***");
                return lstItems;
            }
            
            IntPtr memLoc = VirtualAllocEx(handleX, IntPtr.Zero, 0x1000, AllocationType.Commit, MemoryProtection.ReadWrite);

            

            for (int i = 0; i < itemCount; i++)
            {
                var strBuffer = VirtualAllocEx(handleX, IntPtr.Zero, 0x200, AllocationType.Commit, MemoryProtection.ReadWrite);

                LVITEMA lvItem = new LVITEMA { mask = 1, iItem = i, iSubItem = 0, pszText = strBuffer, cchTextMax = 0x100 };

                var lvItemSize = Marshal.SizeOf(lvItem);
                // alloc mem for unmanaged obj
                var lvItemLocalPtr = Marshal.AllocHGlobal(lvItemSize);
                // copy struct to unmanaged space
                Marshal.StructureToPtr(lvItem, lvItemLocalPtr, false);
                // copy unmanaged struct to the target process
                WriteProcessMemory(handleX, memLoc, lvItemLocalPtr, (uint)lvItemSize, IntPtr.Zero);

                IntPtr response = SendMessage(handleListView, MessageConst.LVM_GETITEMA, i, memLoc);

                LogText("Response #" + (i + 1) + " " + response.ToString());

                string str = ReadString(handleX, strBuffer, 0x100);
                lstItems.Add(str);

                Marshal.FreeHGlobal(lvItemLocalPtr);
                VirtualFreeEx(handleX, strBuffer, 0x200, AllocationType.Release);
            }


            VirtualFreeEx(handleX, memLoc, 0x1000, AllocationType.Release);
            CloseHandle(handleX);
            return lstItems;
        }

        public static void LogText(string text)
        {
            Form1.mainForm.Invoke((MethodInvoker)delegate
            {
                Form1.listbox.Items.Add(text);
            });
        }

        public static string ReadString(IntPtr handle, IntPtr loc, int maxcharcount)
        {
            byte[] bytebuffer = new byte[maxcharcount * 2];
            ReadProcessMemory(handle, loc, bytebuffer, (uint)bytebuffer.Length, IntPtr.Zero);
            string str = System.Text.Encoding.Unicode.GetString(bytebuffer);
            str = str.Substring(0, str.IndexOf('\0'));
            return str;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct LVITEMA
        {
            public uint mask;
            public int iItem;
            public int iSubItem;
            public uint state;
            public uint stateMask;
            public IntPtr pszText; // LPSTR
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam; // LPARAM 
            public int iIndent;
            public int iGroupId;
            public uint cColumns;
            public UIntPtr puColumns; // PUINT
            public IntPtr piColFmt; // int*
            public int iGroup;
        }

    }
}