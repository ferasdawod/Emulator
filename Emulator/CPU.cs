using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Emulator
{
    public class CPU
    {
        private object _lockObj = new object();
        private bool _running = false;
        private Thread _backgroundThread;
        
        #region Flags

        public bool CF
        {
            get;
            protected set;
        }

        public bool ZF
        {
            get;
            protected set;
        }

        public bool SF
        {
            get;
            protected set;
        }

        public bool PF
        {
            get;
            protected set;
        }

        #endregion

        #region Registers

        public byte AX
        {
            get;
            protected set;
        }

        public byte BX
        {
            get;
            protected set;
        }

        public byte CX
        {
            get;
            protected set;
        }

        public byte DX
        {
            get;
            protected set;
        }

        public byte PC
        {
            get;
            protected set;
        }

        public byte ACC
        {
            get;
            protected set;
        }

        #endregion

        private byte[] _romMemory = new byte[128];
        private byte[] _ramMemory = new byte[128];

        public CPU()
        {
            Reset();

            _backgroundThread = new Thread(Work);
        }

        private void Reset()
        {
            CF = SF = ZF = PF = false;
            AX = BX = CX = DX = ACC = PC = 0x0;
        }

        public void LoadRomMemory(byte[] romMemory)
        {
            if (_running)
                throw new InvalidOperationException("stop the cpu before loading the rom");

            _romMemory = romMemory;
        }

        public void Start()
        {
            if (_running)
                throw new InvalidOperationException("cpu is already running");

            _running = true;
            _backgroundThread.Start();
        }

        public void Pause()
        {
            lock (_lockObj) { _running = false; }
            _backgroundThread.Join();
        }

        public void Stop()
        {
            lock (_lockObj) { _running = false; }
            _backgroundThread.Join();

            Reset();
        }

        private void Work()
        {
            while (_running)
            {
                byte opCodeBytes = 1;
                var opCode = _romMemory[PC];

                switch (opCode)
                {
                    case 0x00:  // MOV Rn, Rn
                        {
                            var destenation = opCode & 0xC;
                            var source = opCode & 0x03;
                            var sourceVal = GetRegisterValue(source);
                            WriteToRegister(destenation, sourceVal);
                        }
                        break;

                    case 0x01:  // MOV Rn, XXXX
                        {
                            var destenation = opCode & 0xC;
                            var source = opCode & 0x3;
                            byte val = 0;
                            if (source == 0)
                            {
                                opCodeBytes = 2;
                                val = _romMemory[PC + 1];
                            }
                            else if (source == 1)
                            {
                                opCodeBytes = 2;
                                var address = _romMemory[PC + 1];
                                val = _ramMemory[address & 0x7F];
                            }
                            else if (source == 2)
                            {
                                val = ACC;
                            }
                            else if (source == 3)
                            {
                                val = PC;
                            }

                            WriteToRegister(destenation, val);
                        }
                        break;

                    case 0x02:
                        {
                            var source = opCode & 0x3;
                            var sourceVal = GetRegisterValue(source);

                            var destenation = 0xC;
                            if (destenation == 0)
                            {
                                opCodeBytes = 2;
                                var memAddress = _romMemory[PC + 1];
                                _ramMemory[memAddress & 0x7F] = sourceVal;
                            }
                            else if (destenation == 1)
                            {
                                ACC = sourceVal;
                            }
                        }
                        break;

                    case 0x03:
                        {
                            opCodeBytes = 2;
                            if ((opCode & 0x1) == 0)
                            {
                                var immVal = _romMemory[PC + 1];
                                ACC = immVal;
                            }
                            else
                            {
                                var address = _romMemory[PC + 2];
                                var memVal = _ramMemory[address & 0x7F];
                                ACC = memVal;
                            }
                        }
                        break;

                    case 0x04:
                        {
                            var source = opCode & 0x3;
                            if ((opCode & 0x4) == 0)
                            {
                                byte val = GetRegisterValue(source);
                                val++;
                                WriteToRegister(source, val);
                            }
                            else
                            {
                                byte val = GetRegisterValue(source);
                                val--;
                                WriteToRegister(source, val);
                            }
                        }
                        break;
                }

                PC += opCodeBytes;
            }
        }

        private byte GetRegisterValue(int registerIndex)
        {
            switch (registerIndex)
            {
                case 0x00:
                    return AX;
                case 0x01:
                    return BX;
                case 0x02:
                    return CX;
                case 0x03:
                    return DX;
                default:
                    return 0;
            }
        }

        private void WriteToRegister(int registerIndex, byte val)
        {
            switch (registerIndex)
            {
                case 0x00:
                    AX = val;
                    break;
                case 0x01:
                    BX = val;
                    break;
                case 0x02:
                    CX = val;
                    break;
                case 0x03:
                    DX = val;
                    break;
            }
        }
    }
}
