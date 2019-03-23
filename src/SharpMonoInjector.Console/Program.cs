using System;
using System.IO;

namespace SharpMonoInjector.Console
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0) {
                PrintHelp();
                return;
            }

            CommandLineArguments cla = new CommandLineArguments(args);

            bool inject = cla.IsSwitchPresent("inject");
            bool eject = cla.IsSwitchPresent("eject");

            if (!inject && !eject) {
                System.Console.WriteLine("No operation (inject/eject) specified");
                return;
            }

            Injector injector;

            if (cla.GetIntArg("-p", out int pid)) {
                injector = new Injector(pid);
            } else if (cla.GetStringArg("-p", out string pname)) {
                injector = new Injector(pname);
            } else {
                System.Console.WriteLine("No process id/name specified");
                return;
            }

            if (inject)
                Inject(injector, cla);
            else
                Eject(injector, cla);
        }

        private static void PrintHelp()
        {
            const string help =
                "SharpMonoInjector 2.2\r\n\r\n" +
                "Usage:\r\n" +
                "smi.exe <inject/eject> <options>\r\n\r\n" +
                "Options:\r\n" +
                "-p - The id or name of the target process\r\n" +
                "-a - When injecting, the path of the assembly to inject. When ejecting, the address of the assembly to eject\r\n" +
                "-n - The namespace in which the loader class resides\r\n" +
                "-c - The name of the loader class\r\n" +
                "-m - The name of the method to invoke in the loader class\r\n\r\n" +
                "Examples:\r\n" +
                "smi.exe inject -p testgame -a ExampleAssembly.dll -n ExampleAssembly -c Loader -m Load\r\n" +
                "smi.exe eject -p testgame -a 0x13D23A98 -n ExampleAssembly -c Loader -m Unload\r\n";
            System.Console.WriteLine(help);
        }

        private static void Inject(Injector injector, CommandLineArguments args)
        {
            string assemblyPath, @namespace, className, methodName;
            byte[] assembly;

            if (args.GetStringArg("-a", out assemblyPath)) {
                try {
                    assembly = File.ReadAllBytes(assemblyPath);
                } catch {
                    System.Console.WriteLine("Could not read the file " + assemblyPath);
                    return;
                }
            } else {
                System.Console.WriteLine("No assembly specified");
                return;
            }

            args.GetStringArg("-n", out @namespace);

            if (!args.GetStringArg("-c", out className)) {
                System.Console.WriteLine("No class name specified");
                return;
            }

            if (!args.GetStringArg("-m", out methodName)) {
                System.Console.WriteLine("No method name specified");
                return;
            }

            using (injector) {
                IntPtr remoteAssembly = IntPtr.Zero;

                try {
                    remoteAssembly = injector.Inject(assembly, @namespace, className, methodName);
                } catch (InjectorException ie) {
                    System.Console.WriteLine("Failed to inject assembly: " + ie);
                } catch (Exception exc) {
                    System.Console.WriteLine("Failed to inject assembly (unknown error): " + exc);
                }

                if (remoteAssembly == IntPtr.Zero)
                    return;

                System.Console.WriteLine($"{Path.GetFileName(assemblyPath)}: " +
                    (injector.Is64Bit
                    ? $"0x{remoteAssembly.ToInt64():X16}"
                    : $"0x{remoteAssembly.ToInt32():X8}"));
            }
        }

        private static void Eject(Injector injector, CommandLineArguments args)
        {
            IntPtr assembly;
            string @namespace, className, methodName;

            if (args.GetIntArg("-a", out int intPtr)) {
                assembly = (IntPtr)intPtr;
            } else if (args.GetLongArg("-a", out long longPtr)) {
                assembly = (IntPtr)longPtr;
            } else {
                System.Console.WriteLine("No assembly pointer specified");
                return;
            }

            args.GetStringArg("-n", out @namespace);

            if (!args.GetStringArg("-c", out className)) {
                System.Console.WriteLine("No class name specified");
                return;
            }

            if (!args.GetStringArg("-m", out methodName)) {
                System.Console.WriteLine("No method name specified");
                return;
            }

            using (injector) {
                try {
                    injector.Eject(assembly, @namespace, className, methodName);
                    System.Console.WriteLine("Ejection successful");
                } catch (InjectorException ie) {
                    System.Console.WriteLine("Ejection failed: " + ie);
                } catch (Exception exc) {
                    System.Console.WriteLine("Ejection failed (unknown error): " + exc);
                }
            }
        }
    }
}
