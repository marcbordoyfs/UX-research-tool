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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using System.Drawing;
using Accord.Audio;
using Accord.DirectSound;
using UXTool_V4.Features;
using ZedGraph;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace UXTool_V4._Page
{
    /// <summary>
    /// Interaction logic for Edit_Page.xaml
    /// </summary>

    public partial class Edit_Page : Page
    {
        AffectivaEmotionClass affectiva;
        AffectivaScreenClass affscreen;
        EmotionContainer emotions;    //values for each emotion when a keyframe should be made automatically during affectiva processing
        EmotionItem[] emotion_controls;     //wraps functionality for identifying emotions, toggling the visibility of emotions on the graph, and modifying the value in emotion_values
        MediaController master;

        DispatcherTimer timerMediaTime;
        TimeSpan totalTime;
        int ticksPerSecond;
        Thumb position_thumb;
        Thumb volume_thumb;
        string directory;
        string Camera_Filepath;
        string Screen_Filepath;
        string Audio_Filepath;
        GraphPane zedPane;
        LineItem currentCurve;
        LineItem[] emotionCurves;

        int scrollIndex;
        static int numOptions;
        int numImagesInView;
        static int[] imageScrollOptions;
        static double[] graphScrollOptions;

        List<string> camera_thumbs;
        List<string> screen_thumbs;

        public Edit_Page()
        {
            InitializeComponent();

            camera.MediaEnded += Player_MediaEnded;
            master = new MediaController(camera, screen, audio, sound);

            timerMediaTime = new DispatcherTimer();
            timerMediaTime.Tick += TimerMediaTime_Tick;
            timerMediaTime.Stop();
            timerMediaTime.Interval = TimeSpan.FromSeconds(1);

            PlayButton.Click += PlayButton_Click;
            PauseButton.Click += PauseButton_Click;
            PauseButton.IsEnabled = false;
            StopButton.Click += StopButton_Click;
            StopButton.IsEnabled = false;
            ResumeButton.Click += ResumeButton_Click;
            ResumeButton.IsEnabled = false;
            KeyframeButton.Click += KeyframeButton_Click;
            MergeButton.Click += MergeButton_Click;
            MergeButton.IsEnabled = false;
            ProcessButton.Click += ProcessButton_Click;

            ticksPerSecond = 4;
            position_slider.Loaded += Position_Loaded;
            volume_slider.Loaded += Volume_Loaded;
            position_slider.ValueChanged += Position_ValueChanged;
            volume_slider.ValueChanged += Volume_ValueChanged;
            position_slider.MouseRightButtonDown += Position_MouseRightButtonDown;
            position_slider.IsEnabled = false;

            emotions = new EmotionContainer
            (
                (int)angerSlider.Value,
                (int)contemptSlider.Value,
                (int)disgustSlider.Value,
                (int)engagementSlider.Value,
                (int)fearSlider.Value,
                (int)joySlider.Value,
                (int)sadnessSlider.Value,
                (int)surpriseSlider.Value,
                (int)valenceSlider.Value
            );
            //group together the functionality
            emotion_controls = new EmotionItem[9]
            {
                new EmotionItem(angerBox, angerValue, angerSlider),
                new EmotionItem(contemptBox, contemptValue, contemptSlider),
                new EmotionItem(disgustBox, disgustValue, disgustSlider),
                new EmotionItem(engagementBox, engagementValue, engagementSlider),
                new EmotionItem(fearBox, fearValue, fearSlider),
                new EmotionItem(joyBox, joyValue, joySlider),
                new EmotionItem(sadnessBox, sadnessValue, sadnessSlider),
                new EmotionItem(surpriseBox, surpriseValue, surpriseSlider),
                new EmotionItem(valenceBox, valenceValue, valenceSlider)
            };

            foreach (var emotion in emotion_controls)
            {
                emotion.CB.Checked += Update_CurveList;
                emotion.CB.Unchecked += Update_CurveList;
            }

            ZoomInButton.Click += ZoomInButton_Click;
            ZoomInButton.IsEnabled = false;
            ZoomOutButton.Click += ZoomOutButton_Click;
            ZoomOutButton.IsEnabled = false;

            scrollIndex = 0;                                                         //maximum of 5 scroll options for now
            numOptions = 5;                                                          //number of zoom options
            numImagesInView = 10;                                                    //number of max images to be displayed at a time
            imageScrollOptions = new int[] { 3, 6, 18, 60, 180 };                    //number of seconds between each image visible in the list
            //imageScrollOptions = new int[] { 1, 2, 6, 20, 60 };                    //every nth image is put in the listview, images are saved every 3 seconds
            graphScrollOptions = new double[] { 30d, 60d, 180d, 600d, 1800d };       //number of seconds to show on timeline, basically controls the scale for the other data channels

            cameraView.Items.CurrentChanged += Items_CurrentChanged;
            cameraView.IsSynchronizedWithCurrentItem = true;
            screenView.Items.CurrentChanged += Items_CurrentChanged;
            screenView.IsSynchronizedWithCurrentItem = true;
            zedHost.Loaded += ZedHost_Loaded;
            currentCurve = new LineItem("Position");
            emotionCurves = new LineItem[9];
        }

        private void ZedHost_Loaded(object sender, RoutedEventArgs e)
        {
            SetupGraph();
        }

        private void Update_CurveList(object sender, RoutedEventArgs e)
        {
            if (zedPane != null && zedPane.CurveList.Count > 0)
            {
                double xmin = zedPane.XAxis.Scale.Min;
                double xmax = zedPane.XAxis.Scale.Max;

                for (int i = 0; i < emotionCurves.Length; ++i)
                    zedPane.CurveList.Remove(emotionCurves[i]);

                for (int i = 0; i < emotions.Length; ++i)
                {
                    if (emotion_controls[i].CB.IsChecked.Value)
                        zedPane.CurveList.Add(emotionCurves[i]);
                }

                zedPane.XAxis.Scale.Min = xmin;
                zedPane.XAxis.Scale.Max = xmax;
                zedGraph.AxisChange();
                zedGraph.Invalidate();
            }
        }

        public string SliderFormattedPosition
        {
            get
            {
                TimeSpan curr_time = TimeSpan.FromSeconds(position_slider.Value / ticksPerSecond);
                if (curr_time.Ticks == 0)
                    return "0:00";
                else if (curr_time.Hours != 0)
                    return curr_time.Hours + ":" + curr_time.Minutes.ToString("00") + ":" + curr_time.Seconds.ToString("00");
                else
                    return curr_time.Minutes + ":" + curr_time.Seconds.ToString("00");
            }
        }

        public string FormattedMaxDuration
        {
            get
            {
                if (totalTime.Ticks == 0)
                    return "0:00";
                else if (totalTime.Hours != 0)
                    return totalTime.Hours + ":" + totalTime.Minutes.ToString("00") + ":" + totalTime.Seconds.ToString("00");
                else
                    return totalTime.Minutes + ":" + totalTime.Seconds.ToString("00");
            }
        }

        public string Directory
        {
            get
            {
                return directory;
            }
            set
            {
                directory = value;
            }
        }

        private void TimerMediaTime_Tick(object sender, EventArgs e)
        {
            double curr_position = camera.Position.TotalSeconds;
            int curr_second = (int)curr_position;
            if (camera.NaturalDuration.HasTimeSpan && totalTime != camera.NaturalDuration.TimeSpan)
            {
                totalTime = camera.NaturalDuration.TimeSpan;
                position_slider.Maximum = totalTime.TotalSeconds * ticksPerSecond;
            }

            if (totalTime.TotalSeconds > 0)
                position_slider.Value = curr_position * ticksPerSecond;

            if (curr_second < zedPane.XAxis.Scale.Min || curr_second > zedPane.XAxis.Scale.Max)
            {
                int image_position = (curr_second / imageScrollOptions[scrollIndex]) + numImagesInView - 1;

                if (cameraView.Items.Count > 0)
                {
                    cameraView.UpdateLayout();
                    if (image_position < cameraView.Items.Count)
                        cameraView.ScrollIntoView(cameraView.Items.GetItemAt(image_position));
                    else
                        cameraView.ScrollIntoView(cameraView.Items[cameraView.Items.Count - 1]);
                }

                if (screenView.Items.Count > 0)
                {
                    screenView.UpdateLayout();
                    if (image_position < screenView.Items.Count)
                        screenView.ScrollIntoView(screenView.Items.GetItemAt(image_position));
                    else
                        screenView.ScrollIntoView(screenView.Items[screenView.Items.Count - 1]);
                }

                if (Waveform.WaveStream != null)
                {
                    double newPosition = (curr_position + graphScrollOptions[scrollIndex]) * Waveform.WaveStream.WaveFormat.SampleRate;
                    double newStart = newPosition - (graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                    if (newPosition < 0)
                    {
                        Waveform.WaveStream.Position = (long)(graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                        Waveform.StartPosition = 0;
                    }
                    else if (newPosition < Waveform.WaveStream.Length)
                    {
                        Waveform.WaveStream.Position = (long)newPosition;
                        Waveform.StartPosition = (long)newStart;
                    }
                    else
                    {
                        Waveform.WaveStream.Position = Waveform.WaveStream.Length;
                        Waveform.StartPosition = Waveform.WaveStream.Position - (long)(graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                    }
                }

                if (zedPane != null)
                {
                    if (curr_position + graphScrollOptions[scrollIndex] < totalTime.TotalSeconds)
                    {
                        zedPane.XAxis.Scale.Min += graphScrollOptions[scrollIndex];
                        zedPane.XAxis.Scale.Max += graphScrollOptions[scrollIndex];
                    }
                    else
                    {
                        zedPane.XAxis.Scale.Max = totalTime.TotalSeconds;
                        zedPane.XAxis.Scale.Min = zedPane.XAxis.Scale.Min - graphScrollOptions[scrollIndex];
                    }
                }
            }

            if (zedPane != null && curr_position < totalTime.TotalSeconds)
            {
                for (int i = 0; i < currentCurve.Points.Count; ++i)
                    currentCurve.Points[i].X = curr_position;
            }

            Waveform.Invalidate();
            zedGraph.AxisChange();
            zedGraph.Invalidate();
        }

        private void Items_CurrentChanged(object sender, EventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.Left) && !Keyboard.IsKeyDown(Key.Right) && master.MediaBehavior == MediaState.Pause)
            {
                double curr_position = 0;
                if (sender == cameraView.Items)
                    curr_position = (cameraView.Items.CurrentPosition * imageScrollOptions[scrollIndex]) - imageScrollOptions[scrollIndex];
                else if (sender == screenView.Items)
                    curr_position = (screenView.Items.CurrentPosition * imageScrollOptions[scrollIndex]) - imageScrollOptions[scrollIndex];

                if (curr_position > 0)
                {
                    int image_position = ((int)curr_position / imageScrollOptions[scrollIndex]) + numImagesInView - 1;

                    if (cameraView.Items.Count > 0)
                    {
                        cameraView.UpdateLayout();
                        if (image_position < cameraView.Items.Count)
                            cameraView.ScrollIntoView(cameraView.Items.GetItemAt(image_position));
                        else
                            cameraView.ScrollIntoView(cameraView.Items[cameraView.Items.Count - 1]);
                    }

                    if (screenView.Items.Count > 0)
                    {
                        screenView.UpdateLayout();
                        if (image_position < screenView.Items.Count)
                            screenView.ScrollIntoView(screenView.Items.GetItemAt(image_position));
                        else
                            screenView.ScrollIntoView(screenView.Items[screenView.Items.Count - 1]);
                    }

                    if (Waveform.WaveStream != null && curr_position > graphScrollOptions[scrollIndex])
                    {
                        double newPosition = (curr_position + graphScrollOptions[scrollIndex]) * Waveform.WaveStream.WaveFormat.SampleRate;
                        double newStart = newPosition - (graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                        if (newPosition < Waveform.WaveStream.Length)
                        {
                            Waveform.WaveStream.Position = (long)newPosition;
                            Waveform.StartPosition = (long)newStart;
                        }
                        else
                        {
                            Waveform.WaveStream.Position = Waveform.WaveStream.Length;
                            Waveform.StartPosition = Waveform.WaveStream.Length - (long)(graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                        }
                    }

                    if (zedPane != null)
                    {
                        zedPane.XAxis.Scale.Min = curr_position;
                        zedPane.XAxis.Scale.Max = curr_position + graphScrollOptions[scrollIndex];

                        for (int i = 0; i < currentCurve.Points.Count; ++i)
                            currentCurve.Points[i].X = curr_position;
                    }

                    master.MediaPosition = curr_position;

                    Waveform.Invalidate();
                    zedGraph.AxisChange();
                    zedGraph.Invalidate();
                }
            }
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomOutButton.IsEnabled = true;
            double curr_position = 0;

            if (--scrollIndex == 0)
                ZoomInButton.IsEnabled = false;

            if (zedPane != null)
                curr_position = currentCurve.Points[0].X;
            else
                curr_position = 0;

            if (cameraView.Items.Count > 0)
            {
                int everyNthImage = imageScrollOptions[scrollIndex] / 3;
                double imageWidth = cameraView.ActualWidth / numImagesInView;
                for (int i = 0; i < cameraView.Items.Count; ++i)
                {
                    if (i % everyNthImage == 0)
                        (cameraView.Items[i] as System.Windows.Controls.Image).Width = imageWidth;
                    else
                        (cameraView.Items[i] as System.Windows.Controls.Image).Width = 0;
                }
            }

            if (screenView.Items.Count > 0)
            {
                int everyNthImage = imageScrollOptions[scrollIndex] / 3;
                double imageWidth = screenView.ActualWidth / numImagesInView;
                for (int i = 0; i < screenView.Items.Count; ++i)
                {
                    if (i % everyNthImage == 0)
                        (screenView.Items[i] as System.Windows.Controls.Image).Width = imageWidth;
                    else
                        (screenView.Items[i] as System.Windows.Controls.Image).Width = 0;
                }
            }

            if (Waveform.WaveStream != null)
            {
                Waveform.SamplesPerPixel = (int)(Waveform.WaveStream.WaveFormat.SampleRate * (graphScrollOptions[scrollIndex] / Waveform.Width));
                double newPosition = (curr_position + graphScrollOptions[scrollIndex]) * Waveform.WaveStream.WaveFormat.SampleRate;
                double newStart = newPosition - (graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                if (newPosition < 0)
                {
                    Waveform.WaveStream.Position = (long)(graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                    Waveform.StartPosition = 0;
                }
                else if (newPosition < Waveform.WaveStream.Length)
                {
                    Waveform.WaveStream.Position = (long)newPosition;
                    Waveform.StartPosition = (long)newStart;
                }
                else
                {
                    Waveform.WaveStream.Position = Waveform.WaveStream.Length;
                    Waveform.StartPosition = Waveform.WaveStream.Position - (long)(graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                }
            }

            zedPane.XAxis.Scale.Min = curr_position;
            zedPane.XAxis.Scale.Max = curr_position + graphScrollOptions[scrollIndex];

            Waveform.Invalidate();
            zedGraph.AxisChange();
            zedGraph.Invalidate();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomInButton.IsEnabled = true;
            double curr_position = 0;

            if (++scrollIndex == numOptions - 2)
                ZoomOutButton.IsEnabled = false;

            if (zedPane != null)
                curr_position = currentCurve.Points[0].X;
            else
                curr_position = 0;

            if (cameraView.Items.Count > 0)
            {
                int everyNthImage = imageScrollOptions[scrollIndex] / 3;
                double imageWidth = cameraView.ActualWidth / numImagesInView;
                for (int i = 0; i < cameraView.Items.Count; ++i)
                {
                    if (i % everyNthImage == 0)
                        (cameraView.Items[i] as System.Windows.Controls.Image).Width = imageWidth;
                    else
                        (cameraView.Items[i] as System.Windows.Controls.Image).Width = 0;
                }
            }

            if (screenView.Items.Count > 0)
            {
                int everyNthImage = imageScrollOptions[scrollIndex] / 3;
                double imageWidth = screenView.ActualWidth / numImagesInView;
                for (int i = 0; i < screenView.Items.Count; ++i)
                {
                    if (i % everyNthImage == 0)
                        (screenView.Items[i] as System.Windows.Controls.Image).Width = imageWidth;
                    else
                        (screenView.Items[i] as System.Windows.Controls.Image).Width = 0;
                }
            }

            if (Waveform.WaveStream != null)
            {
                Waveform.SamplesPerPixel = (int)(Waveform.WaveStream.WaveFormat.SampleRate * (graphScrollOptions[scrollIndex] / Waveform.Width));
                double newPosition = (curr_position + graphScrollOptions[scrollIndex]) * Waveform.WaveStream.WaveFormat.SampleRate;
                double newStart = newPosition - (graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                if (newPosition < 0)
                {
                    Waveform.WaveStream.Position = (long)(graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                    Waveform.StartPosition = 0;
                }
                else if (newPosition < Waveform.WaveStream.Length)
                {
                    Waveform.WaveStream.Position = (long)newPosition;
                    Waveform.StartPosition = (long)newStart;
                }
                else
                {
                    Waveform.WaveStream.Position = Waveform.WaveStream.Length;
                    Waveform.StartPosition = Waveform.WaveStream.Position - (long)(graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                }
            }

            zedPane.XAxis.Scale.Min = curr_position;
            zedPane.XAxis.Scale.Max = curr_position + graphScrollOptions[scrollIndex];

            Waveform.Invalidate();
            zedGraph.AxisChange();
            zedGraph.Invalidate();
        }

        private void KeyframeButton_Click(object sender, RoutedEventArgs e)
        {
            if(!File.Exists(Directory + "Manual_Keyframes.csv"))
            {
                StreamWriter data_writer = new StreamWriter(Directory + @"\Manual_Keyframes.csv");
                data_writer.Close();
            }

            FileInfo fi = new FileInfo(Directory + @"\Manual_Keyframes.csv");

            if (!IsFileLocked(fi) && Camera_Filepath != null)
            {
                using (StreamWriter outputfile = new StreamWriter(Directory + @"\Manual_Keyframes.csv", true))
                {
                    int timestamp;
                    int duration;
                    if (int.TryParse(keyframe_end.Text, out duration))
                    {
                        //duration -= master.FormattedPosition;
                        if (duration >= totalTime.TotalSeconds)
                            duration = (int)totalTime.TotalSeconds - master.FormattedPosition;
                    }
                    else if (duration <= 0)
                        duration = 5;
                    else
                        duration = 5;

                    if ((master.FormattedPosition - 5) < 0)
                        timestamp = 0;
                    else
                        timestamp = master.FormattedPosition;

                    outputfile.Write("Manual" + ',');
                    outputfile.Write("" + ',');
                    outputfile.Write(timestamp.ToString() + ',');
                    outputfile.Write(duration.ToString() + ',');
                    outputfile.Write("Notes" + ',');
                    outputfile.WriteLine(Camera_Filepath);
                    outputfile.Close();
                }
            }
            else
                keyframe_end.Text = "Can't access keyframe file or no video selected";
        }

        private void MergeButton_Click(object sender, RoutedEventArgs e)
        {
            string camera_video = string.Empty;
            string camera_audio = string.Empty;
            if (camera.Source != null && audio.Source != null)
            {
                camera_video = camera.Source.OriginalString;
                camera_audio = audio.Source.OriginalString;
            }

            string screen_video = string.Empty;
            string screen_sound = string.Empty;
            if (screen.Source != null && sound.Source != null)
            {
                screen_video = screen.Source.OriginalString;
                screen_sound = sound.Source.OriginalString;
            }

            master.Close_Files();

            if (camera_video != string.Empty && camera_audio != string.Empty)
            {
                string output = directory + "CameraMedia.wmv";
                //ffmpeg -i input.mp4 -i input.mp3 -c copy -map 0:0 -map 1:0 output.mp4
                string args = "-i \"" + camera_video + "\" -i \"" + camera_audio + "\" -c copy -map 0:0 -map 1:0 \"" + output + '\"';
                System.Diagnostics.Process.Start("ffmpeg.exe", args);
            }

            if (screen_video != string.Empty && screen_sound != string.Empty)
            {
                string output = directory + "ScreenMedia.wmv";
                //ffmpeg -i input.mp4 -i input.mp3 -c copy -map 0:0 -map 1:0 output.mp4
                string args = "-i \"" + screen_video + "\" -i \"" + screen_sound + "\" -c copy -map 0:0 -map 1:0 \"" + output + '\"';
                System.Diagnostics.Process.Start("ffmpeg.exe", args);
            }
        }

        public void Quickload_Media()
        {
            camera.ScrubbingEnabled = true;
            screen.ScrubbingEnabled = true;
            camera.Play();
            camera.Pause();
            screen.Play();
            screen.Pause();
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            master.Click_Stop();
            timerMediaTime.Stop();
            position_slider.Value = position_slider.Minimum;

            PlayButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            ResumeButton.IsEnabled = false;
            KeyframeButton.IsEnabled = false;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (master.hasfilepath != null)
            {
                totalTime = TimeSpan.FromSeconds(0);

                position_slider.IsEnabled = true;

                if (cameraView.Items.Count > 0)
                {
                    cameraView.UpdateLayout();
                    cameraView.ScrollIntoView(cameraView.Items.GetItemAt(0));
                }

                if (Waveform.WaveStream != null)
                {
                    Waveform.StartPosition = 0;
                }

                if (screenView.Items.Count > 0)
                {
                    screenView.UpdateLayout();
                    screenView.ScrollIntoView(screenView.Items.GetItemAt(0));
                }

                if (zedPane != null)
                {
                    for (int i = 0; i < currentCurve.Points.Count; ++i)
                        currentCurve.Points[i].X = 0;

                    zedPane.XAxis.Scale.Min = 0;
                    zedPane.XAxis.Scale.Max = graphScrollOptions[scrollIndex];

                    zedGraph.AxisChange();
                    zedGraph.Invalidate();
                }

                master.Click_Start();

                timerMediaTime.Start();

                PlayButton.IsEnabled = false;
                PauseButton.IsEnabled = true;
                StopButton.IsEnabled = true;
                ResumeButton.IsEnabled = false;
                KeyframeButton.IsEnabled = true;
                MergeButton.IsEnabled = true;
            }
            else
            {
                FolderBrowserDialog filepath = new FolderBrowserDialog();
                filepath.RootFolder = Environment.SpecialFolder.MyComputer;
                if (filepath.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Directory = filepath.SelectedPath + "\\";

                    if (File.Exists(Directory + "DoNotRenameMe.txt"))
                    {
                        StreamReader outputfile = new StreamReader(Directory + "DoNotRenameMe.txt");

                        ReadIn_From_File(outputfile.ReadLine(), outputfile.ReadLine(), outputfile.ReadLine(), outputfile.ReadLine());

                        position_slider.IsEnabled = true;

                        if (camera_thumbs != null)
                        {
                            cameraView.UpdateLayout();
                            cameraView.ScrollIntoView(cameraView.Items.GetItemAt(0));
                        }

                        if (screen_thumbs != null)
                        {
                            screenView.UpdateLayout();
                            screenView.ScrollIntoView(screenView.Items.GetItemAt(0));
                        }

                        if (zedPane != null)
                        {
                            for (int i = 0; i < currentCurve.Points.Count; ++i)
                                currentCurve.Points[i].X = 0;

                            zedPane.XAxis.Scale.Min = 0;
                            zedPane.XAxis.Scale.Max = graphScrollOptions[scrollIndex];

                            zedGraph.AxisChange();
                            zedGraph.Invalidate();
                        }

                        master.Click_Start();
                        timerMediaTime.Start();

                        PauseButton.IsEnabled = true;
                        StopButton.IsEnabled = true;
                        ResumeButton.IsEnabled = false;
                        MergeButton.IsEnabled = true;
                    }
                }
            }
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (master.MediaBehavior == MediaState.Pause)
            {
                master.Click_Resume();
                timerMediaTime.Start();
            }
            PauseButton.IsEnabled = true;
            StopButton.IsEnabled = true;
            ResumeButton.IsEnabled = false;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            master.Click_Stop();

            timerMediaTime.Stop();
            PlayButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            ResumeButton.IsEnabled = false;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            master.Click_Pause();

            timerMediaTime.Stop();
            PauseButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            ResumeButton.IsEnabled = true;
        }

        public void Pause_Media()
        {
            master.Click_Pause();

            timerMediaTime.Stop();
            PauseButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            ResumeButton.IsEnabled = false;
            PlayButton.IsEnabled = true;
        }

        public void LetGoOfVideos()
        {
            master.Close_Files();
            if (cameraView.Items.Count > 0)
            {
                cameraView.Items.Clear();
                cameraView.DataContext = null;
                camera_thumbs.Clear();
            }

            if (screenView.Items.Count > 0)
            {
                screenView.Items.Clear();
                screenView.DataContext = null;
                screen_thumbs.Clear();
            }

            if (Waveform.WaveStream != null)
            {
                Waveform.WaveStream.Dispose();
                Waveform.WaveStream = null;
            }
            if (zedPane != null)
                zedPane.CurveList.Clear();

            cameraView.Items.Clear();
            screenView.Items.Clear();
        }

        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            timerMediaTime.Stop();
            master.Click_Stop();
            PlayButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            PauseButton.IsEnabled = false;
            ResumeButton.IsEnabled = false;
            KeyframeButton.IsEnabled = false;
            if (System.IO.Directory.Exists(Directory + "temp_thumbs\\"))
            {
                if (File.Exists(directory + "Automatic_Keyframes.csv") && File.Exists(directory + "temp_data.csv") && System.IO.Directory.GetFiles(Directory + "temp_thumbs\\").Length > 0)
                {

                    FileInfo fi4 = new FileInfo(Directory + "temp_data.csv");
                    FileInfo fi5 = new FileInfo(Directory + "Automatic_Keyframes.csv");
                    if (!IsFileLocked(fi4) && !IsFileLocked(fi5))
                    {
                        Finish_Process();
                        return;
                    }
                }
            }
            FileInfo fi1 = new FileInfo(Directory + "temp_data.csv");
            FileInfo fi2 = new FileInfo(Directory + "Automatic_Keyframes.csv");
            if (!File.Exists(Directory + "temp_data.csv"))
            {
                StreamWriter tempdata = new StreamWriter(Directory + "temp_data.csv", false);
                tempdata.Close();
            }
            if (!File.Exists(Directory + "Automatic_Keyframes.csv"))
            {
                StreamWriter tempkeyframe = new StreamWriter(Directory + "Automatic_Keyframes.csv", false);
                tempkeyframe.Close();
            }
            if (!IsFileLocked(fi1) && !IsFileLocked(fi2) && File.Exists(Camera_Filepath) && ProcessTextblock.Text != "Processing")
            {
                Dispatcher.Invoke((Action)delegate
                {
                    ProcessButton.IsEnabled = false;
                    ProcessTextblock.Text = "Processing";
                });
                Begin_Process();
            }
            else
            {
                ProcessButton.IsEnabled = true;
                PlayButton.IsEnabled = true;
                StopButton.IsEnabled = false;
                PauseButton.IsEnabled = false;
                ResumeButton.IsEnabled = false;
                KeyframeButton.IsEnabled = true;
                ProcessTextblock.Text = "Missing files or no video selected.";
            }

        }

        private void Begin_Process()
        {
            affectiva = new AffectivaEmotionClass();
            EmotionContainer newEmotions = new EmotionContainer
            (
                (int)angerSlider.Value,
                (int)contemptSlider.Value,
                (int)disgustSlider.Value,
                (int)engagementSlider.Value,
                (int)fearSlider.Value,
                (int)joySlider.Value,
                (int)sadnessSlider.Value,
                (int)surpriseSlider.Value,
                (int)valenceSlider.Value
            );
            emotions = newEmotions;
            affectiva.setEmotionThresholds(emotions);
            affectiva.ProcessFunction = Process_Screen;
            affectiva.beginFaceProcessing(Camera_Filepath, Directory + "Automatic_Keyframes.csv");
        }

        private void Process_Screen()
        {
            if (File.Exists(Screen_Filepath))
            {
                affscreen = new AffectivaScreenClass();
                affscreen.ProcessFunction = Finish_Process;
                affscreen.beginScreenProcessing(Screen_Filepath);
            }
            else
            {
                Finish_Process();
            }

        }

        private void Finish_Process()
        {
            Dispatcher.Invoke((Action)delegate
            {
                CreateBitmapLists(camera_thumbs, screen_thumbs);
                double imageWidth = screenView.ActualWidth / numImagesInView;
                for (int i = 0; i < camera_thumbs.Count; ++i)
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = new Uri(camera_thumbs[i]);
                    image.EndInit();
                    cameraView.Items.Add(new System.Windows.Controls.Image
                    {
                        Width = imageWidth,
                        Source = image
                    });
                }
                for (int i = 0; i < screen_thumbs.Count; ++i)
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = new Uri(screen_thumbs[i]);
                    image.EndInit();
                    screenView.Items.Add(new System.Windows.Controls.Image
                    {
                        Width = imageWidth,
                        Source = image
                    });
                }

                if (cameraView.Items.Count < screenView.Items.Count)
                {
                    int numImages = screenView.Items.Count - cameraView.Items.Count;
                    for (int i = 0; i < numImages; ++i)
                    {
                        cameraView.Items.Add(new System.Windows.Controls.Image
                        {
                            Width = imageWidth
                        });
                    }
                }
                else if (screenView.Items.Count < cameraView.Items.Count)
                {
                    int numImages = cameraView.Items.Count - screenView.Items.Count;
                    for (int i = 0; i < numImages; ++i)
                    {
                        screenView.Items.Add(new System.Windows.Controls.Image
                        {
                            Width = imageWidth
                        });
                    }
                }

                if (Waveform.WaveStream != null)
                {
                    Waveform.WaveStream.Dispose();
                    Waveform.WaveStream = null;
                }
                Waveform.WaveStream = new NAudio.Wave.WaveFileReader(Audio_Filepath);
                Waveform.SamplesPerPixel = (int)(Waveform.WaveStream.WaveFormat.SampleRate * (graphScrollOptions[scrollIndex] / Waveform.Width));
                Waveform.StartPosition = 0;

                CreateGraph();

                ProcessTextblock.Text = "Finished";
                ProcessButton.IsEnabled = true;
                PlayButton.IsEnabled = true;
                KeyframeButton.IsEnabled = true;
                ZoomOutButton.IsEnabled = true;
            });
        }

        private void Position_Loaded(object sender, RoutedEventArgs e)
        {
            position_thumb = (position_slider.Template.FindName("PART_Track", position_slider) as Track).Thumb;

            position_thumb.DragStarted += (object s, DragStartedEventArgs args) =>
            {
                timerMediaTime.Stop();
                master.Click_Pause();

                position_info.Placement = PlacementMode.Center;
                position_info.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                position_info.HorizontalOffset = position_thumb.ActualWidth;
                position_info.VerticalOffset = -position_thumb.ActualHeight;

                position_info.PlacementTarget = position_thumb;
                position_info.IsOpen = true;
            };

            position_thumb.DragCompleted += (object s, DragCompletedEventArgs args) =>
            {
                double curr_position = position_slider.Value / (double)ticksPerSecond;

                timerMediaTime.Start();
                master.Click_Resume();
                PauseButton.IsEnabled = true;
                ResumeButton.IsEnabled = false;
                master.MediaPosition = curr_position;
                position_info.PlacementTarget = position_slider;
                position_info.IsOpen = false;

                int image_position = 0;
                if (args.VerticalChange > 0)
                    image_position = ((int)curr_position / imageScrollOptions[scrollIndex]) + numImagesInView - 1;
                else
                    image_position = ((int)curr_position / imageScrollOptions[scrollIndex]);

                if (camera_thumbs != null)
                {
                    cameraView.UpdateLayout();
                    if (image_position < cameraView.Items.Count)
                        cameraView.ScrollIntoView(cameraView.Items.GetItemAt(image_position));
                    else
                        cameraView.ScrollIntoView(cameraView.Items[cameraView.Items.Count - 1]);
                }

                if (screen_thumbs != null)
                {
                    screenView.UpdateLayout();
                    if (image_position < screenView.Items.Count)
                        screenView.ScrollIntoView(screenView.Items.GetItemAt(image_position));
                    else
                        screenView.ScrollIntoView(screenView.Items[screenView.Items.Count - 1]);
                }

                if (Waveform.WaveStream != null)
                {
                    double newPosition = (curr_position + graphScrollOptions[scrollIndex]) * Waveform.WaveStream.WaveFormat.SampleRate;
                    double newStart = newPosition - (graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                    if (newPosition < Waveform.WaveStream.Length)
                    {
                        Waveform.WaveStream.Position = (long)newPosition;
                        Waveform.StartPosition = (long)newStart;
                    }
                    else
                    {
                        Waveform.WaveStream.Position = Waveform.WaveStream.Length;
                        Waveform.StartPosition = Waveform.WaveStream.Length - (long)(graphScrollOptions[scrollIndex] * Waveform.WaveStream.WaveFormat.SampleRate);
                    }
                }

                if (zedPane != null)
                {
                    zedPane.XAxis.Scale.Min = curr_position;
                    zedPane.XAxis.Scale.Max = curr_position + graphScrollOptions[scrollIndex];

                    for (int i = 0; i < currentCurve.Points.Count; ++i)
                        currentCurve.Points[i].X = curr_position;
                }
            };
        }

        private void Volume_Loaded(object sender, RoutedEventArgs e)
        {
            volume_thumb = (volume_slider.Template.FindName("PART_Track", volume_slider) as Track).Thumb;

            volume_thumb.DragStarted += (object s, DragStartedEventArgs args) =>
            {
                volume_info.Placement = PlacementMode.Right;
                volume_info.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                volume_info.HorizontalOffset = volume_thumb.Height;

                volume_info.PlacementTarget = volume_slider;
                volume_info.IsOpen = true;
            };

            volume_thumb.DragCompleted += (object s, DragCompletedEventArgs args) =>
            {
                volume_info.IsOpen = false;
            };
        }

        private void Position_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            string time = SliderFormattedPosition;
            position_text.Text = time + " / " + FormattedMaxDuration;
            position_info.HorizontalOffset += 1;
            position_info.HorizontalOffset -= 1;
            position_info_text.Text = time;
        }

        private void Position_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int select_value = (int)(e.GetPosition(position_slider).X * (position_slider.Maximum / position_slider.Width));
            select_value /= ticksPerSecond;
            if (select_value > master.MediaPosition)
            {
                select_value -= (int)master.MediaPosition;
                //string length = (select_value / 60) + ":" + (select_value % 60).ToString("00");
                keyframe_end.Text = select_value.ToString();
            }
        }

        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            volume_info.VerticalOffset += 1;
            volume_info.VerticalOffset -= 1;
            master.MediaVolume = volume_slider.Value;
            volume_info_text.Text = (int)(master.MediaVolume * 100) + "";
        }

        public void ReadIn_From_File(string filepath, string camera_filepath, string screen_filepath, string audio_filepath, string sound_filepath)
        {
            Directory = filepath;
            Camera_Filepath = camera_filepath;
            Screen_Filepath = screen_filepath;
            Audio_Filepath = audio_filepath;
            master.Load_Files(camera_filepath, screen_filepath, audio_filepath, sound_filepath);
        }

        private void ReadIn_From_File(string camera_filepath, string screen_filepath, string audio_filepath, string sound_filepath)
        {
            Camera_Filepath = camera_filepath;
            Screen_Filepath = screen_filepath;
            Audio_Filepath = audio_filepath;
            master.Load_Files(camera_filepath, screen_filepath, audio_filepath, sound_filepath);
        }

        public void directorychanged(string directory)
        {
            Directory = directory;
            if (File.Exists(Directory + "DoNotRenameMe.txt"))
            {
                StreamReader outputfile = new StreamReader(Directory + "DoNotRenameMe.txt");

                ReadIn_From_File(outputfile.ReadLine(), outputfile.ReadLine(), outputfile.ReadLine(), outputfile.ReadLine());

                if (camera_thumbs != null)
                {
                    cameraView.Items.Clear();
                    camera_thumbs.Clear();
                    camera_thumbs = null;
                }

                if (screen_thumbs != null)
                {
                    screenView.Items.Clear();
                    screen_thumbs.Clear();
                    screen_thumbs = null;
                }

                if (Waveform.WaveStream != null)
                {
                    Waveform.WaveStream.Dispose();
                    Waveform.WaveStream = null;
                }

                if (zedPane != null)
                {
                    zedPane.CurveList.Clear();
                    SetupGraph();
                    //zedPane = null;
                }

                position_slider.IsEnabled = true;
                PauseButton.IsEnabled = false;
                StopButton.IsEnabled = false;
                ResumeButton.IsEnabled = false;
                PlayButton.IsEnabled = true;
                MergeButton.IsEnabled = true;

                cameraView.InvalidateVisual();
                cameraView.InvalidateVisual();
                Waveform.Invalidate();
                zedGraph.AxisChange();
                zedGraph.Invalidate();
            }
        }

        private void SetupGraph()
        {
            zedGraph.Location = new System.Drawing.Point(0, 0);
            zedPane = zedGraph.GraphPane;

            zedPane.Title.IsVisible = false;

            zedPane.XAxis.Title.Gap = 0.2f;
            zedPane.XAxis.Title.Text = "Time (in seconds)";
            zedPane.XAxis.Scale.Min = 0;
            zedPane.XAxis.Scale.Max = graphScrollOptions[scrollIndex];

            zedPane.YAxis.Title.IsVisible = false;
            //zedPane.YAxis.Title.Text = "Emotion Values";
            //zedPane.YAxis.Scale.IsLabelsInside = true;
            zedPane.YAxis.Scale.IsVisible = false;
            zedPane.YAxis.Scale.Min = -100;
            zedPane.YAxis.Scale.Max = 100;

            zedGraph.AxisChange();
            zedGraph.Invalidate();
        }

        private void CreateGraph()
        {
            PointPairList angerPairs = new PointPairList();
            PointPairList contemptPairs = new PointPairList();
            PointPairList disgustPairs = new PointPairList();
            PointPairList engagementPairs = new PointPairList();
            PointPairList fearPairs = new PointPairList();
            PointPairList joyPairs = new PointPairList();
            PointPairList sadnessPairs = new PointPairList();
            PointPairList surprisePairs = new PointPairList();
            PointPairList valencePairs = new PointPairList();

            StreamReader values = new StreamReader(Directory + "temp_data.csv"); // may need a check
            string[] temp;
            while (!values.EndOfStream)
            {
                temp = values.ReadLine().Split(',');
                angerPairs.Add(double.Parse(temp[0]), double.Parse(temp[1]));
                temp = values.ReadLine().Split(',');
                contemptPairs.Add(double.Parse(temp[0]), double.Parse(temp[1]));
                temp = values.ReadLine().Split(',');
                disgustPairs.Add(double.Parse(temp[0]), double.Parse(temp[1]));
                temp = values.ReadLine().Split(',');
                engagementPairs.Add(double.Parse(temp[0]), double.Parse(temp[1]));
                temp = values.ReadLine().Split(',');
                fearPairs.Add(double.Parse(temp[0]), double.Parse(temp[1]));
                temp = values.ReadLine().Split(',');
                joyPairs.Add(double.Parse(temp[0]), double.Parse(temp[1]));
                temp = values.ReadLine().Split(',');
                sadnessPairs.Add(double.Parse(temp[0]), double.Parse(temp[1]));
                temp = values.ReadLine().Split(',');
                surprisePairs.Add(double.Parse(temp[0]), double.Parse(temp[1]));
                temp = values.ReadLine().Split(',');
                valencePairs.Add(double.Parse(temp[0]), double.Parse(temp[1]));
            }
            values.Close();

            zedPane.CurveList.Clear();

            double[] x = new double[2] { 0, 0 };
            double[] y = new double[2] { -100, 100 };
            currentCurve = zedPane.AddCurve("Position", x, y, Color.Red);

            emotionCurves = new LineItem[9];
            emotionCurves[0] = zedPane.AddCurve("Anger", angerPairs, Color.Crimson);
            emotionCurves[1] = zedPane.AddCurve("Contempt", contemptPairs, Color.HotPink);
            emotionCurves[2] = zedPane.AddCurve("Disgust", disgustPairs, Color.ForestGreen);
            emotionCurves[3] = zedPane.AddCurve("Engagement", engagementPairs, Color.DarkViolet);
            emotionCurves[4] = zedPane.AddCurve("Fear", fearPairs, Color.Black);
            emotionCurves[5] = zedPane.AddCurve("Joy", joyPairs, Color.Orange);
            emotionCurves[6] = zedPane.AddCurve("Sadness", sadnessPairs, Color.Blue);
            emotionCurves[7] = zedPane.AddCurve("Surprise", surprisePairs, Color.Chocolate);
            emotionCurves[8] = zedPane.AddCurve("Valence", valencePairs, Color.Gray);

            zedPane.Legend.FontSpec.Size = 24;
            zedPane.XAxis.Scale.Min = 0;
            zedPane.XAxis.Scale.Max = graphScrollOptions[scrollIndex];

            zedGraph.AxisChange();
            zedGraph.Invalidate();
        }

        private int Thumbnail_Comparer(string a, string b)
        {
            char[] char_a = a.Split('\\').Last().ToCharArray();
            string actual_a = String.Empty;
            for (int i = 0; i < char_a.Length; i++)
            {
                if (char.IsNumber(char_a[i]))
                    actual_a += char_a[i];
            }

            char[] char_b = b.Split('\\').Last().ToCharArray();
            string actual_b = String.Empty;
            for (int i = 0; i < char_b.Length; i++)
            {
                if (char.IsNumber(char_b[i]))
                    actual_b += char_b[i];
            }

            if (int.Parse(actual_a) < int.Parse(actual_b))
                return -1;
            else if (int.Parse(actual_a) > int.Parse(actual_b))
                return 1;
            else
                return 0;
        }

        private void CreateBitmapLists(List<string> _cameraThumbs, List<string> _desktopThumbs)
        {
            cameraView.Items.Clear();
            screenView.Items.Clear();

            List<string> pics = new List<string>(System.IO.Directory.EnumerateFiles(Directory + "temp_thumbs\\"));
            camera_thumbs = new List<string>();
            screen_thumbs = new List<string>();

            for (int i = 0; i < pics.Count; ++i)
            {
                string image_name = pics[i].Split('\\').Last();
                if (image_name.Contains("face"))
                    camera_thumbs.Add(pics[i]);

                if (image_name.Contains("screen"))
                    screen_thumbs.Add(pics[i]);
            }

            Comparison<string> thumb_comparer = new Comparison<string>(Thumbnail_Comparer);
            camera_thumbs.Sort(thumb_comparer);
            screen_thumbs.Sort(thumb_comparer);
            pics.Clear();
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

    struct EmotionItem
    {
        public System.Windows.Controls.CheckBox CB;
        public TextBlock TB;
        public Slider S;

        public EmotionItem(System.Windows.Controls.CheckBox _cb, TextBlock _tb, Slider _s)
        {
            CB = _cb;
            TB = _tb;
            S = _s;

            CB.Checked += EmotionBox_Checked;
            CB.Unchecked += EmotionBox_Unchecked;
            S.ValueChanged += EmotionSlider_ValueChanged;

            TB.Text = ((int)S.Value).ToString();
        }

        private void EmotionBox_Checked(object sender, RoutedEventArgs e)
        {
            S.Value = 50;
            TB.Text = ((int)S.Value).ToString();
        }

        private void EmotionBox_Unchecked(object sender, RoutedEventArgs e)
        {
            S.Value = 0;
            TB.Text = ((int)S.Value).ToString();
        }

        private void EmotionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TB.Text = ((int)S.Value).ToString();
            if (S.Value == 0)
                CB.IsChecked = false;
            else
                CB.IsChecked = true;
        }

    }
}
