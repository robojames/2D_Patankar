using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Patankar_Model
{
    class BoundaryConditions
    {
        Node[][] Nodes;

        public BoundaryConditions(Node[][] Nodal_Array)
        {
            Nodes = Nodal_Array;
        }
    }
}
