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

		public InstructionCode Code;
		public byte Param1 = 0;
		public byte Param2 = 0;

		public Instruction(short value)
		{
			Value = value;
		}

		public enum InstructionCode
		{
			NOP = 0,

			// Byte oriented (00 xxxx dfff ffff)
			// MASK 0x3F00
			CLRW = 0b00000100000000,
			SUBWF = 0b00001000000000,
			DECF = 0b00001100000000,
			IORWF = 0b00010000000000,
			ANDWF = 0b00010100000000,
			XORWF = 0b00011000000000,
			ADDWF = 0b00011100000000,
			MOVF = 0b00100000000000,
			COMF = 0b00100100000000,
			INCF = 0b00101000000000,
			DECFSZ = 0b00101100000000,
			RRF = 0b00110000000000,
			RLF = 0b00110100000000,
			SWAPF = 0b00111000000000,
			INCFSZ = 0b00111100000000,
			// MASK 0x3F80
			CLRF = 0b00000100000000,
			MOVWF = 0b00000010000000,

			// Bit-Oriented (01 xxbb bfff ffff)

			// Literal and Control (


		}

		public string GetCommandName()
		{
			// <n1:2> <n2:4> <n3:4> <n4:4>
			short temp = Value;
			byte n4 = (byte)(temp & 0xFF); temp >>= 4;
			byte n3 = (byte)(temp & 0xFF); temp >>= 4;
			byte n2 = (byte)(temp & 0xFF); temp >>= 4;
			byte n1 = (byte)(temp & 0xFF);

			byte d = (byte)((n3 >> 3) & 0b1);
			byte f = (byte)(Value & 0x7F);
			byte b = (byte)((Value >> 7) & 0b111);

			if (n1 == 0x00)
			{



			}

			// Byte oriented
			if ((Value & 0x3000) == 0x00)
			{
				short cmd = (short)(Value & 0xFFF);
				InstructionCode cmdCode = (InstructionCode)(cmd >> 8);
				byte lowerByte = (byte)(cmd & 0xFF);
				byte destCode = (byte)((cmd >> 7) & 0x01);
				byte regCode = (byte)(cmd & 0x7F);

				string output = cmdCode.ToString();

				bool f = true, d = true;

				switch (cmdCode)
				{
					case InstructionCode.CLR:
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

					case InstructionCode.MOVWF:
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
								return "Unknown : " + Value.ToString("X4");
						}

						break;

					default:
						return "0x" + Value.ToString("X4");
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
				return "0x" + Value.ToString("X4");
			}
			else
			{
				return "0x" + Value.ToString("X4");
			}
		}

		public short GetKValue(byte bits)
		{
			if (bits > 16)
				throw new ArgumentOutOfRangeException("bits");
			return (byte)(Value & (byte)(Math.Pow(2, bits) - 1));
		}


	}
}
