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
    public class RegisterMapTests
    {
        [TestMethod()]
        public void Test1()
        {
            RegisterMap rm = new RegisterMap(255);
            rm.SetRegister(0x10, 0xA5);

            Assert.AreEqual(0xA5, rm.GetRegister(0x10));
        }

        [TestMethod()]
        public void Test2()
        {
            RegisterMap rm = new RegisterMap(255, new Func<uint, Register>((i) =>
                { return new Register(Register.RegisterMode.Unimplemented); }
            ));
            rm.SetRegister(0x10, 0xA5);

            Assert.AreEqual(0x00, rm.GetRegister(0x10));
        }

        [TestMethod()]
        public void Test3()
        {
            RegisterMap rm = new RegisterMap(255);
            rm.SetRegister(254, 0xA5);

            Assert.AreEqual(0xA5, rm.GetRegister(254));
        }

        [TestMethod()]
        public void Test4()
        {
            RegisterMap rm = new RegisterMap(255);

            bool hadError = false;
            try
            {
                rm.SetRegister(255, 0xA5);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                if (ex.ParamName.Equals("address"))
                    hadError = true;
            }

            Assert.IsTrue(hadError);
        }
    }
}