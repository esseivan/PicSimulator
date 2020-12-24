using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulator.MCU_Modules
{
    public class MCUGenerator
    {
        public enum MCUReference
        {
            PIC16F2817,
        }

        public MCU_Simulator GenerateSimulator(MCUReference reference)
        {
            MCU_Simulator mcu = new MCU_Simulator();


        }
    }
}
