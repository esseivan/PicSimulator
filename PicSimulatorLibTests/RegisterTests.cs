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
    public class RegisterTests
    {
        [TestMethod()]
        public void SetValueTest1()
        {
            Register r = new Register(Register.RegisterMode.ReadOnly);
            byte value = 0xA5;

            r.SetValue(value);

            Assert.AreEqual(0x00, r.Value);
        }

        [TestMethod()]
        public void SetValueTest2()
        {
            Register r = new Register(Register.RegisterMode.ReadWrite);
            byte value = 0xA5;

            r.SetValue(value);

            Assert.AreEqual(value, r.Value);
        }

        [TestMethod()]
        public void SetValueTest3()
        {
            Register r = new Register(Register.RegisterMode.Unimplemented);
            byte value = 0xA5;

            r.SetValue(value);

            Assert.AreEqual(0x00, r.Value);
        }

        [TestMethod()]
        public void SetValueTest4()
        {
            Register r = new Register(Register.RegisterMode.ReadWrite, 0b11000111);
            byte value = 0xA5;

            r.SetValue(value);

            Assert.AreEqual(0x20, r.Value);
        }

        [TestMethod()]
        public void SyncTest1()
        {
            Register r1 = new Register();
            Register r2 = new Register(r1);
            byte value = 0xA5;

            r2.SetValue(value);

            Assert.AreEqual(value, r1.Value);
        }
    }
}