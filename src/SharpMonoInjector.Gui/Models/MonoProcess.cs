using System;

namespace SharpMonoInjector.Gui.Models
{
    public class MonoProcess
    {
        public IntPtr MonoModule { get; set; }

        public string Name { get; set; }

        public int Id { get; set; }
    }
}
