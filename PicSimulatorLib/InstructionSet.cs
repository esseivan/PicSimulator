using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class InstructionSet
    {
        private Dictionary<long, Instruction> instructions = new Dictionary<long, Instruction>();

        public Dictionary<long, Instruction> Instructions => instructions;

        public Instruction this[long i] => instructions[i];

        public InstructionSet() { }

        public InstructionSet(Dictionary<long, short> instructions) : this(HexFileDecoder.GetInstructions(instructions)) { }

        public InstructionSet(Dictionary<long, Instruction> instructions) : this()
        {
            SetInstructions(instructions);
        }

        public void SetInstructions(Dictionary<long, Instruction> instructions)
        {
            this.instructions = instructions;
            SetLabels();
            DeterminePCLATHValues();
        }

        private void SetLabels()
        {
            // First pass, indicate that a label has to be set later
            foreach (var pair in instructions)
            {
                if (pair.Value.Code == Instruction.InstructionCode.GOTO)
                {
                    // Get destination
                    short destination = pair.Value.Parameter1;
                    if (!instructions.ContainsKey(destination))
                        throw new IndexOutOfRangeException("Destination to GOTO instruction not found : '" + pair.Value.ToString() + "'");
                    Instruction instructionDestination = instructions[destination];
                    instructionDestination.SetLabelName();
                }
            }

            // Second pass, set the lower address the lower index
            Instruction.ResetLabelCounter();
            foreach (var pair in instructions)
            {
                if (pair.Value.Label == "L")
                {
                    pair.Value.SetLabelName(true);
                }
            }

            // Third pass, set the comment to the goto
            foreach (var pair in instructions)
            {
                if (pair.Value.Code == Instruction.InstructionCode.GOTO)
                {
                    // Get destination
                    short destination = pair.Value.Parameter1;
                    if (!instructions.ContainsKey(destination))
                        throw new IndexOutOfRangeException("Destination to GOTO instruction not found : '" + pair.Value.ToString() + "'");
                    Instruction instructionDestination = instructions[destination];
                    pair.Value.AddComment("== " + instructionDestination.Label);
                }
            }
        }

        public void DeterminePCLATHValues()
        {
            // First pass, indicate that a label has to be set later
            byte pcLatHVal = 0;
            foreach (var pair in instructions)
            {
                if (pair.Value.Code == Instruction.InstructionCode.MOVLP)
                {
                    pcLatHVal = (byte)pair.Value.Parameter1;
                }
                else if (pair.Value.Code == Instruction.InstructionCode.CALL
                  || pair.Value.Code == Instruction.InstructionCode.CALLW
                  || pair.Value.Code == Instruction.InstructionCode.GOTO)
                {
                    pair.Value.AddComment($"PCLATH = {pcLatHVal:X2}h");
                }
            }
        }

    }
}
