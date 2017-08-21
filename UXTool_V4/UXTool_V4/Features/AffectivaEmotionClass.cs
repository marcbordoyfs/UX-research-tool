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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Affdex;

namespace UXTool_V4.Features
{
    class AffectivaEmotionClass : ImageListener, ProcessStatusListener
    {
        private VideoDetector faceDetector;             //handles affectiva's analysis of a video file for emotions 
        private string faceDirectory;                   //path to the video when it is processing
        private StreamWriter data_writer;               //writes points to be displayed by the graph (time, emotion_value)
        private EmotionContainer curr_emotions;         //emotions the current frame has

        private StreamWriter keyframe_writer;           //a writer to make automatic keyframes (points of interest)
        private EmotionContainer emotion_thresholds;    //container that holds thresholds to trigger writing a keyframe
        private float[] keyframe_length_tracker;        //keeps the time each emotion stayed over threshold
        private int[] peaks;                            //contains the peak value when emotions go over threshold
        private EmotionContainer prev_emotions;         //emotions the previous frame had to track peaks

        private string thumb_directory;                 //folder to save thumbnails
        private int bitmap_count;                       //naming for each picture saved
        private float frame_count;                      //count that determines how many seconds should pass before saving the next frame for a thumbnail
        private string videofilepath;

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

        public AffectivaEmotionClass()
        {
            faceDirectory = String.Empty;
            faceDetector = new VideoDetector(4);
            faceDetector.setClassifierPath("data");
            faceDetector.setImageListener(this);
            faceDetector.setProcessStatusListener(this);
            bitmap_count = 1;
            frame_count = -3;
            faceDetector.setDetectAllAppearances(false);
            faceDetector.setDetectAllEmotions(true);
            faceDetector.setDetectAllExpressions(false);
            faceDetector.setDetectAllEmojis(false);

            curr_emotions = new EmotionContainer(0);
            prev_emotions = new EmotionContainer(0);
            peaks = new int[9];
            keyframe_length_tracker = new float[9];
            emotion_thresholds = new EmotionContainer(50);
        }

        //values used to make keyframes automatically
        public void setEmotionThresholds(EmotionContainer _emotions)
        {
            emotion_thresholds = _emotions;
        }

        //starts processing a video file with faces and a database of keyframes to write to
        public void beginFaceProcessing(string filepath, string keyframe_dir)
        {
            videofilepath = filepath;
            faceDirectory = filepath.Substring(0, filepath.Length - filepath.Split('\\').Last().Length);
            thumb_directory = faceDirectory + "temp_thumbs\\";
            if (!Directory.Exists(thumb_directory))
                Directory.CreateDirectory(thumb_directory);
            faceDetector.start();
            //check it
            faceDetector.process(filepath);
            Console.WriteLine("Face process started. " + DateTime.Now.ToString());
            data_writer = new StreamWriter(faceDirectory + "temp_data.csv", false);
            keyframe_writer = new StreamWriter(keyframe_dir, false);
        }

        //callback function, no need to call this
        public void onImageCapture(Frame frame)
        {
            if (frame.getTimestamp() > frame_count + 3)
            {
                byte[] pixels = frame.getBGRByteArray();
                Bitmap bm = new Bitmap(frame.getWidth(), frame.getHeight(), PixelFormat.Format24bppRgb);
                var bounds = new Rectangle(0, 0, frame.getWidth(), frame.getHeight());
                BitmapData bmpData = bm.LockBits(bounds, ImageLockMode.WriteOnly, bm.PixelFormat);
                IntPtr ptr = bmpData.Scan0;

                int data_x = 0;
                int ptr_x = 0;
                int row_bytes = frame.getWidth() * 3;

                for (int y = 0; y < frame.getHeight(); y++)
                {
                    Marshal.Copy(pixels, data_x, ptr + ptr_x, row_bytes);
                    data_x += row_bytes;
                    ptr_x += bmpData.Stride;
                }
                bm.UnlockBits(bmpData);
                bm.GetThumbnailImage(128, 96, new Image.GetThumbnailImageAbort(() => { return false; }), IntPtr.Zero).Save(thumb_directory + "face " + bitmap_count++ + ".jpeg"); 
                frame_count = frame.getTimestamp();
            } 

            frame.Dispose();
        }

        //callback function, no need to call this
        public void onImageResults(Dictionary<int, Face> faces, Frame frame)
        {
            float timestamp = frame.getTimestamp();
            if (faces.Count > 0)
            {
                curr_emotions.setValues
                (
                   (int)faces.First().Value.Emotions.Anger,
                   (int)faces.First().Value.Emotions.Contempt,
                   (int)faces.First().Value.Emotions.Disgust,
                   (int)faces.First().Value.Emotions.Engagement,
                   (int)faces.First().Value.Emotions.Fear,
                   (int)faces.First().Value.Emotions.Joy,
                   (int)faces.First().Value.Emotions.Sadness,
                   (int)faces.First().Value.Emotions.Surprise,
                   (int)faces.First().Value.Emotions.Valence
                );

                for (int i = 0; i < emotion_thresholds.Length; ++i)
                {
                    data_writer.Write(timestamp);
                    data_writer.Write(',');
                    data_writer.WriteLine(curr_emotions[i].ToString());

                    if (curr_emotions[i] >= emotion_thresholds[i])
                    {
                        if (keyframe_length_tracker[i] == 0)
                        {
                            keyframe_length_tracker[i] = timestamp;
                            peaks[i] = curr_emotions[i];
                        }

                        else if (curr_emotions[i] > prev_emotions[i])
                            peaks[i] = curr_emotions[i];
                    }

                    if (curr_emotions[i] < emotion_thresholds[i])
                    {
                        if (keyframe_length_tracker[i] > 0)
                        {
                            float len = timestamp - keyframe_length_tracker[i];
                            Keyframe k = new Keyframe(emotion_thresholds.emotion_string_list[i], peaks[i], timestamp, len, videofilepath);
                            k.Comment = "Automatic";

                            keyframe_writer.WriteLine(k.ToString());

                            keyframe_length_tracker[i] = 0;
                            peaks[i] = 0;
                        }
                    }
                }

                prev_emotions.copyValues(curr_emotions);
                curr_emotions.clearValues();
            }

            frame.Dispose();
        }

        //callback function, no need to call this
        public void onProcessingException(AffdexException ex)
        {
            Console.WriteLine(ex.Message);
        }

        //callback function, no need to call this
        public void onProcessingFinished()
        {
           
            data_writer.Close();
			keyframe_writer.Close();
            faceDirectory = String.Empty;
            Console.WriteLine("Emotions processed successfully. " + DateTime.Now.ToString());
            process();
        }

        public void stopProcessing()
        {
            data_writer.Close();
            keyframe_writer.Close();
            faceDirectory = String.Empty;
            Console.WriteLine("Emotion processing stopped manually.");
        }

    }
}
