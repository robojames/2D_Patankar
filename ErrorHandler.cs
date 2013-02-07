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
    public class ErrorHandler : Form
    {
        TextBox ErrorBox;
       
        public ErrorHandler(Main Main_Screen)
        {
            ErrorBox = Main_Screen.textBox1;
        }

        public void Post_Error(string text)
        {
            ErrorBox.Text += text + Environment.NewLine; 
        }
    }
}
