using System;
using System.IO;

namespace SharpMonoInjector.Injection
{
    public class InjectionConfig
    {
        public byte[] Assembly { get; set; }

        public string AssemblyPath { get; set; }

        public IntPtr AssemblyPointer { get; set; }

        public string Namespace { get; set; }

        public string Class { get; set; }

        public string Method { get; set; }

#if _x64
        public override string ToString()
        {
            return $"0x{AssemblyPointer.ToInt64():X16} - {Path.GetFileName(AssemblyPath)}";
        }
#else
        public override string ToString()
        {
            return $"0x{AssemblyPointer.ToInt64():X8} - {Path.GetFileName(AssemblyPath)}";
        }
#endif
    }
}
