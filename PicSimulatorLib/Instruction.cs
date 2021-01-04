using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class Instruction : INotifyPropertyChanged
    {
        public const byte InstructionCodeBits = 14;

        private short value;
        private InstructionCode code;
        private short p1, p2, p = 0;

        public long Address { get; set; }

        public short Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                code = DecodeInstruction();
                GetParameters();
            }
        }

        public InstructionCode Code { get => code; }
        public short Parameter1 { get => p1; }
        public short Parameter2 { get => p2; }

        private bool hasComment = false;
        public string Comment { get; set; } = string.Empty;
        public void AddComment(string comment)
        {
            this.Comment += $"{(hasComment ? " ; " : "")}{comment}";
            hasComment = true;
        }

        public string Label { get; set; }

        private static int labelCtr = 1;
        public string SetLabelName(bool index = false)
        {
            Label = "L";
            if (index)
                Label += (labelCtr++).ToString();
            return Label;
        }
        public static void ResetLabelCounter() => labelCtr = 1;

        private bool isNext = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsNext
        {
            get => isNext;
            set
            {
                isNext = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsNext"));
            }
        }

        public Instruction() { }

        public Instruction(short value)
        {
            Value = value;
        }

        public Instruction(short value, long address) : this(value)
        {
            this.Address = address;
        }

        public enum InstructionCode
        {
            // Byte oriented file register operation (xx xxxx dfff ffff)
            ADDWF = 0b00011100000000,
            ADDWFC = 0b11110100000000,
            ANDWF = 0b00010100000000,
            ASRF = 0b11011100000000,
            LSLF = 0b11010100000000,
            LSRF = 0b11011000000000,
            CLRF = 0b00000110000000,
            CLRW = 0b00000100000000,
            COMF = 0b00100100000000,
            DECF = 0b00001100000000,
            INCF = 0b00101000000000,
            IORWF = 0b00010000000000,
            MOVF = 0b00100000000000,
            MOVWF = 0b00000010000000,
            RLF = 0b00110100000000,
            RRF = 0b00110000000000,
            SUBWF = 0b00001000000000,
            SUBWFB = 0b11101100000000,
            SWAPF = 0b00111000000000,
            XORWF = 0b00011000000000,

            // Byte oriented skip operation (xx xxxx dfff ffff)
            DECFSZ = 0b00101100000000,
            INCFSZ = 0b00111100000000,

            // Bit oriented file register operation (xx xxbb bfff ffff)
            BCF = 0b01000000000000,
            BSF = 0b01010000000000,

            // Bit oriented skip operation (xx xxbb bfff ffff)
            BTFSC = 0b01100000000000,
            BTFSS = 0b01110000000000,

            // Literal operations (xx xxxx kkkk kkkk)
            ADDLW = 0b11111000000000,
            ANDLW = 0b11100100000000,
            IORLW = 0b11100000000000,
            MOVLB = 0b00000000100000,
            MOVLP = 0b11000110000000,
            MOVLW = 0b11000000000000,
            SUBLW = 0b11110000000000,
            XORLW = 0b11101000000000,

            // Control operations (xx xkkk kkkk kkkk)
            BRA = 0b11001000000000,
            BRW = 0b00000000001011,
            CALL = 0b10000000000000,
            CALLW = 0b00000000001010,
            GOTO = 0b10100000000000,
            RETFIE = 0b00000000001001,
            RETLW = 0b11010000000000,
            RETURN = 0b00000000001000,

            // Inherent operations (xx xxxx xxxx xfff)
            CLRWDT = 0b00000001100100,
            NOP = 0b00000000000000,
            OPTION = 0b00000001100010,
            RESET = 0b00000000000001,
            SLEEP = 0b00000001100011,
            TRIS = 0b00000001100000,

            // C-Compiler optimized
            ADDFSR = 0b11000100000000,
            MOVIW = 0b00000000010000,
            MOVIW2 = 0b11111100000000,
            MOVWI = 0b00000000011000,
            MOVWI2 = 0b11111110000000,
        }

        public InstructionCode DecodeInstruction()
        {
            short temp = Value;
            short mask = 0;
            byte c = 0;
            while (!Enum.IsDefined(typeof(InstructionCode), (InstructionCode)temp))
            {
                mask <<= 1; mask |= 1;
                if (++c >= InstructionCodeBits)
                    return (InstructionCode)Value;
                temp |= mask; temp -= mask;
            }
            return (InstructionCode)temp;
        }

        public void GetParameters()
        {
            switch (code)
            {
                case InstructionCode.ADDWF:
                case InstructionCode.ADDWFC:
                case InstructionCode.ANDWF:
                case InstructionCode.ASRF:
                case InstructionCode.LSLF:
                case InstructionCode.LSRF:
                case InstructionCode.COMF:
                case InstructionCode.DECF:
                case InstructionCode.INCF:
                case InstructionCode.IORWF:
                case InstructionCode.MOVF:
                case InstructionCode.RLF:
                case InstructionCode.RRF:
                case InstructionCode.SUBWF:
                case InstructionCode.SUBWFB:
                case InstructionCode.SWAPF:
                case InstructionCode.XORWF:
                case InstructionCode.DECFSZ:
                case InstructionCode.INCFSZ:
                    p = 2;
                    p1 = GetBits(0, 7);
                    p2 = GetBits(7, 1);
                    break;

                case InstructionCode.CLRF:
                case InstructionCode.MOVWF:
                    p = 1;
                    p1 = GetBits(0, 7);
                    break;

                case InstructionCode.BCF:
                case InstructionCode.BSF:
                case InstructionCode.BTFSC:
                case InstructionCode.BTFSS:
                    p = 2;
                    p1 = GetBits(0, 7);
                    p2 = GetBits(7, 3);
                    break;

                case InstructionCode.ADDLW:
                case InstructionCode.ANDLW:
                case InstructionCode.IORLW:
                case InstructionCode.MOVLW:
                case InstructionCode.SUBLW:
                case InstructionCode.XORLW:
                case InstructionCode.RETLW:
                    p = 1;
                    p1 = GetBits(0, 8);
                    break;

                case InstructionCode.MOVLB:
                    p = 1;
                    p1 = GetBits(0, 5);
                    break;

                case InstructionCode.MOVLP:
                    p = 1;
                    p1 = GetBits(0, 7);
                    break;

                case InstructionCode.BRA:
                    p = 1;
                    p1 = GetBits(0, 9);
                    break;

                case InstructionCode.CALL:
                case InstructionCode.GOTO:
                    p = 1;
                    p1 = GetBits(0, 11);
                    break;

                case InstructionCode.TRIS:
                    p = 1;
                    p1 = GetBits(0, 3);
                    break;

                case InstructionCode.ADDFSR:
                case InstructionCode.MOVIW2:
                case InstructionCode.MOVWI2:
                    p = 2;
                    p1 = GetBits(0, 6);
                    p2 = GetBits(6, 1);
                    break;

                case InstructionCode.MOVIW:
                case InstructionCode.MOVWI:
                    p = 2;
                    p1 = GetBits(0, 2);
                    p2 = GetBits(2, 1);
                    break;

                default:
                    p = 0;
                    break;
            }
        }

        public short GetKValue(byte bits)
        {
            if (bits > 16)
                throw new ArgumentOutOfRangeException("bits");
            return GetBits(0, bits);
        }

        public short GetBits(byte from, byte count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException("count");

            if (from < 0)
                throw new ArgumentOutOfRangeException("from");

            return (short)((value >> from) & ((int)Math.Pow(2, count) - 1));
        }

        public override string ToString()
        {
            string output = $"{code,6}";
            if (p > 0)
                output += $"    {p1}";
            if (p > 1)
                output += $",{p2}";

            return output;
        }
    }
}
