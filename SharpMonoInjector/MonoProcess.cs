using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;

namespace SharpMonoInjector
{
    public class MonoProcess
    {
        public Process Process { get; }

        public MonoProcess(Process process)
        {
            Process = process;
        }

        public static MonoProcess[] GetProcesses()
        {
            List<MonoProcess> procs = new List<MonoProcess>();

            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    foreach (ProcessModule pm in p.Modules)
                        if (pm.ModuleName.Equals("mono.dll", StringComparison.OrdinalIgnoreCase))
                            procs.Add(new MonoProcess(p));
                }
                catch (Win32Exception)
                {
                    // ignore
                }
            }

            return procs.ToArray();
        }

        public override string ToString()
        {
            return $"{Process.Id} - {Process.ProcessName}";
        }
    }
}
