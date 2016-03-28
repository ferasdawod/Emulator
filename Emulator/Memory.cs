using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator
{
    public class CPUMemory
    {
        private byte[] _memory = new byte[256];

        public CPUMemory(byte[] romMemory)
        {
            if (romMemory == null)
                throw new ArgumentNullException(nameof(romMemory));

            if (romMemory.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(romMemory));

            Array.Clear(_memory, 0, _memory.Length);
            Array.Copy(romMemory, _memory, romMemory.Length);
        }

        public void Reset(bool clearRom = false)
        {
            if (clearRom)
                Array.Clear(_memory, 0, _memory.Length);
            else
                Array.Clear(_memory, 128, 128);
        }

        public byte Read(byte position)
        {
            if (position < 0 || position >= _memory.Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            return _memory[position];
        }

        public void Write(byte position, byte val)
        {
            if (position < 128 || position >= _memory.Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            _memory[position] = val;
        }

        public void ForceWrite(byte position, byte val)
        {
            if (position < 0 || position > _memory.Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            _memory[position] = val;
        }
    }
}
