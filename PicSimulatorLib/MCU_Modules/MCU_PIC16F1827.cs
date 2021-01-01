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
        public override string mcuName => "pic16f1827";
        public override short ResetVector => 0;
        public override short InterruptVector => 4;
        public override byte addrBits => 7;
        public override byte bankBits => 5;
        public override short bankLength => 128;
        public override byte bankCount => 32;
        public override short[] UserID { get => configurationMemory.Take(4).ToArray(); }
        public override short[] ConfigurationWords { get => configurationMemory.Skip(7).Take(2).ToArray(); }
        public override short DeviceID { get => configurationMemory.Skip(6).Take(1).First(); }
        public override byte SelectedBank { get => data[8].Value; set => data[8].Value = value; }
        public override byte WReg { get => data[9].Value; set => data[9].Value = value; }
        public override byte PCL { get => data[2].Value; set => data[2].Value = value; }
        public override byte PCLATH { get => data[10].Value; set => data[10].Value = value; }

        public MCU_PIC16F1827()
        {
            data = new Register[4096];
            gpios = new IO[16];
            stack = new short[16];
            configurationMemory = new short[10];
            configurationMemory[6] = 0b10100010000000;

            Populate();
            DefineRegistersNameFromMicrochipFile(mcuName);
        }

        public void Populate()
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Register();
            }
        }

        protected override Dictionary<long, Instruction> _decode(Dictionary<long, Instruction> data)
        {
            // from 8000h, data is configuration memory
            var configMemory = data.Where((x) => x.Key >= 0x8000);

            foreach (var item in configMemory)
            {
                int index = (int)(item.Key - 0x8000);
                if (index < 0 || index >= 10) continue;
                configurationMemory[index] = item.Value.Value;
            }

            var output = data.Where((x) => x.Key < 0x8000).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            program.SetInstructions(output);

            return output;
        }
    }
}
