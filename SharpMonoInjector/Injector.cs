using System;
using Process.NET;
using Process.NET.Memory;
using Process.NET.Assembly;
using Process.NET.Native.Types;
using Binarysharp.Assemblers.Fasm;
using Process.NET.Assembly.Assemblers;
using NETProcess = System.Diagnostics.Process;

namespace SharpMonoInjector
{
    public class Injector : IDisposable, IAssembler
    {
        private static Injector instance = new Injector();
        public static Injector Instance { get { return instance; } }

        ProcessSharp target;
        AssemblyFactory factory;

        IntPtr rootDomain;
        IntPtr threadAttach;
        private bool attach;

        public event Action<string> Error;
        public event Action<InjectionConfig> SuccessfulInjection;
        public event Action<InjectionConfig> SuccessfulEjection;

        private Injector()
        {

        }

        private void OnError(string msg, InjectionConfig cfg = null)
        {
            /* If an injection attempt fails after the assembly was loaded, 
               it needs to be closed. Otherwise the next injection will cause
               a crash since the assembly is already loaded */
            if (cfg != null && cfg.AssemblyPointer != IntPtr.Zero)
                CloseAssembly(cfg);

            Error?.Invoke(msg);
        }

        private void Setup(NETProcess process)
        {
            factory?.Dispose();
            target?.Dispose();
            target = new ProcessSharp(process, MemoryType.Remote);
            factory = new AssemblyFactory(target, this);
            attach = false;
        }

        public void Inject(InjectionConfig config)
        {
            Setup(config.Process);

            threadAttach = target.ModuleFactory["mono.dll"]["mono_thread_attach"].BaseAddress;

            if (threadAttach == IntPtr.Zero)
            {
                OnError("Could not find mono_thread_attach");
                return;
            }

            IntPtr rawImage, image, iklass, imethod;

            if ((rootDomain = GetRootDomain()) == IntPtr.Zero)
            {
                OnError("GetRootDomain returned 0");
                return;
            }

            if ((rawImage = OpenImageFromData(config.Assembly)) == IntPtr.Zero)
            {
                OnError("OpenImageFromData returned 0");
                return;
            }

            /* From this point on, the threads created by AssemblyFactory.Execute
               will have to be registered with the Mono runtime since we
               are accessing Mono objects. Otherwise the target process crashes. */

            attach = true;

            if ((config.AssemblyPointer = OpenAssemblyFromImage(rawImage)) == IntPtr.Zero)
            {
                OnError("OpenAssemblyFromImage returned 0", config);
                return;
            }

            if ((image = GetImageFromAssembly(config.AssemblyPointer)) == IntPtr.Zero)
            {
                OnError("GetImageFromAssembly returned 0", config);
                return;
            }

            if ((iklass = GetClassFromName(image, config.Namespace, config.Class)) == IntPtr.Zero)
            {
                OnError("GetClassFromName returned 0", config);
                return;
            }

            if ((imethod = GetMethodFromName(iklass, config.Method)) == IntPtr.Zero)
            {
                OnError("GetMethodFromName returned 0", config);
                return;
            }

            if (RuntimeInvoke(imethod) == 0)
                SuccessfulInjection?.Invoke(config);
            else
                OnError($"Execution of {config.Method} failed", config);
        }

        private IntPtr GetRootDomain()
        {
            IntPtr addr = target.ModuleFactory["mono.dll"]["mono_get_root_domain"].BaseAddress;

            if (addr != IntPtr.Zero)
                return factory.Execute<IntPtr>(addr, CallingConventions.Cdecl);

            OnError("Could not find mono_get_root_domain");
            return IntPtr.Zero;
        }

        private IntPtr OpenImageFromData(byte[] assembly)
        {
            IntPtr addr = target.ModuleFactory["mono.dll"]["mono_image_open_from_data"].BaseAddress;

            if (addr != IntPtr.Zero)
            {
                IAllocatedMemory alloc = target.MemoryFactory.Allocate("assembly", assembly.Length);
                alloc.Write(0, assembly);
                IntPtr image = factory.Execute<IntPtr>(addr, CallingConventions.Cdecl, alloc.BaseAddress, assembly.Length, 1, IntPtr.Zero);
                target.MemoryFactory.Deallocate(alloc);
                return image;
            }

            OnError("Could not find mono_image_open_from_data");
            return IntPtr.Zero;
        }

