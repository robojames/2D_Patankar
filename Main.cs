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
            bool[] BC_CONVECTION = new bool[4];
            bool[] BC_CONST_T = new bool[4];
            bool[] BC_ADIABATIC = new bool[4];

            // 0 - TOP
            // 1 - LEFT
            // 2 - SOUTH
            // 3 - RIGHT
            BC_CONST_T[0] = false;
            BC_CONST_T[1] = false;
            BC_CONST_T[2] = true;
            BC_CONST_T[3] = false;

            BC_CONVECTION[0] = true;
            BC_CONVECTION[1] = false;
            BC_CONVECTION[2] = false;
            BC_CONVECTION[3] = false;

            BC_ADIABATIC[0] = false;
            BC_ADIABATIC[1] = true;
            BC_ADIABATIC[2] = false;
            BC_ADIABATIC[3] = true;

            float[] h_Coefficient = new float[4];
            float[] T_Constant = new float[4];
            float[] T_infinity = new float[4];

            h_Coefficient[0] = 15.0f;
            h_Coefficient[1] = 0.0f;
            h_Coefficient[2] = 0.0f;
            h_Coefficient[3] = 0.0f;

            T_Constant[0] = 0.0f;
            T_Constant[1] = 0.0f;
            T_Constant[2] = 275.0f;
            T_Constant[3] = 0.0f;

            T_infinity[0] = 288.0f;
            T_infinity[1] = 0.0f;
            T_infinity[2] = 0.0f;
            T_infinity[3] = 0.0f;

            MaterialManager myManager = new MaterialManager(myError);

            TEMGeometry myGeometry = new TEMGeometry(myError);

            Mesh myMesh = new Mesh(myError, myGeometry.Layer_List);

            NodeInitializer myInitializer = new NodeInitializer(myMesh.NodeArray, myError, myManager, myGeometry.Layer_List, 0.5f, 5.0f, false);

            BoundaryConditions myBC = new BoundaryConditions(myMesh.NodeArray, myError, BC_CONVECTION, BC_CONST_T, BC_ADIABATIC, h_Coefficient, T_infinity, T_Constant);

            Solver mySolver = new Solver(myError, myMesh.NodeArray);

            mySolver.Solve_2D(0.005f);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            myError = new ErrorHandler(this);
        }

        public void UpdateProgress(int percent_Complete)
        {
            if (percent_Complete >= 0 && percent_Complete <= 100)
                progressBar1.Value = percent_Complete;

            if (percent_Complete <= 100)
                button1.Enabled = false;
            else
                button1.Enabled = true;
        }

        public void UpdateProgress_Text(string status_Text)
        {
            label1.Text = "";
            label1.Text = "Progress: " + status_Text;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }


    }
}
