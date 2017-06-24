using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private int _shift;
        private uint _jumped = 4;

        public ulong Count = 0;

        public State()
        {

        }

        public void Loop()
        {
            while(true)
            {
                Step();
                Count++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Step()
        {
            Decode();
            Execute();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Decode()
        {
            uint word = Memory.ReadWord(PC);
            _opcode = (word & (0x3F << 26)) >> 26;
            _s = (word & (0x1F << 21)) >> 21;
            _t = (word & (0x1F << 16)) >> 16;
            _d = (word & (0x1F << 11)) >> 11;
            _shift = (int)((word & (0x1F << 6)) >> 6);
            _i = (int)(word & (0xFFFF));
            _func = word & (0x3F);

            //Utilities.DumpInstruction(word);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute()
        {
            switch (_opcode)
            {
                case 0x00:
                    HandleALU();
                    break;
                case 0x08:
                case 0x09: // TODO: handle addiu separately
                    Registers[_t] = (_i + Registers[_s]);
                    break;
            }

            //DumpRegisters();

            PC += _jumped;
            _jumped = 4;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleALU()
        {
            switch ((AluFuncs)_func)
            {
                #region addition, subtraction
                case AluFuncs.ADD:
                case AluFuncs.ADDU:
                    Registers[_d] = Registers[_s] + Registers[_t];
                    break;
                case AluFuncs.SUB:
                case AluFuncs.SUBU:
                    Registers[_d] = Registers[_s] - Registers[_t];
                    break;
                #endregion

                #region division, multiplication
                case AluFuncs.DIV:
                case AluFuncs.DIVU:
                    if (Registers[_t] != 0)
                    {
                        HI = Registers[_s] / Registers[_t];
                        LO = Registers[_s] % Registers[_t];
                    }
                    break;
                case AluFuncs.MULT:
                case AluFuncs.MULTU:
                    long mult = Registers[_s] * Registers[_t];
                    HI = unchecked((int)((mult & (long)0xFFFFFFFF00000000) >> 32));
                    LO = unchecked((int)(mult & 0x00000000FFFFFFFF));
                    break;
                #endregion

                #region bitwise
                case AluFuncs.AND:
                    Registers[_d] = Registers[_s] & Registers[_t];
                    break;
                case AluFuncs.OR:
                    Registers[_d] = Registers[_s] | Registers[_t];
                    break;
                case AluFuncs.XOR:
                    Registers[_d] = Registers[_s] ^ Registers[_t];
                    break;
                case AluFuncs.NOR:
                    Registers[_d] = ~(Registers[_s] | Registers[_t]);
                    break;
                #endregion

                #region jump
                case AluFuncs.JR:
                    PC = unchecked((uint)Registers[_s]);
                    _jumped = 0;
                    break;
                #endregion

                #region mfhi, mflo
                case AluFuncs.MFHI:
                    Registers[_d] = HI;
                    break;
                case AluFuncs.MFLO:
                    Registers[_d] = LO;
                    break;
                #endregion

                #region shifts
                case AluFuncs.SLL:
                    Registers[_d] = Registers[_t] << _shift;
                    break;
                case AluFuncs.SLLV:
                    Registers[_d] = Registers[_t] << Registers[_s];
                    break;
                case AluFuncs.SRA:
                    Registers[_d] = Registers[_t] >> _shift;
                    break;
                case AluFuncs.SRAV:
                    Registers[_d] = Registers[_t] >> Registers[_s];
                    break;
                case AluFuncs.SRL:
                    Registers[_d] = unchecked((int)((uint)Registers[_t] >> _shift));
                    break;
                case AluFuncs.SRLV:
                    Registers[_d] = unchecked((int)((uint)Registers[_t] >> Registers[_s]));
                    break;
                #endregion

                #region conditionals
                case AluFuncs.SLT:
                    Registers[_d] = (Registers[_s] < Registers[_t]) ? 1 : 0;
                    break;
                case AluFuncs.SLTU:
                    Registers[_d] = unchecked((uint)Registers[_s] < (uint)Registers[_t]) ? 1 : 0;
                    break;
                #endregion
            }
        }
    }

    public enum AluFuncs
    {
        ADD = 0x20,
        ADDU = 0x21,
        SUB = 0x22,
        SUBU = 0x23,

        AND = 0x24,
        NOR = 0x27,
        XOR = 0x26,
        OR = 0x25,

        MULT = 0x18,
        MULTU = 0x19,
        DIV = 0x1A,
        DIVU = 0x1B,

        MFHI = 0x10,
        MFLO = 0x12,

        JR = 0x08,

        SLT = 0x2A,
        SLTU = 0x2B,

        SLL = 0x00,
        SLLV = 0x04,
        SRL = 0x02,
        SRLV = 0x06,
        SRA = 0x03,
        SRAV = 0x07,
    }
}
