using System;

namespace SharpMonoInjector.Gui.Models
{
    public class InjectedAssembly
    {
        public int ProcessId { get; set; }

        public IntPtr Address { get; set; }

        public bool Is64Bit { get; set; }

        public string Name { get; set; }
    }
}
