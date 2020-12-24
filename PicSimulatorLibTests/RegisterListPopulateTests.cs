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
    public class RegisterListPopulateTests
    {
        [TestMethod()]
        public void PopulateTest()
        {
            RegisterList rl = RegisterListPopulate.Populate(@"C:\Workspace\PicSimulator\PicSimulator\MCU_Modules\Generators\PIC16F1827.csv");

            Assert.AreEqual(4096, rl.Registers.Count);

        }
    }
}