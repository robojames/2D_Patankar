using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Patankar_Model
{
    class Layer
    {
        // x_max - Maximum x location in meters.  Utilized to check if coordinate set passed in 
        //         are valid.
        //
        // y_max - Maximum y location in meters.  Utilized as the same as x_max.
        private const float x_max = 0.0401828f;
        private const float y_max = 0.004864f;

        // Holds value of Layer Area
        public float Layer_Area;

        // Rectangle
        //
        // Struct which holds two coordinate pairs representing the upper right corner (x0,y0)
        // and the lower right corner (xf,yf) which represents a rectangle.  This rectangle
        // encloses a certain part of the computational domain.
        struct Rectangle
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
        private static int LayerID = 0;

        // The Layer_ID is the value of LayerID specific to this individual Layer.  Each
        // time the Layer constructor is called, LayerID is assigned to Layer_ID and then 
        // incremented
        private int Layer_ID;

        // Local ErrorHandler to allow for debug messages to potentially be displayed on 
        // the main UI
        ErrorHandler Layer_Errors;

        // Rectangle for this layer.  
        Rectangle Layer_Rectangle;

        // Holds the string value of the material that is used on this layer.  This will
        // eventually be used in nodal assignment to match layer materials to their respective
        // nodes.
        public string Layer_Material;

        // Layer()
        //
        // Layer constructor which requires the initialization of both coordinate pairs, material name
        // and the ErrorHandler function to be passed in.
        public Layer(ErrorHandler local_ErrorHandler, float x_0, float y_0, float x_f, float y_f, string Mat_Name)
        {
            Layer_Errors = local_ErrorHandler;

            Check_Positioning(x_0, y_0, x_f, y_f);
            
            Layer_Rectangle = new Rectangle(x_0, y_0, x_f, y_f);

            Layer_Material = Mat_Name;

            Layer_Area = Layer_Rectangle.Area();

            Layer_ID = LayerID++;
        }

        public int getID()
        {
            // Simply returns the Layer_ID to the user.
            return Layer_ID;
        }

        // Check_Positioning()
        //
        // When a coordinate pair is passed in (x0,y0) and (xf,yf), the points are checked
        // to ensure that they are in the right position (or at least positioned close to correct)
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


    }
}
