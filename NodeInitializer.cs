using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Patankar_Model
{
    class NodeInitializer
    {
        ErrorHandler Node_I_ErrorHandler;

        Node[][] NodeArray;

        MaterialManager Mat_Manager;

        public NodeInitializer(Node[][] Nodes, ErrorHandler local_ErrorHandler, MaterialManager Materials)
        {
            NodeArray = Nodes;

            Node_I_ErrorHandler = local_ErrorHandler;

            Mat_Manager = Materials;

            Assign_Materials();

            Calculate_dxdy();
        }

        

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

        private void Calculate_dxdy()
        {
            int boundaryNodes = 0;

            for (int i = 0; i < NodeArray.Count(); i++)
            {
                for (int j = 0; j < NodeArray[i].Count(); j++)
                {
                    if (NodeArray[i][j].is_Boundary)
                    {
                        boundaryNodes++;
                    }

                }

            }

            Node_I_ErrorHandler.Post_Error("Boundary nodes:  " + boundaryNodes.ToString());
        }
        
    }
}
