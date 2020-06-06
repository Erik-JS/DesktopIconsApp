using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DesktopIconsApp.NativeMethods;

namespace DesktopIconsApp
{
    class Class1
    {

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
            IntPtr iconccount = SendMessage(desktopListViewHandle, LVM_GETITEMCOUNT, 0, IntPtr.Zero);
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
            int MaxChar = 0x100;
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
            LVITEMA lvItem = new LVITEMA() { mask = LVIF_TEXT, iSubItem = 0, cchTextMax = MaxChar } ;
            int lvItemSize = Marshal.SizeOf(lvItem);
            byte[] itemBuffer = new byte[lvItemSize];

            for (int i = 0; i < itemCount; i++)
            {
                lvItem.iItem = i;
                lvItem.pszText = memLoc + 0x300;

                // alloc mem for unmanaged obj
                var lvItemLocalPtr = Marshal.AllocHGlobal(lvItemSize);
                // copy struct to unmanaged space
                Marshal.StructureToPtr(lvItem, lvItemLocalPtr, false);
                // copy unmanaged struct to the target process
                WriteProcessMemory(handleX, memLoc, lvItemLocalPtr, (uint)lvItemSize, IntPtr.Zero);

                SendMessage(handleListView, LVM_GETITEMW, i, memLoc);

                // read updated item from target processs
                ReadProcessMemory(handleX, memLoc, Marshal.UnsafeAddrOfPinnedArrayElement(itemBuffer, 0), (uint)lvItemSize, IntPtr.Zero);
                lvItem = (LVITEMA)Marshal.PtrToStructure(Marshal.UnsafeAddrOfPinnedArrayElement(itemBuffer, 0), typeof(LVITEMA));
                //LogText(String.Format("Response #{0} : {1} | {2} {3}", i + 1, response, strBuffer.ToString("X8"), lvItem.pszText.ToString("X8")));                              
                string str = ReadString(handleX, lvItem.pszText, MaxChar);
                lstItems.Add(str);

                Marshal.FreeHGlobal(lvItemLocalPtr);
            }

            VirtualFreeEx(handleX, memLoc, 0, AllocationType.Release);
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

        public static List<string> GetDesktopItemPositionList()
        {
            var lstPositions = new List<string>();
            IntPtr handleListView = GetSysListView32();
            int itemCount = GetDesktopItemCount(handleListView);
            GetWindowThreadProcessId(handleListView, out uint pid);
            IntPtr handleX = OpenProcess(ProcessAccessFlags.All, false, pid);
            if (handleX == IntPtr.Zero)
            {
                LogText("*** OpenProcess failed ***");
                return lstPositions;
            }

            IntPtr memLoc = VirtualAllocEx(handleX, IntPtr.Zero, 0x1000, AllocationType.Commit, MemoryProtection.ReadWrite);
            POINT pp = new POINT();
            int pointVarSize = Marshal.SizeOf(pp);
            byte[] pBuffer = new byte[pointVarSize];

            for (int i = 0; i < itemCount; i++)
            {
                // alloc mem for unmanaged obj
                var pointUnmanagedPtr = Marshal.AllocHGlobal(pointVarSize);
                // copy struct to unmanaged space
                Marshal.StructureToPtr(pp, pointUnmanagedPtr, false);
                // copy unmanaged struct to the target process
                WriteProcessMemory(handleX, memLoc, pointUnmanagedPtr, (uint)pointVarSize, IntPtr.Zero);

                SendMessage(handleListView, LVM_GETITEMPOSITION, i, memLoc);

                ReadProcessMemory(handleX, memLoc, Marshal.UnsafeAddrOfPinnedArrayElement(pBuffer, 0), (uint)pointVarSize, IntPtr.Zero);
                pp = (POINT)Marshal.PtrToStructure(Marshal.UnsafeAddrOfPinnedArrayElement(pBuffer, 0), typeof(POINT));
                lstPositions.Add(String.Format("{0:D2} X={1:D4} Y={2:D4}", i + 1, pp.X, pp.Y));

                Marshal.FreeHGlobal(pointUnmanagedPtr);
            }

            VirtualFreeEx(handleX, memLoc, 0, AllocationType.Release);
            CloseHandle(handleX);
            return lstPositions;
        }

    }
}