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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UXTool_V4.Features;
using System.IO;
using Accord.Video.DirectShow;
using Accord.DirectSound;
using Accord.Audio;
using Accord.Controls;

namespace UXTool_V4._Page
{
    /// <summary>
    /// Interaction logic for Record_Page.xaml
    /// </summary>
    public partial class Record_Page : Page
    {
        CameraRecorder camera = null;
        ScreenRecorder screen = null;
        AudioRecorder in_audio = null;
        AudioRecorder out_audio = null;
        VideoCaptureDevice video_capture_device;
        string directory;
        private string full_directory;
        private string camera_filepath;
        private string screen_filepath;
        private string in_audio_filepath;
        private string out_audio_filepath;
        bool stop_pressed = false;
        bool is_recording = false;
        FilterInfoCollection video_devices;
        IAudioSource in_audio_source;
        IAudioSource out_sound_source;

        public string Record_Directory
        {
            get { return directory; }
            set { directory = value; }
        }

        public string Camera_Filepath
        {
            get { return camera_filepath; }
            set { camera_filepath = value; }
        }

        public string Screen_Filepath
        {
            get { return screen_filepath; }
            set { screen_filepath = value; }
        }

        public string AudioInput_Filepath
        {
            get { return in_audio_filepath; }
            set { in_audio_filepath = value; }
        }

        public string AudioOutput_Filepath
        {
            get { return out_audio_filepath; }
            set { out_audio_filepath = value; }
        }

        public string Full_Directory
        {
            get { return full_directory; }
            set { full_directory = value; }
        }

        public bool Is_Recording
        {
            get { return is_recording; }
        }

        public VideoCaptureDevice Video_Capture_Device
        {
            get { return video_capture_device; }
            set { video_capture_device = value; }
        }

        public bool Stop_Pressed
        {
            get { return stop_pressed; }
            set { stop_pressed = value; }
        }

        public Record_Page(string _filepath)
        {
            InitializeComponent();
            directory = full_directory = _filepath;

            RecordButton.Click += Record_Button_Click;
            StopButton.Click += Stop_Button_Click;
            StopButton.IsEnabled = false;
            AddStudyButton.Click += AddStudy_Button_Click;

            Identify_VideoSources();
            Identify_AudioInputs();
            Identify_AudioOutputs();
        }

        private void AddStudy_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog filepath = new System.Windows.Forms.FolderBrowserDialog();
            filepath.RootFolder = Environment.SpecialFolder.MyComputer;
            if (filepath.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                full_directory = filepath.SelectedPath + '\\';
        }

        private void Record_Button_Click(object sender, RoutedEventArgs e)
        {
            if (CameraRecordCheckbox.IsChecked.Value && VideoDevices.Items.Count != 0)
                Setup_Camera(full_directory, Remove_Empty_Space(CameraFilenameTextbox.Text));
            if (ScreenRecordCheckbox.IsChecked.Value)
                Setup_Screen(full_directory, Remove_Empty_Space(ScreenFilenameTextbox.Text));
            if (MicRecordCheckbox.IsChecked.Value && MicrophoneDevices.Items.Count != 0)
                Setup_AudioInput(full_directory, Remove_Empty_Space(AudioInputFilenameTextbox.Text));
            if (SoundRecordCheckbox.IsChecked.Value && SoundDevices.Items.Count != 0)
                Setup_AudioOutput(full_directory, Remove_Empty_Space(AudioOutputFilenameTextbox.Text));

            if (ScreenRecordCheckbox.IsChecked.Value)
                screen.Start_ScreenRecording();
            if (CameraRecordCheckbox.IsChecked.Value && VideoDevices.Items.Count != 0)
                camera.Start_CameraRecording();
            if (MicRecordCheckbox.IsChecked.Value && MicrophoneDevices.Items.Count != 0)
                in_audio.Start_AudioRecording();
            if (SoundRecordCheckbox.IsChecked.Value && SoundDevices.Items.Count != 0)
                out_audio.Start_AudioRecording();

            RecordButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            CameraFeed.VideoSource = video_capture_device;
            CameraFeed.Width = video_capture_device.VideoResolution.FrameSize.Width;
            CameraFeed.Height = video_capture_device.VideoResolution.FrameSize.Height;
            CameraFeed.Start();

            CameraRecordCheckbox.IsEnabled = false;
            ScreenRecordCheckbox.IsEnabled = false;
            MicRecordCheckbox.IsEnabled = false;
            SoundRecordCheckbox.IsEnabled = false;

            VideoDevices.IsEnabled = false;
            MicrophoneDevices.IsEnabled = false;
            SoundDevices.IsEnabled = false;

            Console.WriteLine("Recording started. " + DateTime.Now.ToString());
            if (ScreenRecordCheckbox.IsChecked.Value)
                screen.Start();
            if (CameraRecordCheckbox.IsChecked.Value && VideoDevices.Items.Count != 0)
                camera.Start();
            if (MicRecordCheckbox.IsChecked.Value && MicrophoneDevices.Items.Count != 0)
                in_audio.Start();
            if (SoundRecordCheckbox.IsChecked.Value && SoundDevices.Items.Count != 0)
                out_audio.Start();
            is_recording = true;
        }

        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            Stop_Recording();
        }

