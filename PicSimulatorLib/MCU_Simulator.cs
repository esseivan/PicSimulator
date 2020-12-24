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

        private int registerPerBank = -1, bankCount = -1;

        private void PopulateRegisters(int bankCount, int registerPerBank)
        {
            this.bankCount = bankCount;
            this.registerPerBank = registerPerBank;

            registers = new Register[bankCount * registerPerBank];
        }
    }
}
