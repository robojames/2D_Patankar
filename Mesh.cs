using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Patankar_Model
{
    // Mesh.cs
    //
    // Generates Mesh (2D Array of Node Objects) based on the number of nodes in both
    // the X and Y directions.  Also generates DX and DY values based on a constant
    // grid size in both x and y directions.  Node values are layered as follows:
    // 
    // 1.  Initial grid is generated based on XNODES and YNODES
    //
    // 2.  Boundary locations are checked to ensure that nodes are placed on material 
    //     boundaries.  If there aren't any nodes placed on the material boundaries a
    //     second group of nodes is inserted in to the final node array which corrects
    //     this.
    class Mesh
    {

        ErrorHandler Mesh_Errors;

        public Mesh(ErrorHandler local_ErrorHandler)
        {
            Mesh_Errors = local_ErrorHandler;
            Generate_Mesh();
        }


        private int XNODES;
        private int YNODES;



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
                    XNODES = 0;
            }
        }

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
                    YNODES = 0;
            }
        }


        private void Generate_Mesh()
        {
        }
    }
}
