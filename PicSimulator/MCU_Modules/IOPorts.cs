﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulator.MCU_Modules
{
    public class IO
    {
        public Register reference = null;
        public byte bitIndex = 0;

        public IO(Register reference, byte bitIndex)
        {
            if (reference == null)
                throw new ArgumentNullException("reference");
            if (bitIndex >= 8)
                throw new ArgumentOutOfRangeException("bitIndex");
            this.reference = reference;
            this.bitIndex = bitIndex;
        }

        private byte GetBit()
        {
            return (byte)(1 << bitIndex);
        }

        public bool GetValue()
        {
            return (reference.GetValue() & GetBit()) > 0;
        }

        public void SetValue(bool val)
        {
            byte current = reference.GetValue();
            if (val)
                current |= GetBit();
            else
                current &= (byte)(0xff - GetBit());
            reference.SetValue(current);
        }
    }
}
