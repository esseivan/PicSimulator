using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulator.MCU_Modules
{
    public class RegisterMap
    {
        public RegisterList registers;

        // Memory size
        private readonly int memorySize = 0;
        public int MemorySize => memorySize;

        public RegisterMap(int memorySize) : this(memorySize, DefaultFillMapFunction) { }

        public RegisterMap(int memorySize, Func<int, Dictionary<string, Register>> fillMapFunction)
        {
            if (memorySize <= 0)
                throw new ArgumentOutOfRangeException("memorySize");

            this.memorySize = memorySize;
            PopulateMap(fillMapFunction);
        }

        private bool PopulateMap(Func<int, Dictionary<string, Register>> fillMapFunction)
        {
            if (fillMapFunction == null)
                throw new ArgumentNullException("fillMapFunction");

            registers = fillMapFunction(memorySize);

            return true;
        }

        public static Dictionary<string, Register> DefaultFillMapFunction(int memorySize)
        {
            Dictionary<string, Register> regs = new Dictionary<string, Register>(memorySize);
            for (int i = 0; i < memorySize; i++)
            {
                regs.Add(string.Format("REG{0}", i), new Register());
            }
            return regs;
        }

        private bool CheckAddress(int address)
        {
            return address < memorySize;
        }

        public byte GetRegister(int address)
        {
            if (!CheckAddress(address))
                throw new ArgumentOutOfRangeException("address");

            return registers[address].GetValue();
        }

        public byte GetRegister(string registerName)
        {
            if (!registers.ContainsKey(registerName))
                throw new ArgumentOutOfRangeException("registerName");

            return registers[registerName].GetValue();
        }

        public void SetRegister(int address, byte value)
        {
            if (!CheckAddress(address))
                throw new ArgumentOutOfRangeException("address");

            registers[address].SetValue(value);
        }

        public void SetRegister(string registerName, byte value)
        {
            if (!registers.ContainsKey(registerName))
                throw new ArgumentOutOfRangeException("registerName");

            registers[registerName].SetValue(value);
        }
    }
}
