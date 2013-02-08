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
    // material objects from Material.cs class definition.  Material manager also handles the interpolation of thermophysical 
    // material properties.
    class MaterialManager
    {
        ErrorHandler MaterialManager_Errors;

        public MaterialManager(ErrorHandler local_ErrorHandler)
        {
            MaterialManager_Errors = local_ErrorHandler;
        }


    }
}