        private void Identify_VideoSources()
        {
            video_devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (video_devices.Count != 0)
            {
                //all devices that are webcams/cameras
                foreach (FilterInfo device in video_devices)
                    VideoDevices.Items.Add(device.Name);

                VideoDevices.SelectedIndex = 0;
                VideoDevices.SelectionChanged += delegate(object sender, SelectionChangedEventArgs e)
                {
                    video_capture_device = new VideoCaptureDevice(video_devices[VideoDevices.SelectedIndex].MonikerString);
                };

                video_capture_device = new VideoCaptureDevice(video_devices[VideoDevices.SelectedIndex].MonikerString);
            }
            else
            {
                //no webcams/cameras
                CameraRecordCheckbox.IsEnabled = false;
                CameraRecordCheckbox.IsChecked = false;
            }
        }

        private void Identify_AudioInputs()
        {
            AudioDeviceCollection mic_devices = new AudioDeviceCollection(AudioDeviceCategory.Capture);
            foreach (AudioDeviceInfo device in mic_devices)
                MicrophoneDevices.Items.Add(device);

            if (MicrophoneDevices.Items.Count != 0)
            {
                //all devices that are microphones
                MicrophoneDevices.SelectedIndex = 0;
                MicrophoneDevices.SelectionChanged += delegate(object sender, SelectionChangedEventArgs e)
                {
                    var curr = SoundDevices.SelectedItem as AudioDeviceInfo;
                    if (curr == null)
                    {
                        MessageBox.Show("No sound devices available.");
                        return;
                    }

                    in_audio_source = new AudioCaptureDevice(curr)
                    {
                        Format = SampleFormat.Format16Bit,
                        DesiredFrameSize = 1370,
                        SampleRate = 44100
                    };
                };

                var info = MicrophoneDevices.SelectedItem as AudioDeviceInfo;
                in_audio_source = new AudioCaptureDevice(info)
                {
                    Format = SampleFormat.Format16Bit,
                    DesiredFrameSize = 1370,
                    SampleRate = 44100
                };
            }
            else
            {
                //no microphones
                MicRecordCheckbox.IsEnabled = false;
                MicRecordCheckbox.IsChecked = false;
            }
        }

        private void Identify_AudioOutputs()
        {
            AudioDeviceCollection sound_devices = new AudioDeviceCollection(AudioDeviceCategory.Output);
            foreach (AudioDeviceInfo device in sound_devices)
                SoundDevices.Items.Add(device);

            if (SoundDevices.Items.Count != 0)
            {
                //all devices that are speakers
                SoundDevices.SelectedIndex = 0;
                SoundDevices.SelectionChanged += delegate(object sender, SelectionChangedEventArgs e)
                {
                    var curr = SoundDevices.SelectedItem as AudioDeviceInfo;
                    if (curr == null)
                    {
                        MessageBox.Show("No audio devices available.");
                        return;
                    }

                    out_sound_source = new AudioCaptureDevice(curr)
                    {
                        Format = SampleFormat.Format16Bit,
                        DesiredFrameSize = 1370,
                        SampleRate = 44100
                    };
                };

                var info = SoundDevices.SelectedItem as AudioDeviceInfo;
                out_sound_source = new AudioCaptureDevice(info)
                {
                    Format = SampleFormat.Format16Bit,
                    DesiredFrameSize = 1370,
                    SampleRate = 44100
                };
            }
            else
            {
                //no speakers
                SoundRecordCheckbox.IsEnabled = false;
                SoundRecordCheckbox.IsChecked = false;
            }
        }

