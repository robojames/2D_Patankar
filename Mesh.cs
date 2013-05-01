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
        // max_Y - Maximum physical y location [m]
        //
        // The maximum y-direction is governed by the entire height of the microDTA
        // apparatus, from the base ceramic of the TEM to the thermocouple and beyond.
        /// <summary>
        /// Maximum x-direction [m]
        /// </summary>
        float max_X;

        /// <summary>
        /// Maximum y-direction [m]
        /// </summary>
        float max_Y;

        // Total number of nodes in physical system
        /// <summary>
        /// Total number of nodes utilized in the physical system
        /// </summary>
        private int t_Nodes;

        /// <summary>
        /// List of Nodes with which to temporarily store nodes prior to conversion to jagged array
        /// </summary>
        private List<Node> NodeList;

        /// <summary>
        /// Jagged array of nodes which is utilized during the rest of the analysis, organized by spatial distance from origin
        /// </summary>
        public Node[][] NodeArray;

        // Errorhandler class to handle errors with mesh calculations
        /// <summary>
        /// Local errorhandler
        /// </summary>
        ErrorHandler Mesh_Errors;

        /// <summary>
        /// Mesh Constructor
        /// </summary>
        /// <param name="local_ErrorHandler">Main UI ErrorHandler</param>
        /// <param name="LayerList">LayerList generated via the custom Geometry class files (TEMGeometry.cs for example)</param>
        public Mesh(ErrorHandler local_ErrorHandler, List<Layer> LayerList)
        {
            Mesh_Errors = local_ErrorHandler;

            t_Nodes = 0;

            NodeList = new List<Node>();

            Generate_Mesh_ByLayer(LayerList);

            Find_X_max_Y_max();

            SetBoundaryNodes(LayerList);
        }

        private void Find_X_max_Y_max()
        {
            max_X = 0.0f;
            max_Y = 0.0f;

            foreach (Node[] nodes in NodeArray)
            {
                foreach (Node node in nodes)
                {
                    if (node.x_pos > max_X)
                    {
                        max_X = node.x_pos;
                    }

                    if (node.y_pos > max_Y)
                    {
                        max_Y = node.y_pos;
                    }
                }
            }

            Debug.WriteLine("Maximum X:  " + max_X.ToString() + "       Maximum Y:  " + max_Y.ToString());
        }

        // Main subroutine which generates the mesh, given XNODES and YNODES
        /// <summary>
        /// Generates the nodal mesh
        /// </summary>
        /// <param name="LayerList">List of layers which is iterated over to generate the nodal mesh</param>
        private void Generate_Mesh_ByLayer(List<Layer> LayerList)
        {
            Mesh_Errors.UpdateProgress_Text("Meshing...");

            // Iterates over each Layer in the included geometry files (Currently just TEMGeometry.cs)
            foreach (Layer Layers in LayerList)
            {
                int layer_Nodes = 0;

                // Obtains current Layer ID
                int k = Layers.getID();

                // Calculates current progress to display on the Main UI
                int progress = (k) / LayerList.Count;

                // Passes calculated progress to the ErrorHandler which feeds in the data to the main UI
                Mesh_Errors.UpdateProgress(progress);

                // Main double-for loop which iterates twice over Layers.Nodes (this assumes that each
                // direction (x & y) has the same number of nodes, so that the total nodes for each layer
                // is Layers.Nodes^2
                for (int i = 0; i < Layers.Nodes; i++)
                {
                    for (int j = 0; j < Layers.Nodes; j++)
                    {
                        layer_Nodes++;

                        // Calculates the dX and dY values (can be changed depending on layer)
                        // but currently uses a step offset for the cv width with an otherwise
                        // uniform distribution
                        float X_POS = Layers.X((float)i);
                        float Y_POS = Layers.Y((float)j);

                        // Adds Node to NodeList
                        NodeList.Add(new Node(Mesh_Errors, X_POS, Y_POS, t_Nodes, k));

                        if (Check_Boundary(i, j, Layers.Nodes))
                            NodeList.Last().is_Boundary = true;

                        if (X_POS == Layers.adjusted_X0 && Y_POS == Layers.adjusted_Y0)
                        {
                            NodeList.Last().is_Corner = true;
                        }

                        if (X_POS == Layers.adjusted_XF && Y_POS == Layers.adjusted_YF)
                        {
                            NodeList.Last().is_Corner = true;
                        }                        


                        // Sets the Control Volume width for the current node equivalent to the equal spacing
                        // of the nodes
                        NodeList.Last().delta_X = Layers.node_Spacing;
                        NodeList.Last().delta_Y = Layers.node_Spacing;

                        // INITIALIZES the node directions, however, for boundary nodes this initialized
                        // value will NOT be true, and will need to be manually calculated by the NodeInitializer.cs
                        // class.
                        NodeList.Last().delta_x_E = Layers.node_Spacing;
                        NodeList.Last().delta_x_W = Layers.node_Spacing;
                        NodeList.Last().delta_y_S = Layers.node_Spacing;
                        NodeList.Last().delta_y_N = Layers.node_Spacing;

                        // Assigns layer material to current node
                        NodeList.Last().Material = Layers.Layer_Material;

                        // Feeds in the just-added node and checks it against the layer geometry to ensure proper positioning
                        Check_Node(NodeList.Last(), Layers);

                        // Increments the total number of perceived nodes--this value is used to report the number of nodes 
                        // generated for debugging purposes.
                        t_Nodes++;
                    }

                }

                // Reports that the current layer has been succesfully been meshed
                Mesh_Errors.Post_Error("Note:  Layer " + Layers.getID() + " meshed succesfully:  Nodes:  " + layer_Nodes.ToString() + " / " + t_Nodes.ToString());
            }

            // Clears Status text on the MainUI
            Mesh_Errors.UpdateProgress_Text("");


            
            // Calls the Sort_Nodes function which arranges the nodes into a jagged array Nodes[][]
            // and organizes them with respect to their physical distance from origin.
            Sort_Nodes(NodeList);
        }

        /// <summary>
        /// Checks to see if the current node being added is a boundary node
        /// </summary>
        /// <param name="i">Node index in the i (x) direction</param>
        /// <param name="j">Node index in the j (y) direction</param>
        /// <param name="NODES">Total number of nodes in the system in both (x) and (y) directions</param>
        /// <returns>Is current node a boundary</returns>
        private bool Check_Boundary(int i, int j, int NODES)
        {
            bool is_Boundary = false;

            if (i == 0)
                is_Boundary = true;
            if (i == NODES)
                is_Boundary = true;
            if (j == 0)
                is_Boundary = true;
            if (j == NODES)
                is_Boundary = true;

            return is_Boundary;
        }
        

        /// <summary>
        /// Sorts nodes by first eliminating duplicate nodes (typically around half), as well as arranges them into a 
        /// jagged array Node[][] which is arranged with respect to the physical distance in [m] from the origin
        /// </summary>
        /// <param name="NodeList">List of Node objects currently utilized by the system</param>
        private void Sort_Nodes(List<Node> NodeList)
        {
            // Grabs the initial number of nodes passed in via the NodeList
            int initial_NodeCount = NodeList.Count;

            // Updates the MainUI status Text
            Mesh_Errors.UpdateProgress_Text("Sorting...");

            // The NodeList is then sorted first by the x position, and then by the yposition (and then grouped via x position 'keys' before
            // being passed into the ListtoJaggedArray() function.
            var Sorted_NodeList = NodeList.OrderBy(node => node.x_pos).ThenBy(node => node.y_pos).GroupBy(pt => pt.x_pos).ToList();

            Mesh_Errors.UpdateProgress(100);

            NodeArray = ListtoJaggedArray(Sorted_NodeList);
        }

        // ListToJaggedArray
        //
        /// <summary>
        ///          When passed in a sorted NodeList, ListToJaggedArray arranges the nodes into a 2D jagged array 
        ///          of Node[][].  This ensures that Node[0][0] is the least node in the (x,y) and Node[0][1] is just
        ///          to the right of it, etc.
        /// </summary>
        /// <param name="p_NodeList">Grouped nodes with which to convert to a jagged array for further analysis</param>
        /// <returns></returns>
        private Node[][] ListtoJaggedArray(IList<IGrouping<float, Node>> p_NodeList)
        {
            // Creates new jagged array to hold NodeList data
            var result = new Node[p_NodeList.Count][];

            for (var i = 0; i < p_NodeList.Count; i++)
            {
                result[i] = p_NodeList[i].ToArray();
            }

            Mesh_Errors.Post_Error("NOTE:  Finished arranging node list into jagged array");

            // Display Jagged Array to User
            for (var i = 0; i < result[i].Count(); i++)
            {
                Mesh_Errors.Post_Error("NOTE:  Final Array is Node[" + result.Count().ToString() + ", " + result[i].Count().ToString() + "]");
            }

            for (int i = 0; i < result.Count(); i++)
            {
                for (int j = 0; j < result[i].Count(); j++)
                {
                    result[i][j].i = i;
                    result[i][j].j = j;
                }
            }

            // Returns Node[][] to user
            return result;

        }

        /// <summary>
        /// Checks the passed in node to see if it is inside the layer bounds
        /// </summary>
        /// <param name="node">Current node object to inspect</param>
        /// <param name="Current_Layer">Current layer in which the node object resides</param>
        private void Check_Node(Node node, Layer Current_Layer)
        {

            if (node.x_pos > Current_Layer.Layer_xf)
                Mesh_Errors.Post_Error("MESH ERROR:  Node assignment outside of layer bounds {xf-" + Current_Layer.Layer_xf.ToString() + ", x_node-" + node.x_pos.ToString() + "}");

            if (node.y_pos > Current_Layer.Layer_y0)
                Mesh_Errors.Post_Error("MESH ERROR:  Node assignment outside of layer bounds {y0-" + Current_Layer.Layer_y0.ToString() + ", y_node-" + node.y_pos.ToString() + "}");

            if (node.y_pos < Current_Layer.Layer_yf)
                Mesh_Errors.Post_Error("MESH ERROR:  Node assignment outside of layer bounds {yf-" + Current_Layer.Layer_yf.ToString() + ", y_node-" + node.y_pos.ToString() + "}");

            if (node.x_pos < Current_Layer.Layer_x0)
                Mesh_Errors.Post_Error("MESH ERROR:  Node assignment outside of layer bounds {x0-" + Current_Layer.Layer_x0.ToString() + ", x_node-" + node.x_pos.ToString() + "}");

        }

        private void SetBoundaryNodes(List<Layer> LayerList)
        {
            int n_Set = 0;

            foreach (Node[] Array in NodeArray)
            {
                foreach (Node node in Array)
                {
                    if (node.is_Boundary == true && node.is_Corner == false) // Need way to check for comp boundary
                    {
                        // For the case when the node is on the left hand side
                        if (node.x_pos == LayerList[node.Layer_ID].adjusted_X0)
                        {
                            // Look west until position variable != same node.Layer_ID by decrementing
                            // phi_x_s and checking its layer position
                            float phi_x_s = node.x_pos;
                            int Layer_ID_Current = node.Layer_ID;

                            while (Layer_ID_Current == node.Layer_ID)
                            {
                                // Check if negative
                                phi_x_s = phi_x_s - (node.delta_x_E);

                                if (phi_x_s < 0)
                                    break;

                                // Check which layer phi_x_s resides in
                                foreach (Layer layer in LayerList)
                                {
                                    // If this evaluates to TRUE then the node resides in the current layer
                                    if ((phi_x_s < layer.Layer_xf) && (phi_x_s > layer.Layer_x0) && (node.y_pos < layer.Layer_y0) && (node.y_pos > layer.Layer_yf))
                                    {
                                        // Pulls the layer ID out for use
                                        Layer_ID_Current = layer.getID();
                                    }
                                }
                            }

                            // Now that the adjacent layer ID is found, iterate over each node at the neighboring layer's x_f line (in the y direction)
                            // to find the two nodes that have the minimum dY to the current node
                            float Xf_Of_Neighbor = LayerList[Layer_ID_Current].adjusted_XF;

                            List<float> minDY = new List<float>();
                            List<int> minDY_NodeIDs = new List<int>();

                            float current_Min = 1000.0f;

                            foreach (Node[] nodes in NodeArray)
                            {
                                foreach (Node node_iter in nodes)
                                {
                                    // This pulls all of the nodes on the xf line of the neighbor
                                    if (node_iter.Layer_ID == Layer_ID_Current && node_iter.x_pos == Xf_Of_Neighbor)
                                    {
                                        if (Math.Abs(node_iter.y_pos - node.y_pos) <= current_Min)
                                        {
                                            current_Min = Math.Abs(node_iter.y_pos - node.y_pos);

                                            minDY.Add(current_Min);
                                            minDY_NodeIDs.Add(node_iter.Node_ID);
                                        }
                                    }
                                }
                            }

                            // This is to ensure that you are not dealing with a boundary node
                            if (minDY_NodeIDs.Count < 2)
                                break;

                            // Pull two smallest values from abs_Delta_Y list
                            int minID_1 = minDY_NodeIDs[minDY_NodeIDs.Count - 1];
                            int minID_2 = minDY_NodeIDs[minDY_NodeIDs.Count - 2];

                            Debug.WriteLine("Node ID_1:  " + minID_1.ToString() + "        Node ID_2:  " + minID_2.ToString());

                            // Since the nodes are always constructed from upper left to bottom right, the higher
                            // the ID number, the lower the y-value, thus minID_1 has to be the closest neighbor
                            // but this doesn't matter since I need the respective y-values of each
                            float Y_Pos_ID_1 = 0.0f;
                            float Y_Pos_ID_2 = 0.0f;

                            foreach (Node[] nodes in NodeArray)
                            {
                                foreach (Node node_iter in nodes)
                                {
                                    if (node_iter.Node_ID == minID_1)
                                        Y_Pos_ID_1 = node_iter.y_pos;

                                    if (node_iter.Node_ID == minID_2)
                                        Y_Pos_ID_2 = node_iter.y_pos;
                                }
                            }

                            if (Y_Pos_ID_1 > Y_Pos_ID_2)
                            {
                                node.Neighbor_1_ID = minID_1;
                                node.Neighbor_2_ID = minID_2;
                                n_Set++;
                            }
                            else
                            {
                                node.Neighbor_1_ID = minID_2;
                                node.Neighbor_2_ID = minID_1;
                                n_Set++;
                            }
                        }

                        // For the case when the node is on the right hand side
                        if (node.x_pos == LayerList[node.Layer_ID].adjusted_XF)
                        {

                        }

                        // For the case when the node is on top of a given layer
                        if (node.y_pos == LayerList[node.Layer_ID].adjusted_Y0)
                        {

                        }
 
                        // For the case when the node is on the bottom of a given layer
                        if (node.y_pos == LayerList[node.Layer_ID].adjusted_YF)
                        {

                        }
                    }

                }
            }

            Debug.WriteLine("Number of Nodes hit:  " + n_Set.ToString());
        }
    }
}
