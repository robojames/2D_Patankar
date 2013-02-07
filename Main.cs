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
            myError = new ErrorHandler(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Main_Load(object sender, EventArgs e)
        {
            Material copper = new Material(myError, "Copper");
            copper.k = -5;
        }

       
    }
}
