using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpMonoInjector
{
    public class Memory : IDisposable
    {
        private readonly IntPtr _handle;

        private readonly Dictionary<IntPtr, int> _allocations = new Dictionary<IntPtr, int>();

        public Memory(IntPtr processHandle)
        {
            _handle = processHandle;
        }

        public string ReadString(IntPtr address, int length, Encoding encoding)
        {
            List<byte> bytes = new List<byte>();

            for (int i = 0; i < length; i++) {
                byte read = ReadBytes(address + bytes.Count, 1)[0];

                if (read == 0x00)
                    break;

                bytes.Add(read);
            }

            return encoding.GetString(bytes.ToArray());
        }

        public string ReadUnicodeString(IntPtr address, int length)
        {
            return Encoding.Unicode.GetString(ReadBytes(address, length));
        }

        public short ReadShort(IntPtr address)
        {
            return BitConverter.ToInt16(ReadBytes(address, 2), 0);
        }

        public int ReadInt(IntPtr address)
        {
            return BitConverter.ToInt32(ReadBytes(address, 4), 0);
        }

        public long ReadLong(IntPtr address)
        {
            return BitConverter.ToInt64(ReadBytes(address, 8), 0);
        }

        public byte[] ReadBytes(IntPtr address, int size)
        {
            byte[] bytes = new byte[size];

            if (!Native.ReadProcessMemory(_handle, address, bytes, size))
                throw new InjectorException("Failed to read process memory", new Win32Exception(Marshal.GetLastWin32Error()));

            return bytes;
        }

        public IntPtr AllocateAndWrite(byte[] data)
        {
            IntPtr addr = Allocate(data.Length);
            Write(addr, data);
            return addr;
        }

        public IntPtr AllocateAndWrite(string data) => AllocateAndWrite(Encoding.UTF8.GetBytes(data));

        public IntPtr AllocateAndWrite(int data) => AllocateAndWrite(BitConverter.GetBytes(data));

        public IntPtr AllocateAndWrite(long data) => AllocateAndWrite(BitConverter.GetBytes(data));

        public IntPtr Allocate(int size)
        {
            IntPtr addr =
                Native.VirtualAllocEx(_handle, IntPtr.Zero, size,
                    AllocationType.MEM_COMMIT, MemoryProtection.PAGE_EXECUTE_READWRITE);

            if (addr == IntPtr.Zero)
                throw new InjectorException("Failed to allocate process memory", new Win32Exception(Marshal.GetLastWin32Error()));

            _allocations.Add(addr, size);
            return addr;
        }

        public void Write(IntPtr addr, byte[] data)
        {
            if (!Native.WriteProcessMemory(_handle, addr, data, data.Length))
                throw new InjectorException("Failed to write process memory", new Win32Exception(Marshal.GetLastWin32Error()));
        }

        public void Dispose()
        {
            foreach (var kvp in _allocations)
                Native.VirtualFreeEx(_handle, kvp.Key, kvp.Value, MemoryFreeType.MEM_DECOMMIT);
        }
    }
}
