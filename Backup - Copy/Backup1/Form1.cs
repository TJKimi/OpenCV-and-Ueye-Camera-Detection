using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SimpleLive
{
    public partial class uEye_DotNet_Simple_Live : Form
    {
        private uEye.Camera Camera;
        IntPtr displayHandle = IntPtr.Zero;
        private bool bLive          = false;

        public uEye_DotNet_Simple_Live()
        {
            InitializeComponent();

            displayHandle = DisplayWindow.Handle;
            InitCamera();
        }

        private void InitCamera()
        {
            Camera = new uEye.Camera();

            uEye.Defines.Status statusRet = 0;

            // Open Camera
            statusRet = Camera.Init();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Camera initializing failed");
                Environment.Exit(-1);
            }

            // Allocate Memory
            statusRet = Camera.Memory.Allocate();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Allocate Memory failed");
                Environment.Exit(-1);
            }

            // Start Live Video
            statusRet = Camera.Acquisition.Capture();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Start Live Video failed");
            }
            else
            {
                bLive = true;
            }

            // Connect Event
            Camera.EventFrame += onFrameEvent;

            CB_Auto_Gain_Balance.Enabled = Camera.AutoFeatures.Software.Gain.Supported;
            CB_Auto_White_Balance.Enabled = Camera.AutoFeatures.Software.WhiteBalance.Supported;
        }

        private void onFrameEvent(object sender, EventArgs e)
        {
            uEye.Camera Camera = sender as uEye.Camera;

            Int32 s32MemID;
            Camera.Memory.GetActive(out s32MemID);

            Camera.Display.Render(s32MemID, displayHandle, uEye.Defines.DisplayRenderMode.FitToWindow);
        }
        
        private void Button_Live_Video_Click(object sender, EventArgs e)
        {
            // Open Camera and Start Live Video
            if (Camera.Acquisition.Capture() == uEye.Defines.Status.Success)
            {
                bLive = true;
            }
        }

        private void Button_Stop_Video_Click(object sender, EventArgs e)
        {
            // Stop Live Video
            if (Camera.Acquisition.Stop() == uEye.Defines.Status.Success)
            {
                bLive = false;
            }
        }
        
        private void Button_Freeze_Video_Click(object sender, EventArgs e)
        {
            if (Camera.Acquisition.Freeze() == uEye.Defines.Status.Success)
            {
                bLive = false;
            }
        }

        private void CB_Auto_White_Balance_CheckedChanged(object sender, EventArgs e)
        {
            Camera.AutoFeatures.Software.WhiteBalance.SetEnable(CB_Auto_White_Balance.Checked);
        }

        private void CB_Auto_Gain_Balance_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_Auto_Gain_Balance.Checked)
            {
                Camera.AutoFeatures.Software.Gain.SetEnable(true);
            }
            else
            {
                Camera.AutoFeatures.Software.Gain.SetEnable(false);
            }
        }

        private void Button_Load_Parameter_Click(object sender, EventArgs e)
        {
            uEye.Defines.Status statusRet = 0;

            Camera.Acquisition.Stop();

            Int32[] memList;
            statusRet = Camera.Memory.GetList(out memList);
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Get memory list failed: " + statusRet);
                Environment.Exit(-1);
            }

            statusRet = Camera.Memory.Free(memList);
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Free memory list failed: " + statusRet);
                Environment.Exit(-1);
            }

            statusRet = Camera.Parameter.Load("");
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Loading parameter failed: " + statusRet);
            }

            statusRet = Camera.Memory.Allocate();
            if (statusRet != uEye.Defines.Status.SUCCESS)
            {
                MessageBox.Show("Allocate Memory failed");
                Environment.Exit(-1);
            }

            if (bLive == true)
            {
                Camera.Acquisition.Capture();
            }
        }

        private void Button_Exit_Prog_Click(object sender, EventArgs e)
        {
            // Close the Camera
            Camera.Exit();
            Close();
        }
    }
}
