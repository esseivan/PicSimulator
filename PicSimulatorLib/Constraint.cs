using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class Constraint
    {
        public Func<object, byte> action;

        public string output;
        public string[] inputs;

        public Constraint(Func<object, byte> action, string output, string[] inputs)
        {
            this.action = action;
            this.output = output;
            this.inputs = inputs;
        }
    }
}
