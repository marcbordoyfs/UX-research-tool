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
using System.Windows.Forms;
using Accord.Audio;
using Accord.Audio.Formats;
using Accord.DirectSound;
using System.Windows.Interop;
using System.Diagnostics;
using Accord.Video.DirectShow;

namespace UXTool_V4.Features
{
    class AudioRecorder
    {
        IAudioSource Audio_Source;
        WaveEncoder Wave_Encoder;
        Stream filestream;
        
        string directory;

        bool paused;
        bool isready;

        public AudioRecorder(string directive)
        {
            Audio_Source = new AudioCaptureDevice()
            {
                Format = SampleFormat.Format16Bit,
                DesiredFrameSize = 1370,
                SampleRate = 44100
            };
            Audio_Source.NewFrame += Audio_Source_NewFrame;
            Audio_Source.AudioSourceError += Audio_Source_AudioSourceError;

            directory = directive;
            paused = false;
        }

        public AudioRecorder(string directive, AudioDeviceCollection adc)
        {
            Audio_Source = new AudioCaptureDevice(adc.Default)
            {

                Format = SampleFormat.Format16Bit,
                DesiredFrameSize = 1370,
                SampleRate = 44100
            };
            Audio_Source.NewFrame += Audio_Source_NewFrame;
            Audio_Source.AudioSourceError += Audio_Source_AudioSourceError;
            
            directory = directive;
            paused = false;
        }
        public AudioRecorder(string directive, IAudioSource selectedsource )
        {
            Audio_Source = selectedsource;
            Audio_Source.NewFrame += Audio_Source_NewFrame;
            Audio_Source.AudioSourceError += Audio_Source_AudioSourceError;

            directory = directive;
            paused = false;
        }

        public void Start_AudioRecording()
        {            
            bool check = File.Exists(directory);

            if (check)
                filestream = new FileStream(directory, FileMode.Truncate);
            else
                filestream = new FileStream(directory, FileMode.CreateNew); 

            Wave_Encoder = new WaveEncoder(filestream);
        }

        public void Start()
        {
           Audio_Source.Start();
        }

        public void Stop_AudioRecording()
        {
            Audio_Source.SignalToStop();
            Audio_Source.WaitForStop();
            isready = false;
            filestream.Close();
        }

        public bool Is_Running()
        {
            return Audio_Source.IsRunning;
        }

        public bool Is_Paused()
        {
            return paused;
        }

        public bool Is_Ready()
        {
            return isready;
        }

        public void Puased_Clicked()
        {
            Audio_Source.SignalToStop();
            Audio_Source.WaitForStop();
            paused = true;
        }

        public void Resume_Clicked()
        {
            Audio_Source.Start();
            paused = false;
        }

        public void Form_Closed()
        {
            if (Audio_Source != null)
                Audio_Source.SignalToStop();
        }

        private void Audio_Source_NewFrame(object sender, NewFrameEventArgs e)
        {
            Wave_Encoder.Encode(e.Signal);
        }

        private void Audio_Source_AudioSourceError(object sender, AudioSourceErrorEventArgs e)
        {
            throw new Exception(e.Description);
        }
    }
}
