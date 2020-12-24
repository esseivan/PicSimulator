using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class Generator
    {
        public enum MCUReference
        {
            PIC16F1827,
            PIC16F18334,
        }

        public static MCU_Simulator GenerateSimulator(MCUReference reference)
        {
            Type mcuType;
            try
            {
                string typeName = $"PicSimulatorLib.MCU_{reference}";
                mcuType = Type.GetType(typeName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (!mcuType.IsSubclassOf(typeof(MCU_Simulator)))
                throw new ArgumentException("Type not supported");

            var constructor = mcuType.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
                throw new ArgumentException("Type not supported");

            MCU_Simulator mcu = constructor.Invoke(null) as MCU_Simulator;

            return mcu;
        }
    }
}
