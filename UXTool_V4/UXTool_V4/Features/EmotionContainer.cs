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
    public class EmotionContainer
    {
        public string[] emotion_string_list; //identifiers for which emotion is at what position in the emotion_values array
        public int[] emotion_values;   //holds the values for each emotion

        public int Length
        {
            get { return emotion_values.Length; }
        }

        public EmotionContainer(int _anger, int _contempt, int _disgust, int _engagement, int _fear, int _joy, int _sadness, int _surprise, int _valence)
        {
            //sets up the two arrays with the ints passed in
            emotion_string_list = new string[]
            {
                "Anger",
                "Contempt",
                "Disgust",
                "Engagement",
                "Fear",
                "Joy",
                "Sadness",
                "Surprise",
                "Valence"
            };
            emotion_values = new int[]
            {
                _anger,         //0 - 100
                _contempt,      //0 - 100
                _disgust,       //0 - 100
                _engagement,    //0 - 100
                _fear,          //0 - 100
                _joy,           //0 - 100
                _sadness,       //0 - 100
                _surprise,      //0 - 100
                _valence        //-100 - 100
            };
        }

        public EmotionContainer(int _default)
        {
            //sets up the arrays with a default value
            emotion_string_list = new string[]
            {
                "Anger",
                "Contempt",
                "Disgust",
                "Engagement",
                "Fear",
                "Joy",
                "Sadness",
                "Surprise",
                "Valence"
            };
            emotion_values = new int[]
            {
                _default,     //0 - 100
                _default,     //0 - 100
                _default,     //0 - 100
                _default,     //0 - 100
                _default,     //0 - 100
                _default,     //0 - 100
                _default,     //0 - 100
                _default,     //0 - 100
                _default,     //-100 - 100
            };
        }

        public int this[int i]
        {
            get { return emotion_values[i]; }
        }

        public void setValues(int _anger, int _contempt, int _disgust, int _engagement, int _fear, int _joy, int _sadness, int _surprise, int _valence)
        {
            //sets values for each emotion
            emotion_values[0] = _anger;
            emotion_values[1] = _contempt;
            emotion_values[2] = _disgust;
            emotion_values[3] = _engagement;
            emotion_values[4] = _fear;
            emotion_values[5] = _joy;
            emotion_values[6] = _sadness;
            emotion_values[7] = _surprise;
            emotion_values[8] = _valence;
        }

        public void copyValues(EmotionContainer data)
        {
            //sets values for each emotion
            emotion_values[0] = data[0];
            emotion_values[1] = data[1];
            emotion_values[2] = data[2];
            emotion_values[3] = data[3];
            emotion_values[4] = data[4];
            emotion_values[5] = data[5];
            emotion_values[6] = data[6];
            emotion_values[7] = data[7];
            emotion_values[8] = data[8];
        }

        public void clearValues()
        {
            //clears all emotion values to 0
            emotion_values[0] = 0;
            emotion_values[1] = 0;
            emotion_values[2] = 0;
            emotion_values[3] = 0;
            emotion_values[4] = 0;
            emotion_values[5] = 0;
            emotion_values[6] = 0;
            emotion_values[7] = 0;
            emotion_values[8] = 0;
        }
    }
}
