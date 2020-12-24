using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    /// <summary>
    /// Register
    /// </summary>
    public class Register
    {
        public byte UnimplementedMask = 0x00;
        public byte UnimplementedMaskInvert => (byte)(0xff - UnimplementedMask);
        public byte ReadableMask = 0xff;
        public byte WritableMask = 0xff;
        public byte Value => GetValue();

        public Register SyncRegister = null;
        public int SyncToAddr = SyncToAddr_None;
        public const int SyncToAddr_None = -1;

        private byte value = 0x00;

        public Register() { }

        public Register(byte unimplementedMask)
        {
            this.UnimplementedMask = unimplementedMask;
        }

        public Register(byte unimplementedMask, byte initialValue) : this(unimplementedMask)
        {
            this.value = initialValue;
        }

        public Register(Register syncRegister)
        {
            this.SyncRegister = syncRegister;
        }

        public Register(int syncAddr)
        {
            this.SyncToAddr = syncAddr;
        }

        public void SetValue(byte value)
        {
            if (SyncRegister != null)
                SyncRegister.SetValue(value);
            else if (SyncToAddr != SyncToAddr_None)
            {
                throw new InvalidOperationException("Register not synced. Use RegisterList.ApplySyncs() method");
            }
            else
            {
                value &= WritableMask;
                value &= UnimplementedMaskInvert;
                this.value = value;
            }
        }

        public byte GetValue()
        {
            if (SyncRegister != null)
                return SyncRegister.GetValue();
            else if (SyncToAddr != SyncToAddr_None)
            {
                throw new InvalidOperationException("Register not synced. Use RegisterList.ApplySyncs() method");
            }
            else
            {
                byte result = (byte)(value & ReadableMask);
                result &= UnimplementedMaskInvert;
                return result;
            }
        }

        public static explicit operator byte(Register t)
        {
            return t.GetValue();
        }
    }
}
