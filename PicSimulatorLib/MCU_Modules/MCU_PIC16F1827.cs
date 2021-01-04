using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class MCU_PIC16F1827 : MCU
    {
        private const int regBSR = 0x08,
            regStatus = 0x03,
            regWREG = 0x09,
            regPCL = 0x02,
            regPCLATH = 0x0A,
            regStackPtr = 0xFED;
        public override byte Bank => data[regBSR].Value;

        private byte pcH = 0;
        private byte _pcl
        {
            get => data[regPCL].Value;
            set => data[regPCL].Value = value;
        }
        private byte pcL
        {
            get => data[regPCL].Value;
            set
            {
                data[regPCL].Value = value;
                pcH = (byte)(data[regPCLATH].Value & 0x7F);
            }
        }
        public override short PC
        {
            get
            {
                return (short)((pcH << 8) + pcL);
            }
            set
            {
                data[regPCLATH].Value = (byte)(value >> 8);
                pcL = (byte)(value & 0xFF);
            }
        }
        public override void IncrementPC()
        {
            if (++_pcl == 0)
                pcH++;
        }

        public override byte wreg
        {
            get => data[regWREG].Value;
            set => data[regWREG].Value = value;
        }

        public override string MCU_Name { get => "pic16f1827"; }

        protected override byte StackPtr
        {
            get => data[regStackPtr].Value;
            set => data[regStackPtr].Value = value;
        }

        public MCU_PIC16F1827()
        {
            settings = new MCUSettings()
            {
                addrBits = 7,
                bankBits = 5,
                bankLength = 128,
                bankCount = 32,
                resetVector = 0,
                interruptVector = 4,
                dataMemoryCount = 4096,
                programMemoryCount = 4096,
                configurationMemoryCount = 10,
                stackCount = 16,
            };
            data = new MemoryMap(Settings);
            stack = new short[settings.stackCount];
            configuration = new short[settings.configurationMemoryCount];
            configuration[6] = 0b10100010000000; // Device ID

            DefineRegistersNameFromMicrochipFile(MCU_Name);
        }

        protected override Dictionary<long, Instruction> _decode(Dictionary<long, Instruction> data)
        {
            // from 8000h, data is configuration memory
            var configMemory = data.Where((x) => x.Key >= 0x8000);

            foreach (var item in configMemory)
            {
                int index = (int)(item.Key - 0x8000);
                if (index < 0 || index >= 10) continue;
                configuration[index] = item.Value.Value;
            }

            var output = data.Where((x) => x.Key < 0x8000).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (program == null)
                program = new InstructionSet(output);
            else
                program.SetInstructions(output);

            return output;
        }

        public override void Call(short addr)
        {
            throw new NotImplementedException();
        }

        public override void DelegateInstruction(Instruction ins)
        {
            byte f = (byte)ins.Parameter1;
            short k_short = ins.Parameter1;
            byte dTemp = (byte)ins.Parameter2;
            byte b = (byte)ins.Parameter2;
            byte value = 0, value2 = 0, value3 = 0;
            bool carry;
            byte k = f;
            if (dTemp < 0 || dTemp > 1)
                throw new ArgumentOutOfRangeException("d");
            bool d = dTemp == 1;
            bool incrementPC = true;

            switch (ins.Code)
            {
                case Instruction.InstructionCode.MOVLB:
                    data[regBSR].SetValue(k);
                    break;

                case Instruction.InstructionCode.MOVLP:
                    data[regPCLATH].SetValue(k);
                    break;

                case Instruction.InstructionCode.CALL:
                    incrementPC = false;
                    PushStack();
                    _pcl = (byte)(k_short & 0xFF);
                    pcH = (byte)(((k_short >> 8) & 0x07) + (data[regPCLATH].Value & 0x78));
                    break;

                case Instruction.InstructionCode.CALLW:
                    incrementPC = false;
                    PushStack();
                    pcL = wreg; // writing to pcl automatically writes pclath to pch
                    break;

                case Instruction.InstructionCode.GOTO:
                    incrementPC = false;
                    _pcl = (byte)(k_short & 0xFF);
                    pcH = (byte)(((k_short >> 8) & 0x07) + (data[regPCLATH].Value & 0x78));
                    break;

                case Instruction.InstructionCode.RETFIE:
                    PC = PopStack();
                    // GIE = 1
                    break;

                case Instruction.InstructionCode.RETLW:
                    wreg = k;
                    PC = PopStack();
                    break;

                case Instruction.InstructionCode.RETURN:
                    PC = PopStack();
                    break;

                default:
                    break;
            }

            if (incrementPC)
                IncrementPC();
        }

        public override void SetStatusRegister()
        {
            status = StatusRegister.CopyFrom(data[regStatus]);
        }
    }
}
