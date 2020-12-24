using Microsoft.VisualStudio.TestTools.UnitTesting;
using PicSimulatorLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib.Tests
{
    [TestClass()]
    public class GeneratorTests
    {
        [TestMethod()]
        public void Generate_PIC16F1827_Test()
        {
            MCU_Simulator mcu = Generator.GenerateSimulator(Generator.MCUReference.PIC16F1827);

            Assert.IsTrue(mcu.GetType().IsEquivalentTo(typeof(MCU_PIC16F1827)));
        }
    }
}