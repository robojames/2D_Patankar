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
            LayerList = Layers;

            NodeArray = Nodes;

            Node_I_ErrorHandler = local_ErrorHandler;

            Mat_Manager = Materials;

            x_MinpDX = Nodes[0][0].x_pos;
            y_MinpDY = Nodes[0][0].y_pos;

            Assign_Materials();

            Calculate_dxdy();
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
        private void Calculate_dxdy()
        {
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

                if (LayerList[node.Layer_ID].adjusted_X0 == node.x_pos && node.x_pos != x_MinpDX)
                {
                    // Adjust delta_x_W
                    node.delta_x_W = FindClosestBoundary_X(node, Positions_X);
                }

                if (LayerList[node.Layer_ID].adjusted_XF == node.x_pos && node.x_pos != x_MinpDX)
                {
                    // Adjust delta_x_E
                    node.delta_x_E = FindClosestBoundary_X(node, Positions_X);
                }

                if (LayerList[node.Layer_ID].adjusted_Y0 == node.y_pos && node.y_pos != y_MinpDY)
                {
                    // Adjust delta_y_N
                    node.delta_y_N = FindClosestBoundary_Y(node, Positions_Y);
                }

                if (LayerList[node.Layer_ID].adjusted_YF == node.y_pos && node.y_pos != y_MinpDY)
                {
                    // Adjust delta_y_S
                    node.delta_y_S = FindClosestBoundary_Y(node, Positions_Y);
                }

                percent_Progress++;
            }

            Node_I_ErrorHandler.UpdateProgress(100);

            Node_I_ErrorHandler.UpdateProgress_Text("");

            Node_I_ErrorHandler.Post_Error("Boundary nodes:  " + BoundaryNodes.Count.ToString());
        }

        /// <summary>
        /// Finds the closest boundary position (either vertical (Y) or horizontal (X)) to the passed
        /// in node element
        /// </summary>
        /// <param name="node">Node object's position is used to calculate the distance from a list of positions</param>
        /// <param name="Positions">List of positions to be checked</param>
        /// <returns>Distance in meters of the closest nodal boundary</returns>
        private float FindClosestBoundary_X(Node node, List<float> Positions)
        {
            float delta_X = 100.0f;

            foreach (float pos in Positions)
            {
                if (pos != node.x_pos)
                {
                    float val = Math.Abs(node.x_pos - pos);

                    if (val < delta_X)
                        delta_X = val;
                }
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
            float delta_X = 100.0f;

            foreach (float pos in Positions)
            {
                if (pos != node.x_pos)
                {
                    float val = Math.Abs(node.x_pos - pos);

                    if (val < delta_X)
                        delta_X = val;
                }
            }

            return delta_X;
        }
        
    }
}
