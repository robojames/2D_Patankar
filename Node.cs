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
        /// <summary>
        /// Passes error messages to the Main UI
        /// </summary>
        ErrorHandler NodeErrors;

       
        /// <summary>
        /// Integer to hold the value of the node ID which is unique per each node.  This allows for greater clarity when reporting node errors
        /// </summary>
        public int Node_ID;

        /// <summary>
        /// Holds value of Layer_ID to which the node belongs.  This allows for easy assignment of material properties, etc.
        /// </summary>
        public int Layer_ID;

        /// <summary>
        /// Holds boolean value to describe if the current node is a boundary node or not
        /// </summary>
        public bool is_Boundary = false;

        /// <summary>
        /// Material string that indicates the material of this node for later conductivity retrieval
        /// </summary>
        public string Material;

        // Default constructor of Node object 
        // 
        // Requires passing in of the local error handler so messages can be passed into the main UI, in addition
        // to the specification of CV width in both X and Y directions, as well as the physical position on both
        // the x and y axis.  The node indices (i,j) are also specified.
        /// <summary>
        /// Node Constructor 
        /// </summary>
        /// <param name="local_ErrorHandler">ErrorHandler to pass messages to the main UI</param>
        /// <param name="x_POS">Physical location in the x-direction [m]</param>
        /// <param name="y_POS">Physical location in the y-direction [m]</param>
        /// <param name="p_Node_ID">Node ID (starts at 0 to N_Nodes)</param>
        /// <param name="p_Layer_ID">Layer at which this created node resides</param>
        public Node(ErrorHandler local_ErrorHandler, float x_POS, float y_POS, int p_Node_ID, int p_Layer_ID)
        {
            NodeErrors = local_ErrorHandler;
 
            x_pos = x_POS;
            y_pos = y_POS;

            Node_ID = p_Node_ID;

            Layer_ID = p_Layer_ID;

        }


        // Node Physical Index
        // XPOS - Physical location in the x direction [m]
        // YPOS - Physical location in the y direction [m]
        private float XPOS;
        private float YPOS;

        // Accessor function for the physical x direction
        /// <summary>
        /// X Position in meters on the physical plane
        /// </summary>
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
                    NodeErrors.Post_Error("NODE ERROR:  Physical x location attempted set to negative value - Node" + this.Node_ID.ToString());
                }
            }
        }

        // Accessor function for the physical y direction
        /// <summary>
        /// Y Position in meters on the physical plane
        /// </summary>
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
                    NodeErrors.Post_Error("NODE ERROR:  Physical y location attempted set to negative value - Node" + this.Node_ID.ToString());
                }
            }
        }
        
        // Control Volume Width both in the x (DX) and y (DY) directions.  Where the total control volume is DX*DY [m^2]
        // DX - Control volume width in x-direction
        // DY - Control volume width in y-direction
        private float DX;
        private float DY;

        // Accessor function for the CV width in the x direction
        /// <summary>
        /// Sets or gets the control volume width in the x-direction
        /// </summary>
        public float delta_X
        {
            get
            {
                return DX;
            }
            set
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
        /// <summary>
        /// Sets or gets the control volume width in the y-direction
        /// </summary>
        public float delta_Y
        {
            get
            {
                return DY;
            }
            set
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

        /// <summary>
        /// Temperature at this node
        /// </summary>
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
                    NodeErrors.Post_Error("NODE ERROR:  Phi (T) < or equal to zero - Node" + this.Node_ID.ToString());
                }
            }
        }

        // Gamma (Diffusion) Coefficient at this node [unsure]
        private float GAMMA;

        // Accessor Function for Gamma
        /// <summary>
        /// Diffusion Coefficient for this node
        /// </summary>
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
                    NodeErrors.Post_Error("NODE ERROR:  Gamma attempted to be set to < 0 - Node" + this.Node_ID.ToString());
                }
            }
        }

        // Variable indicated delta_x_E, delta_y_N, delta_x_W, and delta_y_S
        // which indicate the distance between this node and the neighboring node
        // in the indicated direction:
        //
        // dx_E - distance between this node and the one to the EAST
        // dx_W - distance between this node and the node to the WEST
        // dy_N - distance between this node and the node to the NORTH
        // dy_S - distance between this node and the node to the SOUTH
        //
        // These values are assigned via the accessor functions by the NodeInitializer.cs
        // class.  If no node exists in the indicated directions above the current node
        // is assumed to be a boundary node and treated as such.
        private float dx_E;
        private float dx_W;
        private float dy_N;
        private float dy_S;

        // Accessor function for dx_E
        /// <summary>
        /// Distance between this node and the node to the east
        /// </summary>
        public float delta_x_E
        {
            get
            {
                return dx_E;
            }
            set
            {
                if (value >= 0)
                {
                    dx_E = value;
                }
                else
                {
                    dx_E = 0;
                    NodeErrors.Post_Error("NODE ERROR:  Value for delta_x_E attempted to be set less than 0 - " + Node_ID.ToString());
                }
            }
        }

        // Accessor function for dx_W
        /// <summary>
        /// Distance between this node and the node to the west
        /// </summary>
        public float delta_x_W
        {
            get
            {
                return dx_W;
            }
            set
            {
                if (value >= 0)
                {
                    dx_W = value;
                }
                else
                {
                    dx_W = 0;
                    NodeErrors.Post_Error("NODE ERROR:  Value for delta_x_W attempted to be set less than 0 - " + Node_ID.ToString());
                }
            }
        }

        // Accessor function for dy_N
        /// <summary>
        /// Distance between this node and the node to the north
        /// </summary>
        public float delta_y_N
        {
            get
            {
                return dy_N;
            }
            set
            {
                if (value >= 0)
                {
                    dy_N = value;
                }
                else
                {
                    dy_N = 0;
                    NodeErrors.Post_Error("NODE ERROR:  Value for delta_y_N attempted to be set less than 0 - " + Node_ID.ToString());
                }
            }
        }

        // Accessor function for dy_S
        /// <summary>
        /// Distance between this node and the node to the south
        /// </summary>
        public float delta_y_S
        {
            get
            {
                return dy_S;
            }
            set
            {
                if (value >= 0)
                {
                    dy_S = value;
                }
                else
                {
                    dy_S = 0;
                    NodeErrors.Post_Error("NODE ERROR:  Value for delta_y_S attempted to be set less than 0 - " + Node_ID.ToString());
                }
            }
        }

        /// <summary>
        /// Index in the x direction for this particular node
        /// </summary>
        public int i { get; set; }

        /// <summary>
        /// Indext in the y direction for this particular node
        /// </summary>
        public int j { get; set; }

        /// <summary>
        /// Influence coefficient of the node EAST of this node
        /// </summary>
        public float AE { get; set; }

        /// <summary>
        /// Influence coefficient of this node
        /// </summary>
        public float AP { get; set; }

        /// <summary>
        /// Influence coefficient of the node to the NORTH of this node
        /// </summary>
        public float AN { get; set; }

        /// <summary>
        /// Influence coefficient of the node to the SOUTH of this node
        /// </summary>
        public float AS { get; set; }

        /// <summary>
        /// Influence coefficient of the node to the WEST of this node
        /// </summary>
        public float AW { get; set; }

        /// <summary>
        /// Phi (Temperature) for this node.  Currently duplicated via T [NEED TO FIX]
        /// </summary>
        public float phi { get; set; }

        /// <summary>
        /// Solution value, P
        /// </summary>
        public float P { get; set; }

        /// <summary>
        /// Solution value Q
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// Non temperature dependent source term
        /// </summary>
        public float sc { get; set; }

        /// <summary>
        /// Temperature dependent source term
        /// </summary>
        public float sp { get; set; }

        /// <summary>
        /// Solution vector term
        /// </summary>
        public float d { get; set; }
    }
}
