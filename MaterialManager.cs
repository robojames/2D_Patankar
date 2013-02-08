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
    // material objects from Material.cs class definition.
    class MaterialManager
    {
        ErrorHandler MaterialManager_Errors;

        List<Material> Material_List;

        // Default constructor for the MaterialManager class.  Error handler is passed in to allow for reporting of errors
        // through the base class, Material.
        public MaterialManager(ErrorHandler local_ErrorHandler)
        {
            MaterialManager_Errors = local_ErrorHandler;
            Material_List = new List<Material>();
            Create_Materials();
        }


        // Create_Materials()
        //
        // Initializes materials utilized in the numeric simulation.  Currently set up to allow for the determination of constant thermal conductivities
        // though a simple modification to the material class will allow for eventual interpolation of the thermal conductivity
        private void Create_Materials()
        {
            // Number of materials to be created.  Could possibly be pulled from the main UI if necessary.
            const int n_Materials = 5;

            // String array to hold the material names for visualization and error reporting
            string[] MatList_Name = new string[n_Materials] { "Copper", "Glass", "BiTe", "Aluminum", "Ceramic" };

            // Float array to hold value of thermal conductivites
            float[] k = new float[n_Materials] { 400.0f, 35.0f, 12.0f, 200.0f, 40.0f };

            // Iterates from 0 to n_Materials - 1 to create n_Materials number of Material objects.  
            for (int i = 0; i < n_Materials; i++ )
            {
                // Creates new Material object and adds it to the Material_List for later recollection or modification
                Material_List.Add(new Material(MaterialManager_Errors, MatList_Name[i]));

                Material_List[i].k = k[i];

                // Reports to the user what materials have been added
                MaterialManager_Errors.Post_Error("NOTE: " + MatList_Name[i] + " has been added as a material to the project");
            }



        }

        

       

    }
}
