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
using UXTool_V4.Features;
using System.IO;
using System.Windows.Forms;
using UXTool_V4._Page;
using System.Windows.Controls;
using System.Collections.Generic;

namespace UXTool_V4
{
    //definition for delegate that holds steps in the Affectiva analysis process
    public delegate void ProcessDelegate();

    public partial class MainWindow : Window
    {
        string directory;                   //directory where the project will start from
        Record_Page record_page;            //page control that holds everything related to recording video and audio
        Edit_Page edit_page;                //page control that holds media playback and extracting data from the media
        
        KeyFrame_Page keyframe_page;        //page control that displays keyframes with their emotion, peak value, timestamp, length, and note

        public MainWindow()
        {
            InitializeComponent();
            ExitButton.Click += Exit_option_Click;
            DirectoryButton.Click += DirectoryButton_Click;
            
            RecordView.Click += Record_View_Click;
            EditView.Click += Edit_View_Click;
            KeyframeView.Click += KeyFrame_View_Click;

            //the working directory of the project will always be here
            directory = @"C:\AffectivaEmotionStudy";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            edit_page = new Edit_Page();
            keyframe_page = new KeyFrame_Page();
            record_page = new Record_Page(directory +"\\" );
            //starts on edit_page
            PageFrame.Content = record_page;
        }

        private void Exit_option_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Record_View_Click(object sender, RoutedEventArgs e)
        {
            //releases the videos before switching to record_page
            if (PageFrame.Content == edit_page)
            {
                record_page.Stop_Pressed = false;
                edit_page.Pause_Media();
                edit_page.LetGoOfVideos();
            }

            PageFrame.Content = record_page;
        }

        private void KeyFrame_View_Click(object sender, RoutedEventArgs e)
        {
            if (PageFrame.Content == record_page)
            {
                //stops recording just in case
                if (record_page.Is_Recording)
                    record_page.Stop_Recording();

                PageFrame.Content = keyframe_page;
            }
            else if (PageFrame.Content == edit_page)
            {
                //pauses the video and gets all the keyframe data
                PageFrame.Content = keyframe_page;
                edit_page.Pause_Media();
                keyframe_page.ReadIn_From_File(edit_page.Directory);
            }

        }

        private void Edit_View_Click(object sender, RoutedEventArgs e)
        {
            if (PageFrame.Content == record_page)
            {
                //stopd recording when going to edit_page
                if (record_page.Is_Recording)
                    record_page.Stop_Recording();

                if (record_page.Stop_Pressed == true)
                {
                    //clears data when new media is recorded
                    if (File.Exists(directory + "\\temp_data.csv"))
                        File.Delete(directory + "\\temp_data.csv");

                    if (File.Exists(directory + "\\Automatic_Keyframes.csv"))
                        File.Delete(directory + "\\Automatic_Keyframes.csv");

                    if (Directory.Exists(directory + "\\temp_thumbs\\"))
                    {
                        List<string> pics = new List<string>(System.IO.Directory.EnumerateFiles(directory + "\\temp_thumbs\\"));
                        for (int i = 0; i < pics.Count; i++)
                            File.Delete(pics[i]);

                        Directory.Delete(directory + "\\temp_thumbs\\");
                    }
                }
            }

            PageFrame.Content = edit_page;

            //edit_page now has the new media
            if (record_page.Stop_Pressed)
                edit_page.ReadIn_From_File(record_page.Full_Directory, record_page.Full_Directory + record_page.Camera_Filepath, record_page.Full_Directory + record_page.Screen_Filepath, record_page.Full_Directory + record_page.AudioInput_Filepath, record_page.Full_Directory + record_page.AudioOutput_Filepath);

            if (edit_page.camera.Source != null && edit_page.screen.Source != null)
                edit_page.Quickload_Media();
        }

        private void DirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog filepath = new FolderBrowserDialog();
            filepath.RootFolder = Environment.SpecialFolder.MyComputer;
            if (filepath.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                directory = filepath.SelectedPath;
                record_page.Directory_Changed(directory + "\\");
                edit_page.directorychanged(directory + "\\");
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            record_page.Closing();
            base.OnClosing(e);
        }
    }
}

