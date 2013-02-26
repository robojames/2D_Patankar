using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace _2D_Patankar_Model
{
    class BoundaryConditions
    {
        /// <summary>
        /// Node array containing the entire mesh
        /// </summary>
        Node[][] Nodes;

        /// <summary>
        /// Errorhandler to pass messages to the main UI
        /// </summary>
        ErrorHandler BC_ErrorHandler;

        private float T_Base;

        private float h_Top;

        private float T_inf;

        /// <summary>
        /// Constructor for BoundaryConditions class
        /// </summary>
        /// <param name="Nodal_Array">Node array with which to apply the boundary conditions to</param>
        /// <param name="local_ErrorHandler">Errorhandler to pass messages to the Main UI</param>
        /// <param name="T_Base">Constant temperature of the south face of the TEM in Kelvin</param>
        /// <param name="h_Top">Heat transfer coefficient (initial) operating off of the top surface of the TEM</param>
        /// <param name="T_inf">Ambient temperature utilized for the convective coefficient off of the top surface</param>
        public BoundaryConditions(Node[][] Nodal_Array, ErrorHandler local_ErrorHandler, float T_Base, float h_Top, float T_inf)
        {
            Nodes = Nodal_Array;

            BC_ErrorHandler = local_ErrorHandler;

            Apply_Boundary_Conditions();

            this.T_Base = T_Base;
            this.h_Top = h_Top;
            this.T_inf = T_inf;
        }

        public void Apply_Boundary_Conditions()
        {
            BC_ErrorHandler.UpdateProgress(0);

            BC_ErrorHandler.UpdateProgress_Text("Applying Boundary Conditions...");

            BC_ErrorHandler.Post_Error("NOTE:  Harmonic means need to be checked and their order of precedence, as well as dt needs to be added in ");
            // Sets the constant temperature boundary condition on the South Face of the TEM
            for (int i = 0; i < Nodes.Count(); i++)
            {
                Nodes[i][0].AS = 0.0f;
                Nodes[i][0].AW = 0.0f;
                Nodes[i][0].AN = 0.0f;
                Nodes[i][0].AE = 0.0f;
                Nodes[i][0].AP0 = Nodes[i][0].cp * Nodes[i][0].rho * Nodes[i][0].delta_X * Nodes[i][0].delta_Y; // NEEDS to be divided by delta T
                Nodes[i][0].d = T_Base + Nodes[i][0].sc * Nodes[i][0].delta_X * Nodes[i][0].delta_Y * 0.5f * T_Base;
                Nodes[i][0].AP = Nodes[i][0].AE + Nodes[i][0].AW + Nodes[i][0].AN + Nodes[i][0].AS + 1;
                Nodes[i][0].phi = T_Base;
            }

            // Sets the convective boundary condition on the north face of the TEM (initially) -- later to be applied only to the bridge
            for (int i = 0; i < Nodes.Count(); i++)
            {
                int y_Max = Nodes[i].Count() - 1;

                Nodes[i][y_Max].AS = Nodes[i][y_Max].AS; // This might require a manual re-calculation
                Nodes[i][y_Max].AW = 0.0f;
                Nodes[i][y_Max].AN = 0.0f;
                Nodes[i][y_Max].AE = 0.0f;
                Nodes[i][y_Max].AP0 = Nodes[i][y_Max].cp * Nodes[i][y_Max].rho * Nodes[i][y_Max].delta_X * Nodes[i][y_Max].delta_Y; // NEEDS to be divided by delta-T
                Nodes[i][y_Max].phi = 0.0f;
                Nodes[i][y_Max].d = h_Top * Nodes[i][y_Max].delta_Y * T_inf;
                Nodes[i][y_Max].AP = Nodes[i][y_Max].AE + Nodes[i][y_Max].AW + Nodes[i][y_Max].AN + Nodes[i][y_Max].AS + h_Top * Nodes[i][y_Max].delta_Y;
            }

            // Sets the adiabatic (q = 0) boundary condition on the West Face of the TEM
            for (int j = 0; j < Nodes[0].Count(); j++)
            {
                int x_Min = Nodes[0].Count() - 1;

                Nodes[0][j].AS = 0.0f;
                Nodes[0][j].AW = 0.0f;
                Nodes[0][j].AN = 0.0f;
                Nodes[0][j].AE = Nodes[0][j].AE;
                Nodes[0][j].AP0 = Nodes[0][j].rho * Nodes[0][j].cp * Nodes[0][j].delta_X * Nodes[0][j].delta_Y; // Needs to be divided by dt
                Nodes[0][j].AP = Nodes[0][j].AE + Nodes[0][j].AW + Nodes[0][j].AN + Nodes[0][j].AS;
                Nodes[0][j].d = 0.0f;
                // Just a test to ensure no errors
                Nodes[0][j].phi = 0.0f;              
            }

            // Sets the adiabatic (q=0) boundary condition on the East Face of the TEM
            for (int j = 0; j < Nodes[Nodes.Count() - 1].Count(); j++)
            {
                int x_Max = Nodes.Count() - 1;

                Nodes[x_Max][j].AS = 0.0f;
                Nodes[x_Max][j].AN = 0.0f;
                Nodes[x_Max][j].AW = Nodes[x_Max][j].AW;
                Nodes[x_Max][j].AE = 0.0f;
                Nodes[x_Max][j].AP0 = Nodes[x_Max][j].rho * Nodes[x_Max][j].cp * Nodes[x_Max][j].delta_X * Nodes[x_Max][j].delta_Y; // Needs to be divided by dt
                Nodes[x_Max][j].AP = Nodes[x_Max][j].AE + Nodes[x_Max][j].AW + Nodes[x_Max][j].AN + Nodes[x_Max][j].AS;
                Nodes[x_Max][j].d = 0.0f;

                // Not sure if this is accurate to set phi for this boundary to 0.
                Nodes[x_Max][j].phi = 0.0f;
            }


            BC_ErrorHandler.UpdateProgress(0);
            BC_ErrorHandler.UpdateProgress_Text("");
        }
    }
}
