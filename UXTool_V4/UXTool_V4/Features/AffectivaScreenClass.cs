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
    class AffectivaScreenClass : ImageListener, ProcessStatusListener
    {
        private VideoDetector screenDetector;   //handles affectiva's analysis of a video file of the screen
        private string screenDirectory;         //path to file being processed

        private string thumb_directory;         //folder to save thumbnails
        private int bitmap_count;               //naming for each picture saved
        private float frame_count;              //count that determines how many seconds should pass before saving the next frame for a thumbnail

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

        public AffectivaScreenClass()
        {
            screenDirectory = String.Empty;
            screenDetector = new VideoDetector(4, 1, FaceDetectorMode.LARGE_FACES);
            screenDetector.setClassifierPath("data");
            screenDetector.setImageListener(this);
            screenDetector.setProcessStatusListener(this);
            bitmap_count = 1;
            frame_count = -3;

            screenDetector.setDetectAllAppearances(false);
            screenDetector.setDetectAllEmotions(false);
            screenDetector.setDetectAllExpressions(false);
            screenDetector.setDetectAllEmojis(false);
        }

        //starts processing a video file of the computer screen
        public void beginScreenProcessing(string filepath)
        {
            screenDirectory = filepath.Substring(0, filepath.Length - filepath.Split('\\').Last().Length);
            thumb_directory = screenDirectory + "temp_thumbs\\";
            if (!Directory.Exists(thumb_directory))
                Directory.CreateDirectory(thumb_directory);
            screenDetector.start();
            screenDetector.process(filepath);
            Console.WriteLine("Screen process started. " + DateTime.Now.ToString());
        }

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
                bm.GetThumbnailImage(128, 96, new Image.GetThumbnailImageAbort(() => { return false; }), IntPtr.Zero).Save(thumb_directory + "screen " + bitmap_count++ + ".jpeg");
                frame_count = frame.getTimestamp();
            }

            frame.Dispose();
        }

        public void onImageResults(Dictionary<int, Face> faces, Frame frame)
        {
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
            screenDirectory = String.Empty;
            Console.WriteLine("Screen processed successfully. " + DateTime.Now.ToString());
            process();
        }

        public void stopProcessing()
        {
            screenDirectory = String.Empty;
            Console.WriteLine("Screen processing stopped manually.");
        }
    }
}
