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

        // Minimal incremental dx and dy changes
        private float min_DX;
        private float min_DY;

        // Node[,] is a 2D array of Node objects, which constitute the mesh of the given
        // system.
        public Node[, ,] Nodes;

        // Errorhandler class to handle errors with mesh calculations
        ErrorHandler Mesh_Errors;

        public Mesh(ErrorHandler local_ErrorHandler, List<Layer> LayerList)
        {
            Mesh_Errors = local_ErrorHandler;

            t_Nodes = 0;

            x_Nodes = 0;
            y_Nodes = 0;

            min_DX = 5;
            min_DY = 5;

            Assign_NodeCounts(LayerList);

            Nodes = new Node[20,20,48];

            Generate_Mesh_ByLayer(LayerList);
        }

        public void Assign_NodeCounts(List<Layer> LayerList)
        {
            foreach (Layer c_Layer in LayerList)
            {
                c_Layer.Nodes = 20;
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
                    x_Nodes++;

                    for (int j = 0; j < Layers.Nodes; j++)
                    {
                        if (j==0)
                            y_Nodes++;

                        float X_POS = Layers.dX((float)i);
                        float Y_POS = Layers.dY((float)i);

                        if (Layers.dX() < min_DX)
                        {
                            min_DX = Layers.dX();
                            Mesh_Errors.Post_Error("NOTE:  New min_DX set:  " + min_DX.ToString());
                        }

                        if (Layers.dY() < min_DY)
                        {
                            min_DY = Layers.dY();
                            Mesh_Errors.Post_Error("NOTE:  New min_DY set:  " + min_DY.ToString());
                        }

                        Nodes[i, j, k] = new Node(Mesh_Errors, i, j, 0.01f, 0.01f, X_POS, Y_POS);

                        Check_Node(Nodes[i, j, k], Layers);

                        t_Nodes++;
                    }
                }

                Mesh_Errors.Post_Error("Note:  Layer " + Layers.getID() + " meshed succesfully.");

 
            }


            Mesh_Errors.UpdateProgress_Text("");

            Mesh_Errors.Post_Error("NOTE:  Total Nodes:  " + t_Nodes.ToString() + " = XNODES (" + x_Nodes.ToString() + ") + YNODES (" + y_Nodes.ToString() + ")");

            Sort_Nodes(Nodes);
        }

        private void Sort_Nodes(Node[,,] NodeList)
        {
            Node[,] Sorted_Nodes = new Node[140,140];
            int ii = 0;
            int jj = 0;

            for (int x = 0; x < 5000; x++)
            {
                float currX = 0.00f;
                float currY = 0.00f;
                foreach (Node node in NodeList)
                {
                    if (node != null)
                    {
                        for (int i = 0; i < 5000; i++)
                        {
                            if (node.x_pos <= (float)i * min_DX && node.y_pos <= currY && node.is_Sorted == false)
                            {
                                Sorted_Nodes[ii, jj] = node;
                                node.is_Sorted = true;
                                ii++;
                            }

                            if (node.x_pos <= currX && node.y_pos <= (float)i * min_DY && node.is_Sorted == false)
                            {
                                Sorted_Nodes[ii, jj] = node;
                                node.is_Sorted = true;
                                jj++;
                            }

                        }

                    }
                }

                currX += min_DX;
                currY += min_DY;
            }
            for (int i = 0; i < 140; i++)
            {
                for (int j = 0; j < 140; j++)
                {
                    Mesh_Errors.Post_Error("Node (" + i.ToString() + "," + j.ToString() + ") = (" + Sorted_Nodes[i, j].x_pos.ToString() + "," + Sorted_Nodes[i, j].y_pos.ToString() + ")");
                }
            }

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
