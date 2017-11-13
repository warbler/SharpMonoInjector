using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using NETProcess = System.Diagnostics.Process;

namespace SharpMonoInjector
{
    public class MonoProcess
    {
        public string Text { get { return $"{Process.Id} - {Process.ProcessName}"; } }
        public NETProcess Process;

        public MonoProcess(NETProcess process)
        {
            Process = process;
        }

        public static MonoProcess[] GetProcesses()
        {
            List<MonoProcess> procs = new List<MonoProcess>();

            foreach (NETProcess p in NETProcess.GetProcesses())
            {
                try
                {
                    foreach (ProcessModule pm in p.Modules)
                        if (pm.ModuleName.Equals("mono.dll", StringComparison.OrdinalIgnoreCase))
                            procs.Add(new MonoProcess(p));
                }
                catch (Win32Exception) { continue; }
            }
            return procs.ToArray();
        }
    }
}