        private IntPtr OpenAssemblyFromImage(IntPtr image)
        {
            IntPtr addr = target.ModuleFactory["mono.dll"]["mono_assembly_load_from_full"].BaseAddress;

            if (addr != IntPtr.Zero)
                return factory.Execute<IntPtr>(addr, CallingConventions.Cdecl, image, "UNUSED", IntPtr.Zero, 0);

            OnError("Could not find mono_assembly_load_from_full");
            return IntPtr.Zero;
        }

        private IntPtr GetImageFromAssembly(IntPtr assembly)
        {
            IntPtr addr = target.ModuleFactory["mono.dll"]["mono_assembly_get_image"].BaseAddress;

            if (addr != IntPtr.Zero)
                return factory.Execute<IntPtr>(addr, CallingConventions.Cdecl, assembly);

            OnError("Could not find mono_assembly_get_image");
            return IntPtr.Zero;
        }

        private IntPtr GetClassFromName(IntPtr image, string name_space, string klass)
        {
            IntPtr addr = target.ModuleFactory["mono.dll"]["mono_class_from_name"].BaseAddress;
            
            if (addr != IntPtr.Zero)
                return factory.Execute<IntPtr>(addr, CallingConventions.Cdecl, image, name_space, klass);

            OnError("Could not find mono_class_from_name");
            return IntPtr.Zero;
        }

        private IntPtr GetMethodFromName(IntPtr klass, string method)
        {
            IntPtr addr = target.ModuleFactory["mono.dll"]["mono_class_get_method_from_name"].BaseAddress;

            if (addr != IntPtr.Zero)
                return factory.Execute<IntPtr>(addr, CallingConventions.Cdecl, klass, method, 0);

            OnError("Could not find mono_class_get_method_from_name");
            return IntPtr.Zero;
        }

        private int RuntimeInvoke(IntPtr method)
        {
            IntPtr addr = target.ModuleFactory["mono.dll"]["mono_runtime_invoke"].BaseAddress;

            if (addr != IntPtr.Zero)
                return factory.Execute<int>(addr, CallingConventions.Cdecl, method, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            OnError("Could not find mono_runtime_invoke");
            return 1;
        }

        private void CloseAssembly(InjectionConfig config)
        {
            IntPtr addr = target.ModuleFactory["mono.dll"]["mono_assembly_close"].BaseAddress;

            if (addr != IntPtr.Zero)
            {
                factory.Execute(addr, CallingConventions.Cdecl, config.AssemblyPointer);
                SuccessfulEjection?.Invoke(config);
                return;
            }

            OnError("Could not find mono_assembly_close");
        }

        public void UnloadAndCloseAssembly(InjectionConfig config, string method)
        {
            if (!string.IsNullOrEmpty(method))
            {
                IntPtr image, klass, imethod;

                if ((image = GetImageFromAssembly(config.AssemblyPointer)) == IntPtr.Zero)
                    OnError("GetImageFromAssembly returned 0");
                else
                {
                    if ((klass = GetClassFromName(image, config.Namespace, config.Class)) == IntPtr.Zero)
                        OnError("GetClassFromName returned 0");
                    else
                    {
                        if ((imethod = GetMethodFromName(klass, method)) == IntPtr.Zero)
                            OnError("GetMethodFromName returned 0");
                        else
                        {
                            if (RuntimeInvoke(imethod) != 0)
                                OnError($"Execution of {method} failed");
                        }
                    }
                }
            }

            CloseAssembly(config);
        }

        public void Dispose()
        {
            factory?.Dispose();
            target?.Dispose();
        }

        // IAssembler implementation

        public byte[] Assemble(string asm)
        {
            return Assemble(asm, IntPtr.Zero);
        }

        public byte[] Assemble(string asm, IntPtr baseAddress)
        {
            if (!attach)
                asm = $"use32\norg {baseAddress}\n" + asm;
            else
                // Call mono_thread_attach first
                asm = $"use32\norg {baseAddress}\npush {rootDomain}\ncall {threadAttach}\nadd esp, 4\n" + asm;
            return FasmNet.Assemble(asm);
        }
    }
}
