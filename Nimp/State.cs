using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimp
{
    public class State
    {
        public Memory Memory = new Memory();
        public int[] Registers = new int[32];
        public int HI = 0;
        public int LO = 0;

        public uint PC = 0;

        private long _opcode;
        private long _func;
        private int _i;
        private long _d;
        private long _s;
        private long _t;

        public State()
        {

        }

        public void Step()
        {
            uint instruction = Memory.ReadWord(PC);

            Utilities.DumpInstruction(instruction);

            _opcode = Utilities.GetOpcode(instruction);
            _func = Utilities.GetFunc(instruction);
            _i = Utilities.GetI(instruction);
            _d = Utilities.GetD(instruction);
            _s = Utilities.GetS(instruction);
            _t = Utilities.GetT(instruction);

            switch(_opcode)
            {
                case 0x00:
                    HandleALU(instruction);
                    break;
                case 0x08:
                case 0x09: // TODO: handle addiu separately
                    Registers[_t] = (int)(_i + Registers[_s]);
                    break;
            }

            DumpRegisters();

            PC += 4;
        }

        private void DumpRegisters()
        {
            bool flag = false;

            for (int i = 0; i < 32; i++)
            {
                if (Registers[i] != 0)
                {
                    flag = true;
                    Console.Write("${0:00}: {1}{2}", i, Registers[i], i == 31 ? "" : ", ");
                }
            }

            if (flag)
                Console.WriteLine();
        }

        private void HandleALU(uint instruction)
        {
            switch (_func)
            {
                case 0x08:
                    PC = unchecked((uint)Registers[_s]);
                    break;
                case 0x10:
                    Registers[_d] = HI;
                    break;
                case 0x12:
                    Registers[_d] = LO;
                    break;
                case 0x18:
                case 0x19:
                    long mult = Registers[_s] * Registers[_t];
                    HI = unchecked((int)((mult & (long)0xFFFFFFFF00000000) >> 32));
                    LO = unchecked((int)(mult & 0x00000000FFFFFFFF));
                    break;
                case 0x1A:
                case 0x1B:
                    if (Registers[_t] != 0)
                    {
                        HI = Registers[_s] / Registers[_t];
                        LO = Registers[_s] % Registers[_t];
                    }
                    break;
                case 0x20:
                case 0x21:
                    Registers[_d] = Registers[_s] + Registers[_t];
                    break;
                case 0x24:
                    Registers[_d] = Registers[_s] & Registers[_t];
                    break;
            }
        }
    }
}
