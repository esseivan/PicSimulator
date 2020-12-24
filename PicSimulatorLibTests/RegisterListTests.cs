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
    public class RegisterListTests
    {
        [TestMethod()]
        public void AddTest1()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(0, r1, "RAM1");

            Register r2 = rl[0];

            Assert.AreEqual(r1, r2);
        }

        [TestMethod()]
        public void AddTest2()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(255, r1, "RAM1");

            Register r2 = rl[255];

            Assert.AreEqual(r1, r2);
        }

        [TestMethod()]
        public void AddTest3()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(255, r1, "RAM1");
            bool c1 = false;

            try
            {
                rl.Add(255, r1, "RAM2");
            }
            catch (Exception)
            {
                c1 = true;
            }

            Assert.IsTrue(c1);
        }

        [TestMethod()]
        public void AddTest4()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(255, r1, "RAM1");
            bool c1 = false;

            try
            {
                rl.Add(24, r1, "RAM1");
            }
            catch (Exception)
            {
                c1 = true;
            }

            Assert.IsTrue(c1);
        }

        [TestMethod()]
        public void GetRegisterTest1()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(25, r1, "RAM1");

            Register r2 = rl.GetRegister(25);
            Register r3 = rl.GetRegister(24);

            Assert.AreEqual(r1, r2);
            Assert.AreEqual(null, r3);
        }

        [TestMethod()]
        public void GetRegisterTest2()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(25, r1, "RAM1");

            Register r2 = rl.GetRegister("RAM1");
            Register r3 = rl.GetRegister("RAM2");

            Assert.AreEqual(r1, r2);
            Assert.AreEqual(null, r3);
        }

        [TestMethod()]
        public void SetRegisterTest1()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(25, r1, "RAM1");
            Register r2 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0x00);

            rl.SetRegister(25, r2);
            rl.SetRegister(24, r2);

            Register r3 = rl.GetRegister(25);
            Register r4 = rl.GetRegister(24);

            Assert.AreEqual(r2, r3);
            Assert.AreEqual(r2, r4);
            Assert.AreNotEqual(r1, r2);
        }

        [TestMethod()]
        public void SetRegisterTest2()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(25, r1, "RAM1");
            Register r2 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0x00);
            bool hadError = false;

            rl.SetRegister("RAM1", r2);
            try
            {
                rl.SetRegister("RAM2", r2);
            }
            catch (Exception)
            {
                hadError = true;
            }

            Register r3 = rl.GetRegister("RAM1");
            Register r4 = rl.GetRegister("RAM2");

            Assert.AreEqual(r2, r3);
            Assert.AreEqual(null, r4);
            Assert.IsTrue(hadError);
        }

        [TestMethod()]
        public void ClearTest()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(25, r1, "RAM1");

            Assert.AreEqual(1, rl.Registers.Count);
            Assert.AreEqual(1, rl.Indexes.Count);
            Assert.AreEqual(1, rl.Names.Count);
            rl.Clear();

            Assert.AreEqual(0, rl.Registers.Count);
            Assert.AreEqual(0, rl.Indexes.Count);
            Assert.AreEqual(0, rl.Names.Count);
        }

        [TestMethod()]
        public void ContainsKeyTest1()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(25, r1, "RAM1");
            bool c1, c2;

            c1 = rl.ContainsKey(25);
            c2 = rl.ContainsKey(24);

            Assert.IsTrue(c1);
            Assert.IsFalse(c2);
        }

        [TestMethod()]
        public void ContainsKeyTest2()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(25, r1, "RAM1");
            bool c1, c2;

            c1 = rl.ContainsKey("RAM1");
            c2 = rl.ContainsKey("RAM2");

            Assert.IsTrue(c1);
            Assert.IsFalse(c2);
        }

        [TestMethod()]
        public void RemoveTest1()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(24, r1, "RAM1");
            rl.Add(25, r1, "RAM2");

            Assert.AreEqual(2, rl.Registers.Count);
            Assert.AreEqual(2, rl.Indexes.Count);
            Assert.AreEqual(2, rl.Names.Count);

            rl.Remove(24);

            Assert.AreEqual(1, rl.Registers.Count);
            Assert.AreEqual(1, rl.Indexes.Count);
            Assert.AreEqual(1, rl.Names.Count);

            Register r2 = rl.GetRegister(25);
            Register r3 = rl.GetRegister(24);

            Assert.AreEqual(r1, r2);
            Assert.AreEqual(null, r3);
        }

        [TestMethod()]
        public void RemoveTest2()
        {
            Register r1 = new Register(Register.RegisterMode.ReadWrite, 0x00, 0xff);
            RegisterList rl = new RegisterList();
            rl.Add(24, r1, "RAM1");
            rl.Add(25, r1, "RAM2");

            Assert.AreEqual(2, rl.Registers.Count);
            Assert.AreEqual(2, rl.Indexes.Count);
            Assert.AreEqual(2, rl.Names.Count);

            rl.Remove("RAM1");

            Assert.AreEqual(1, rl.Registers.Count);
            Assert.AreEqual(1, rl.Indexes.Count);
            Assert.AreEqual(1, rl.Names.Count);

            Register r2 = rl.GetRegister(25);
            Register r3 = rl.GetRegister(24);

            Assert.AreEqual(r1, r2);
            Assert.AreEqual(null, r3);
        }

        [TestMethod()]
        public void SyncTest1()
        {
            RegisterList rl = new RegisterList();
            Register r1 = new Register();
            Register r2 = new Register(10);
            byte v1 = 0xA5, v2 = 0x5A;
            rl.Add(10, r1, "RAM1");
            rl.Add(255, r2, "RAM2");
            bool c1 = false;

            r1.SetValue(v1);
            try
            {
                r2.SetValue(v2);
            }
            catch (Exception)
            {
                c1 = true;
            }

            Assert.IsTrue(c1);
            Assert.AreEqual(v1, r1.Value);

            rl.ApplySyncs();

            r1.SetValue(v2);

            Assert.AreEqual(v2, r1.Value);
        }
    }
}