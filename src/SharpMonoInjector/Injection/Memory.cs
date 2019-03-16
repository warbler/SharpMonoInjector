using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMonoInjector.Injection
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

            for (int i = 0; i < length; i++)
            {
                byte read = ReadBytes(address + bytes.Count, 1)[0];

                if (read != 0x00)
                    bytes.Add(read);
                else break;
            }

            return encoding.GetString(bytes.ToArray());
        }

        public short ReadShort(IntPtr address)
        {
            return BitConverter.ToInt16(ReadBytes(address, 2), 0);
        }

        public int ReadInt(IntPtr address)
        {
            return BitConverter.ToInt32(ReadBytes(address, 4), 0);
        }

        public byte[] ReadBytes(IntPtr address, int size)
        {
            byte[] bytes = new byte[size];
            Native.ReadProcessMemory(_handle, address, bytes, size, out _);
            return bytes;
        }

        public IntPtr AllocateAndWrite(byte[] data)
        {
            IntPtr addr = Allocate(data.Length);
            Write(addr, data);
            return addr;
        }

        public IntPtr Allocate(int size)
        {
            IntPtr addr =
                Native.VirtualAllocEx(_handle, IntPtr.Zero, size,
                    AllocationType.MEM_COMMIT, MemoryProtection.PAGE_EXECUTE_READWRITE);
            _allocations.Add(addr, size);
            return addr;
        }

        public void Write(IntPtr addr, byte[] data)
        {
            Native.WriteProcessMemory(_handle, addr, data, data.Length, out _);
        }

        public void Dispose()
        {
            foreach (var kvp in _allocations)
                Native.VirtualFreeEx(_handle,
                    kvp.Key, kvp.Value, MemoryFreeType.MEM_DECOMMIT);
        }
    }
}
