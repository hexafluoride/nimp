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

            Console.WriteLine("opcode: 0x{0:X2}{6}, $s: {1}, $t: {2}, $d: {3}, imm: {4}, func: 0x{5:X2}{7}",
                GetOpcode(i),
                RegisterNames[GetS(i)],
                RegisterNames[GetT(i)],
                RegisterNames[GetD(i)],
                GetI(i),
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

        public static Dictionary<string, string> CommandHelp = new Dictionary<string, string>()
        {
            {"dump", "Usage: dump\n" +
                "\n" +
                "Dumps all integer registers, then displays PC and dumps the current instruction.\n"},

            {"break", "Usage: break [0x<addr> | <opcode> | <func> | clear] [quiet]\n" +
                "\n" +
                "Sets a breakpoint to match the given parameter.\n" +
                "\n" +
                "\t0xaddr\tSets a breakpoint at the address 0x<addr>.\n" + 
                "\topcode\tSets a breakpoint for the given opcode.\n" +
                "\tfunc\tSets a breakpoint for the given func.\n" +
                "\tclear\tClears all breakpoints.\n" +
                "\tquiet\tSteps quietly to next breakpoint.\n"},

            {"step", "Usage: step <count> [quiet]\n" +
                "\n" +
                "Steps forward <count> instructions.\n" +
                "\n" +
                "\tcount\tThe number of instructions to step forward.\n" + 
                "\tquiet\tSteps quietly.\n"},

            {"continue", "Usage: continue [quiet]\n" +
                "\n" +
                "Clears all breakpoints and continues execution.\n" +
                "\n" +
                "\tquiet\tIf provided, instructions will no longer be printed on each cycle."},

            {"register", "Usage: register [$<register mnemonic> | <register number>]\n" +
                "\n" +
                "Displays the contents of a register.\n" +
                "\n" +
                "\t$<register mnemonic>\tThe shorthand form of a register(i.e. $s0, $ra, $sp).\n" +
                "\t<register number>\tThe numeric form of a register.\n"},

            {"memory", "Usage: memory [<address in hex> | $<register mnemonic> | <register number>]\n" +
                "\n" +
                "Displays the word, halfword and byte in base 16 and 10 at the given address.\n" +
                "\n" +
                "\t$<register mnemonic>\tUses the address in a register provided by its shorthand(i.e. $s0, $ra, $sp).\n" +
                "\t<register number>\tUses the address in a register provided by its number.\n" +
                "\t<address in hex>\tUses the provided address.\n"},

            {"help", "Usage: help <command>\n" +
                "\n" +
                "Displays help for the given command. If no command is specified, displays this text.\n" +
                "\n" +
                "Available commands:\n"}
        };

        public static List<string> AutocompleteCommands = new List<string>()
        {
            "dump",
            "break",
            "step",
            "continue",
            "register",
            "memory",
            "help"
        };

        public static List<string> OpcodeCommands = new List<string>()
        {
            "break"
        };

        public static List<string> RegisterCommands = new List<string>()
        {
            "register",
            "memory"
        };

        public static Dictionary<string, List<string>> AdditionalComplete = new Dictionary<string, List<string>>()
        {
            {"break", new List<string>() { "clear", "quiet" } },
            {"continue", new List<string>() { "quiet" } }
        };

        public static List<string> History = new List<string>();
        public static List<string> OpcodeList = new List<string>();

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
            bool autocomplete_full = false;
            int history_index = -1;
 
            while(true)
            {
                var words = buffer.Split(' ');
                List<string> autocomplete_pool = new List<string>();

                if (words.Length == 1)
                {
                    autocomplete_pool = AutocompleteCommands.ToList();
                    autocomplete_full = false;
                }
                else if(words.Length > 1)
                {
                    if (OpcodeCommands.Contains(words[0]))
                        autocomplete_pool = OpcodeList;
                    else if (RegisterCommands.Contains(words[0]))
                        autocomplete_pool = RegisterNames.ToList();
                    else if (words[0] == "help")
                        autocomplete_pool = CommandHelp.Keys.ToList();

                    if (AdditionalComplete.ContainsKey(words[0]))
                        autocomplete_pool = autocomplete_pool.Concat(AdditionalComplete[words[0]]).ToList();

                    autocomplete_full = true;
                }

                autocomplete_word_index = words.Length - 1;

                string partial = words.Last().ToLower();

                if (autocomplete_full || !autocomplete_pool.Contains(partial)) // we don't wanna autocomplete full commands
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

                if (buffer.Length == 0)
                    autocomplete = false;

                string autocomplete_left = "";

                if(autocomplete)
                    autocomplete_left = possibilities[autocomplete_index].Substring(words.Last().Length);

                string possibility_indicator = string.Format("({0} possibilit{1})", possibilities.Count, possibilities.Count == 1 ? "y" : "ies");

                if (4 + buffer.Length + autocomplete_left.Length + 2 + possibility_indicator.Length >= Console.BufferWidth)
                    autocomplete = false;

                if (!autocomplete)
                    possibility_indicator = "";

                int cursor = index + 4;

                Console.CursorLeft = 0;
                Console.Write(">>> ");

                if(4 + buffer.Length + autocomplete_left.Length + 2 + possibility_indicator.Length >= Console.BufferWidth)
                {
                    int offset = (4 + buffer.Length + autocomplete_left.Length + 2 + possibility_indicator.Length) - Console.BufferWidth;
                    cursor -= offset;
                    int front_offset = cursor - offset - 4;
                    if (cursor - offset < 4)
                    {

                    }
                    Console.Write(buffer.Substring(offset));
                }
                else
                    Console.Write(buffer);

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(autocomplete_left);
                Console.Write(new string(' ', Console.BufferWidth - (Console.CursorLeft + 2 + possibility_indicator.Length)));
                Console.Write(possibility_indicator);
                Console.ForegroundColor = ConsoleColor.Gray;

                Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft - 1));
                Console.CursorLeft = cursor;
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
                        History.Add(buffer);
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
                        if (autocomplete)
                        {
                            autocomplete_index = (autocomplete_index - 1) < 0 ? 0 : (autocomplete_index - 1);
                        }
                        else if (History.Any())
                        {
                            if (history_index == -1)
                                history_index = History.Count - 1;
                            else
                                history_index = (history_index - 1) < 0 ? 0 : (history_index - 1);

                            buffer = History[history_index];
                            index = 0;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if(autocomplete)
                        {
                            autocomplete_index = (autocomplete_index + 1) >= possibilities.Count ? autocomplete_index : (autocomplete_index + 1);
                        }
                        else if(History.Any())
                        {
                            if (history_index == -1)
                                history_index = History.Count - 1;
                            else
                            {
                                if ((history_index + 1) >= History.Count)
                                {
                                    history_index = -1;
                                    buffer = "";
                                    index = 0;
                                    break;
                                }
                                else
                                    history_index++;
                            }

                            buffer = History[history_index];
                            index = 0;
                        }
                        break;
                    default:
                        if (KeyAliases.ContainsKey(key.Key))
                        {
                            return KeyAliases[key.Key];
                        }

                        if (key.KeyChar == '\0')
                            break;

                        if (4 + buffer.Length + autocomplete_left.Length + 2 + possibility_indicator.Length + 1 >= Console.BufferWidth)
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
