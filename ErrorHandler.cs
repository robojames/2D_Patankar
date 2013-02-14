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
        // Initialize two localized references to form objects on Main UI
        Main MainUIReference;
        TextBox ErrorBox;

        // Default Constructor
        //
        // Requires the passing of Main_Screen (object of type Main:Form) to allow
        // for all other classes to interface with the main UI to allow for both
        // debug updates and progress reports.
        public ErrorHandler(Main Main_Screen)
        {
            // Initializes the localized reference form objects
            ErrorBox = Main_Screen.textBox1;

            MainUIReference = Main_Screen;

        }

        // Post_Error
        //
        // Allows for each class and subroutine to post messages to the "Debug" text box
        // listed on the main user interface.  Messages sent in are written to the text
        // box only if the Debug check box is checked on the Main UI.  
        public void Post_Error(string text)
        {
            if (MainUIReference.CheckBoxDebug.Checked)
            {
                // Enters debug message along with a return (enter) to allow for further
                // messages to be readable. 
                ErrorBox.Text += text + Environment.NewLine;

                // If lots of messages are being sent (such as during Mesh generation, this
                // keeps the UI from being completely unresponsive.
                Application.DoEvents();
            }
        }

        // Clear
        //
        // Clears the debug box of all message text
        public void Clear()
        {
            // Clears the Debug error box of all text
            ErrorBox.Clear();
        }

        // UpdateProgress
        //
        // int - percent_Complete, number of 0-100 defining the completeness of a task
        //
        // Allows for all other classes to pass along update information to the Main UI to
        // keep the user from being bored as the mesh is generated and the numeric simulation
        // is ran.
        public void UpdateProgress(int percent_Complete)
        {
            // Calls UpdateProgress() on Main.cs
            MainUIReference.UpdateProgress(percent_Complete);

            if (percent_Complete == 100)
            {
                MainUIReference.UpdateProgress(0);
                UpdateProgress_Text("");
            }

            Application.DoEvents();
        }

        // UpdateProgress_Text
        //
        // string - status_Text, string containing status information for the progress bar label
        //
        // Allows for all other classes to pass along status text with which to label the progress
        // bar progress with.  Its fun.
        public void UpdateProgress_Text(string status_Text)
        {
            // Calls UpdateProgress_Text() on Main.cs
            MainUIReference.UpdateProgress_Text(status_Text);
        }
        

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ErrorHandler
            // 
            this.ClientSize = new System.Drawing.Size(344, 58);
            this.Name = "ErrorHandler";
            this.Load += new System.EventHandler(this.ErrorHandler_Load);
            this.ResumeLayout(false);

        }

        private void ErrorHandler_Load(object sender, EventArgs e)
        {

        }
    }
}
