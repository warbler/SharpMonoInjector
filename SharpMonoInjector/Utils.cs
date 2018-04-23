using System;
using System.IO;

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
    }
}
