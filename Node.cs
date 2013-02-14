using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Patankar_Model
{
    // Node Class:  the entire numerical area is comprised of a 2D array of these node (nodal) objects
    //              
    //
    class Node
    {
        // Each instance of the Node Object has its own ErrorHandler with which to feed errors onto the main UI
        ErrorHandler NodeErrors;

        public int Node_ID;

        // Default constructor of Node object 
        // 
        // Requires passing in of the local error handler so messages can be passed into the main UI, in addition
        // to the specification of CV width in both X and Y directions, as well as the physical position on both
        // the x and y axis.  The node indices (i,j) are also specified.
        public Node(ErrorHandler local_ErrorHandler, int _i, int _j, float dX, float dY, float x_POS, float y_POS, int ID)
        {
            NodeErrors = local_ErrorHandler;
            
            i = _i;
            j = _j;

            delta_X = dX;
            delta_Y = dY;

            x_pos = x_POS;
            y_pos = y_POS;

            Node_ID = ID;

            is_Sorted = false;
        }

        // Node index
        // i - index in the x-direction
        // j - index in the y-direction
        private int I;
        private int J;

        public bool is_Sorted;

        // Accessor function for the x-direction
        public int i
        {
            get
            {
                return I;
            }
            private set
            {
                if (value >= 0)
                    I = value;
                else
                {
                    I = 0;
                    NodeErrors.Post_Error("NODE ERROR:  Attempt to set node index i to less than 0");
                }

            }
        }

        // Accessor function for the j-direction
        public int j
        {
            get
            {
                return J;
            }
            private set
            {
                if (value >= 0)
                    J = value;
                else
                {
                    J = 0;
                    NodeErrors.Post_Error("NODE ERROR:  Attempt to set node index j to less than 0");
                }

            }
        }

        // Node Physical Index
        // XPOS - Physical location in the x direction [m]
        // YPOS - Physical location in the y direction [m]
        private float XPOS;
        private float YPOS;

        // Accessor function for the physical x direction
        public float x_pos
        {
            get
            {
                return XPOS;
            }
            private set
            {
                if (value >= 0)
                    XPOS = value;
                else
                {
                    XPOS = 0;
                    NodeErrors.Post_Error("NODE ERROR:  Physical x location attempted set to negative value");
                }
            }
        }

        // Accessor function for the physical y direction
        public float y_pos
        {
            get
            {
                return YPOS;
            }
            private set
            {
                if (value >= 0)
                    YPOS = value;
                else
                {
                    YPOS = 0;
                    NodeErrors.Post_Error("NODE ERROR:  Physical y location attempted set to negative value");
                }
            }
        }
        
        // Control Volume Width both in the x (DX) and y (DY) directions.  Where the total control volume is DX*DY [m^2]
        // DX - Control volume width in x-direction
        // DY - Control volume width in y-direction
        private float DX;
        private float DY;

        // Accessor function for the CV width in the x direction
        public float delta_X
        {
            get
            {
                return DX;
            }
            private set
            {
                if (value >= 0)
                    DX = value;
                else
                {
                    DX = 0;
                    NodeErrors.Post_Error("NODE ERROR:  DX attempted to set to 0 or less");
                }
            }
        }

        // Accessor function for the CV width in the y direction
        public float delta_Y
        {
            get
            {
                return DY;
            }
            private set
            {
                if (value >= 0)
                    DY = value;
                else
                {
                    DY = 0;
                    NodeErrors.Post_Error("NODE ERROR:  DY attempted to set to 0 or less");
                }
            }
        }

        // Temperature at this node  [K]
        //
        // TEMP - Temperature at this node
        private float TEMP;
        public float T
        {
            get
            {
                return TEMP;
            }
            set
            {
                if (value >= 0)
                    TEMP = value;
                else
                {
                    TEMP = 0;
                    NodeErrors.Post_Error("NODE ERROR:  Phi (T) < or equal to zero at Node (" + I.ToString() + ", " + J.ToString() + ")");
                }
            }
        }

        // Gamma (Diffusion) Coefficient at this node [unsure]
        private float GAMMA;
        public float gamma
        {
            get
            {
                return GAMMA;
            }
            set
            {
                if (value >= 0)
                {
                    GAMMA = value;
                }
                else
                {
                    GAMMA = 0;
                    NodeErrors.Post_Error("NODE ERROR:  Gamma at Node (" + I.ToString() + ", " + J.ToString() + ") attempted to be set to < 0.");
                }
            }
        }

        public float AE { get; set; }
        public float AP { get; set; }
        public float AN { get; set; }
        public float AS { get; set; }
        public float AW { get; set; }

        public float phi { get; set; }
        public float P { get; set; }
        public float Q { get; set; }
        public float sc { get; set; }
        public float sp { get; set; }
        public float d { get; set; }
    }
}
