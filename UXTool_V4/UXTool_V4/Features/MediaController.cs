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
using System.Windows;
using System.Windows.Controls;


namespace UXTool_V4.Features
{
    class MediaController
    {
        MediaElement camera;
        MediaElement screen;
        MediaElement audio;
        MediaElement sound;
        double start;

        public Uri hasfilepath
        {
            get
            {
                return camera.Source;
            }
        }

        public double MediaPosition
        {
            get
            {
                return camera.Position.TotalSeconds;
            }
            set
            {
                camera.Position = TimeSpan.FromSeconds(value);
                screen.Position = TimeSpan.FromSeconds(value);
                audio.Position = TimeSpan.FromSeconds(value);
                sound.Position = TimeSpan.FromSeconds(value);
            }
        }

        public double MediaVolume
        {
            get
            {
                return audio.Volume;
            }
            set
            {
                audio.Volume = value;
                sound.Volume = value;
            }
        }

        public int FormattedPosition
        {
            //get
            //{
            //    if (camera.Position.Hours != 0)
            //        return camera.Position.Hours + ":" + camera.Position.Minutes + ":" + camera.Position.Seconds.ToString("00");
            //    else
            //        return camera.Position.Minutes + ":" + camera.Position.Seconds.ToString("00");
            //}
            get
            {
                return (camera.Position.Hours * 60 * 60) + (camera.Position.Minutes * 60) + camera.Position.Seconds;
            }
        }

        public MediaState MediaBehavior
        {
            get
            {
                return camera.LoadedBehavior;
            }
        }

        public MediaController(MediaElement _camera, MediaElement _screen, MediaElement _audio, MediaElement _sound)
        {
            camera = _camera;
            screen = _screen;
            audio = _audio;
            sound = _sound;
            start = 0;

            camera.MediaOpened += Media_Player_Opened;
            camera.UnloadedBehavior = MediaState.Manual;
            camera.LoadedBehavior = MediaState.Play;
            

            screen.MediaOpened += Media_Player_Opened;
            screen.UnloadedBehavior = MediaState.Manual;
            screen.LoadedBehavior = MediaState.Play;

            audio.MediaOpened += Media_Player_Opened;
            audio.UnloadedBehavior = MediaState.Manual;
            audio.LoadedBehavior = MediaState.Play;

            sound.MediaOpened += Media_Player_Opened;
            sound.UnloadedBehavior = MediaState.Manual;
            sound.LoadedBehavior = MediaState.Play;


        }

        private void Media_Player_Opened(object sender, RoutedEventArgs e)
        {
            camera.LoadedBehavior = MediaState.Manual;
            screen.LoadedBehavior = MediaState.Manual;
            audio.LoadedBehavior = MediaState.Manual;
            sound.LoadedBehavior = MediaState.Manual;
            MediaPosition = start;
        }

        public void Click_Start()
        {
            camera.Play();
            camera.LoadedBehavior = MediaState.Play;
            screen.Play();
            screen.LoadedBehavior = MediaState.Play;
            audio.Play();
            audio.LoadedBehavior = MediaState.Play;
            sound.Play();
            sound.LoadedBehavior = MediaState.Play;
        }

        public void Click_Pause()
        {
            camera.Pause();
            camera.LoadedBehavior = MediaState.Pause;
            screen.Pause();
            screen.LoadedBehavior = MediaState.Pause;
            audio.Pause();
            audio.LoadedBehavior = MediaState.Pause;
            sound.Pause();
            sound.LoadedBehavior = MediaState.Pause;
        }

        public void Click_Resume()
        {
            Click_Start();
        }

        public void Click_Stop()
        {
            if (camera.LoadedBehavior == MediaState.Play || camera.LoadedBehavior == MediaState.Manual)
            {
                camera.Stop();
                camera.LoadedBehavior = MediaState.Stop;
                screen.Stop();
                screen.LoadedBehavior = MediaState.Stop;
                audio.Stop();
                audio.LoadedBehavior = MediaState.Stop;
                sound.Stop();
                sound.LoadedBehavior = MediaState.Stop;
            }

        }

        public void Load_Files(string _camera, string _screen, string _audio , string _sound)
        {
            FileInfo fi = new FileInfo(_camera);
            if (File.Exists(_camera) && !IsFileLocked(fi))
                camera.Source = new Uri(_camera);
            fi = new FileInfo(_screen);
            if (File.Exists(_screen) && !IsFileLocked(fi))
                screen.Source = new Uri(_screen);
            fi = new FileInfo(_audio);
            if (File.Exists(_audio) && !IsFileLocked(fi))
                audio.Source = new Uri(_audio);
            fi = new FileInfo(_sound);
            if (File.Exists(_sound) && !IsFileLocked(fi))
                sound.Source = new Uri(_sound);
        }

        public void Close_Files()
        {
            camera.Close();         
            screen.Close();
            audio.Close();
            sound.Close();
        }

        public void Load_Files_At_Position(string _camera, string _screen, string _audio, string _sound, double _start)
        {
            FileInfo fi = new FileInfo(_camera);
            if (File.Exists(_camera) && !IsFileLocked(fi))
                camera.Source = new Uri(_camera);
            fi = new FileInfo(_screen);
            if (File.Exists(_screen) && !IsFileLocked(fi))
                screen.Source = new Uri(_screen);
            fi = new FileInfo(_audio);
            if (File.Exists(_audio) && !IsFileLocked(fi))
                audio.Source = new Uri(_audio);
            fi = new FileInfo(_sound);
            if (File.Exists(_sound) && !IsFileLocked(fi))
                sound.Source = new Uri(_sound);
            start = _start;
        }

        public void Form_Closed()
        {
            if (camera != null)
            {
                camera.Stop();
                camera.Close();
            }
            if (screen != null)
            {
                screen.Stop();
                screen.Close();
            }
            if (audio != null)
            {
                audio.Stop();
                audio.Close();
            }
            if (sound != null)
            {
                sound.Stop();
                sound.Close();
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
