using System;
using System.IO;

namespace SharpMonoInjector.Injection
{
    public class InjectionConfig
    {
        public MonoProcess Target { get; set; }

        public byte[] Assembly { get; set; }

        public string AssemblyPath { get; set; }

        public IntPtr AssemblyPointer { get; set; }

        public string Namespace { get; set; }

        public string Class { get; set; }

        public string Method { get; set; }

        public override string ToString()
        {
            return Target.Process.Is64Bit() 
                ? $"0x{AssemblyPointer.ToInt64():X16} - {Path.GetFileName(AssemblyPath)}" 
                : $"0x{AssemblyPointer.ToInt64():X8} - {Path.GetFileName(AssemblyPath)}";
        }
    }
}
