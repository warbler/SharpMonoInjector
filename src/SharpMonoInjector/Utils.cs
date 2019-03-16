using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using SharpMonoInjector.Injection;

namespace SharpMonoInjector
{
    public static class Utils
    {
        public static bool ReadFile(string path, out byte[] bytes)
        {
            try
            {
                bytes = File.ReadAllBytes(path);
                return true;
            }
            catch
            {
                bytes = null;
                return false;
            }
        }

        public static void EnsureNotZero(IntPtr ptr, string name)
        {
            if (ptr == IntPtr.Zero)
                throw new ApplicationException($"{name} = 0");
        }

        public static bool Is64Bit(this Process p)
        {
            if (!Environment.Is64BitOperatingSystem)
                return false;

            if (!Native.IsWow64Process(p.Handle, out bool isWow64))
                throw new Win32Exception();

            return !isWow64;
        }
    }
}
