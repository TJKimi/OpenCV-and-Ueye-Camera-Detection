using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge.Imaging;
using System.Drawing.Imaging;
using System.IO;
using AForge.Imaging.Filters;
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
        
        int pictureCounter = 1;
        int referenceX =0;
        int referenceY =0;
        int calibratedNum = 16;
        Bitmap referencePic;
        private void button1_Click(object sender, EventArgs e)
        {//picture button and save
        
            try
            {
                //AForge.Imaging.Filters.Invert inv = new AForge.Imaging.Filters.Invert();
                String pictureName = textBox1.Text;

                //Bitmap picture = new Bitmap(DisplayWindow.Image); //videoSourcePlayer.GetCurrentVideoFrame();
                //ResizeBilinear refilt = new ResizeBilinear(400, 300);
                Camera.Image.Save(pictureCounter + pictureName + ".jpg");
                
                //Bitmap picture = Camera.Image; //videoSourcePlayer.GetCurrentVideoFrame();
                Bitmap pictureCopy = new Bitmap(pictureCounter + pictureName + ".jpg");
                //refilt.Apply(pictureCopy);
                pictureBox1.Image = pictureCopy;
                //Bitmap pictureCopy = new Bitmap(picture);
                //pictureCopy.Save(pictureName + pictureCounter + ".jpg", ImageFormat.Jpeg);// save into Bin folder as a jpeg file
                
               
                
                if (pictureCounter == 1)//get the reference
                {
                    //referencePicture = ()videoSourcePlayer.GetCurrentVideoFrame();
                    //BlobCounter bc = new BlobCounter(pictureCopy);
                    //bc.MinHeight = 1000;//required parameters
                    //bc.MinWidth = 1000;
                    //bc.MaxHeight = 1100;
                    //bc.MaxWidth = 1100;
                    //inv.ApplyInPlace(pictureCopy);
                    //bc.BackgroundThreshold = Color.Black;
                    //bc.ProcessImage(pictureCopy);
                    //Rectangle[] rects = bc.GetObjectsRectangles(); // gets the toop left hand corner of the x and y coordinates

                    //Crop filter = new Crop(new Rectangle (400, 50, 500, 500));
                    referencePic = pictureCopy;
                    

                    Grayscale grayScale = new Grayscale(0.2125, 0.7154, 0.0721);
                    referencePic = grayScale.Apply(referencePic);

                    Threshold threshold = new Threshold(50);
                    threshold.ApplyInPlace(referencePic);

                    BlobCounter bc = new BlobCounter(referencePic);
                    bc.FilterBlobs = true;
                    bc.MinHeight = 900;//these values will have to change in order to fit your tests
                    bc.MinWidth = 900;

                    bc.ProcessImage(referencePic);
                    Blob[] blobs = bc.GetObjectsInformation();
                    Rectangle[] rects = bc.GetObjectsRectangles();
                    
                    foreach (Blob blob in blobs)
                    {
                        bc.ExtractBlobsImage(referencePic, blob, true);
                        //Bitmap blobimage = new Bitmap(blob.Image.ToManagedImage());
                        
                    }

                    referenceX = rects[0].X;//(rects[0].Width / 2) + rects[0].X;
                    referenceY = rects[0].Y;//(rects[0].Height / 2) - rects[0].Y;
                }
                    //pictureCopy.Dispose();//free up memory in system
                    //pictureCopy = null;
                    pictureCounter++;

                    //DirectoryInfo parentDirectory = new DirectoryInfo(Environment.CurrentDirectory);//Directory where you can find the pictures
                    //FileInfo[] files = parentDirectory.GetFiles("CaptureImage.jpg", SearchOption.TopDirectoryOnly);
                    //foreach (FileInfo fi in files)
                    //{
                    //    files[0].Delete();
                    //}
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something Didn't work \r\n"+ex);
            }

        }//picture button and save end

        private void AnalyizeButton_Click(object sender, EventArgs e)
        {//analyize button
             

            //string CurrDate = string.Format("YY-MM-dd", DateTime.Now);
            //string txtPath = Environment.CurrentDirectory;
            //using (StreamWriter outputFile = new StreamWriter(txtPath + @"DistanceAnalysis"))
            //{
            //    outputFile.WriteLine("54321");
            //}

            StreamWriter tx = new StreamWriter("Displacement Analysis.txt");//should save into directory
            tx.Write("Displacement Analysis\r\n\r\nCalibrated Number:\t "+calibratedNum +"\r\nReference X:\t" + referenceX + "\r\nReference Y:\t" + referenceY);
            tx.Write("\r\n\r\nRepetition\tX\t\t\tY\r\n");

            //AForge.Imaging.Filters.Invert inv = new AForge.Imaging.Filters.Invert();
            DirectoryInfo parentDirectory = new DirectoryInfo(Environment.CurrentDirectory);//Directory where you can find the pictures
            FileInfo[] files = parentDirectory.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);//This will only get the jpg image on the top directory. Sub directorys not included.

            //ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.925f);

            int pictureFilesCounter = 0;
            foreach (FileInfo fi in files)
            {
                Bitmap refimg = new Bitmap(files[pictureFilesCounter].FullName);

                //TemplateMatch[] matchings = tm.ProcessImage(refimg, referencePic);

                //BitmapData data = refimg.LockBits(new Rectangle(0, 0, refimg.Width, refimg.Height), ImageLockMode.ReadWrite, refimg.PixelFormat);

                int x = 0;
                int y = 0;

                if (pictureFilesCounter == 0)
                {
                    foreach (TemplateMatch m in matchings)
                    {
                        Drawing.Rectangle(data, m.Rectangle, Color.White);
                        x = m.Rectangle.X;
                        y = m.Rectangle.Y;
                    }
                }
                else
                {
                    foreach (TemplateMatch m in matchings)
                    {
                        Drawing.Rectangle(data, m.Rectangle, Color.White);
                        x = m.Rectangle.X;
                        y = m.Rectangle.Y;
                    }
                }
                refimg.UnlockBits(data);
                //Bitmap refimgClone = new Bitmap(refimg.Width, refimg.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                //using (Graphics gr = Graphics.FromImage(refimgClone))
                //{
                //    gr.DrawImage(refimg, new Rectangle(0, 0, refimgClone.Width, refimgClone.Height));
                //}
                //refimg.Dispose();
                //refimg = null;

                //inv.ApplyInPlace(refimgClone);

                //BlobCounter bc = new BlobCounter(refimgClone);

                //bc.MinHeight = 900;//required parameters
                //bc.MinWidth = 900;

                //bc.BackgroundThreshold = Color.Black;

                //bc.ProcessImage(refimgClone);
                //Rectangle[] rects = bc.GetObjectsRectangles();
                //pictureBox1.Image = refimgClone;

                //double x = rects[0].X;
                //double y = rects[0].Y;

                tx.Write(pictureFilesCounter + "\t\t" + x + "\t\t\t" + y + "\r\n");

                //double x = (rects[pictureFilesCounter].Width / 2) + rects[pictureFilesCounter].X;
                //double y = (rects[pictureFilesCounter].Height / 2) - rects[pictureFilesCounter].Y;

                pictureFilesCounter++;
            }//for each loop end
            tx.Close();
        }

        private void DisplayWindow_Click(object sender, EventArgs e)
        {

        }

        private void uEye_DotNet_Simple_Live_Load(object sender, EventArgs e)
        {

        }
    }
}
