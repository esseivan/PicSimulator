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

    }
}
