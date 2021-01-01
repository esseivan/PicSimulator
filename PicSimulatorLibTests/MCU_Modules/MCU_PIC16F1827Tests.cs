using Microsoft.VisualStudio.TestTools.UnitTesting;
using PicSimulatorLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib.Tests
{
    [TestClass()]
    public class MCU_PIC16F1827Tests
    {
        private void WriteTestFile(string path)
        {
            string fileData = @":020000040000FA
:08000000803102288731FD2F39
:100F6A001630210097000800583021009900980196
:100F7A002200960108008731CA2787318731B927AD
:100F8A0087318731B5278731080022008C018D010E
:100F9A00FE3021008C00FF308D00FE3023008D00D2
:100FAA001F308C0024008D018C012100951722002E
:100FBA009D019E0108008731C02787310310220056
:100FCA000C1C03140318EA2FED2F22000C14EF2F28
:100FDA0022000C100230F1004530F000AA30890BD3
:100FEA00F42FF00BF42FF10BF42FE32F80310228AA
:060FFA0020008731E02F0A
:020000040001F9
:04000E00E43FFF3E8E
:00000001FF";
            File.WriteAllText(path, fileData);
        }

        [TestMethod()]
        public void FullTest()
        {
            string file = "test.hex";
            if (!File.Exists(file) || true)
                WriteTestFile(file);

            MCU_PIC16F1827 mcu = new MCU_PIC16F1827();
            mcu.Decode(file);

            Console.WriteLine(string.Join("\n", mcu.Program.Instructions));
        }
    }
}