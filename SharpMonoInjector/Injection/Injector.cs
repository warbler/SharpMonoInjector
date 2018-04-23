using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMonoInjector.Injection
{
    public class Injector
    {
        public IntPtr ProcessHandle;

        private Memory _memory;

        private IntPtr _rootDomain;

        private bool _attach;

        private static readonly Dictionary<string, IntPtr> Exports = new Dictionary<string, IntPtr>
        {
            {"mono_get_root_domain", IntPtr.Zero},
            {"mono_thread_attach", IntPtr.Zero},
            {"mono_image_open_from_data", IntPtr.Zero},
            {"mono_assembly_load_from_full", IntPtr.Zero},
            {"mono_assembly_get_image", IntPtr.Zero},
            {"mono_class_from_name", IntPtr.Zero},
            {"mono_class_get_method_from_name", IntPtr.Zero},
            {"mono_runtime_invoke", IntPtr.Zero},
            {"mono_assembly_close", IntPtr.Zero}
        };

        public static bool ExportsLoaded { get; private set; }

        public Injector(IntPtr processHandle)
        {
            ProcessHandle = processHandle;
        }

        public static bool LoadMonoFunctions(string monoDllPath)
        {
            IntPtr handle = UnsafeNativeMethods.LoadLibrary(monoDllPath);

            if (handle == IntPtr.Zero)
                return false;

            foreach (string func in Exports.Keys.ToArray())
                Exports[func] = UnsafeNativeMethods.GetProcAddress(handle, func);

            UnsafeNativeMethods.FreeLibrary(handle);

            return ExportsLoaded = Exports.Values.All(v => v != IntPtr.Zero);
        }

        public void Inject(InjectionConfig cfg)
        {
            if (!ExportsLoaded)
                throw new ApplicationException("The required mono functions have not been loaded");

            using (_memory = new Memory(ProcessHandle))
            {
                _rootDomain = GetRootDomain();

                Utils.EnsureNotZero(_rootDomain, "GetRootDomain()");

                IntPtr rawImage = OpenImageFromData(cfg.Assembly);

                Utils.EnsureNotZero(rawImage, "OpenImageFromData()");

                _attach = true;

                IntPtr assembly = cfg.AssemblyPointer = OpenAssemblyFromImage(rawImage);

                Utils.EnsureNotZero(assembly, "OpenAssemblyFromImage()");

                IntPtr image = GetImageFromAssembly(assembly);

                Utils.EnsureNotZero(image, "GetImageFromAssembly()");

                IntPtr @class = GetClassFromName(image, cfg.Namespace, cfg.Class);

                Utils.EnsureNotZero(@class, "GetClassFromName()");

                IntPtr method = GetMethodFromName(@class, cfg.Method);

                Utils.EnsureNotZero(method, "GetMethodFromName()");

                bool result = RuntimeInvoke(method);

                _attach = false;

                if (!result)
                    throw new ApplicationException($"The invocation of {cfg.Class}.{cfg.Method} failed");
            }
        }

        private IntPtr GetRootDomain()
        {
            return Execute(Exports["mono_get_root_domain"]);
        }

        private IntPtr OpenImageFromData(byte[] assembly)
        {
            return Execute(Exports["mono_image_open_from_data"],
                _memory.AllocateAndWrite(assembly),
                (IntPtr)assembly.Length,
                (IntPtr)1,
                IntPtr.Zero);
        }

        private IntPtr OpenAssemblyFromImage(IntPtr image)
        {
            return Execute(Exports["mono_assembly_load_from_full"],
                image, _memory.AllocateAndWrite(new byte[1]), IntPtr.Zero, IntPtr.Zero);
        }

        private IntPtr GetImageFromAssembly(IntPtr assembly)
        {
            return Execute(Exports["mono_assembly_get_image"], assembly);
        }

        private IntPtr GetClassFromName(IntPtr image, string @namespace, string @class)
        {
            return Execute(Exports["mono_class_from_name"], image,
                _memory.AllocateAndWrite(Encoding.UTF8.GetBytes(@namespace)),
                _memory.AllocateAndWrite(Encoding.UTF8.GetBytes(@class)));
        }

        private IntPtr GetMethodFromName(IntPtr @class, string method)
        {
            return Execute(Exports["mono_class_get_method_from_name"], @class,
                _memory.AllocateAndWrite(Encoding.UTF8.GetBytes(method)), IntPtr.Zero);
        }

        private bool RuntimeInvoke(IntPtr method)
        {
            return (int)Execute(Exports["mono_runtime_invoke"], method, IntPtr.Zero,
                IntPtr.Zero, IntPtr.Zero) == 0;
        }

        private void CloseAssembly(IntPtr assembly)
        {
            Execute(Exports["mono_assembly_close"], assembly);
        }

        public void UnloadAndCloseAssembly(InjectionConfig config, string methodName)
        {
            using (_memory = new Memory(ProcessHandle))
            {
                _attach = true;

                IntPtr image = GetImageFromAssembly(config.AssemblyPointer);

                Utils.EnsureNotZero(image, "GetImageFromAssembly()");

                IntPtr @class = GetClassFromName(image, config.Namespace, config.Class);

                Utils.EnsureNotZero(@class, "GetClassFromName()");

                IntPtr method = GetMethodFromName(@class, methodName);

                Utils.EnsureNotZero(method, "GetMethodFromName()");

                RuntimeInvoke(method);

                CloseAssembly(config.AssemblyPointer);

                _attach = false;
            }
        }

        private IntPtr Execute(IntPtr address, params IntPtr[] args)
        {
            byte[] code = Assemble(address, args);
            IntPtr alloc = _memory.AllocateAndWrite(code);

            return GetThreadReturnValue(
                UnsafeNativeMethods.CreateRemoteThread(
                    ProcessHandle, IntPtr.Zero, 0, alloc, IntPtr.Zero, 0, out _));
        }

#if _x64

        private byte[] Assemble(IntPtr address, IntPtr[] args)
        {
            Assembler asm = new Assembler();

            asm.SubRsp(40);

            if (_attach)
            {
                asm.MovRax(Exports["mono_thread_attach"]);
                asm.MovRcx(_rootDomain);
                asm.CallRax();
            }

            asm.MovRax(address);

            for (int i = 0; i < args.Length; i++)
            {
                switch (i)
                {
                    case 0: asm.MovRcx(args[i]);
                        break;
                    case 1: asm.MovRdx(args[i]);
                        break;
                    case 2: asm.MovR8(args[i]);
                        break;
                    case 3: asm.MovR9(args[i]);
                        break;
                }
            }

            asm.CallRax();
            asm.AddRsp(40);
            asm.Return();

            return asm.ToByteArray();
        }

#else

        private byte[] Assemble(IntPtr address, IntPtr[] args)
        {
            Assembler asm = new Assembler();

            if (_attach)
            {
                asm.Push(_rootDomain);
                asm.MovEax(Exports["mono_thread_attach"]);
                asm.CallEax();
                asm.AddEsp(4);
            }

            for (int i = args.Length - 1; i >= 0; i--)
                asm.Push(args[i]);

            asm.MovEax(address);
            asm.CallEax();
            asm.AddEsp((byte)(args.Length * 4));
            asm.Return();

            return asm.ToByteArray();
        }

#endif
        private IntPtr GetThreadReturnValue(IntPtr hThread)
        {
            UnsafeNativeMethods.WaitForSingleObject(hThread, -1);
            UnsafeNativeMethods.GetExitCodeThread(hThread, out IntPtr exitCode);
            return exitCode;
        }
    }
}
