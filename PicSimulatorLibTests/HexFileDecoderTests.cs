﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class HexFileDecoderTests
    {
        private void WriteTestFile()
        {
            string fileData = @":020000040000FA
:040000008031132810
:100008007E1480318B1D09280A2811280B1C0D2805
:100018000E2811288131352180317E100900803168
:100028001528FC012000A201A3017E1020008031C8
:100038007128003022000D180130F60076080319E7
:100048002628292822000D142B2822000D100A30FA
:100058008031FF2080317108F8007008F7007808B7
:10006800F5007708F400F435F50DF435F50DF435A1
:10007800F50D7408F7007508F80022000C158C15AA
:100088000E307802103003197702031C4C284D28D3
:100098004F280C1167280C307802E43003197702D6
:1000A800031C572858285B280C118C1167280B3023
:1000B8007802B83003197702031C6328642866287D
:1000C8008C1167288D108130890B6828760C2200E6
:1000D800031C0D1003180D140800813116218031FE
:1000E80022008D100D101530F9002030890B7A2868
:1000F800F90B7A2822008D140D14CA30F000003054
:10010800F1008131462180318B170B1780311D207A
:100118008031FA01FB010B30F9000F30890B92286E
:10012800F90B922800000130FA070030FB3D00303F
:100138007B02783003197A02031CA328A4288F288D
:100148008A28803113280C3022008C0003308D005F
:10015800F33021008C00F4308D00043023008D0032
:100168008C0124008D018C012100951722009D012E
:100178009E0127009612951294166030F000013007
:10018800F1008131462180318B15080020000C1CBC
:10019800CE28CF28D3288031E7208031E6288C1F4D
:1001A800D628D728D9280000E6280C1FDC28DD2807
:1001B800DE28D7288C1CE128E228E62881315A213C
:1001C8008031E6280800031022008D1D031403184F
:1001D800EE28F12822008D15F32822008D11031036
:1001E8008D1D03140318F928FC2822000D14FE287D
:1001F80022000D100800F3007308F200F20DF20D52
:1002080021001D087206833972069D001D14210005
:100218009D149D18102911290D291C08F1001B088F
:10022800F00008008031A720813181314C218131D3
:1002380081315221813181315621813181313E21F3
:100248008131080020002008210403192C292D29B8
:10025800322921088A0020080A00813127009612D5
:1002680008002700961E39293A293D29813126217F
:1002780081310800833021009E009B019C010130E0
:100288009D00080071082000A1007008A000080067
:1002980021009901980122009601080016302100DA
:1002A80097000800823022009700080022008D1174
:0A02B8000D1408007834003408002B
:020000040001F9
:04000E00E43FFF3E8E
:00000001FF";
            File.WriteAllText("test.hex", fileData);
        }

        [TestMethod()]
        public void DecodeTest()
        {
            var data = HexFileDecoder.Decode("test.hex");
            var dataInstructions = HexFileDecoder.GetInstructions(data);
            var dataString = HexFileDecoder.GetNames(dataInstructions);

            Console.WriteLine(string.Join("\n", dataString));
        }
    }
}