using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpMonoInjector
{
    public static class ProcessUtils
    {
        public static bool GetMonoModule(IntPtr handle, out IntPtr monoModule)
        {
            int size = Is64BitProcess(handle) ? 8 : 4;

            IntPtr[] modulePointers = new IntPtr[0];

            if (!Native.EnumProcessModulesEx(
                handle, modulePointers, 0, out int bytesNeeded, ModuleFilter.LIST_MODULES_ALL)) {
                throw new InjectorException("Failed to enumerate process modules", new Win32Exception(Marshal.GetLastWin32Error()));
            }

            int count = bytesNeeded / size;
            modulePointers = new IntPtr[count];

            if (!Native.EnumProcessModulesEx(
                handle, modulePointers, bytesNeeded, out bytesNeeded, ModuleFilter.LIST_MODULES_ALL)) {
                throw new InjectorException("Failed to enumerate process modules", new Win32Exception(Marshal.GetLastWin32Error()));
            }

            for (int i = 0; i < count; i++) {
                StringBuilder path = new StringBuilder(260);

                Native.GetModuleFileNameEx(
                    handle, modulePointers[i], path, 260);

                if (path.ToString().EndsWith("mono.dll", StringComparison.OrdinalIgnoreCase)) {
                    if (!Native.GetModuleInformation(
                        handle, modulePointers[i], out MODULEINFO info, (uint)(size * modulePointers.Length))) {
                        throw new InjectorException("Failed to get module information for mono.dll", new Win32Exception(Marshal.GetLastWin32Error()));
                    }

                    monoModule = info.lpBaseOfDll;
                    return true;
                }
            }

            monoModule = IntPtr.Zero;
            return false;
        }

        public static bool Is64BitProcess(IntPtr handle)
        {
            if (!Environment.Is64BitOperatingSystem)
                return false;

            if (!Native.IsWow64Process(handle, out bool isWow64))
                return IntPtr.Size == 8; // assume it's the same as the current process

            return !isWow64;
        }
    }
}
