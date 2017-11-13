using System;
using System.IO;

namespace SharpMonoInjector
{
    public class InjectionConfig
    {
        public System.Diagnostics.Process Process { get; set; }
        public byte[] Assembly { get; set; }
        public string AssemblyPath { get; set; }
        public IntPtr AssemblyPointer { get; set; }
        public string Namespace { get; set; }
        public string Class { get; set; }
        public string Method { get; set; }
        public string Text { get { return $"0x{AssemblyPointer.ToInt64():X8} - {Path.GetFileName(AssemblyPath)}"; } }
    }
}
