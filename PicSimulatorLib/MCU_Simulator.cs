using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public abstract class MCU_Simulator
    {
        protected Register[] registers;
        protected byte[] program;
        protected IO[] gpios;
        protected byte wreg = 0;

        protected byte addrBits => 0;
        protected byte bankBits => 0;
        protected short bankLength => 0;
        protected byte bankCount => 0;

        public int GetBank(int addr)
        {
            return (addr >> addrBits) & (int)(Math.Pow(2, bankBits) - 1);
        }

        public int GetShortAddr(int addr)
        {
            return addr & (int)(Math.Pow(2, addrBits) - 1);
        }
    }
}
