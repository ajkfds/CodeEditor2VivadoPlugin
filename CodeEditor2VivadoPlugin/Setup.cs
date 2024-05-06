using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pluginVivado
{
    public static class Setup
    {
        public static string BinPath = @"C:\Xilinx\Vivado\2023.1\bin\";
        public static string SimulationPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Simulation\xSim";
    }
}
