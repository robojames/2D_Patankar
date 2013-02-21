using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Patankar_Model
{
    // Material Manager.cs
    //
    // Class to provide a means with which to manage any imported or utilized material.  Accesses data to pull in and generate
    // material objects from Material.cs class definition.  Will eventually be updated to allow for temperature dependent
    // properties to be fully utilized, along with interpolation.

    class MaterialManager
    {
        // A series of const floats which represent several material properties of materials utilized in the numeric model:
        //
        // rho_XX = Density of Material XX [kg/m^3]
        // k_XX = Thermal Conductivity of Material XX [W/mK]
        // cp_XX = Heat Capacity of Material XX [J/kgK]
        // alpha_XX = Seebeck Coefficient of Material XX [V/K]

        /// <summary>
        /// Density of Copper [kg/m^3] at 20 C
        /// </summary>
        const float rho_Copper = 8960.0f;

        /// <summary>
        /// Thermal Conductivity of Copper [W / m K] at 20 C
        /// </summary>
        const float k_Copper = 401.0f;

        /// <summary>
        /// Heat Capacity of Copper [J/kg K] at 20 C
        /// </summary>
        const float cp_Copper = 390.0f;

        /// <summary>
        /// Density of Ceramic Alumina used in TEM [kg/m^3] at 20 C
        /// </summary>
        const float rho_Ceramic = 3750.0f;

        /// <summary>
        /// Thermal Conductivity of Ceramic Alumina used in TEM [W / m K] at 20 C
        /// </summary>
        const float k_Ceramic = 35.0f;
        
        /// <summary>
        /// Heat Capacity of Ceramic Alumina used in TEM [J/kg K] at 20 C
        /// </summary>
        const float cp_Ceramic = 775.0f;

        /// <summary>
        /// Density of Bismuth Telluride [kg/m^3] at 20 C
        /// </summary>
        const float rho_BiTe = 7700.0f;

        /// <summary>
        /// Thermal Conductivity of Bismuth Telluride [W / m K] at 20 C
        /// </summary>
        const float k_BiTe = 1.48f;
        
        /// <summary>
        /// Heat Capacity of Bismuth Telluride [J/kg K] at 20 C
        /// </summary>
        const float cp_BiTe = 122.0f;
        
        /// <summary>
        /// Density of glass [kg/m^3] at 20 C
        /// </summary>
        const float rho_Glass = 2225.0f;
        
        /// <summary>
        /// Thermal Conductivity of Glass [W / m K] at 20 C
        /// </summary>
        const float k_Glass = 14.0f;

        /// <summary>
        /// Heat Capacity of glass [J/kg K] at 20 C
        /// </summary>
        const float cp_Glass = 835.0f;

        // FIX THESE VALUES HAHHHHHH
        
        /// <summary>
        /// Density of Air [kg/m^3] at 20 C
        /// </summary>
        const float rho_Air = 25.0f;

        /// <summary>
        /// Thermal Conductivity of Air [W / m K] at 20 C
        /// </summary>
        const float k_Air = 25.0f;

        /// <summary>
        /// Heat Capacity of air [J/kg K] at 20 C
        /// </summary>
        const float cp_Air = 25.0f;

        const float alpha_BiTE = 0.2f; // Need to fix this value

        /// <summary>
        /// Errorhandler to pass messages to the main UI
        /// </summary>
        ErrorHandler MaterialManager_Errors;

        /// <summary>
        /// Material list for passing into other sub-classes
        /// </summary>
        public List<Material> Material_List;

        // Default constructor for the MaterialManager class.  Error handler is passed in to allow for reporting of errors
        // through the base class, Material.
        /// <summary>
        /// Manages Materials, eventually to be updated to include temperature based thermophysical property interpolation
        /// </summary>
        /// <param name="local_ErrorHandler">Main UI ErrorHandler</param>
        public MaterialManager(ErrorHandler local_ErrorHandler)
        {
            MaterialManager_Errors = local_ErrorHandler;
            Material_List = new List<Material>();
            Create_Materials();
        }

        /// <summary>
        /// Iterates over the material list and finds a match to the passed in string, returning its thermal conductivity
        /// </summary>
        /// <param name="material">Passed in material string, ie "Air"</param>
        /// <returns>Thermal Conductivity</returns>
        public float Get_Gamma(string material)
        {
            float Gamma = -10.0f;

            for (int i = 0; i < Material_List.Count; i++)
            {
                if (Material_List[i].Mat_Name == material)
                {
                    Gamma = Material_List[i].k;
                }
            }

            if (Gamma == -10.0f)
            {
                MaterialManager_Errors.Post_Error("MATERIAL MANAGER ERROR:  No match found for material " + material);
            }

            return Gamma;
        }

        // Create_Materials()
        /// <summary>
        /// Initializes materials utilized in the numeric simulation.  Currently set up to allow for the determination 
        /// of constant thermal conductivities though a simple modification to the material class will allow for eventual 
        /// interpolation of the thermal conductivity 
        /// </summary>
        private void Create_Materials()
        {
            MaterialManager_Errors.UpdateProgress_Text("Creating Materials");
            // Number of materials to be created.  Could possibly be pulled from the main UI if necessary.
            const int n_Materials = 5;

            // String array to hold the material names for visualization and error reporting
            string[] MatList_Name = new string[n_Materials] { "Copper", "Glass", "BiTe", "Ceramic" , "Air"};

            // Float array to hold value of thermal conductivites
            float[] k = new float[n_Materials] { k_Copper, k_Glass, k_BiTe, k_Ceramic, k_Air};

            float[] rho = new float[n_Materials] { rho_Copper, rho_Glass, rho_BiTe, rho_Ceramic, rho_Air };

            float[] cp = new float[n_Materials] { cp_Copper, cp_Glass, cp_BiTe, cp_Ceramic, cp_Air };

            // Iterates from 0 to n_Materials - 1 to create n_Materials number of Material objects.  
            for (int i = 0; i < n_Materials; i++ )
            {
                // Creates new Material object and adds it to the Material_List for later recollection or modification
                Material_List.Add(new Material(MaterialManager_Errors, MatList_Name[i]));

                Material_List[i].k = k[i];

                Material_List[i].rho = rho[i];

                if (MatList_Name[i] != "BiTe")
                {
                    Material_List[i].alpha = 0.0f;
                }
                else
                {
                    Material_List[i].alpha = alpha_BiTE;
                }

                Material_List[i].cp = cp[i];

                // Reports to the user what materials have been added
                MaterialManager_Errors.Post_Error("NOTE: " + MatList_Name[i] + " has been added as a material to the project:  k=" + k[i].ToString() + ", rho = " + rho[i].ToString() + ", alpha = " + Material_List[i].alpha.ToString() + ", Cp = " + Material_List[i].cp.ToString());
            }



        }


    }
}
