using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private byte _callRegister = 0;

        public CPU()
        {
            Reset();

            _backgroundThread = new Thread(Work);
        }

        private void Reset()
        {
            CF = SF = ZF = PF = false;
            AX = BX = CX = DX = ACC = PC = 0x0;

            for (int i = 0; i < _ramMemory.Length; i++)
            {
                _ramMemory[i] = 0;
            }
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
                var instruction = _romMemory[PC];
                PC++;
                var opCode = (instruction & 0xF0) >> 4;

                switch (opCode)
                {
                    case 0x00:  // MOV Rn, Rn
                        {
                            var destenation = instruction & 0xC;
                            var source = instruction & 0x03;
                            var sourceVal = GetRegisterValue(source);
                            WriteToRegister(destenation, sourceVal);
                        }
                        break;

                    case 0x01:  // MOV Rn, XXXX
                        {
                            var destenation = instruction & 0xC;
                            var source = instruction & 0x3;
                            byte val = 0;
                            if (source == 0)
                            {
                                val = _romMemory[PC];
                                PC++;
                            }
                            else if (source == 1)
                            {
                                var address = _romMemory[PC];
                                PC++;
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

                    case 0x02:  // MOV XXXX, Rn
                        {
                            var source = instruction & 0x3;
                            var sourceVal = GetRegisterValue(source);

                            var destenation = 0xC;
                            if (destenation == 0)
                            {
                                var memAddress = _romMemory[PC];
                                PC++;
                                _ramMemory[memAddress & 0x7F] = sourceVal;
                            }
                            else if (destenation == 1)
                            {
                                ACC = sourceVal;
                            }
                        }
                        break;

                    case 0x03:  // MOV ACC, XXXX
                        {
                            if ((instruction & 0x1) == 0)
                            {
                                var immVal = _romMemory[PC];
                                PC++;
                                ACC = immVal;
                            }
                            else
                            {
                                var address = _romMemory[PC];
                                PC++;
                                var memVal = _ramMemory[address & 0x7F];
                                ACC = memVal;
                            }
                        }
                        break;

                    case 0x04:  // INC/DEC Rn
                        {
                            var source = instruction & 0x3;
                            if ((instruction & 0x4) == 0)
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

                    case 0x05:  // JXX [MEM]
                        {
                            var flag = instruction & 0x3;
                            var flagValue = GetFlagValue(flag);

                            var withFlag = (instruction & 0x4) != 0;
                            var directJmp = (instruction & 0x8) != 0;

                            var shouldJump = !(flagValue ^ withFlag) ^ directJmp;

                            if (shouldJump)
                            {
                                var jmpAddress = _romMemory[PC];
                                PC = jmpAddress;
                            }
                        }
                        break;

                    case 0x06:  // INC/DEC ACC
                        {
                            var inc = (instruction & 0x1) != 0;
                            if (inc)
                                ACC++;
                            else
                                ACC--;
                        }
                        break;

                    case 0x07:  // CLEAR ACC
                        {
                            ACC = 0;
                        }
                        break;

                    case 0x08:  // LOGIC OP ACC, Rn
                        {

                        }
                        break;

                    case 0x09:  // ARITH OP ACC, Rn
                        {
                        }
                        break;

                    case 0x0A:  // LOGIC OP ACC, XXXX
                        {
                        }
                        break;

                    case 0x0B:  // ARITH OP ACC, XXXX
                        {
                        }
                        break;

                    case 0x0C:  // CALL/RET
                        {
                            var isCall = (instruction & 0x1) != 0;
                            if (isCall)
                            {
                                var memAddress = _romMemory[PC];
                                _callRegister = PC;
                                PC = memAddress;
                            }
                        }
                        break;

                    case 0x0D:  // XCHNG ACC, Rn
                        {
                            var register = instruction & 0x3;
                            var registerValue = GetRegisterValue(register);
                            WriteToRegister(register, ACC);
                            ACC = registerValue;
                        }
                        break;

                    case 0x0E:  // SET/CLEAR FLAG
                        {
                        }
                        break;

                    case 0x0F:  // RESET
                        {
                            Reset();
                        }
                        break;

                    default:
                        {
                            Debug.WriteLine("UnImplemented Instruction " + Convert.ToString(opCode, 16));
                        }
                        break;
                }
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

        private bool GetFlagValue(int flagIndex)
        {
            switch (flagIndex)
            {
                case 0:
                    return CF;
                case 1:
                    return SF;
                case 2:
                    return ZF;
                case 3:
                    return PF;
                default:
                    return false;
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
