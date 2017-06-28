using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nimp
{
    public class Utilities
    {
        public static void DumpInstruction(uint i)
        {
            var op = (int)GetOpcode(i);
            var func = (int)GetFunc(i);

            var op_str = "";
            var func_str = "";

            if (Enum.IsDefined(typeof(Opcodes), op))
                op_str = string.Format("({0})", Enum.GetName(typeof(Opcodes), op));

            if (op == 0x00 && Enum.IsDefined(typeof(AluFuncs), func))
                func_str = string.Format("({0})", Enum.GetName(typeof(AluFuncs), func));

            Console.WriteLine("opcode: 0x{0:X2}{5}, $s: ${1:00}, $t: ${2:00}, $d: ${3:00}, func: 0x{4:X2}{6}",
                GetOpcode(i),
                GetS(i),
                GetT(i),
                GetD(i),
                GetFunc(i),
                op_str,
                func_str);
        }

        public static int ParseRegister(string reg)
        {
            reg = reg.TrimStart('$').ToLower();
            int r = 0;

            if (!int.TryParse(reg, out r) || r > 31)
            {
                if (!Utilities.RegisterNames.Contains("$" + reg))
                {
                    return -1;
                }

                r = Array.IndexOf(Utilities.RegisterNames, "$" + reg);
            }

            return r;
        }

        public static Dictionary<ConsoleKey, string> KeyAliases = new Dictionary<ConsoleKey, string>()
        {
            { ConsoleKey.F10, "step-once" },
            { ConsoleKey.F11, "step-until-broken" },
            { ConsoleKey.F12, "continue" },
            { ConsoleKey.Enter, "step-smart" }
        };

        public static List<string> AutocompleteCommands = new List<string>()
        {
            "dump",
            "break",
            "step",
            "continue",
            "register",
            "memory"
        };

        public static string SmartReadline()
        {
            var key = Console.ReadKey(true);

            if(KeyAliases.ContainsKey(key.Key))
            {
                return KeyAliases[key.Key];
            }

            string buffer = "";
            if (key.KeyChar != '\0' && key.KeyChar != '\t')
                buffer += key.KeyChar;

            int index = buffer.Length;

            bool autocomplete = false;
            List<string> possibilities = new List<string>();
            int autocomplete_index = -1;
            int autocomplete_word_index = 0;
 
            while(true)
            {
                var words = buffer.Split(' ');
                List<string> autocomplete_pool = new List<string>();

                if (words.Length == 1)
                {
                    autocomplete_pool = AutocompleteCommands.ToList();
                }
                else
                {
                    autocomplete_pool = RegisterNames.ToList();
                }

                autocomplete_word_index = words.Length - 1;

                string partial = words.Last().ToLower();

                if (!autocomplete_pool.Contains(partial)) // we don't wanna autocomplete full commands
                {
                    if (autocomplete_pool.Any(c => c.StartsWith(partial)))
                    {
                        var possibilities_temp = autocomplete_pool.Where(c => c.StartsWith(partial)).ToList();
                        if (!possibilities.SequenceEqual(possibilities_temp)) // don't reset autocomplete index
                        {
                            possibilities = possibilities_temp;
                            autocomplete_index = 0;
                        }
                        autocomplete = true;
                    }
                    else
                        autocomplete = false;
                }
                else
                    autocomplete = false;

                string autocomplete_left = "";

                if(autocomplete)
                    autocomplete_left = possibilities[autocomplete_index].Substring(words.Last().Length);

                string possibility_indicator = string.Format("({0} possibilit{1})", possibilities.Count, possibilities.Count == 1 ? "y" : "ies");
                if (!autocomplete)
                    possibility_indicator = "";

                Console.CursorLeft = 0;
                Console.Write(">>> ");
                Console.Write(buffer);

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(autocomplete_left);
                Console.Write(new string(' ', Console.BufferWidth - (Console.CursorLeft + 2 + possibility_indicator.Length)));
                Console.Write(possibility_indicator);
                Console.ForegroundColor = ConsoleColor.Gray;

                Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft - 1));
                Console.CursorLeft = index + 4;
                key = Console.ReadKey(true);

                switch(key.Key)
                {
                    case ConsoleKey.LeftArrow:
                        index = (index - 1) < 0 ? 0 : (index - 1);
                        break;
                    case ConsoleKey.RightArrow:
                        if(index == buffer.Length && autocomplete)
                        {
                            words[autocomplete_word_index] = possibilities[autocomplete_index];
                            buffer = string.Join(" ", words);
                            index = buffer.Length;
                            break;
                        }

                        index = (index + 1) > buffer.Length ? buffer.Length : (index + 1);
                        break;
                    case ConsoleKey.Tab:
                        if (autocomplete)
                        {
                            words[autocomplete_word_index] = possibilities[autocomplete_index];
                            buffer = string.Join(" ", words);
                            index = buffer.Length;
                        }
                        break;
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        return buffer;
                    case ConsoleKey.Backspace:
                        if (index == 0)
                            break;
                        if (index == buffer.Length)
                        {
                            buffer = buffer.Substring(0, buffer.Length - 1);
                        }
                        else
                        {
                            buffer = buffer.Substring(0, index - 1) + buffer.Substring(index);
                        }
                        index--;
                        break;
                    case ConsoleKey.Delete:
                        if (index == buffer.Length)
                            break;
                        if (index == 0)
                        {
                            buffer = buffer.Substring(1);
                        }
                        else
                        {
                            buffer = buffer.Substring(0, index) + buffer.Substring(index + 1);
                        }

                        break;
                    case ConsoleKey.Home:
                        index = 0;
                        break;
                    case ConsoleKey.End:
                        index = buffer.Length;
                        break;
                    case ConsoleKey.UpArrow:
                        if(autocomplete)
                        {
                            autocomplete_index = (autocomplete_index - 1) < 0 ? 0 : (autocomplete_index - 1);
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if(autocomplete)
                        {
                            autocomplete_index = (autocomplete_index + 1) >= possibilities.Count ? autocomplete_index - 1 : (autocomplete_index + 1);
                        }
                        break;
                    default:
                        if (key.KeyChar == '\0')
                            break;

                        if(index == buffer.Length)
                        {
                            buffer += key.KeyChar;
                        }
                        else
                        {
                            buffer = buffer.Substring(0, index) + key.KeyChar + buffer.Substring(index);
                        }
                        index++;
                        break;
                }
            }
        }

        public static string[] RegisterNames = new string[]
        {
            "$00",
            "$at",
            "$v0",
            "$v1",
            "$a0",
            "$a1",
            "$a2",
            "$a3",
            "$t0",
            "$t1",
            "$t2",
            "$t3",
            "$t4",
            "$t5",
            "$t6",
            "$t7",
            "$s0",
            "$s1",
            "$s2",
            "$s3",
            "$s4",
            "$s5",
            "$s6",
            "$s7",
            "$t8",
            "$t9",
            "$k0",
            "$k1",
            "$gp",
            "$sp",
            "$fp",
            "$ra"
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // just to be safe
        public static long GetOpcode(uint word)
        {
            return (word & (0x3F << 26)) >> 26;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetS(uint word)
        {
            return (word & (0x1F << 21)) >> 21;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetT(uint word)
        {
            return (word & (0x1F << 16)) >> 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetD(uint word)
        {
            return (word & (0x1F << 11)) >> 11;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetShift(uint word)
        {
            return (int)((word & (0x1F << 6)) >> 6);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetI(uint word)
        {
            return (int)(word & (0xFFFF));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetFunc(uint word)
        {
            return word & (0x3F);
        }
    }
}
