using System;
using System.Collections.Generic;

namespace SharpMonoInjector
{
    public class Assembler
    {
        private readonly List<byte> _asm = new List<byte>();

        public void MovRax(IntPtr arg)
        {
            _asm.AddRange(new byte[] {0x48, 0xB8});
            _asm.AddRange(BitConverter.GetBytes((long)arg));
        }

        public void MovRcx(IntPtr arg)
        {
            _asm.AddRange(new byte[] {0x48, 0xB9});
            _asm.AddRange(BitConverter.GetBytes((long)arg));
        }

        public void MovRdx(IntPtr arg)
        {
            _asm.AddRange(new byte[] {0x48, 0xBA});
            _asm.AddRange(BitConverter.GetBytes((long)arg));
        }

        public void MovR8(IntPtr arg)
        {
            _asm.AddRange(new byte[] {0x49, 0xB8});
            _asm.AddRange(BitConverter.GetBytes((long)arg));
        }

        public void MovR9(IntPtr arg)
        {
            _asm.AddRange(new byte[] {0x49, 0xB9});
            _asm.AddRange(BitConverter.GetBytes((long)arg));
        }

        public void SubRsp(byte arg)
        {
            _asm.AddRange(new byte[] {0x48, 0x83, 0xEC});
            _asm.Add(arg);
        }

        public void CallRax()
        {
            _asm.AddRange(new byte[] {0xFF, 0xD0});
        }

        public void AddRsp(byte arg)
        {
            _asm.AddRange(new byte[] {0x48, 0x83, 0xC4});
            _asm.Add(arg);
        }

        public void Push(IntPtr arg)
        {
            _asm.Add((int)arg < 128 ? (byte)0x6A : (byte)0x68);
            _asm.AddRange((int)arg <= 255 ? new[] {(byte)arg} : BitConverter.GetBytes((int)arg));
        }

        public void MovEax(IntPtr arg)
        {
            _asm.Add(0xB8);
            _asm.AddRange(BitConverter.GetBytes((int)arg));
        }

        public void CallEax()
        {
            _asm.AddRange(new byte[] {0xFF, 0xD0});
        }

        public void AddEsp(byte arg)
        {
            _asm.AddRange(new byte[] {0x83, 0xC4});
            _asm.Add(arg);
        }

        public void Return()
        {
            _asm.Add(0xC3);
        }

        public byte[] ToByteArray() => _asm.ToArray();
    }
}
