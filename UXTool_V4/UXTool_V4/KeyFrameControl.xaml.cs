// UX Research Tools Applications
//
// Inital build worked on by:
// 
//  Marc Bordoy
//      Email: marcbordoy135@gmail.com
//      Skype: marc.bordoy1
//      November 2016 until March 2017
//  
//  Randall Missun
//      rmissun123@gmail.com
//
//      December 2016 until April 2017 
//

using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace UXTool_V4
{
    /// <summary>
    /// Interaction logic for KeyFrameControl.xaml
    /// </summary>
    public partial class KeyFrameControl : UserControl
    {
        string filepath;
        int idnumber;
        private ProcessDelegate process;

        public ProcessDelegate ProcessFunction
        {
            get
            {
                return process;
            }
            set
            {
                process = value;
            }
        }

        public string Filepath
        {
            get
            {
                return filepath;
            }
            set
            {
                filepath = value;
            }
        }

        public int Idnumber
        {
            get
            {
                return idnumber;
            }

            set
            {
                idnumber = value;
            }
        }

        public KeyFrameControl(string name)
        {
            InitializeComponent();
            Line.Name = name;
            checkBox.Checked += CheckBox_Checked;
            checkBox.Unchecked += CheckBox_Unchecked;
            button.Click += Button_Click;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int start_time = (int)float.Parse(TimeStamp.Text);
            int end_time = 1 + (int)float.Parse(Duration.Text);
            

            string filepathdirectory = filepath.Substring(0, filepath.Length - filepath.Split('\\').Last().Length);
            filepathdirectory = filepathdirectory + "VideoClips\\";
            if (!Directory.Exists(filepathdirectory))
            {
                Directory.CreateDirectory(filepathdirectory);
            }
            string output = filepathdirectory + Line.Name + ".wmv";
            string args = "-i \"" + filepath + "\" -ss " + start_time + " -t " + end_time + " \"" + output + '\"';
            System.Diagnostics.Process.Start("ffmpeg.exe", args);
        }
        public void ChangeColor(Brush newcolor)
        {
            Line.Background = newcolor;
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ItemsControl hi = Parent as ItemsControl;
            if (hi.Name == "Manual")
            {

            }
            else
            {
                Line.Background = Brushes.Yellow;
                string filepathdirectory = filepath.Substring(0, filepath.Length - filepath.Split('\\').Last().Length);

                if (!File.Exists(filepathdirectory + @"\Manual_Keyframes.csv"))
                {
                    StreamWriter outputfile = new StreamWriter(filepathdirectory + @"\Manual_Keyframes.csv", true);
                    outputfile.Close();
                }
                FileInfo fi = new FileInfo(filepathdirectory + @"\Manual_Keyframes.csv");
                if (!IsFileLocked(fi))
                {
                    using (StreamWriter outputfile = new StreamWriter(filepathdirectory + @"\Manual_Keyframes.csv", true))
                    {
                        outputfile.Write(Emotion.Text + ',');
                        outputfile.Write(EmotionValue.Text + ',');
                        outputfile.Write(TimeStamp.Text + ',');
                        outputfile.Write(Duration.Text + ',');
                        outputfile.Write("Automatic Keyframe" + ',');
                        outputfile.WriteLine(filepath);
                        outputfile.Close();
                        process();
                    }
                    
                }
            }
        }

        private void CheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            Line.Background = Brushes.White;
        }

        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to , opened or doesnt exist
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}
