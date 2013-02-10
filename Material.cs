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
        // Need to implement Material Properties as F(T) as well
        
        // Declare ErrorHandler object to display errors to the user
        ErrorHandler Material_Errors;

        // String to cover the name of the material
        public string Mat_Name { get; private set; }

        // Constructor, currently requires the passing of the local ErrorHandler (on the main form) and the Material Name
        public Material(ErrorHandler local_ErrorHandler, string Material_Name)
        {
            Material_Errors = local_ErrorHandler;
            Mat_Name = Material_Name;
        }

        //
        // Uppercase letters denote class-only functions.  Access is given to other members (lowercase) via the get and set accessors.
        //

        // Thermal Conductivity
        private float K;

        // Seebeck Coefficient
        private float Alpha;

        // Specific Heat (Constant Pressure)
        private float CP;

        // Density
        private float Density;


        //
        // Specific Heat Capacity (Constant Pressure) [J / kg K]
        //
        public float cp
        {
            get
            {
                return CP;
            }
            set
            {

                if (value >= 0)
                    CP = value;
                else
                {
                    CP = 0.0f;
                    Material_Errors.Post_Error("MATERIAL ERROR:  Specific Heat set to 0 for Material:  " + Mat_Name);
                }
            }
        }

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
                    Material_Errors.Post_Error("MATERIAL ERROR:  Thermal Conductivity Set to 0 for Material:  " + Mat_Name);
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
                if (value >= 0)
                    Alpha = value;
                else
                {
                    Alpha = 0.0f;
                    Material_Errors.Post_Error("MATERIAL ERROR:  Seebeck Coefficient set to 0 for Material:  " + Mat_Name);
                }
            }
        }

        //
        // Density (kg/m^3)
        //
        public float rho
        {
            get
            {
                return Density;
            }
            set
            {
                if (value >= 0)
                    Density = value;
                else
                {
                    Density = 0;
                    Material_Errors.Post_Error("MATERIAL ERROR:  Density set to 0 for Material:  " + Mat_Name);
                }
            }
        }


    }
}
