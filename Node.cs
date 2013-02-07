using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Patankar_Model
{
    class Node
    {
        ErrorHandler NodeErrors;

        public Node(ErrorHandler local_ErrorHandler)
        {
            NodeErrors = local_ErrorHandler;
        }

        public int i { get; private set; }
        public int j { get; private set; }
        public float x_pos { get; set; }
        public float y_pos { get; set; }
        public float T { get; set; }
        public float gamma { get; set; }
        public float delta_X { get; set; }
        public float delta_Y { get; set; }
        public float P { get; set; }
        public float Q { get; set; }
        public float sc { get; set; }
        public float sp { get; set; }
        public float d { get; set; }
    }
}
