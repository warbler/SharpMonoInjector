using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;

namespace SharpMonoInjector
{
    public class Injector : IDisposable
    {
        private const string mono_get_root_domain = "mono_get_root_domain";

        private const string mono_thread_attach = "mono_thread_attach";

        private const string mono_image_open_from_data = "mono_image_open_from_data";

        private const string mono_assembly_load_from_full = "mono_assembly_load_from_full";

        private const string mono_assembly_get_image = "mono_assembly_get_image";

        private const string mono_class_from_name = "mono_class_from_name";

        private const string mono_class_get_method_from_name = "mono_class_get_method_from_name";

        private const string mono_runtime_invoke = "mono_runtime_invoke";

        private const string mono_assembly_close = "mono_assembly_close";

        private const string mono_image_strerror = "mono_image_strerror";

        private const string mono_object_get_class = "mono_object_get_class";

        private const string mono_class_get_name = "mono_class_get_name";

        private readonly Dictionary<string, IntPtr> Exports = new Dictionary<string, IntPtr>
        {
            { mono_get_root_domain, IntPtr.Zero },
            { mono_thread_attach, IntPtr.Zero },
            { mono_image_open_from_data, IntPtr.Zero },
            { mono_assembly_load_from_full, IntPtr.Zero },
            { mono_assembly_get_image, IntPtr.Zero },
            { mono_class_from_name, IntPtr.Zero },
            { mono_class_get_method_from_name, IntPtr.Zero },
            { mono_runtime_invoke, IntPtr.Zero },
            { mono_assembly_close, IntPtr.Zero },
            { mono_image_strerror, IntPtr.Zero },
            { mono_object_get_class, IntPtr.Zero },
            { mono_class_get_name, IntPtr.Zero }
        };

        private Memory _memory;

        private IntPtr _rootDomain;

        private bool _attach;

        private readonly IntPtr _handle;

        private IntPtr _mono;

        public bool Is64Bit { get; private set; }

        public Injector(string processName)
        {
            Process process = Process.GetProcesses()
                .FirstOrDefault(p => p.ProcessName
                .Equals(processName, StringComparison.OrdinalIgnoreCase));

            if (process == null)
                throw new InjectorException($"Could not find a process with the name {processName}");

            if ((_handle = Native.OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, process.Id)) == IntPtr.Zero)
                throw new InjectorException("Failed to open process", new Win32Exception(Marshal.GetLastWin32Error()));

            Is64Bit = ProcessUtils.Is64BitProcess(_handle);

            if (!ProcessUtils.GetMonoModule(_handle, out _mono))
                throw new InjectorException("Failed to find mono.dll in the target process");

            _memory = new Memory(_handle);
        }

        public Injector(int processId)
        {
            Process process = Process.GetProcesses()
                .FirstOrDefault(p => p.Id == processId);

            if (process == null)
                throw new InjectorException($"Could not find a process with the id {processId}");

            if ((_handle = Native.OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, process.Id)) == IntPtr.Zero)
                throw new InjectorException("Failed to open process", new Win32Exception(Marshal.GetLastWin32Error()));

            Is64Bit = ProcessUtils.Is64BitProcess(_handle);

            if (!ProcessUtils.GetMonoModule(_handle, out _mono))
                throw new InjectorException("Failed to find mono.dll in the target process");

