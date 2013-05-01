using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace _2D_Patankar_Model
{
    class CSVWriter
    {
        public CSVWriter(Node[][] NodeArray)
        {
            writeNodePositions(NodeArray);
        }

        public void writeNodePositions(Node[][] NodeArray)
        {

            try
            {
                SaveFileDialog saveFile = new SaveFileDialog();

                saveFile.Filter = "CSV|*.csv";
                saveFile.Title = "Save the data file:";
                saveFile.DefaultExt = "*.csv";

                saveFile.ShowDialog();

                string directory = saveFile.FileName;

                TextWriter dataWrite = new StreamWriter(directory);

                dataWrite.WriteLine("Node ID" + "," + "XPOS" + "," + "YPOS" + "," + "Material");

                //for (int i = 1; i < scanCount; i++)
                  //  dataWrite.WriteLine(ampArraytoWrite[i] + "," + timeArray[i] + "," + tempArray[i] + "," + dTdt[i]);

                foreach (Node[] node in NodeArray)
                {
                    foreach (Node ind_node in node)
                    {
                        int node_Material = 0;

                        if (ind_node.Material == "Copper")
                            node_Material = 1;
                        else if (ind_node.Material == "BiTe")
                            node_Material = 2;
                        else if (ind_node.Material == "Ceramic")
                            node_Material = 3;
                        else if (ind_node.Material == "Air")
                            node_Material = 4;

                        dataWrite.WriteLine(ind_node.Node_ID + "," + ind_node.x_pos + "," + ind_node.y_pos + "," + node_Material);
                    }
                }
                
                 
        

                dataWrite.Close();
            }
            catch
            {
                MessageBox.Show("Error saving file, or writing was canceled ", "Error!", MessageBoxButtons.OK);
            }
        }
    }
}
