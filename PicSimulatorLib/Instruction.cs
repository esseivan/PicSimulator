using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class Instruction
    {
        public short Value;

        public Instruction(short value)
        {
            Value = value;
        }

        public enum InstructionValues
        {
            // Byte oriented (00 xxxx dfff ffff)
            ADDWF = 0b0111,
            ANDWF = 0b0101,
            CLR = 0b0001,
            COMF = 0b1001,
            DECF = 0b0011,
            DECFSZ = 0b1011,
            INCF = 0b1010,
            INCFSZ = 0b1111,
            IORWF = 0b0100,
            MOVF = 0b1000,
            MOVWF = 0b0000,
            RLF = 0b1101,
            RRF = 0b1100,
            SUBWF = 0b0010,
            SWAPF = 0b1110,
            XORWF = 0b0110,

            // Bit-Oriented (01 xxbb bfff ffff)

            // Literal and Control (


        }

        public string GetCommandName()
        {
            // Byte oriented
            if ((Value & 0x3000) == 0x00)
            {
                short cmd = (short)(Value & 0xFFF);
                InstructionValues cmdCode = (InstructionValues)(cmd >> 8);
                byte lowerByte = (byte)(cmd & 0xFF);
                byte destCode = (byte)((cmd >> 7) & 0x01);
                byte regCode = (byte)(cmd & 0x7F);

                string output = cmdCode.ToString();

                bool f = true, d = true;

                switch (cmdCode)
                {
                    case InstructionValues.CLR:
                        d = false;
                        if (destCode == 1)
                        {
                            output = "CLRF";
                        }
                        else
                        {
                            output = "CLRW";
                            f = false;
                        }
                        break;

                    case InstructionValues.MOVWF:
                        // Further inspection
                        d = false;
                        if (destCode == 1)
                        {
                            output = cmdCode.ToString();
                        }
                        else
                        {
                            f = false;
                            if ((regCode & 0b10011111) == 0x00)
                                output = "NOP";
                            else if (regCode == 0x64)
                                output = "CLRWDT";
                            else if (regCode == 0x09)
                                output = "RETFIE";
                            else if (regCode == 0x63)
                                output = "SLEEP";
                            else
                                return Value.ToString("X4");
                        }

                        break;

                    default:
                        return Value.ToString("X4");
                }

                if (f)
                {
                    output = $"{output}\t{regCode}";
                }
                if (d)
                {
                    output = $"{output}\t{destCode}";
                }


                return output;
            }
            else if ((Value & 0x3000) == 0x1000)
            {
                return Value.ToString("X4");
            }
            else
            {
                return Value.ToString("X4");
            }
        }



    }
}