            _memory = new Memory(_handle);
        }

        public Injector(IntPtr processHandle, IntPtr monoModule)
        {
            if ((_handle = processHandle) == IntPtr.Zero)
                throw new ArgumentException("Argument cannot be zero", nameof(processHandle));

            if ((_mono = monoModule) == IntPtr.Zero)
                throw new ArgumentException("Argument cannot be zero", nameof(monoModule));

            Is64Bit = ProcessUtils.Is64BitProcess(_handle);
            _memory = new Memory(_handle);
        }

        public void Dispose()
        {
            _memory.Dispose();
            Native.CloseHandle(_handle);
        }

        private void ObtainMonoExports()
        {
            foreach (ExportedFunction ef in ProcessUtils.GetExportedFunctions(_handle, _mono))
                if (Exports.ContainsKey(ef.Name))
                    Exports[ef.Name] = ef.Address;

            foreach (var kvp in Exports)
                if (kvp.Value == IntPtr.Zero)
                    throw new InjectorException($"Failed to obtain the address of {kvp.Key}()");
        }

        public IntPtr Inject(byte[] rawAssembly, string @namespace, string className, string methodName)
        {
            if (rawAssembly == null)
                throw new ArgumentNullException(nameof(rawAssembly));

            if (rawAssembly.Length == 0)
                throw new ArgumentException($"{nameof(rawAssembly)} cannot be empty", nameof(rawAssembly));

            if (className == null)
                throw new ArgumentNullException(nameof(className));

            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));

            IntPtr rawImage, assembly, image, @class, method;

            ObtainMonoExports();
            _rootDomain = GetRootDomain();
            rawImage = OpenImageFromData(rawAssembly);
            _attach = true;
            assembly = OpenAssemblyFromImage(rawImage);
            image = GetImageFromAssembly(assembly);
            @class = GetClassFromName(image, @namespace, className);
            method = GetMethodFromName(@class, methodName);
            RuntimeInvoke(method);
            return assembly;
        }

        public void Eject(IntPtr assembly, string @namespace, string className, string methodName)
        {
            if (assembly == IntPtr.Zero)
                throw new ArgumentException($"{nameof(assembly)} cannot be zero", nameof(assembly));

            if (className == null)
                throw new ArgumentNullException(nameof(className));

            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));

            IntPtr image, @class, method;

            ObtainMonoExports();
            _rootDomain = GetRootDomain();
            _attach = true;
            image = GetImageFromAssembly(assembly);
            @class = GetClassFromName(image, @namespace, className);
            method = GetMethodFromName(@class, methodName);
            RuntimeInvoke(method);
            CloseAssembly(assembly);
        }

        private static void ThrowIfNull(IntPtr ptr, string methodName)
        {
            if (ptr == IntPtr.Zero)
                throw new InjectorException($"{methodName}() returned NULL");
        }

        private IntPtr GetRootDomain()
        {
            IntPtr rootDomain = Execute(Exports[mono_get_root_domain]);
            ThrowIfNull(rootDomain, mono_get_root_domain);
            return rootDomain;
        }

        private IntPtr OpenImageFromData(byte[] assembly)
        {
            IntPtr statusPtr = _memory.Allocate(4);
            IntPtr rawImage = Execute(Exports[mono_image_open_from_data],
                _memory.AllocateAndWrite(assembly), (IntPtr)assembly.Length, (IntPtr)1, statusPtr);

            MonoImageOpenStatus status = (MonoImageOpenStatus)_memory.ReadInt(statusPtr);
            
            if (status != MonoImageOpenStatus.MONO_IMAGE_OK) {
                IntPtr messagePtr = Execute(Exports[mono_image_strerror], (IntPtr)status);
                string message = _memory.ReadString(messagePtr, 256, Encoding.UTF8);
                throw new InjectorException($"{mono_image_open_from_data}() failed: {message}");
            }

            return rawImage;
        }

        private IntPtr OpenAssemblyFromImage(IntPtr image)
        {
            IntPtr statusPtr = _memory.Allocate(4);
            IntPtr assembly = Execute(Exports[mono_assembly_load_from_full],
                image, _memory.AllocateAndWrite(new byte[1]), statusPtr, IntPtr.Zero);

            MonoImageOpenStatus status = (MonoImageOpenStatus)_memory.ReadInt(statusPtr);

            if (status != MonoImageOpenStatus.MONO_IMAGE_OK) {
                IntPtr messagePtr = Execute(Exports[mono_image_strerror], (IntPtr)status);
                string message = _memory.ReadString(messagePtr, 256, Encoding.UTF8);
                throw new InjectorException($"{mono_assembly_load_from_full}() failed: {message}");
            }

            return assembly;
        }

        private IntPtr GetImageFromAssembly(IntPtr assembly)
        {
            IntPtr image = Execute(Exports[mono_assembly_get_image], assembly);
            ThrowIfNull(image, mono_assembly_get_image);
            return image;
        }

        private IntPtr GetClassFromName(IntPtr image, string @namespace, string className)
        {
            IntPtr @class = Execute(Exports[mono_class_from_name],
                image, _memory.AllocateAndWrite(@namespace), _memory.AllocateAndWrite(className));
            ThrowIfNull(@class, mono_class_from_name);
            return @class;
        }

        private IntPtr GetMethodFromName(IntPtr @class, string methodName)
        {
            IntPtr method = Execute(Exports[mono_class_get_method_from_name],
                @class, _memory.AllocateAndWrite(methodName), IntPtr.Zero);
            ThrowIfNull(method, mono_class_get_method_from_name);
            return method;
        }

        private string GetClassName(IntPtr monoObject)
        {
            IntPtr @class = Execute(Exports[mono_object_get_class], monoObject);
            ThrowIfNull(@class, mono_object_get_class);
            IntPtr className = Execute(Exports[mono_class_get_name], @class);
            ThrowIfNull(className, mono_class_get_name);
            return _memory.ReadString(className, 256, Encoding.UTF8);
        }

        private string ReadMonoString(IntPtr monoString)
        {
            int len = _memory.ReadInt(monoString + (Is64Bit ? 0x10 : 0x8));
            return _memory.ReadUnicodeString(monoString + (Is64Bit ? 0x14 : 0xC), len * 2);
        }

        private void RuntimeInvoke(IntPtr method)
        {
            IntPtr excPtr = Is64Bit ? _memory.AllocateAndWrite((long)0) : _memory.AllocateAndWrite(0);

            IntPtr result = Execute(Exports[mono_runtime_invoke],
                method, IntPtr.Zero, IntPtr.Zero, excPtr);

            IntPtr exc = (IntPtr)_memory.ReadInt(excPtr);

            if (exc != IntPtr.Zero) {
                string className = GetClassName(exc);
                string message = ReadMonoString((IntPtr)_memory.ReadInt(exc + (Is64Bit ? 0x20 : 0x10)));
                throw new InjectorException($"The managed method threw an exception: ({className}) {message}");
            }
        }

        private void CloseAssembly(IntPtr assembly)
        {
            IntPtr result = Execute(Exports[mono_assembly_close], assembly);
            ThrowIfNull(result, mono_assembly_close);
        }

        private IntPtr Execute(IntPtr address, params IntPtr[] args)
        {
            IntPtr retValPtr = Is64Bit
                ? _memory.AllocateAndWrite((long)0)
                : _memory.AllocateAndWrite(0);

            byte[] code = Assemble(address, retValPtr, args);
            IntPtr alloc = _memory.AllocateAndWrite(code);

            IntPtr thread = Native.CreateRemoteThread(
                _handle, IntPtr.Zero, 0, alloc, IntPtr.Zero, 0, out _);

            if (thread == IntPtr.Zero)
                throw new InjectorException("Failed to create a remote thread", new Win32Exception(Marshal.GetLastWin32Error()));

            WaitResult result = Native.WaitForSingleObject(thread, -1);

            if (result == WaitResult.WAIT_FAILED)
                throw new InjectorException("Failed to wait for a remote thread", new Win32Exception(Marshal.GetLastWin32Error()));

            IntPtr ret = Is64Bit
                ? (IntPtr)_memory.ReadLong(retValPtr)
                : (IntPtr)_memory.ReadInt(retValPtr);

            if ((long)ret == 0x00000000C0000005)
                throw new InjectorException($"An access violation occurred while executing {Exports.First(e => e.Value == address).Key}()");

            return ret;
        }

        private byte[] Assemble(IntPtr functionPtr, IntPtr retValPtr, IntPtr[] args)
        {
            return Is64Bit
                ? Assemble64(functionPtr, retValPtr, args)
                : Assemble86(functionPtr, retValPtr, args);
        }

        private byte[] Assemble86(IntPtr functionPtr, IntPtr retValPtr, IntPtr[] args)
        {
            Assembler asm = new Assembler();

            if (_attach) {
                asm.Push(_rootDomain);
                asm.MovEax(Exports[mono_thread_attach]);
                asm.CallEax();
                asm.AddEsp(4);
            }

            for (int i = args.Length - 1; i >= 0; i--)
                asm.Push(args[i]);

            asm.MovEax(functionPtr);
            asm.CallEax();
            asm.AddEsp((byte)(args.Length * 4));
            asm.MovEaxTo(retValPtr);
            asm.Return();

            return asm.ToByteArray();
        }

        private byte[] Assemble64(IntPtr functionPtr, IntPtr retValPtr, IntPtr[] args)
        {
            Assembler asm = new Assembler();

            asm.SubRsp(40);

            if (_attach) {
                asm.MovRax(Exports[mono_thread_attach]);
                asm.MovRcx(_rootDomain);
                asm.CallRax();
            }

            asm.MovRax(functionPtr);

            for (int i = 0; i < args.Length; i++) {
                switch (i) {
                    case 0:
                        asm.MovRcx(args[i]);
                        break;
                    case 1:
                        asm.MovRdx(args[i]);
                        break;
                    case 2:
                        asm.MovR8(args[i]);
                        break;
                    case 3:
                        asm.MovR9(args[i]);
                        break;
                }
            }

            asm.CallRax();
            asm.AddRsp(40);
            asm.MovRaxTo(retValPtr);
            asm.Return();

            return asm.ToByteArray();
        }
    }
}
