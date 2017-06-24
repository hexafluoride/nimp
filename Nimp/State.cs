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
        public uint[] Registers = new uint[32];
        public uint HI = 0;
        public uint LO = 0;

        public uint PC = 0;

        public State()
        {

        }

        public void Step()
        {
            uint instruction = Memory.ReadWord(PC);
            Utilities.DumpInstruction(instruction);
            PC += 4;
        }
    }
}
