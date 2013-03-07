using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Patankar_Model
{
    class Solver
    {
        public void Solve_2D(Node[][] Nodes, float sol_Tol)
        {
            int n_iter = 0;

            float[] Phi_X = new float[Nodes.GetLength(0)];
            float[] P_X = new float[Nodes.GetLength(0)];
            float[] Q_X = new float[Nodes.GetLength(0)];

            float[] Phi_Y = new float[Nodes.GetLength(1)];
            float[] P_Y = new float[Nodes.GetLength(1)];
            float[] Q_Y = new float[Nodes.GetLength(1)];

            float max_Err = 1;

            int x_nodes_max = Nodes.GetLength(0) - 1;
            int y_nodes_max = Nodes.GetLength(1) - 1;

            while (max_Err > sol_Tol)
            {
                // Traveling in positive x-direction
                // A:  AP
                // B:  AE
                // C:  AW
                // D:  d

                for (int j = 1; j < y_nodes_max; j++)
                {
                    P_X[0] = Nodes[0, j].AE / Nodes[0, j].AP;
                    Q_X[0] = Nodes[0, j].d / Nodes[0, j].AP;

                    for (int i = 1; i < x_nodes_max + 1; i++)
                    {
                        P_X[i] = Nodes[i, j].AE / (Nodes[i, j].AP - Nodes[i, j].AW * P_X[i - 1]);
                        Q_X[i] = (Nodes[i, j].d + Nodes[i, j].AN * Nodes[i, j + 1].phi + Nodes[i, j].AS * Nodes[i, j - 1].phi + Nodes[i, j].AW * Q_X[i - 1]) / (Nodes[i, j].AP - Nodes[i, j].AW * P_X[i - 1]);
                    }



                    Nodes[x_nodes_max, j].phi = Q_X[x_nodes_max];

                    for (int i = x_nodes_max - 1; i >= 0; --i)
                    {
                        Nodes[i, j].phi = P_X[i] * Nodes[i + 1, j].phi + Q_X[i];
                        //Debug.WriteLine("Node[" + i.ToString() + "," + j.ToString() + "] = " + Nodes[i, j].phi.ToString());   
                    }

                }

                // max_Err = Calculate_Average_Error(Nodes);

                // Traveling in positive y-direction
                // A: AP
                // B: AN
                // C: AS
                // D: d
                for (int i = 1; i < x_nodes_max; i++)
                {
                    P_Y[0] = Nodes[i, 0].AN / Nodes[i, 0].AP;
                    Q_Y[0] = Nodes[i, 0].d / Nodes[i, 0].AP;

                    for (int j = 1; j < y_nodes_max + 1; j++)
                    {
                        P_Y[j] = Nodes[i, j].AN / (Nodes[i, j].AP - Nodes[i, j].AS * P_Y[j - 1]);
                        Q_Y[j] = (Nodes[i, j].d + Nodes[i, j].AE * Nodes[i + 1, j].phi + Nodes[i, j].AW * Nodes[i - 1, j].phi + Nodes[i, j].AS * Q_Y[j - 1]) / (Nodes[i, j].AP - Nodes[i, j].AS * P_Y[j - 1]);
                    }

                    Nodes[i, y_nodes_max].phi = Q_Y[y_nodes_max];

                    for (int j = y_nodes_max - 1; j >= 0; --j)
                    {
                        Nodes[i, j].phi = P_Y[j] * Nodes[i, j + 1].phi + Q_Y[j];
                        //Debug.WriteLine("Node[" + i.ToString() + "," + j.ToString() + "] = " + Nodes[i, j].phi.ToString());   
                    }
                }


                max_Err = 100;

                n_iter++;

                //Debug.WriteLine("Iteration: " + n_iter.ToString());

            } // End While Loop 

        } // End Function
    }
}
