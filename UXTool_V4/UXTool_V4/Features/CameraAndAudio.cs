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

namespace UXTool_V4.Features
{
    class CameraAndAudio
    {
        VideoCaptureDeviceForm VC_DF;
        VideoFileWriter Video_FileWriter;
        VideoCaptureDevice Video_Capture;
        FilterInfoCollection Video_Devices;
        VideoCapabilities[] Video_Capabilities;


        Accord.Audio.IAudioSource Audio_Source;
        WaveEncoder Wave_Encoder;
        MemoryStream Memory_Stream;
        VideoCodec codec;

        string directory;
        string filename;
        bool paused;
        bool isready;

        public CameraAndAudio(string directive)
        {
            VC_DF = new VideoCaptureDeviceForm();
            Video_FileWriter = new VideoFileWriter();

            Video_Devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (Video_Devices.Count == 0)
            {
                throw new Exception();
            }

            Video_Capture = new VideoCaptureDevice(Video_Devices[0].MonikerString);
            Video_Capabilities = Video_Capture.VideoCapabilities;
            Video_Capture.VideoResolution = Video_Capabilities[0];
            Video_Capture.PlayingFinished += Video_Captuer_PlayingFinished;
            Video_Capture.NewFrame += Video_Captuer_NewFrame;

            Audio_Source = new AudioCaptureDevice()
            {
                Format = Accord.Audio.SampleFormat.Format16Bit,
                DesiredFrameSize = 4096,
                SampleRate = 22050
            };
            Audio_Source.NewFrame += Audio_Source_NewFrame;
            Audio_Source.AudioSourceError += Audio_Source_AudioSourceError;

            directory = directive;
            paused = false;
            filename = "Camera_Recording.wmv";

            Memory_Stream = new MemoryStream();
            Wave_Encoder = new WaveEncoder(Memory_Stream);
        }

        public void Start_CameraRecording()
        {
            codec = VideoCodec.WMV2;

            isready = true;

        }

        public void Start()
        {
            //28 works best after alot of testing
            Video_FileWriter.Open(directory + filename, Video_Capture.VideoResolution.FrameSize.Width, Video_Capture.VideoResolution.FrameSize.Height, 30
                , codec, 33554432, AudioCodec.MP3, 524288, Audio_Source.SampleRate, 1);
            Video_Capture.Start();
            Audio_Source.Start();
        }

        public void Stop_CameraRecording()
        {
            isready = false;
            Video_Capture.SignalToStop();
            Audio_Source.SignalToStop();
            Video_Capture.WaitForStop();
            Audio_Source.WaitForStop();
        }

        public void Pause_Clicked()
        {
            Video_Capture.Stop();
            Audio_Source.Stop();
            //Video_Capture.SignalToStop();
            //Audio_Source.SignalToStop();     Not working now
            paused = true;
        }
        public void Resume_Clicked()
        {
            Video_Capture.Start();
            Audio_Source.Start();
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
                Audio_Source.SignalToStop();
            }
        }

        private void Video_Captuer_PlayingFinished(object sender, ReasonToFinishPlaying reason)
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

        private void Video_Captuer_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Video_FileWriter.WriteVideoFrame(eventArgs.Frame);
        }

        private void Audio_Source_NewFrame(object sender, Accord.Audio.NewFrameEventArgs eventArgs)
        {
            Video_FileWriter.WriteAudioFrame(eventArgs.Signal.RawData);
        }

        private void Audio_Source_AudioSourceError(object sender, Accord.Audio.AudioSourceErrorEventArgs e)
        {
            throw new Exception(e.Description);
        }

    }
}

//SaveFileDialog Save_File;
////Save_File = new SaveFileDialog();
////Save_File.Filter = "WMV files|*.wmv|MPEG files|*.mpeg|AVI files|*.avi|Ogg files|*.ogg|All files|*.*";
////Save_File.Title = "Save Camera Recording";
////Save_File.InitialDirectory = directory;
////Save_File.FileName = "Camera_Recoring";
////Save_File.DefaultExt = "wmv";
////Save_File.FileOk += (sender, e) =>
////{
////    filename = Save_File.FileName;
////    switch (Save_File.FileName.Split('.').Last())
////    {
////        case "mpeg":
////            codec = VideoCodec.MPEG4;
////            break;
////        case "avi":
////            codec = VideoCodec.WMV2;
////            break;
////        case "ogg":
////            codec = VideoCodec.Theora;
////            break;
////        default:
////            break;
////    }
////};
////if (Save_File.ShowDialog() == DialogResult.OK)
////{ }