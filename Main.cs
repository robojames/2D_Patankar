using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2D_Patankar_Model
{
    public partial class Main : Form
    {
        ErrorHandler myError;

        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MaterialManager myManager = new MaterialManager(myError);

            Mesh myMesh = new Mesh(myError, 50, 50);

            TEMGeometry myGeometry = new TEMGeometry(myError);

        }

        private void Main_Load(object sender, EventArgs e)
        {
            myError = new ErrorHandler(this);
        }

        public void UpdateProgress(int percent_Complete)
        {
            if (percent_Complete >= 0 && percent_Complete <= 100)
                progressBar1.Value = percent_Complete;

            if (percent_Complete < 100)
                button1.Enabled = false;
            else
                button1.Enabled = true;
        }

        public void UpdateProgress_Text(string status_Text)
        {
            label1.Text = "Progress: " + status_Text;
        }
       
    }
}
