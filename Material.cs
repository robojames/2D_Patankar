using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace _2D_Patankar_Model
{
    class Material
    {
        ErrorHandler Material_Errors;

        private string Mat_Name;

        public Material(ErrorHandler local_ErrorHandler, string Material_Name)
        {
            Material_Errors = local_ErrorHandler;
            Mat_Name = Material_Name;
        }

        private float K;
        private float Alpha;

        //
        // Thermal Conductivity (W / m K)
        //
        public float k
        {
            get
            {
                return K;
            }
            set
            {
                if (value >= 0)
                    K = value;
                else
                {
                    Material_Errors.Post_Error("ERROR:  Thermal Conductivity Set to 0 for Material:  " + Mat_Name);
                    K = 0;
                }
            }
        }

        //
        // Seebeck Coefficient (V / K), Note:  Need to check to see if p and n values are used, or if Alpha_P = Alpha_N = Alpha.
        //
        public float alpha
        {
            get
            {
                return Alpha;
            }
            set
            {
                Alpha = value;
            }
        }




    }
}
