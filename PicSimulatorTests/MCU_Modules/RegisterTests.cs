using Microsoft.VisualStudio.TestTools.UnitTesting;
using PicSimulator.MCU_Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulator.MCU_Modules.Tests
{
    [TestClass()]
    public class RegisterTests
    {
        [TestMethod()]
        public void Test1()
        {
            Register r = new Register(Register.RegisterMode.ReadOnly);
            byte value = 0xA5;

            r.SetValue(value);

            Assert.AreEqual(0x00, r.Value);
        }

        [TestMethod()]
        public void Test2()
        {
            Register r = new Register(Register.RegisterMode.ReadWrite);
            byte value = 0xA5;

            r.SetValue(value);

            Assert.AreEqual(value, r.Value);
        }

        [TestMethod()]
        public void Test3()
        {
            Register r = new Register(Register.RegisterMode.Unimplemented);
            byte value = 0xA5;

            r.SetValue(value);

            Assert.AreEqual(0x00, r.Value);
        }

        [TestMethod()]
        public void Test4()
        {
            Register r = new Register(Register.RegisterMode.ReadWrite, 0b11000011);
            byte value = 0xA5;

            r.SetValue(value);

            Assert.AreEqual(0x24, r.Value);
        }

        [TestMethod()]
        public void Test5()
        {
            Register r1 = new Register();
            Register r2 = new Register(r1);
            byte value = 0xA5;

            r2.SetValue(value);

            Assert.AreEqual(value, r1.Value);
        }
    }
}