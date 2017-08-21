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
using Accord.Video;
using Accord.Video.DirectShow;
using Accord.Video.FFMPEG;
using Accord.DirectSound;
using Accord.Audio.Formats;
using System.IO;
using System.Linq;

namespace UXTool_V4.Features
{
    class CameraRecorder
    {
        VideoCaptureDeviceForm VC_DF;
        VideoFileWriter Video_FileWriter;
        VideoCaptureDevice Video_Capture;
        FilterInfoCollection Video_Devices;
        VideoCapabilities[] Video_Capabilities;

        VideoCodec codec;
        TimeSpan time;

        string directory;
        string defaultExt;
        bool paused;
        bool isready;

        public string DefaultExt
        {
            get
            {
                return defaultExt;
            }

            set
            {
                defaultExt = value;
            }
        }

        public CameraRecorder(string directive)
        {
            VC_DF = new VideoCaptureDeviceForm();
            Video_FileWriter = new VideoFileWriter();

            Video_Devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (Video_Devices.Count == 0)
            {
                throw new Exception();
            }
            else
            {
                Video_Capture = new VideoCaptureDevice(Video_Devices[0].MonikerString);
                Video_Capabilities = Video_Capture.VideoCapabilities;
                Video_Capture.VideoResolution = Video_Capabilities[0];
                Video_Capture.PlayingFinished += Video_Capture_PlayingFinished;
                Video_Capture.NewFrame += Video_Capture_NewFrame;

                directory = directive;
                paused = false;
            }
        }

        public CameraRecorder(string directive, VideoCaptureDevice video_device)
        {
            Video_FileWriter = new VideoFileWriter();
            Video_Capture = video_device;
            Video_Capabilities = Video_Capture.VideoCapabilities;
            Video_Capture.VideoResolution = Video_Capabilities[0];
            Video_Capture.PlayingFinished += Video_Capture_PlayingFinished;
            Video_Capture.NewFrame += Video_Capture_NewFrame;

            directory = directive;
            paused = false;
        }

        public void Start_CameraRecording()
        {
            codec = VideoCodec.WMV2;
            Video_FileWriter.Open(directory, Video_Capture.VideoResolution.FrameSize.Width, Video_Capture.VideoResolution.FrameSize.Height, 30, codec);
            time = DateTime.Now.TimeOfDay;
        }

        public void Start()
        {
            Video_Capture.Start();
            time = DateTime.Now.TimeOfDay;
        }

        public void Stop_CameraRecording()
        {
            isready = false;
            Video_Capture.SignalToStop();
            Video_Capture.WaitForStop();
        }

        public void Pause_Clicked()
        {
            Video_Capture.Stop();
            paused = true;
        }
        public void Resume_Clicked()
        {
            Video_Capture.Start();
            paused = false;
        }

        public bool Is_Running()
        {
            return Video_Capture.IsRunning;
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
            if (Video_Capture != null)
            {
                Video_Capture.SignalToStop();
            }
        }

        private void Video_Capture_PlayingFinished(object sender, ReasonToFinishPlaying reason)
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

        private void Video_Capture_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Video_FileWriter.WriteVideoFrame(eventArgs.Frame, DateTime.Now.TimeOfDay - time);
        }
    }
}