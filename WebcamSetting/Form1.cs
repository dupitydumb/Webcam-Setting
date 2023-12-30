using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WebcamSetting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            panel1.BackColor = Color.FromArgb(0, Color.Black);
            comboBox1.SelectionChangeCommitted += new EventHandler(comboBox1_SelectionChangeCommitted);
            comboBox2.SelectionChangeCommitted += new EventHandler(comboBox2_SelectionChangeCommitted);
            button1.Click += new EventHandler(button1_Click);


            GetCamera();

        }

        public event EventHandler SelectionChangeCommitted;
        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCaptureDevice;

        string videoSourceName;

        private void GetCamera()
        {
            comboBox1.Items.Add("Select Camera");
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in filterInfoCollection)
            {
                comboBox1.Items.Add(filterInfo.Name);
            }
            comboBox1.SelectedIndex = 0;
            videoSourceName = comboBox1.SelectedItem.ToString();

        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {

            videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[comboBox1.SelectedIndex - 1].MonikerString);
            videoSourceName = comboBox1.SelectedItem.ToString();
            videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            videoCaptureDevice.Start();

            foreach (var resolution in videoCaptureDevice.VideoCapabilities)
            {
                comboBox2.Items.Add(resolution.FrameSize.Width + "x" + resolution.FrameSize.Height);
            }



     
        }

        private void comboBox2_SelectionChangeCommitted(object sender, EventArgs e)
        {
            videoCaptureDevice.Stop();
            videoCaptureDevice.VideoResolution = videoCaptureDevice.VideoCapabilities[comboBox2.SelectedIndex];
            videoCaptureDevice.Start();
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        //On form close

        //Run Bat file to setting webcam
        private void button1_Click(object sender, EventArgs e)
        {
            Setting();
        }


        private void Setting()
        {
            try
            {
                if (videoCaptureDevice.IsRunning != null && videoCaptureDevice.IsRunning)
                {
                    //check if setting.bat is exist at Documents/WebcamSetting/setting.bat
                    if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\WebcamSetting\\setting.bat"))
                    {

                        ExcecuteBat();
                    }
                    else
                    {
                        //Show yes no dialog
                        MessageBox.Show("setting.bat not found, do you want to create it?", "Warning", MessageBoxButtons.YesNo);
                        //if not exist, create it
                        Writebat();
                        //Run bat file

                    }
                }
                else
                {
                    MessageBox.Show("Please select camera", "Warning", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please Select Camera", "Error", MessageBoxButtons.OK);
            }
            
            
        }


        private void Writebat()
        {
            //Create new path file at Documents/WebcamSetting/setting.bat
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\WebcamSetting\\setting.bat";

            //Create new folder at Documents/WebcamSetting
            System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\WebcamSetting");

            //Copy ffmpeg.exe to Documents/WebcamSetting/ffmpeg.exe
            System.IO.File.Copy(AppDomain.CurrentDomain.BaseDirectory + "ffmpeg.exe", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\WebcamSetting\\ffmpeg.exe", true);
            //Write bat file
            string[] lines = { "chcp 65001 > nul", "ffmpeg -f dshow -show_video_device_dialog true -i video=" + '"' + videoSourceName + '"' };
            System.IO.File.WriteAllLines(path, lines);
            MessageBox.Show("Exported to " + path);

        }

        private void ExcecuteBat()
        {
            try
            {
                //get root path
                string path = AppDomain.CurrentDomain.BaseDirectory + "setting.bat";
                //Open bat file
                System.Diagnostics.Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("File not found", "Error", MessageBoxButtons.OK);
            }
            
            
        }
    }
}