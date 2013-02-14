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

            x_Nodes = 0;
            y_Nodes = 0;

            Assign_NodeCounts(LayerList);

            NodeList = new List<Node>();

            Generate_Mesh_ByLayer(LayerList);
        }

        public void Assign_NodeCounts(List<Layer> LayerList)
        {
            foreach (Layer c_Layer in LayerList)
            {
                c_Layer.Nodes = 150;
            }
        }

        // XNODES - Number of nodes in the x direction
        // YNODES - Number of nodes in the y direction
        private int XNODES;
        private int YNODES;


        // Accessor function for XNODES
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
                {
                    XNODES = 0;
                    Mesh_Errors.Post_Error("MESH ERROR:  Assignment of XNODES less than 0.");
                }
            }
        }

        // Accessor function for YNODES
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
                {
                    YNODES = 0;
                    Mesh_Errors.Post_Error("MESH ERROR:  Assignment of YNODES less than 0.");
                }
            }
        }

        // Main subroutine which generates the mesh, given XNODES and YNODES
        private void Generate_Mesh_ByLayer(List<Layer> LayerList)
        {
            Mesh_Errors.UpdateProgress_Text("Meshing...");

            foreach (Layer Layers in LayerList)
            {
                int k = Layers.getID();

                int progress = (k) / LayerList.Count;

                Mesh_Errors.UpdateProgress(progress);

                for (int i = 0; i < Layers.Nodes; i++)
                {
                    for (int j = 0; j < Layers.Nodes; j++)
                    {
                        float X_POS = Layers.dX((float)i);
                        float Y_POS = Layers.dY((float)i);

                        NodeList.Add(new Node(Mesh_Errors, i, j, 0.01f, 0.01f, X_POS, Y_POS, t_Nodes));

                        Check_Node(NodeList.Last(), Layers);

                        t_Nodes++;
                    }
                }

                Mesh_Errors.Post_Error("Note:  Layer " + Layers.getID() + " meshed succesfully.");

 
            }

            Mesh_Errors.UpdateProgress_Text("");

            Sort_Nodes(NodeList);
        }

   
        private void Sort_Nodes(List<Node> NodeList)
        {
            int initial_NodeCount = NodeList.Count;

            Mesh_Errors.UpdateProgress_Text("Sorting...");

            for (int i = 0; i < NodeList.Count; i++)
            {
                float x_int = NodeList[i].x_pos;
                float y_int = NodeList[i].y_pos;

                float percent_complete = ((float)i / (float)NodeList.Count) * 100;

                Mesh_Errors.UpdateProgress((int)Math.Ceiling(percent_complete));

                for (int ii = 0; ii < NodeList.Count; ii++)
                {

                    if (NodeList[ii].x_pos == x_int && NodeList[ii].y_pos == y_int)
                    {
                        NodeList.Remove(NodeList[ii]);
                    }
                    
                    
                }

            }

            Mesh_Errors.Post_Error("NOTE:  Initial node count:  " + initial_NodeCount.ToString() + ", Final node count:  " + NodeList.Count.ToString());
            Mesh_Errors.Post_Error("NOTE:  Finished sorting");

            var Sorted_NodeList = NodeList.OrderBy(node => node.x_pos).ThenBy(node => node.y_pos).GroupBy(pt => pt.x_pos).ToList();

            ListtoJaggedArray(Sorted_NodeList);
        }

        private Node[][] ListtoJaggedArray(IList<IGrouping<float, Node>> p_NodeList)
        {
            var result = new Node[p_NodeList.Count][];

            int n_Nodes_Total = 0;

            for (var i = 0; i < p_NodeList.Count; i++)
            {
                result[i] = p_NodeList[i].ToArray();
            }

            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result[i].Length; j++)
                {
                    n_Nodes_Total++;
                }
            }

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
