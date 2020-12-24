using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public abstract class MCU_Simulator
    {
        protected Register[] registers;
        protected byte[] program;
        protected IO[] gpios;

        private int registerPerBank = -1, bankCount = -1;

        // Pour les bank et addresses :
        /* Indiquer combien de bits utilisés (12 pour 1827)
         * Indiquer combien de bits pour le bank
         * Indiquer combien de bits pour l'adresse
         * Indiquer le nombre de banks
         * Indiquer les bytes par bank
         * 
         * Donc :
         * Indiquer nombre total data memory
         * Indiquer le nombre de bank 
         * 
         * On fait :
         * total / banks = 
         * 
         * 
         * 
         * 
         */

        private void PopulateRegisters(int bankCount, int registerPerBank)
        {
            this.bankCount = bankCount;
            this.registerPerBank = registerPerBank;

            registers = new Register[bankCount * registerPerBank];
        }
    }
}
