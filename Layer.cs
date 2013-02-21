using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Patankar_Model
{
    class Layer
    {
        
        /// <summary>
        /// Maximum x-direction [m]
        /// </summary>
        private const float x_max = 0.0401828f;

        /// <summary>
        /// Maximum y-direction [m]
        /// </summary>
        private const float y_max = 0.004864f;

        
        /// <summary>
        /// Value of spacing used to form interior rectangle to the original layer rectangle [m] in the x-direction
        /// </summary>
        private float layer_dx;

        /// <summary>
        /// Value of spacing used to form interior rectangle to the original layer rectangle [m] in the y-direction
        /// </summary>
        private float layer_dy;

        // Holds value of Layer Area
        /// <summary>
        /// Layer area [m^2]
        /// </summary>
        public float Layer_Area;

        /// <summary>
        /// Point on x-axis which indicates the upper left corner of the layer rectangle's distance from origin [m]
        /// </summary>
        public float Layer_x0;

        /// <summary>
        /// Point on x-axis which indicates the lower right corner of the layer rectangle's distance from origin [m]
        /// </summary>
        public float Layer_xf;

        /// <summary>
        /// Point on the y-axis which indicates the upper left corner of the layer rectangle's distance from origin [m]
        /// </summary>
        public float Layer_y0;

        /// <summary>
        /// Point on the y-axis which indicates the lower right corner of the layer rectangle's distance from origin [m]
        /// </summary>
        public float Layer_yf;

      
        /// <summary>
        /// Struct which holds two coordinate pairs representing the upper right corner (x0,y0)
        /// and the lower right corner (xf,yf) which represents a rectangle.  This rectangle
        /// encloses a certain part of the computational domain.
        /// </summary>
        public struct Rectangle
        {
            public float x_0, y_0, x_f, y_f;

            // Constructor for the Rectangle struct, requires initialization of both
            // coordinate pairs to define the rectangle.
            public Rectangle(float px_0, float py_0, float px_f, float py_f)
            {
                x_0 = px_0;
                y_0 = py_0;
                x_f = px_f;
                y_f = py_f;
            }

            // Area()
            //
            // Calculates the area of the rectangular area
            public float Area()
            {
                // Dx = Final x - Initial x
                float dx = (x_f - x_0);

                // Since y0 is always higher than yf, dy is calculated as y0-yf.
                float dy = (y_0 - y_f);

                // Returns the area of the computational layer [m^2]
                return (dx * dy);
            }
        }

        // Static int (retained in memory) to hold an ID number for each layer.  This is
        // potentially used in debugging, and to ensure that each CV area is consistent 
        // with the physical reality.
        /// <summary>
        /// Layer ID number
        /// </summary>
        private static int LayerID = 0;

        // The Layer_ID is the value of LayerID specific to this individual Layer.  Each
        // time the Layer constructor is called, LayerID is assigned to Layer_ID and then 
        // incremented
        /// <summary>
        /// Layer ID number
        /// </summary>
        private int Layer_ID;

        // Local ErrorHandler to allow for debug messages to potentially be displayed on 
        // the main UI
        /// <summary>
        /// ErrorHandler to pass messages to the main UI
        /// </summary>
        ErrorHandler Layer_Errors;

        // Rectangle for this layer.  
        /// <summary>
        /// This layer's rectangle
        /// </summary>
        public Rectangle Layer_Rectangle;

        // Holds the string value of the material that is used on this layer.  This will
        // eventually be used in nodal assignment to match layer materials to their respective
        // nodes.
        /// <summary>
        /// Material string for this layer
        /// </summary>
        public string Layer_Material;

        // Layer()
        //
        // Layer constructor which requires the initialization of both coordinate pairs, material name
        // and the ErrorHandler function to be passed in.
        /// <summary>
        /// Layer Constructor
        /// </summary>
        /// <param name="local_ErrorHandler">Main UI ErrorHandler</param>
        /// <param name="x_0">X-coordinate, upper left corner</param>
        /// <param name="y_0">Y-coordinate, upper left corner</param>
        /// <param name="x_f">X-coordinate, lower right corner</param>
        /// <param name="y_f">Y-coordinate, lower right corner</param>
        /// <param name="Mat_Name">Material for this layer</param>
        /// <param name="n_Nodes">Number of nodes for this layer</param>
        public Layer(ErrorHandler local_ErrorHandler, float x_0, float y_0, float x_f, float y_f, string Mat_Name, int n_Nodes)
        {
            Layer_Errors = local_ErrorHandler;

            Check_Positioning(x_0, y_0, x_f, y_f);
            
            Layer_Rectangle = new Rectangle(x_0, y_0, x_f, y_f);

            Layer_x0 = x_0;
            Layer_xf = x_f;
            Layer_y0 = y_0;
            Layer_yf = y_f;

            Layer_Material = Mat_Name;

            Layer_Area = Layer_Rectangle.Area();

            Nodes = n_Nodes;

            Layer_ID = LayerID++;
        }

        /// <summary>
        /// Get the layer ID
        /// </summary>
        /// <returns>This layer's ID</returns>
        public int getID()
        {
            // Simply returns the Layer_ID to the user.
            return Layer_ID;
        }

        // Check_Positioning()
        //
        // When a coordinate pair is passed in (x0,y0) and (xf,yf), the points are checked
        // to ensure that they are in the right position (or at least positioned close to correct)
        /// <summary>
        /// Checks positioning to ensure proper coordinate positioning
        /// </summary>
        /// <param name="x0">X-coordinate, upper left corner</param>
        /// <param name="y0">Y-coordinate, upper left corner</param>
        /// <param name="xf">X-coordinate, lower left corner</param>
        /// <param name="yf">Y-coordinate, lower left corner</param>
        public void Check_Positioning(float x0, float y0, float xf, float yf)
        {
            if (x0 > xf)
                Layer_Errors.Post_Error("LAYER ERROR:  (x0 > xf)");
            if (y0 < yf)
                Layer_Errors.Post_Error("LAYER ERROR:  (y0 < yf)");
            if (xf > x_max)
                Layer_Errors.Post_Error("LAYER ERROR:  (x0 > xmax)");
            if (y0 > y_max)
                Layer_Errors.Post_Error("LAYER ERROR:  (y0 > ymax)");
        }

        // NodeCount
        // Holds the value of nodes desired for this specific layer.  Will eventually be used
        // by the Mesh.cs class to apply a uniform node mesh to this layer
        private int NodeCount;

        // Accessor function for NodeCount, accessed through Nodes
        /// <summary>
        /// Number of nodes for this layer
        /// </summary>
        public int Nodes
        {
            get
            {
                return NodeCount;
            }
            set
            {
                if (value > 0)
                {
                    NodeCount = value;
                }
                else
                {
                    NodeCount = 0;
                    Layer_Errors.Post_Error("LAYER ERROR:  Node count for Layer:  " + getID().ToString() + " attempted to be set less than or equal to 0.");
                }
            }
        }

        /// <summary>
        /// Calculates the node positioning (x-direction)
        /// based off layer_dx which this function also calculates
        /// </summary>
        /// <param name="i">Current node out of N_Nodes</param>
        /// <returns>Node Positioning in the x-direction [m]</returns>
        public float dX(float i)
        {
            this.layer_dx = (this.Layer_Rectangle.x_f - this.Layer_Rectangle.x_0) / (float)Nodes;

            float X0 = this.Layer_Rectangle.x_0 + this.layer_dx;
            float XF = this.Layer_Rectangle.x_f - this.layer_dx;

            float dx = X0 + ((XF - X0) / ((float)Nodes - 1.0f)) * i;

            return dx;
        }

        /// <summary>
        /// Calculates the node positioning (y-direction) based off layer_dy which
        /// this function also calculates.
        /// </summary>
        /// <param name="i">Current node out of N_Nodes</param>
        /// <returns>Node positioning in the y-direction</returns>
        public float dY(float i)
        {
            this.layer_dy = (this.Layer_Rectangle.y_0 - this.Layer_Rectangle.y_f) / (float)Nodes;

            float Y0 = this.Layer_Rectangle.y_0 - this.layer_dy;
            float YF = this.Layer_Rectangle.y_f + this.layer_dy;

            float dy = YF + ((Y0 - YF) / ((float)Nodes - 1.0f)) * i;

            return dy;
        }

        /// <summary>
        /// Adjusted X0 used to align CV boundaries with material boundaries
        /// </summary>
        public float adjusted_X0 
        { 
            get
            {
                return (this.Layer_Rectangle.x_0 + this.layer_dx);
            }
            private set
            {
            }
        }
        
        /// <summary>
        /// Adjusted XF used to align CV boundaries with material boundaries
        /// </summary>
        public float adjusted_XF
        {
            get
            {
                return (this.Layer_Rectangle.x_f - this.layer_dx);
            }
            private set
            {
            }
        }

        /// <summary>
        /// Adjusted Y0 used to align CV boundaries with material boundaries
        /// </summary>
        public float adjusted_Y0
        {
            get
            {
                return (this.Layer_Rectangle.y_0 - this.layer_dy);
            }
            private set
            {
            }
        }

        /// <summary>
        /// Adjusted YF used to align CV boundaries with material boundaries
        /// </summary>
        public float adjusted_YF
        {
            get
            {
                return (this.Layer_Rectangle.y_f + this.layer_dy);
            }
            private set
            {
            }
        }


        private float NODESPACING;
        /// <summary>
        /// Indicates the spacing between each node in both x and y directions
        /// </summary>
        public float node_Spacing 
        {
            get
            {
                NODESPACING = Calc_NodeSpacing();
                return NODESPACING;
            }

            private set { value = NODESPACING; }
        }
        
        /// <summary>
        /// Calculates node spacing
        /// </summary>
        /// <returns>Returns distance [m] for each node</returns>
        private float Calc_NodeSpacing()
        {
            float node_Spacing = (this.Layer_xf - this.Layer_x0) / this.Nodes;

            return node_Spacing;
        }
    }
}