        private void Setup_Camera(string _filepath, string _cameraFilepath)
        {
            Camera_Filepath = "Camera_Recording";
            if (_cameraFilepath != string.Empty)
                Camera_Filepath = _cameraFilepath;
            Camera_Filepath += ".wmv";

            if (video_capture_device != null)
                camera = new CameraRecorder(_filepath + Camera_Filepath, video_capture_device);
            else
                camera = new CameraRecorder(_filepath + Camera_Filepath);
        }

        private void Setup_Screen(string _filepath, string _screenFilepath)
        {
            Screen_Filepath = "Screen_Recording";
            if (_screenFilepath != string.Empty)
                Screen_Filepath = _screenFilepath;
            Screen_Filepath += ".wmv";

            screen = new ScreenRecorder(_filepath + Screen_Filepath);
        }

        private void Setup_AudioInput(string _filepath, string _audioFilepath)
        {
            AudioInput_Filepath = "Captured_Audio";
            if (_audioFilepath != string.Empty)
                AudioInput_Filepath = _audioFilepath;
            AudioInput_Filepath += ".mp3";

            if (in_audio_source != null)
                in_audio = new AudioRecorder(_filepath + AudioInput_Filepath, in_audio_source);
            else
                in_audio = new AudioRecorder(_filepath + AudioInput_Filepath);
        }

        private void Setup_AudioOutput(string _filepath, string _soundFilepath)
        {
            AudioOutput_Filepath = "Captured_Sound";
            if (_soundFilepath != string.Empty)
                AudioOutput_Filepath = _soundFilepath;
            AudioOutput_Filepath += ".mp3";

            if (out_sound_source != null)
                out_audio = new AudioRecorder(_filepath + AudioOutput_Filepath, out_sound_source);
            else
                out_audio = new AudioRecorder(_filepath + AudioOutput_Filepath, new AudioDeviceCollection(AudioDeviceCategory.Output));
        }

        public void Directory_Changed(string _newDirectory)
        {
            full_directory = directory = _newDirectory;
            if (File.Exists(full_directory + "DoNotRenameMe.txt"))
                MessageBox.Show("You may override other files if you record.");
        }

        private string Remove_Empty_Space(string _filename)
        {
            string[] arr = _filename.Split(' ');
            string name = "";
            for (int i = 0; i < arr.Length - 1; i++)
            {
                name += arr[i];
                name += "_";
            }
            name += arr[arr.Length - 1];
            return name;
        }

        public void Stop_Recording()
        {
            if (ScreenRecordCheckbox.IsChecked.Value)
                screen.Stop_ScreenRecording();
            if (CameraRecordCheckbox.IsChecked.Value && VideoDevices.Items.Count != 0)
                camera.Stop_CameraRecording();
            if (MicRecordCheckbox.IsChecked.Value && MicrophoneDevices.Items.Count != 0)
                in_audio.Stop_AudioRecording();
            if (SoundRecordCheckbox.IsChecked.Value && SoundDevices.Items.Count != 0)
                out_audio.Stop_AudioRecording();

            CameraRecordCheckbox.IsEnabled = true;
            ScreenRecordCheckbox.IsEnabled = true;
            MicRecordCheckbox.IsEnabled = true;
            SoundRecordCheckbox.IsEnabled = true;

            VideoDevices.IsEnabled = true;
            MicrophoneDevices.IsEnabled = true;
            SoundDevices.IsEnabled = true;

            string date = DateTime.Now.ToString();

            string[] text = { Full_Directory + Camera_Filepath, Full_Directory + Screen_Filepath, Full_Directory + in_audio_filepath, Full_Directory + out_audio_filepath, date, "Do not edit this text file!" };
            using (StreamWriter outputfile = new StreamWriter(Full_Directory + @"\DoNotRenameMe.txt"))
            {
                foreach (string line in text)
                    outputfile.WriteLine(line);
            }

            is_recording = false;
            stop_pressed = true;
            RecordButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            CameraFeed.SignalToStop();
            CameraFeed.WaitForStop();
        }

        public void Closing()
        {
            if (screen != null)
                screen.Form_Closed();
            if (camera != null)
                camera.Form_Closed();
            if (in_audio != null)
                in_audio.Form_Closed();
            if (out_audio != null)
                out_audio.Form_Closed();
        }
    }
}
