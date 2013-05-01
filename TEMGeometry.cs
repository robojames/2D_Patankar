 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace _2D_Patankar_Model
{
    class TEMGeometry
    {
        /// <summary>
        /// Local ErrorHandler to pass through errors to the main UI
        /// </summary>
        ErrorHandler Geometry_Errors;

        /// <summary>
        /// List of layer objects that comprise the entire geometry of the TEM
        /// </summary>
        public List<Layer> Layer_List;

        /// <summary>
        /// Holds values of number of coordinate pairs
        /// </summary>
        private int n_Coords;

        /// <summary>
        /// Number of BiTE elements present on the TEM.  If this value is changed, the ceramic
        /// alumina plates will have to be updated as the sizes will no longer match up
        /// </summary>
        private const int n_elements = 18;

        // Series of constants defining in meters the thicknesses and widths of various
        // repetitive layers for easy loop-generation of the geometry

        /// <summary>
        /// Thickness of the BiTE elements [m] in x-direction
        /// </summary>
        private const float BiTE_Thickness = 0.001397f;

        /// <summary>
        /// Thickness of the airgap between BiTe Elements [m]
        /// </summary>
        private const float BiTE_AirGap = 0.000762f;

        /// <summary>
        /// Copper connector widths [m] in the x-direction
        /// </summary>
        private const float CE_Width = 0.003556f;

        /// <summary>
        /// Copper connector thickness [m] in the y-direction
        /// </summary>
        private const float CE_Thickness = 0.00041910f;

        /// <summary>
        /// Height of the air gap
        /// </summary>
        private const float AIR_Height = 0.00172589f;

        /// <summary>
        /// Number of nodes for the BiTe elements
        /// </summary>
        private const int n_Nodes_BiTe = 30;

        /// <summary>
        /// Number of nodes for the air elements
        /// </summary>
        private const int n_Nodes_Air = 30;

        /// <summary>
        /// Number of nodes for the copper connector elements
        /// </summary>
        private const int n_Nodes_CE = 30;

        /// <summary>
        /// Number of nodes for each ceramic alumina plate
        /// </summary>
        private const int n_Nodes_Ceramic = 50;


        private float[] Coord_Ceramic_Base_Bottom = new float[4] { 0.0f, 0.635f, 39.9796f, 0.00f };
        private float[] Coord_Ceramic_Base_Top = new float[4] { 0.0f, 3.4150f, 39.9796f, 2.780f };

        private float[] Coord_Left_Air_Gap = new float[4] { 0.0f, 2.7800f, 1.016f, 0.635f };

        // Right air gap X_0 needs to be increased I'm pretty sure
        private float[] Coord_Right_Air_Gap = new float[4] { 39.116f, 2.77999f, 39.97960f, 0.635f };

        private float[] Coord_First_Bot_CU = new float[4] { 1.016f, 1.0541f, 2.4130f, 0.635f };
        
        // Re Check this value
        //private float[] Coord_Last_Bot_CU = new float[4] { 37.5666f, 1.0541f, 39.9796f, 0.635f };

        private float[] Coord_Last_Bot_CU = new float[4] { 37.719f, 1.0541f, 39.11600f, 0.635f };

        private float[] Coord_Cu_Bottom = new float[4] { 3.1750f, 1.0541f, 6.7310f, 0.635f };
        private float[] Coord_Cu_Top = new float[4] { 1.016f, 2.7799f, 4.5720f, 2.3749f};

        private float[] Coord_BiTE = new float[4] { 1.016f, 2.3609f, 2.4130f, 1.0541f };

        private float[] Coord_Top_AirGaps = new float[4] { 4.5720f, 2.7800f, 5.3340f,  1.0541f};
        private float[] Coord_Bottom_AirGaps = new float[4] { 2.4130f, 2.3609f, 3.1750f, 0.635f };

        List<float[]> Coord_List;

        // Constructor for the TEMGeometry.cs class.  Requires passing in of the local error
        // handler but nothing else.  Every other value is generated from other classes as
        // the TEMGeometry is very specific.  This file is one of the few that might require
        // editing as the solution process marches on
        /// <summary>
        /// Constructor for TEM Geometry
        /// </summary>
        /// <param name="localErrorHandler">Main UI ErrorHandler</param>
        public TEMGeometry(ErrorHandler localErrorHandler)
        {
            Geometry_Errors = localErrorHandler;

            // Initializes a new List of Layers
            Layer_List = new List<Layer>();

            Coord_List = new List<float[]>();

            Coord_List.Add(Coord_Ceramic_Base_Bottom);
            Coord_List.Add(Coord_Ceramic_Base_Top);
            Coord_List.Add(Coord_Left_Air_Gap);
            Coord_List.Add(Coord_Right_Air_Gap);
            Coord_List.Add(Coord_First_Bot_CU);
            Coord_List.Add(Coord_Last_Bot_CU);
            Coord_List.Add(Coord_Cu_Bottom);
            Coord_List.Add(Coord_Cu_Top);
            Coord_List.Add(Coord_BiTE);
            Coord_List.Add(Coord_Top_AirGaps);
            Coord_List.Add(Coord_Bottom_AirGaps);

            // Function to modify dimensions (which are entered in mm) to m
            ConvertUnits(Coord_List);

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

        private void ConvertUnits(List<float[]> CoordList)
        {
            foreach (float[] array in CoordList)
            {
                array[0] *= (float)Math.Pow(10, -3.0);
                array[1] *= (float)Math.Pow(10, -3.0);
                array[2] *= (float)Math.Pow(10, -3.0);
                array[3] *= (float)Math.Pow(10, -3.0);
            }
        }

        // GenerateGeometry
        //
        // Main function which generates the geometry of the TEM, and organizes it into a list of layers
        /// <summary>
        /// Generates geometry of the TEM, which is essentially a list of rectangular coordinates
        /// </summary>
        /// <returns>List (array) of Layer objects</returns>
        public List<Layer> GenerateGeometry()
        {
            // Update MainUI (ie, the user) with the progress of the geometry generation
            Geometry_Errors.UpdateProgress(0);
            Geometry_Errors.UpdateProgress_Text("Generating Geometry and Material Layers");

            // Generates Layers for both top and bottom ceramic pieces of TEM geometry
            Layer TEM_Bottom = new Layer(Geometry_Errors, Coord_Ceramic_Base_Bottom[0], Coord_Ceramic_Base_Bottom[1], Coord_Ceramic_Base_Bottom[2], Coord_Ceramic_Base_Bottom[3], "Ceramic", n_Nodes_Ceramic);
            Layer TEM_Top = new Layer(Geometry_Errors, Coord_Ceramic_Base_Top[0], Coord_Ceramic_Base_Top[1], Coord_Ceramic_Base_Top[2], Coord_Ceramic_Base_Top[3], "Ceramic", n_Nodes_Ceramic);

            Layer_List.Add(TEM_Bottom);
            Layer_List.Add(TEM_Top);
            // End of ceramic geometry creation

            // Generates Layers for air gaps to the far left and far right of the computational domain
            Layer Left_Air_Gap = new Layer(Geometry_Errors, Coord_Left_Air_Gap[0], Coord_Left_Air_Gap[1], Coord_Left_Air_Gap[2], Coord_Left_Air_Gap[3], "Air", n_Nodes_Air);
            Layer Right_Air_Gap = new Layer(Geometry_Errors, Coord_Right_Air_Gap[0], Coord_Right_Air_Gap[1], Coord_Right_Air_Gap[2], Coord_Right_Air_Gap[3], "Air", n_Nodes_Air);

            Layer_List.Add(Left_Air_Gap);
            Layer_List.Add(Right_Air_Gap);
            // End creation of Air Gaps

            // Generates Layer for first and last Cu pieces on the bottom (stubbed off ones)
            Layer First_Bot_Cu = new Layer(Geometry_Errors, Coord_First_Bot_CU[0], Coord_First_Bot_CU[1], Coord_First_Bot_CU[2], Coord_First_Bot_CU[3], "Copper", n_Nodes_CE);
            Layer Last_Bot_Cu = new Layer(Geometry_Errors, Coord_Last_Bot_CU[0], Coord_Last_Bot_CU[1], Coord_Last_Bot_CU[2], Coord_Last_Bot_CU[3], "Copper", n_Nodes_CE);

            Layer_List.Add(First_Bot_Cu);
            Layer_List.Add(Last_Bot_Cu);
            // End creation of the first and last bottom Cu pieces

            // Generates Layers for each the rest of the bottom Cu pieces
            for (int i = 0; i < 8; i++)
            {
                float x0 = Coord_Cu_Bottom[0] + ((float)i * (BiTE_AirGap + CE_Width));
                float y0 = Coord_Cu_Bottom[1];
                float xf = x0 + CE_Width;
                float yf = Coord_Cu_Bottom[3]; 

                Layer_List.Add(new Layer(Geometry_Errors, x0, y0, xf, yf, "Copper", n_Nodes_CE));
            }
            // End creation of bottom Cu pieces

            // Generates Layers for each of the top of the Cu pieces
            for (int i = 0; i < 9; i++)
            {
                float x0 = Coord_Cu_Top[0] + ((float)i * (BiTE_AirGap + CE_Width));
                float y0 = Coord_Cu_Top[1];
                float xf = x0 + CE_Width;
                float yf = Coord_Cu_Top[3];

                Layer_List.Add(new Layer(Geometry_Errors, x0, y0, xf, yf, "Copper", n_Nodes_CE));
            }
            // End creation of the top Cu pieces

            // Generates layers for each of the Bismuth Telluride Pieces
            for (int i = 0; i < 18; i++)
            {
                float x0 = Coord_BiTE[0] + ((float)i * (BiTE_Thickness + BiTE_AirGap));
                float y0 = Coord_BiTE[1];
                float xf = x0 + BiTE_Thickness;
                float yf = Coord_BiTE[3];

                Layer_List.Add(new Layer(Geometry_Errors, x0, y0, xf, yf, "BiTe", n_Nodes_BiTe));
            }
            // End creation of the Bismuth Telluride Pieces

            // Generates layers for air gaps that 'float' near top
            for (int i = 0; i < 8; i++)
            {
                float x0 = Coord_Top_AirGaps[0] + ((float)i * (2 * (BiTE_Thickness + BiTE_AirGap)));
                float y0 = Coord_Top_AirGaps[1];
                float xf = x0 + BiTE_AirGap;
                float yf = Coord_Top_AirGaps[3];

                Layer_List.Add(new Layer(Geometry_Errors, x0, y0, xf, yf, "Air", n_Nodes_Air));
            }
            // End creation of layers for top air gaps

            // Generates layers for air gaps that 'float' near bottom
            for (int i = 0; i < 9; i++)
            {
                float x0 = Coord_Bottom_AirGaps[0] + ((float)i * (2 * (BiTE_Thickness + BiTE_AirGap)));
                float y0 = Coord_Bottom_AirGaps[1];
                float xf = x0 + BiTE_AirGap;
                float yf = Coord_Bottom_AirGaps[3];

                Layer_List.Add(new Layer(Geometry_Errors, x0, y0, xf, yf, "Air", n_Nodes_Air));
            }
            // End creation of layers for the bottom air gaps

            Geometry_Errors.Post_Error("NOTE:  Geometry for the TEM has been generated succesfully");

            return Layer_List;
        }


        // CheckPoints
        //
        // Checks each layer for coordinate issues.  Pair interactions have already been checked within the 
        // Layer.cs function, however, calculated areas (among other properties) have not at this point.
        /// <summary>
        /// Check each points to ensure that calculates mesh area for a given layer is not negative
        /// </summary>
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
