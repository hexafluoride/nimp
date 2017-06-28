﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nimp
{
    public static class State
    {
        public static int[] Registers = new int[32];
        public static int HI = 0;
        public static int LO = 0;

        public static uint PC = 0x400000;

        public static uint _instruction;
        public static long _opcode;
        public static long _func;
        public static int _i;
        public static long _d;
        public static long _s;
        public static long _t;
        public static int _shift;
        public static uint _jumped = 4;
        public static Stopwatch sw;

        public static bool running = true;
        public static bool step = true;

        public static ulong Count = 0;

        public static void Loop()
        {
            running = true;

            // mock memory layout
            Registers[29] = unchecked((int)Memory.StackStart);
            Registers[28] = 0x10010000;

            sw = Stopwatch.StartNew();

            while(running)
            {
                Step();
                Count++;
#if STEP
                if (step)
                {
                    step = char.ToLower(Console.ReadKey(true).KeyChar) != 'c';
                }
#endif
            }

            sw.Stop();
            Console.WriteLine("Executed {0} instructions in {1} milliseconds({2:0.00} MIPS)", Count, sw.ElapsedMilliseconds, (Count / (sw.ElapsedMilliseconds / 1000d)) / 1000000d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Step()
        {
            Decode();
            Execute();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Decode()
        {
            uint word = Memory.ReadWord(PC);
            _instruction = word;
            _opcode = (word & (0x3F << 26)) >> 26;
            _s = (word & (0x1F << 21)) >> 21;
            _t = (word & (0x1F << 16)) >> 16;
            _d = (word & (0x1F << 11)) >> 11;
            _shift = (int)((word & (0x1F << 6)) >> 6);
            _i = (int)(word & (0xFFFF));
            _func = word & (0x3F);

#if STEP
            Console.Write("[{0:X8}] ", PC);
            Utilities.DumpInstruction(word);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Execute()
        {
            Registers[0] = 0;
            uint i;

            switch ((Opcodes)_opcode)
            {
                case Opcodes.ALU:
                    HandleALU();
                    break;

                #region addition
                case Opcodes.ADDI:
                case Opcodes.ADDIU: // TODO: handle addiu separately
                    Registers[_t] = (unchecked((short)_i) + Registers[_s]);
                    break;
                #endregion

                #region bitwise
                case Opcodes.ANDI:
                    Registers[_t] = (_i & Registers[_s]);
                    break;
                case Opcodes.ORI:
                    Registers[_t] = (_i | Registers[_s]);
                    break;
                case Opcodes.XORI:
                    Registers[_t] = (_i ^ Registers[_s]);
                    break;
                #endregion

                #region jump
                case Opcodes.JAL:
                    Registers[31] = unchecked((int)PC + 4);
                    i = (_instruction & 0x3FFFFFF) << 2;
                    PC = (PC & 0xf0000000) | i;
                    _jumped = 0;
                    break;
                case Opcodes.J:
                    i = (_instruction & 0x3FFFFFF) << 2;
                    PC = (PC & 0xf0000000) | i;
                    _jumped = 0;
                    break;
                #endregion

                #region loads
                case Opcodes.LB:
                    Registers[_t] = unchecked((int)(Memory.ReadByte((uint)_i)));

                    if ((Registers[_t] & 0x80) == 0x80)
                        Registers[_t] = unchecked((int)(0xffffff80 | (uint)Registers[_t]));
                    break;
                case Opcodes.LBU:
                    Registers[_t] = Memory.ReadByte(unchecked((uint)_i));
                    break;
                case Opcodes.LH:
                    Registers[_t] = unchecked((int)(Memory.ReadWord((uint)_i + 2) & 0xFFFF));

                    if ((Registers[_t] & 0x8000) == 0x8000)
                        Registers[_t] = unchecked((int)(0xffff8000 | (uint)Registers[_t]));
                    break;
                case Opcodes.LWR:
                case Opcodes.LHU:
                    Registers[_t] = unchecked((int)(Memory.ReadWord((uint)_i) & 0xFFFF));
                    break;
                case Opcodes.LWL:
                    Registers[_t] = unchecked((int)(Memory.ReadWord((uint)_i) & 0xFFFF0000));
                    break;
                case Opcodes.LUI:
                    Registers[_t] = unchecked((int)(((uint)_i) << 16));
                    break;
                case Opcodes.LW:
                    Registers[_t] = unchecked((int)Memory.ReadWord((uint)(Registers[_s] + _i)));
                    break;
                #endregion

                #region stores
                case Opcodes.SB:
                    Memory.WriteByte(unchecked((uint)_i), unchecked((byte)(Registers[_t] & 0xFF)));
                    break;
                case Opcodes.SH:
                    Memory.WriteByte(unchecked((uint)_i), unchecked((byte)((Registers[_t] & 0xFF00) >> 8)));
                    Memory.WriteByte(unchecked((uint)_i + 1), unchecked((byte)(Registers[_t] & 0xFF)));
                    break;
                case Opcodes.SW:
                    Memory.WriteWord(unchecked((uint)Registers[_t]), unchecked((uint)(_i + Registers[_s])));
                    break;
                #endregion

                #region comparison
                case Opcodes.SLTI:
                    Registers[_t] = Registers[_s] < unchecked((short)_i) ? 1 : 0;
                    break;
                case Opcodes.SLTIU:
                    Registers[_t] = unchecked((uint)Registers[_s] < (ushort)_i) ? 1 : 0; // SPIM seems to also treat $s as unsigned
                                                                                         // don't know if that's correct
                    break;
                #endregion

                #region branch
                case Opcodes.BNE: // TODO: check out delayed branch/load
                    if (Registers[_t] != Registers[_s])
                    {
                        unchecked { PC = (uint)(PC + ((short)_i) * 4); }
                        _jumped = 0;
                    }
                    break;
                case Opcodes.BEQ:
                    if (Registers[_t] == Registers[_s])
                    {
                        unchecked { PC = (uint)(PC + ((short)_i) * 4); }
                        _jumped = 0;
                    }
                    break;
                case Opcodes.BGEZ:
                    if (_t == 11 || _t == 16)
                    {
                        Registers[31] = unchecked((int)PC + 4);
                    }
                    if (_t == 1 || _t == 11)
                    {
                        if (Registers[_s] >= 0)
                        {
                            unchecked { PC = (uint)(PC + ((short)_i) * 4); }
                            _jumped = 0;
                        }
                    }
                    if (_t == 0 || _t == 16)
                    {
                        if (Registers[_s] < 0)
                        {
                            unchecked { PC = (uint)(PC + ((short)_i) * 4); }
                            _jumped = 0;
                        }
                    }
                    break;
                case Opcodes.BGTZ:
                    if (Registers[_s] > 0)
                    {
                        unchecked { PC = (uint)(PC + ((short)_i) * 4); }
                        _jumped = 0;
                    }
                    break;
                case Opcodes.BLEZ:
                    if (Registers[_s] <= 0)
                    {
                        unchecked { PC = (uint)(PC + ((short)_i) * 4); }
                        _jumped = 0;
                    }
                    break;
                #endregion

                default:
                    Console.Write("Unrecognized instruction: ");
                    Utilities.DumpInstruction(_instruction);
                    break;
            }

            unchecked
            {
                PC += _jumped;
                _jumped = 4;
            }
        }

        public static void DumpRegisters()
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
        public static void HandleALU()
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
                case AluFuncs.JALR:
                    Registers[31] = unchecked((int)PC + 4);
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

                case AluFuncs.SYSCALL: // SPIM-like syscall facilities
                    switch(Registers[2])
                    {
                        case 1:
                            Console.Write(Registers[4]);
                            break;
                        case 4:
                            uint p = unchecked((uint)Registers[4]);
                            var page = Memory.GetPage(p);
                            uint location = p & 0xFFF;

                            for (; page[location] > 0 && location < page.Length; location++)
                            {
                                Console.Write((char)page[location]);
                            }
                            break;
                        case 10:
                            Console.WriteLine("Exit SYSCALL");
                            running = false;
                            break;
                        case 11:
                            Console.Write((char)Registers[4]);
                            break;
                    }
                    break;

                default:
                    Console.WriteLine("Unrecognized instruction: ");
                    Utilities.DumpInstruction(_instruction);
                    break;
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

        JALR = 0x09,
        JR = 0x08,

        SLT = 0x2A,
        SLTU = 0x2B,

        SLL = 0x00,
        SLLV = 0x04,
        SRL = 0x02,
        SRLV = 0x06,
        SRA = 0x03,
        SRAV = 0x07,

        SYSCALL = 0x0C
    }

    public enum Opcodes
    {
        ALU = 0x00,
        BLTZ = 0x01,
        J = 0x02,
        JAL = 0x03,
        BEQ = 0x04,
        BNE = 0x05,
        BLEZ = 0x06,
        BGEZ = 0x01,
        BGTZ = 0x07,
        ADDI = 0x08,
        ADDIU = 0x09,
        SLTI = 0x0A,
        SLTIU = 0x0B,
        ANDI = 0x0C,
        ORI = 0x0D,
        XORI = 0x0E,
        LUI = 0x0F,
        MFC0 = 0x10,
        LB = 0x20,
        LH = 0x21,
        LW = 0x23,
        LWL = 0x22,
        LWR = 0x26,
        LBU = 0x24,
        LHU = 0x25,
        SB = 0x28,
        SH = 0x29,
        SW = 0x2B
    }
}
