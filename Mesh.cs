using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace _2D_Patankar_Model
{
    // Mesh.cs
    //
    // Generates Mesh (2D Array of Node Objects) based on the number of nodes in both
    // the X and Y directions.  Also generates DX and DY values based on a constant
    // grid size in both x and y directions.  Node values are layered as follows:
    // 
    // 1.  Initial grid is generated based on evenly spaced XNODES and YNODES
    //
    // 2.  The Check_Boundaries routine then iterates row by row, finding the row of XNODES
    //     that ended up closest to a critical boundary (boundary with adjacent, differing
    //     material).  This entire row of XNODES' Y_POS is then set to the critical Y_POS value.
    //
    // 3.  The process in (2) is then repeated column by column, which should result in a mostly
    //     equally spaced mesh with minor differences at the material boundaries.  
    //
    // 4.  With the nodes in the correct physical positioning, boundary rectangles are created and
    //     checked for node intersection which allows for materials to be assigned to nodes in a 
    //     physically realistic manner.  
    //
    // 
    class Mesh
    {

        
        private float dX;
        private float dY;

        // max_X - Maximum physical x location [m]
        //
        // The maximum x-direction is governed by the width of the TE module

        // max_Y - Maximum physical y location [m]
        //
        // The maximum y-direction is governed by the entire height of the microDTA
        // apparatus, from the base ceramic of the TEM to the thermocouple and beyond.
        const float max_X = 0.0401828f;
        const float max_Y = 0.004864f;

        // Node[,] is a 2D array of Node objects, which constitute the mesh of the given
        // system.
        public Node[,] Nodes;

        // Errorhandler class to handle errors with mesh calculations
        ErrorHandler Mesh_Errors;

        public Mesh(ErrorHandler local_ErrorHandler, int _XNODES, int _YNODES)
        {
            Mesh_Errors = local_ErrorHandler;
            dX = max_X / ((float)_XNODES - 1);
            dY = max_Y / ((float)_YNODES - 1);
            Mesh_Errors.Post_Error("NOTE:  DX = " + dX.ToString() + ", DY = " + dY.ToString());
            Generate_Mesh(_XNODES, _YNODES);
        }

        // XNODES - Number of nodes in the x direction
        // YNODES - Number of nodes in the y direction
        private int XNODES;
        private int YNODES;


        // Accessor function for XNODES
        public int x_Nodes
        {
            get
            {
                return XNODES;
            }
            set
            {
                if (value >= 0)
                    XNODES = value;
                else
                {
                    XNODES = 0;
                    Mesh_Errors.Post_Error("MESH ERROR:  Assignment of XNODES less than 0.");
                }
            }
        }

        // Accessor function for YNODES
        public int y_Nodes
        {
            get
            {
                return YNODES;
            }
            set
            {
                if (value >= 0)
                    YNODES = value;
                else
                {
                    YNODES = 0;
                    Mesh_Errors.Post_Error("MESH ERROR:  Assignment of YNODES less than 0.");
                }
            }
        }

        // Main subroutine which generates the mesh, given XNODES and YNODES
        private void Generate_Mesh(int XNODES, int YNODES)
        {
            float percent_Complete;

            Mesh_Errors.UpdateProgress_Text("Meshing");

            // Initializes Node array 
            Nodes = new Node[XNODES, YNODES];

            // Double-for loop which will begin at X=0, and work from Y = 1 to YNODES, moving
            // column by column until X = XNODES
            for (int i = 0; i < XNODES; i++)
            {
                for (int j = 0; j < YNODES; j++)
                {
                    float x_POS = (float)i * dX;
                    float y_POS = (float)j * dY;

                    percent_Complete = (float)i / (float)(XNODES-1) * 100;

                    Nodes[i, j] = new Node(Mesh_Errors, i, j, dX, dY, x_POS, y_POS);

                    Mesh_Errors.UpdateProgress((int)percent_Complete);

                    Mesh_Errors.Post_Error("NOTE:  Node[" + i.ToString() + "," + j.ToString() + "] created at (" + x_POS.ToString() + ", " + y_POS.ToString() + ")");
                    
                }
            }

            // Sends message to UI that the Meshing is complete
            Mesh_Errors.Post_Error("NOTE:  Meshing has completed succesfully.");
        }
    }
}
