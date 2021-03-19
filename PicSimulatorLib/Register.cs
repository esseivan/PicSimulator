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
        public byte Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public Register SyncRegister = null;
        public int SyncToAddr = SyncToAddr_None;
        public const int SyncToAddr_None = -1;

        public string Name => name;
        internal string name = string.Empty;

        private byte value = 0x00;

        public event EventHandler<RegisterValueChangedEventArgs> ValueChanged;

        public static Func<byte, Register, byte> ApplyConstraints;

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

        public void SetValue(byte value, bool checkConstraints = true)
        {
            ValueChanged?.Invoke(this, new RegisterValueChangedEventArgs(this.value, value));

            if (SyncRegister != null)
                SyncRegister.SetValue(value, checkConstraints);
            else
            {
                if (SyncToAddr != SyncToAddr_None)
                    throw new InvalidOperationException("Register not synced");

                this.value = value;

                // After a change, apply constraints
                if (checkConstraints)
                    ConstraintWork();
            }
        }

        public void ConstraintWork()
        {
            if (name == string.Empty)
                return;

            if (ApplyConstraints != null)
            {
                byte temp = ApplyConstraints(value, this);
                if (value != temp)
                    SetValue(temp);
            }
        }

        public void AddValue(byte value)
        {
            SetValue((byte)(this.value + value));
        }

        public byte GetValue()
        {
            if (SyncRegister != null)
                return SyncRegister.GetValue();
            else if (SyncToAddr != SyncToAddr_None)
            {
                throw new InvalidOperationException("Register not synced");
            }
            else
            {
                return value;
            }
        }

        public static explicit operator byte(Register t)
        {
            return t.GetValue();
        }
    }

    public class StatusRegister : Register
    {
        public virtual bool C
        {
            get => (Value & BitMask(0)) > 0;
            set => SetBit(0, value);
        }
        public virtual bool DC
        {
            get => (Value & BitMask(1)) > 0;
            set => SetBit(1, value);
        }
        public virtual bool Z
        {
            get => (Value & BitMask(2)) > 0;
            set => SetBit(2, value);
        }
        public virtual bool N_PD
        {
            get => (Value & BitMask(3)) > 0;
            set => SetBit(3, value);
        }
        public virtual bool N_TO
        {
            get => (Value & BitMask(4)) > 0;
            set => SetBit(4, value);
        }

        public byte BitMask(byte i)
        {
            return (byte)(1 << i);
        }

        public byte NBitMask(byte i)
        {
            return (byte)(0xFF - (byte)(1 << i));
        }

        public void SetBit(byte i, bool value)
        {
            if (value)
                this.Value |= BitMask(i);
            else
                this.Value &= NBitMask(i);
        }

        public static StatusRegister CopyFrom(Register reg)
        {
            StatusRegister sr = new StatusRegister()
            {
                Value = reg.Value,
                ReadableMask = reg.ReadableMask,
                SyncRegister = reg.SyncRegister,
                SyncToAddr = reg.SyncToAddr,
                UnimplementedMask = reg.UnimplementedMask,
                WritableMask = reg.WritableMask,
            };

            return sr;
        }
    }

    public class RegisterValueChangedEventArgs : EventArgs
    {
        public byte OldValue, NewValue;

        public RegisterValueChangedEventArgs(byte oldValue, byte newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
