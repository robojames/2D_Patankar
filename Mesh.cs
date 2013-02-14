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
        // max_X - Maximum physical x location [m]
        //
        // The maximum x-direction is governed by the width of the TE module

        // max_Y - Maximum physical y location [m]
        //
        // The maximum y-direction is governed by the entire height of the microDTA
        // apparatus, from the base ceramic of the TEM to the thermocouple and beyond.
        const float max_X = 0.0401828f;
        const float max_Y = 0.004864f;

        // Total number of nodes in physical system
        private int t_Nodes;

        public List<Node> NodeList;

        // Errorhandler class to handle errors with mesh calculations
        ErrorHandler Mesh_Errors;

        public Mesh(ErrorHandler local_ErrorHandler, List<Layer> LayerList)
        {
            Mesh_Errors = local_ErrorHandler;

            t_Nodes = 0;

            NodeList = new List<Node>();

            Generate_Mesh_ByLayer(LayerList);
        }

        // Main subroutine which generates the mesh, given XNODES and YNODES
        private void Generate_Mesh_ByLayer(List<Layer> LayerList)
        {
            Mesh_Errors.UpdateProgress_Text("Meshing...");

            // Iterates over each Layer in the included geometry files (Currently just TEMGeometry.cs)
            foreach (Layer Layers in LayerList)
            {
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
                        // Calculates the dX and dY values (can be changed depending on layer)
                        // but currently uses a step offset for the cv width with an otherwise
                        // uniform distribution
                        float X_POS = Layers.dX((float)i, Layers.Nodes);
                        float Y_POS = Layers.dY((float)j, Layers.Nodes);

                        // Adds Node to NodeList
                        NodeList.Add(new Node(Mesh_Errors, 0.01f, 0.01f, X_POS, Y_POS, t_Nodes));

                        // Feeds in the just-added node and checks it against the layer geometry to ensure proper positioning
                        Check_Node(NodeList.Last(), Layers);

                        // Increments the total number of perceived nodes--this value is used to report the number of nodes 
                        // generated for debugging purposes.
                        t_Nodes++;
                    }

                }

                // Reports that the current layer has been succesfully been meshed
                Mesh_Errors.Post_Error("Note:  Layer " + Layers.getID() + " meshed succesfully.");
            }

            // Clears Status text on the MainUI
            Mesh_Errors.UpdateProgress_Text("");

            // Calls the Sort_Nodes function which arranges the nodes into a jagged array Nodes[][]
            // and organizes them with respect to their physical distance from origin.
            Sort_Nodes(NodeList);
        }

        // Sort_Nodes
        //
        // Sorts nodes by first eliminating duplicate nodes (typically around half), as well as
        // arranges them into a jagged array Node[][] which is arranged with respect to the physical distance
        // in [m] from the origin
        private void Sort_Nodes(List<Node> NodeList)
        {
            // Grabs the initial number of nodes passed in via the NodeList
            int initial_NodeCount = NodeList.Count;

            // Updates the MainUI status Text
            Mesh_Errors.UpdateProgress_Text("Sorting...");

            // Main for loop which iterates over the entire node list
            for (int i = 0; i < NodeList.Count; i++)
            {
                // Each node in the node list has an x and y position which is 
                // checked against the entire node array
                float x_int = NodeList[i].x_pos;
                float y_int = NodeList[i].y_pos;

                // Calculates the percentage complete based on the current node
                // count (which is reduced each time a removal is performed)
                float percent_complete = ((float)i / (float)NodeList.Count) * 100;

                // Updates MainUI with progress
                Mesh_Errors.UpdateProgress((int)Math.Ceiling(percent_complete));

                // Secondary for loop which compares the indicated value of x_int and y_int
                // with all other nodes in the nodelist.
                for (int ii = 0; ii < NodeList.Count; ii++)
                {
                    // Checks to see if the node position of interest (xint,yint) is equivalent
                    // to the currently observed node
                    if (NodeList[ii].x_pos == x_int && NodeList[ii].y_pos == y_int && i != ii)
                    {
                        // Removes currently observed node from node list
                        NodeList.Remove(NodeList[ii]);
                    }   
                }

            }

            // Reports to user both the eliminated number of notes, and that the operation is finished
            Mesh_Errors.Post_Error("NOTE:  Initial node count:  " + initial_NodeCount.ToString() + ", Final node count:  " + NodeList.Count.ToString());
            Mesh_Errors.Post_Error("NOTE:  Finished sorting and removing duplicates");

            // The NodeList is then sorted first by the x position, and then by the yposition (and then grouped via x position 'keys' before
            // being passed into the ListtoJaggedArray() function.
            var Sorted_NodeList = NodeList.OrderBy(node => node.x_pos).ThenBy(node => node.y_pos).GroupBy(pt => pt.x_pos).ToList();

            ListtoJaggedArray(Sorted_NodeList);
        }

        // ListToJaggedArray
        //
        // When passed in a sorted NodeList, ListToJaggedArray arranges the nodes into a 2D jagged array 
        // of Node[][].  This ensures that Node[0][0] is the least node in the (x,y) and Node[0][1] is just
        // to the right of it, etc.
        private Node[][] ListtoJaggedArray(IList<IGrouping<float, Node>> p_NodeList)
        {
            // Creates new jagged array to hold NodeList data
            var result = new Node[p_NodeList.Count][];

            for (var i = 0; i < p_NodeList.Count; i++)
            {
                result[i] = p_NodeList[i].ToArray();
            }

            Mesh_Errors.Post_Error("NOTE:  Finished arranging node list into jagged array");

            // Returns Node[][] to user
            return result;

        }

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
    }
}
