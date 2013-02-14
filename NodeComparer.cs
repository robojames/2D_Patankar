using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Patankar_Model
{
    class NodeComparer : IEqualityComparer<Node>
    {
        struct Point
        {
            public float x, y;

            public Point(float p_x, float p_y)
            {
                x = p_x;
                y = p_y;
            }
        }

        public bool Equals(Node node_1, Node node_2)
        {
            Point node1_Point = new Point(node_1.x_pos, node_1.y_pos);
            Point node2_Point = new Point(node_2.x_pos, node_2.y_pos);

            if (node1_Point.x == node2_Point.x && node1_Point.y == node2_Point.y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(Node obj)
        {
            return obj.Node_ID;
        }

        
    }
}
