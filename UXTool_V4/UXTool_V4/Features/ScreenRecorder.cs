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
using System.Windows.Forms;
using Accord.Video;
using Accord.Video.FFMPEG;
using System.Linq;

namespace UXTool_V4.Features
{
    class ScreenRecorder
    {
        ScreenCaptureStream Screen_Capture;
        VideoFileWriter Video_FileWriter;
        VideoCodec codec;
        TimeSpan time;
        string directory;
        bool paused;
        bool isready;
        
        public ScreenRecorder(string directive)
        {
            Screen_Capture = new ScreenCaptureStream(Screen.PrimaryScreen.Bounds);
            Screen_Capture.NewFrame += Screen_Capture_NewFrame;
            Screen_Capture.PlayingFinished += Screen_Capture_PlayingFinished;
            Video_FileWriter = new VideoFileWriter();
            directory = directive;
            time = new TimeSpan();
        }

        public void Start_ScreenRecording()
        {
            codec = VideoCodec.WMV2;
            Video_FileWriter.Open(directory, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, 10, VideoCodec.WMV2);
            time = DateTime.Now.TimeOfDay;
        }

        public void Start()
        {
            Screen_Capture.Start();
            time = DateTime.Now.TimeOfDay;
        }

        public void Stop_ScreenRecording()
        {
            isready = false;
            Screen_Capture.SignalToStop();
            Screen_Capture.WaitForStop();
        }

        public void Pause_Clicked()
        {
            Screen_Capture.Stop();
            paused = true;
        }
        public void Resume_Clicked()
        {
            Screen_Capture.Start();
            paused = false;
        }

        public bool Is_Running()
        {
            return Screen_Capture.IsRunning;
        }

        public bool Is_Paused()
        {
            return paused;
        }

        public bool Is_Ready()
        {
            return isready;
        }

        public void Form_Closed()
        {
            if (Screen_Capture != null)
                Screen_Capture.SignalToStop();
        }

        private void Screen_Capture_PlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            switch (reason)
            {
                case ReasonToFinishPlaying.EndOfStreamReached:
                    Console.WriteLine("Video stream ended successfully.");
                    break;
                case ReasonToFinishPlaying.StoppedByUser:
                    Console.WriteLine("Device was signaled to stop.");
                    break;
                case ReasonToFinishPlaying.DeviceLost:
                    Console.WriteLine("Device disconnected.");
                    break;
                case ReasonToFinishPlaying.VideoSourceError:
                    Console.WriteLine("Some error occured.");
                    break;
                default:
                    break;
            }
            Video_FileWriter.Close();
        }

        private void Screen_Capture_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Video_FileWriter.WriteVideoFrame(eventArgs.Frame, DateTime.Now.TimeOfDay - time);
        }
    }
}
