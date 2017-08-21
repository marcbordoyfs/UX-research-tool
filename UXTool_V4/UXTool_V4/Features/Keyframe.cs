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

namespace UXTool_V4.Features
{
    class Keyframe
    {
        private string emotion;
        private int emotion_number;
        private float timestamp;
        private float length;
        private string comment;
        private string id;

        public float Timestamp
        {
            get
            {
                return timestamp;
            }

            set
            {
                timestamp = value;
            }
        }

        public float Length
        {
            get
            {
                return length;
            }

            set
            {
                length = value;
            }
        }

        public string Comment
        {
            get
            {
                return comment;
            }

            set
            {
                comment = value;
            }
        }

        public string Emotion
        {
            get
            {
                return emotion;
            }

            set
            {
                emotion = value;
            }
        }

        public int Emotion_number
        {
            get
            {
                return emotion_number;
            }

            set
            {
                emotion_number = value;
            }
        }

        public Keyframe(string _emotion, int emotion_value, float _timestamp, float _length, string _id)
        {
            emotion = _emotion;
            emotion_number = emotion_value;
            timestamp = _timestamp;
            length = _length;
            id = _id;
            comment = null;
        }

        public override string ToString()
        {
            return Emotion+ ',' + Emotion_number + ',' + Timestamp.ToString("0.00") + ',' + Length.ToString("0.00") + ',' + Comment + ',' + id;
        }
    }
}
