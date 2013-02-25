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
        Node[][] Nodes;

        ErrorHandler BC_ErrorHandler;

        private float T_Base;

        private float h_Top;

        /// <summary>
        /// Constructor for BoundaryConditions class
        /// </summary>
        /// <param name="Nodal_Array">Node array with which to apply the boundary conditions to</param>
        /// <param name="local_ErrorHandler">Errorhandler to pass messages to the Main UI</param>
        public BoundaryConditions(Node[][] Nodal_Array, ErrorHandler local_ErrorHandler, float T_Base, float h_Top)
        {
            Nodes = Nodal_Array;

            BC_ErrorHandler = local_ErrorHandler;

            Apply_Boundary_Conditions();

            this.T_Base = T_Base;
            this.h_Top = h_Top;
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
                Nodes[i][0].AN = Nodes[i][0].AN;
                Nodes[i][0].AE = 0.0f;
                Nodes[i][0].AP0 = Nodes[i][0].cp * Nodes[i][0].rho * Nodes[i][0].delta_X * Nodes[i][0].delta_Y; // NEEDS to be divided by delta T
                Nodes[i][0].d = T_Base;
                Nodes[i][0].AP = Nodes[i][0].AE + Nodes[i][0].AW + Nodes[i][0].AN + Nodes[i][0].AS;
                Nodes[i][0].phi = T_Base;
            }

            // Sets the convective boundary condition on the north face of the TEM (initially) -- later to be applied only to the bridge
            for (int i = 0; i < Nodes.Count(); i++)
            {
                int y_Max = Nodes[i].Count() - 1;

                Nodes[i][y_Max].AS = 0.0f;
                Nodes[i][y_Max].AW = 0.0f;
                Nodes[i][y_Max].AN = 0.0f;
                Nodes[i][y_Max].AP0 = Nodes[i][y_Max].cp * Nodes[i][y_Max].rho * Nodes[i][y_Max].delta_X * Nodes[i][y_Max].delta_Y; // NEEDS to be divided by delta-T
                Nodes[i][y_Max].phi = 0.0f;
                Nodes[i][y_Max].d = h_Top * Nodes[i][y_Max].delta_Y;
                Nodes[i][y_Max].AP = Nodes[i][y_Max].AE + Nodes[i][y_Max].AW + Nodes[i][y_Max].AN + Nodes[i][y_Max].AS + h_Top * Nodes[i][y_Max].delta_Y + 1.0f;
            }

            // Sets the adiabatic (q = 0) boundary condition on the West Face of the TEM
            for (int j = 0; j < Nodes[0].Count(); j++)
            {
                // Just a test to ensure no errors
                Nodes[0][j].phi = 0.0f;
               
            }


            BC_ErrorHandler.UpdateProgress(0);
            BC_ErrorHandler.UpdateProgress_Text("");
        }
    }
}
