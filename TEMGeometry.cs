using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Patankar_Model
{
    class TEMGeometry
    {
        // Local ErrorHandler to pass through errors to the main UI
        ErrorHandler Geometry_Errors;

        // List of Layer objects that comprise the entire geometry of the TEM
        public List<Layer> Layer_List;

        // Integer to hold the value of the number of specified coordinates
        private int n_Coords;

        // Number of BiTE elements present on the TEM.  Some (x0,y0) or (xf,yf) values may
        // need updating if this value is changed for accurate representation
        private const int n_elements = 18;

        // Series of constants defining in meters the thicknesses and widths of various
        // repetitive layers for easy loop-generation of the geometry
        private const float BiTE_Thickness = 0.001397f;
        private const float BiTE_AirGap = 0.000762f;
        private const float BiTE_Height = 0.0013208f;
        private const float CE_Width = 0.003556f;
        private const float CE_Thickness = 0.00041910f;
        private const float AIR_Height = 0.00172589f;

        private const int n_Nodes_BiTe = 50;
        private const int n_Nodes_Air = 150;
        private const int n_Nodes_CE = 150;
        private const int n_Nodes_Ceramic = 550;

        // Series of x0, y0, xf, and yf values (see Rectangle in Layer.cs) provided for
        // reference, and in case the geometry changes slightly
        private float[] x_0 = new float[4] { 0, 0.001016f, 0.001016f, 0.001016f};
        private float[] y_0 = new float[4] { 0.000635f, 0.0010541f, 0.0023749f, 0.00277999f};

        private float[] x_f = new float[4] { 0.03997960f, 0.002413f, 0.002413f, 0.004572f};
        private float[] y_f = new float[4] { 0, 0.000635f, 0.0010541f, 0.00236089f};

        // Constructor for the TEMGeometry.cs class.  Requires passing in of the local error
        // handler but nothing else.  Every other value is generated from other classes as
        // the TEMGeometry is very specific.  This file is one of the few that might require
        // editing as the solution process marches on
        public TEMGeometry(ErrorHandler localErrorHandler)
        {
            Geometry_Errors = localErrorHandler;

            // Initializes a new List of Layers
            Layer_List = new List<Layer>();

            // Assigns length of xf vector to n_Coords.  Assumes that all the const vector series defined above
            // are of the same length
            n_Coords = x_f.Length;

            // Reports the number of defined coordinates to the user to ensure proper reading
            Geometry_Errors.Post_Error("Note: Calculated length of vectors for Geometry TEM: " + n_Coords.ToString());

            // Function to generate the geometry of the TEM
            GenerateGeometry();

            // Checks coordinate pairs and cv areas for errors
            CheckPoints();

            // Iterates over each layer in the TEM geometry and reports the calculated layer ID and area.  This code
            // may be eventually removed as the code matures
            foreach (Layer layer in Layer_List)
            {
                //Geometry_Errors.Post_Error("Layer ID:  " + layer.getID().ToString() + ": " + layer.Layer_Area.ToString());
            }
        }

        // GenerateGeometry
        //
        // Main function which generates the geometry of the TEM, and organizes it into a list of layers
        public List<Layer> GenerateGeometry()
        {
            // Update MainUI (ie, the user) with the progress of the geometry generation
            Geometry_Errors.UpdateProgress(0);
            Geometry_Errors.UpdateProgress_Text("Generating Geometry and Material Layers");

            // Create base ceramic layer
            Layer_List.Add(new Layer(Geometry_Errors, x_0[0], y_0[0], x_f[0], y_f[0], "Ceramic", n_Nodes_Ceramic));

            // Create thin copper connector (bottom, first and last)
            //
            // First
            Layer_List.Add(new Layer(Geometry_Errors, x_0[1], y_0[1], x_f[1], y_f[1], "Copper", n_Nodes_CE));
            // Last
            Layer_List.Add(new Layer(Geometry_Errors, (x_0[1] + ((n_elements - 1) * (BiTE_AirGap + BiTE_Thickness))), y_0[1], 0.0389636f, y_0[1] - CE_Thickness, "Copper", n_Nodes_CE)); 

            // Create all BiTE elements
            for (int k = 0; k < n_elements; k++)
            {
                float x_0_BITE = x_0[1] + (k * (BiTE_Thickness + BiTE_AirGap));
                float y_0_BiTE = y_0[2];
                float x_f_BITE = x_0_BITE + BiTE_Thickness;
                float y_f_BITE = y_0_BiTE - BiTE_Height;

                Layer_List.Add(new Layer(Geometry_Errors, x_0_BITE, y_0_BiTE, x_f_BITE, y_f_BITE, "BiTe", n_Nodes_BiTe));
            }

            Geometry_Errors.UpdateProgress(20);

            // Create Bottom Copper Elements (excluding first and last)
            for (int j = 0; j < ((n_elements) / 2) - 1; j++)
            {
                float x_0_CE = 0.003175f + (j * (BiTE_AirGap + CE_Width));
                float y_0_CE = y_0[2];

                float x_f_CE = x_0_CE + CE_Width;
                float y_f_CE = y_0_CE - CE_Thickness;

                Layer_List.Add(new Layer(Geometry_Errors, x_0_CE, y_0_CE, x_f_CE, y_f_CE, "Copper", n_Nodes_CE));
            }

            Geometry_Errors.UpdateProgress(40);

            // Create Top Copper Elements (all)
            for (int j = 0; j < ((n_elements) / 2); j++)
            {
                float x_0_CE = x_0[2] + (j * (CE_Width + BiTE_AirGap));
                float y_0_CE = y_0[3];

                float x_f_CE = x_0_CE + CE_Width;
                float y_f_CE = y_0_CE - CE_Thickness;
            }
            Geometry_Errors.UpdateProgress(60);

            // Create top Ceramic Plate
            Layer_List.Add(new Layer(Geometry_Errors, x_0[0], 0.00341499f, 0.0399796f, 0.00277999f, "Ceramic", n_Nodes_Ceramic));

            // Create Air Boxes
            //
            // Area contained via left side by air
            Layer_List.Add(new Layer(Geometry_Errors, x_0[0], y_0[3], x_0[1], y_f[1], "Air", n_Nodes_Air));

            // Middle Boxes
            for (int j = 0; j < ((n_elements) - 1); j++)
            {
                float Air_x0 = x_0[1] + (BiTE_Thickness * j);
                float Air_y0 = 0.0f;

                if ((j % 2) == 0)
                {
                    Air_y0 = 0.0023749f;
                }
                else
                {
                    Air_y0 = y_0[3];
                }

                float Air_xf = Air_x0 + BiTE_AirGap;
                float Air_yf = Air_y0 - AIR_Height;

                Layer_List.Add(new Layer(Geometry_Errors, Air_x0, Air_y0, Air_xf, Air_yf, "Air", n_Nodes_Air));
            }

            Geometry_Errors.UpdateProgress(100);

            Geometry_Errors.Post_Error("NOTE:  Geometry for the TEM has been generated succesfully");

            return Layer_List;
        }


        // CheckPoints
        //
        // Checks each layer for coordinate issues.  Pair interactions have already been checked within the 
        // Layer.cs function, however, calculated areas (among other properties) have not at this point.
        public void CheckPoints()
        {
            foreach (Layer Mesh_Layer in Layer_List)
            {
                if (Mesh_Layer.Layer_Area <= 0)
                {
                    Geometry_Errors.Post_Error("GEOMETRY ERROR:  Calculated Area <= 0");
                }
                else
                {
                    //Geometry_Errors.Post_Error("Note:  Area for Layer " + Mesh_Layer.getID().ToString() + " generated successfully");
                }
            }
        }

    }
}
