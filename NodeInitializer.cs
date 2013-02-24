using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace _2D_Patankar_Model
{
    class NodeInitializer
    {
        /// <summary>
        /// Calculated (during object construction) maximum physical x position in meters
        /// </summary>
        float max_X;

        /// <summary>
        /// Calculated (during object construction) maximum physical y position in meters
        /// </summary>
        float max_Y;

        /// <summary>
        /// Local ErrorHandler to pass messages to the main UI
        /// </summary>
        ErrorHandler Node_I_ErrorHandler;

        /// <summary>
        /// List of layers (to be passed in upon initialization) that hold node information
        /// </summary>
        List<Layer> LayerList;

        /// <summary>
        /// Array of values representing the spatially sorted jagged array of nodes
        /// </summary>
        Node[][] NodeArray;

        /// <summary>
        /// Minimum x nodal position [m]
        /// </summary>
        float x_MinpDX;

        /// <summary>
        /// Minimum y nodal position [m]
        /// </summary>
        float y_MinpDY;

        /// <summary>
        /// Maximum x nodal position [m]
        /// </summary>
        float x_Max_DX;

        /// <summary>
        /// Maximum y nodal position [m]
        /// </summary>
        float y_Max_DY;


        /// <summary>
        /// Material manager object which holds current materials being utilized in the numeric study
        /// </summary>
        MaterialManager Mat_Manager;

        /// <summary>
        /// Constructor of the NodeInitializer Class.
        /// </summary>
        /// <param name="Nodes">Node Array of Numeric Model</param>
        /// <param name="local_ErrorHandler">Error Handler for Message Passing</param>
        /// <param name="Materials">Material Manager which holds all the material properties</param>
        /// <param name="Layers">List of Layers holding material information for each node inside of it</param>
        public NodeInitializer(Node[][] Nodes, ErrorHandler local_ErrorHandler, MaterialManager Materials, List<Layer> Layers)
        {
            x_Max_DX = 0.0f;
            y_Max_DY = 0.0f;

            max_X = 0.0f;
            max_Y = 0.0f;

            foreach (Node[] node_array in Nodes)
            {
                foreach (Node node in node_array)
                {
                    if (node.x_pos > x_Max_DX)
                        x_Max_DX = node.x_pos;

                    if (node.y_pos > y_Max_DY)
                        y_Max_DY = node.y_pos;

                }
            }

            foreach (Layer layer in Layers)
            {
                if (layer.Layer_xf > max_X)
                    max_X = layer.Layer_xf;
                if (layer.Layer_y0 > max_Y)
                    max_Y = layer.Layer_y0;
            }
            
       
            LayerList = Layers;

            NodeArray = Nodes;

            Node_I_ErrorHandler = local_ErrorHandler;

            Mat_Manager = Materials;

            x_MinpDX = Nodes[0][0].x_pos;
            y_MinpDY = Nodes[0][0].y_pos;
            
            Assign_Materials();

            Calculate_DxDy();

            Initialize_Influence_Coefficients(Nodes);
        }

        /// <summary>
        /// Initializes influence coefficients for each node passed in
        /// </summary>
        /// <param name="Nodes">Jagged 2D array of nodes</param>
        private void Initialize_Influence_Coefficients(Node[][] Nodes)
        {
            List<Node> BoundaryNodes = new List<Node>();

            foreach (Node[] Node_Array in Nodes)
            {
                foreach (Node node in Node_Array)
                {
                    node.Initialize_Influence_Coefficients();

                    if (node.is_Boundary)
                    {
                        BoundaryNodes.Add(node); 
                    }
                }
            }

            int boundary_Counter = 0;
            int s_boundary_Counter = 0;

            List<Material> MaterialList = new List<Material>();

            MaterialList = Mat_Manager.Material_List;

            // Need to iterate through Boundary_Nodes, to find nearest material to boundary nodes
            foreach (Node b_Node in BoundaryNodes)
            {
                if (LayerList[b_Node.Layer_ID].Layer_Material != b_Node.Boundary_Material && b_Node.Boundary_Material != "")
                {
                    boundary_Counter++;
                }

                if (LayerList[b_Node.Layer_ID].Layer_Material == b_Node.Boundary_Material && b_Node.Boundary_Material != "")
                {
                    s_boundary_Counter++;
                }

                b_Node.Initialize_Effective_Conductivities(MaterialList);
            }

            Node_I_ErrorHandler.Post_Error("NOTE: " + boundary_Counter.ToString() + " number of material boundaries detected with " + s_boundary_Counter.ToString() +
                " boundary materials matching their own material");
                      
            
            Node_I_ErrorHandler.Post_Error("NOTE:  Finished Initializing Influence Coefficients");
        }

        /// <summary>
        /// Assigns material values for each node according to it's layer
        /// </summary>
        private void Assign_Materials()
        {
            foreach (Node[] node in NodeArray)
            {
                foreach (Node ind_node in node)
                {
                    ind_node.gamma = Mat_Manager.Get_Gamma(ind_node.Material);
                }
            }

            Node_I_ErrorHandler.Post_Error("NOTE:  Finished assigning material properties to each node");

            foreach (Node[] node in NodeArray)
            {
                foreach (Node ind_node in node)
                {
                    if (ind_node.gamma == 0)
                    {
                        Node_I_ErrorHandler.Post_Error("Node Initialization Error:  Gamma set to 0 for Node " + ind_node.Node_ID.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the CV widths and delta_x_DIRECTION's for each boundary node which takes
        /// into account the interfacial phenomenon
        /// </summary>
        private void Calculate_DxDy()
        {
            int nodes_Adjusted = 0;

            List<Node> BoundaryNodes = new List<Node>();

            for (int i = 0; i < NodeArray.Count(); i++)
            {
                for (int j = 0; j < NodeArray[i].Count(); j++)
                {
                    if (NodeArray[i][j].is_Boundary)
                    {
                       BoundaryNodes.Add(NodeArray[i][j]);
                    }
                }
            }
            
            List<float> x0_Interest = new List<float>();
            List<float> xF_Interest = new List<float>();
            List<float> y0_Interest = new List<float>();
            List<float> yF_Interest = new List<float>();
            
            foreach (Node node in BoundaryNodes)
            {
                x0_Interest.Add(LayerList[node.Layer_ID].adjusted_X0);
                y0_Interest.Add(LayerList[node.Layer_ID].adjusted_Y0);
                xF_Interest.Add(LayerList[node.Layer_ID].adjusted_XF);
                yF_Interest.Add(LayerList[node.Layer_ID].adjusted_YF);
            }

            List<float> x0 = new List<float>();
            List<float> xF = new List<float>();
            List<float> y0 = new List<float>();
            List<float> yF = new List<float>();

            foreach (float x in x0_Interest)
            {
                if (!x0.Contains(x))
                {
                    x0.Add(x);
                }
            }

            foreach (float x in xF_Interest)
            {
                if (!xF.Contains(x))
                {
                    xF.Add(x);
                }
            }

            foreach (float y in y0_Interest)
            {
                if (!yF.Contains(y))
                {
                    yF.Add(y);
                }
            }

            foreach (float y in yF_Interest)
            {
                if (!y0.Contains(y))
                {
                    y0.Add(y);
                }
            }

            List<float> Positions_X = new List<float>(x0_Interest.Count + xF_Interest.Count);
            x0_Interest.ForEach(pos => Positions_X.Add(pos));
            xF_Interest.ForEach(pos => Positions_X.Add(pos));

            List<float> Positions_Y = new List<float>(y0_Interest.Count + yF_Interest.Count);
            y0_Interest.ForEach(pos => Positions_Y.Add(pos));
            yF_Interest.ForEach(pos => Positions_Y.Add(pos));

            // Set status text
            Node_I_ErrorHandler.UpdateProgress_Text("Correcting Boundary Nodes");

            int percent_Progress = 0;
            
            // Still need to correct for nodes at x=xmin and y=ymax
            foreach (Node node in BoundaryNodes)
            {
                Node_I_ErrorHandler.UpdateProgress((int)(100 * ((float)(percent_Progress) / (float)(BoundaryNodes.Count))));

                if (node.x_pos == LayerList[node.Layer_ID].adjusted_X0 && node.x_pos < x_Max_DX)
                {
                    // Adjust delta_x_W
                    if (LayerList[node.Layer_ID].Layer_x0 > 0)
                    {
                        node.delta_x_W = FindClosestBoundary_X(node, Positions_X);
                        node.Boundary_Material = get_Boundary_Material_X(node, "W");
                        node.Boundary_Flag = "W";
                        nodes_Adjusted++;
                    }
                }

                if (node.x_pos == LayerList[node.Layer_ID].adjusted_XF && node.x_pos > x_MinpDX)
                {
                    // Adjust delta_x_E
                    if (LayerList[node.Layer_ID].Layer_xf < max_X)
                    {
                        node.delta_x_E = FindClosestBoundary_X(node, Positions_X);
                        node.Boundary_Material = get_Boundary_Material_X(node, "E");
                        node.Boundary_Flag = "E";
                        nodes_Adjusted++;
                    }
                }

                if (LayerList[node.Layer_ID].adjusted_Y0 == node.y_pos && node.y_pos < y_Max_DY)
                {
                    // Adjust delta_y_N
                    if (LayerList[node.Layer_ID].Layer_y0 < max_Y)
                    {
                        node.delta_y_N = FindClosestBoundary_Y(node, Positions_Y);
                        node.Boundary_Material = get_Boundary_Material_Y(node, "N");
                        node.Boundary_Flag = "N";
                        nodes_Adjusted++;
                    }
                }

                if (LayerList[node.Layer_ID].adjusted_YF == node.y_pos && node.y_pos > y_MinpDY)
                {
                    // Adjust delta_y_S
                    if (LayerList[node.Layer_ID].Layer_yf > 0)
                    {
                        node.delta_y_S = FindClosestBoundary_Y(node, Positions_Y);
                        node.Boundary_Material = get_Boundary_Material_Y(node, "S");
                        node.Boundary_Flag = "S";
                        nodes_Adjusted++;
                    }
                }

                percent_Progress++;
            }

            Node_I_ErrorHandler.UpdateProgress(100);

            Node_I_ErrorHandler.UpdateProgress_Text("");

            Node_I_ErrorHandler.Post_Error("NOTE:  Total Boundary nodes:  " + BoundaryNodes.Count.ToString());

            Node_I_ErrorHandler.Post_Error("NOTE:  Nodes Adjusted (minus nodes at Domain edges):  " + nodes_Adjusted.ToString());
        }

        /// <summary>
        /// Finds the closest boundary material for a given boundary node in the x direction (E or W)
        /// </summary>
        /// <param name="node">Node with which to assign the boundary material to</param>
        /// <param name="E_Or_W">Flag value indicating whether to look in the East or West direction</param>
        /// <returns>String value containing the name of the boundary material</returns>
        private string get_Boundary_Material_X(Node node, string E_Or_W)
        {
            float dx = 0.00000015f; // [m]
            float x0 = 0.0f;
            string material = LayerList[node.Layer_ID].Layer_Material;

            x0 = node.x_pos;

            if (E_Or_W == "E") // plus
            {
                while (material == LayerList[node.Layer_ID].Layer_Material)
                {
                    x0 += dx;
                    if (x0 >= max_X)
                        break;
                    material = checkRectangle(x0, node.y_pos);
                }
            }

            if (E_Or_W == "W") // minus
            {
                while (material == LayerList[node.Layer_ID].Layer_Material)
                {
                    x0 -= dx;

                    if (x0 <= 0.0000000f)
                        break;
                    material = checkRectangle(x0, node.y_pos);
                }
            }

            return material;
        }

        /// <summary>
        /// Finds the closest boundary material for a given boundary node in the y direction (N or S)
        /// </summary>
        /// <param name="node">Node with which to assign the boundary material to</param>
        /// <param name="N_Or_S">Flag value indicating whether to look in the North or South direction</param>
        /// <returns>String value containing the name of the boundary material</returns>
        private string get_Boundary_Material_Y(Node node, string N_Or_S)
        {
            float dy = 0.00000015f;
            float y0 = 0.0f;
            string material = LayerList[node.Layer_ID].Layer_Material;

            y0 = node.y_pos;

            if (N_Or_S == "N") // plus
            {
                while (material == LayerList[node.Layer_ID].Layer_Material)
                {
                    y0 += dy;

                    if (y0 > max_Y)
                        break;

                    material = checkRectangle(node.x_pos, y0);
                }
            }

            if (N_Or_S == "S") // minus
            {
                while (material == LayerList[node.Layer_ID].Layer_Material)
                {
                    y0 -= dy;

                    if (y0 < 0.0000000f)
                        break;
                    material = checkRectangle(node.x_pos, y0);
                }
            }

            return material;

        }

        /// <summary>
        /// Finds the closest boundary position (either vertical (Y) or horizontal (X)) to the passed
        /// in node element
        /// </summary>
        /// <param name="node">Node object's position is used to calculate the distance from a list of positions</param>
        /// <param name="Positions">List of positions to be checked</param>
        /// <returns>Distance in meters of the closest nodal boundary</returns>
        private float FindClosestBoundary_X(Node node, List<float> Positions) // This currently has issues as it could be picking from vertical dimensions (slight changes)
        {
            float delta_X = 100.0f;

            Layer currentLayer = LayerList[node.Layer_ID];

            foreach (float pos in Positions)
            {

                if (pos < currentLayer.Layer_x0 | pos > currentLayer.Layer_xf)
                {
                    float val = Math.Abs(node.x_pos - pos);

                    if (val < delta_X)
                    {
                        delta_X = val;
                    }
                }

            }

            if (delta_X == 100.0f)
            {
                Node_I_ErrorHandler.Post_Error("NODE INITIALIZATION ERROR:  DELTA_X SET INCORRECTLY");
            }

            return delta_X;
        }



        /// <summary>
        /// Finds the closest boundary position (either vertical (Y) or horizontal (X)) to the passed
        /// in node element
        /// </summary>
        /// <param name="node">Node object's position is used to calculate the distance from a list of positions</param>
        /// <param name="Positions">List of positions to be checked</param>
        /// <returns>Distance in meters of closest nodal boundary</returns>
        private float FindClosestBoundary_Y(Node node, List<float> Positions)
        {
            float delta_Y = 100.0f;

            Layer currentLayer = LayerList[node.Layer_ID];

            foreach (float pos in Positions)
            {
                if (pos != node.y_pos)
                {
                    if (pos < currentLayer.Layer_yf | pos > currentLayer.Layer_y0)
                    {
                        float val = Math.Abs(node.y_pos - pos);

                        if (val < delta_Y)
                        {
                            delta_Y = val;
                        }
                    }
                }
            }

            if (delta_Y == 100.0f)
            {
                Node_I_ErrorHandler.Post_Error("NODE INITIALIZATION ERROR:  DELTA_Y SET INCORRECTLY");
            }

            return delta_Y;
        }

        /// <summary>
        /// Checks each material rectangle to see which layer the XPOS and YPOS fits into
        /// </summary>
        /// <param name="xPOS">X-coordinate to check</param>
        /// <param name="yPOS">Y-coordinate to check</param>
        /// <returns>Material of the layer in which (xPOS, yPOS) resides</returns>
        public string checkRectangle(float xPOS, float yPOS)
        {
            string sMat = "";

            
            foreach (Layer layer in LayerList)
            {
                if (xPOS > layer.Layer_Rectangle.x_0 & yPOS < layer.Layer_Rectangle.y_0)
                {
                    if (xPOS < layer.Layer_Rectangle.x_f & yPOS > layer.Layer_Rectangle.y_f)
                    {
                        sMat = layer.Layer_Material;
                        break;
                    }

                }
            }

            return sMat;
        }
    }
}
