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

using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using System.Drawing;

namespace UXTool_V4._Page
{
    /// <summary>
    /// Interaction logic for KeyFrame_Page.xaml
    /// </summary>
    public partial class KeyFrame_Page : Page
    {
        string _filepath;
        public KeyFrame_Page()
        {
            InitializeComponent();

            KeyFrameControlHeader keyframeheader1 = new KeyFrameControlHeader();
            KeyFrameControlHeader keyframeheader2 = new KeyFrameControlHeader();
            Automatic.Items.Add(keyframeheader1);
            Manual.Items.Add(keyframeheader2);
        }

        public void ReadIn_From_File(string filepath)
        {
            Automatic.Items.Clear();
            Manual.Items.Clear();
            _filepath = filepath;
            FileInfo fi = new FileInfo(filepath + "Automatic_Keyframes.csv");
            if (File.Exists(filepath + "Automatic_Keyframes.csv") && !IsFileLocked(fi))
            {
                StreamReader inputfile = new StreamReader(filepath + "Automatic_Keyframes.csv");

                KeyFrameControlHeader keyframeheader = new KeyFrameControlHeader();
                Automatic.Items.Add(keyframeheader);
                int i = 1;
                while (!inputfile.EndOfStream)
                {
                    string[] temp = inputfile.ReadLine().Split(',');

                    KeyFrameControl newdataentry = new KeyFrameControl("Automatic" + i.ToString());
                    newdataentry.Idnumber = i;
                    newdataentry.Emotion.Text = temp[0];
                    newdataentry.EmotionValue.Text = temp[1];
                    newdataentry.TimeStamp.Text = temp[2];
                    newdataentry.Duration.Text = temp[3];
                    newdataentry.textBox.Text = temp[4];
                    newdataentry.Filepath = temp[5];
                    newdataentry.ProcessFunction = Update_Manual;
                    Automatic.Items.Add(newdataentry);
                    i++;
                }
                inputfile.Close();
            }

            fi = new FileInfo(filepath + "Manual_Keyframes.csv");
            if (File.Exists(filepath + "Manual_Keyframes.csv") && !IsFileLocked(fi))
            {
                StreamReader inputfile = new StreamReader(filepath + "Manual_Keyframes.csv");


                KeyFrameControlHeader keyframeheader = new KeyFrameControlHeader();
                Manual.Items.Add(keyframeheader);
                int i = 1;
                while (!inputfile.EndOfStream)
                {
                    string[] temp = inputfile.ReadLine().Split(',');

                    KeyFrameControl newdataentry = new KeyFrameControl("Manual" + i.ToString());
                    newdataentry.Idnumber = i;
                    newdataentry.Emotion.Text = temp[0];
                    newdataentry.EmotionValue.Text = temp[1];
                    newdataentry.TimeStamp.Text = temp[2];
                    newdataentry.Duration.Text = temp[3];
                    newdataentry.textBox.Text = temp[4];
                    newdataentry.Filepath = temp[5];
                    newdataentry.checkBox.Checked += CheckBox_Checked;
                    Manual.Items.Add(newdataentry);
                    i++;
                }
                Button saveoff = new Button();
                saveoff.Width = 50;
                saveoff.Height = 50;
                saveoff.Click += Saveoff_Click;
                saveoff.Content = "Save";
                Manual.Items.Add(saveoff);

                inputfile.Close();
            }
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            return;
        }

        private void Saveoff_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!File.Exists(_filepath + @"\Manual_Keyframes.csv"))
            {
                StreamWriter outputfile = new StreamWriter(_filepath + @"\Manual_Keyframes.csv", true);
            }
            FileInfo fi = new FileInfo(_filepath + @"\Manual_Keyframes.csv");
            if (!IsFileLocked(fi))
            {
                using (StreamWriter outputfile = new StreamWriter(_filepath + @"\Manual_Keyframes.csv",false))
                {
                    for (int i = 1; i < Manual.Items.Count-1; i++)
                    {
                        KeyFrameControl kf = Manual.Items[i] as KeyFrameControl;

                        outputfile.Write(kf.Emotion.Text + ',');
                        outputfile.Write(kf.EmotionValue.Text + ',');
                        outputfile.Write(kf.TimeStamp.Text + ',');
                        outputfile.Write(kf.Duration.Text + ',');
                        outputfile.Write(kf.textBox.Text + ',');
                        outputfile.WriteLine(kf.Filepath);
                    }
                    outputfile.Close();
                }
            }
        }



        private void Update_Manual()
        {
            FileInfo fi = new FileInfo(_filepath + "Manual_Keyframes.csv");
            if (File.Exists(_filepath + "Manual_Keyframes.csv") && !IsFileLocked(fi))
            {
                Manual.Items.Clear();
                StreamReader inputfile = new StreamReader(_filepath + "Manual_Keyframes.csv");

                KeyFrameControlHeader keyframeheader = new KeyFrameControlHeader();
                Manual.Items.Add(keyframeheader);
                int i = 1;
                while (!inputfile.EndOfStream)
                {
                    string[] temp = inputfile.ReadLine().Split(',');

                    KeyFrameControl newdataentry = new KeyFrameControl("Manual" + i.ToString());
                    newdataentry.Idnumber = i;
                    newdataentry.Emotion.Text = temp[0];
                    newdataentry.EmotionValue.Text = temp[1];
                    newdataentry.TimeStamp.Text = temp[2];
                    newdataentry.Duration.Text = temp[3];
                    newdataentry.textBox.Text = temp[4];
                    newdataentry.Filepath = temp[5];

                    Manual.Items.Add(newdataentry);
                    i++;
                }
                Button saveoff = new Button();
                saveoff.Width = 50;
                saveoff.Height = 50;
                saveoff.Click += Saveoff_Click;
                saveoff.Content = "Save";
                Manual.Items.Add(saveoff);
                inputfile.Close();
            }
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
