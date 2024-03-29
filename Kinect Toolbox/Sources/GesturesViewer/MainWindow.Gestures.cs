﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Kinect.Toolbox;
using Microsoft.Kinect;



namespace GesturesViewer
{
    partial class MainWindow
    {

        void LoadCircleGestureDetector()
        {
            using (Stream recordStream = File.Open(circleKBPath, FileMode.OpenOrCreate))
            {
                circleGestureRecognizer = new TemplatedGestureDetector("Circle", recordStream);
                circleGestureRecognizer.DisplayCanvas = gesturesCanvas;
                circleGestureRecognizer.OnGestureDetected += OnGestureDetected;

                MouseController.Current.ClickGestureDetector = circleGestureRecognizer;
            }
        }

        private void recordGesture_Click(object sender, RoutedEventArgs e)
        {
            if (circleGestureRecognizer.IsRecordingPath)
            {
                circleGestureRecognizer.EndRecordTemplate();
                recordGesture.Content = "Record Gesture";
                return;
            }

            circleGestureRecognizer.StartRecordTemplate();
            recordGesture.Content = "Stop Recording";
        }

        void OnGestureDetected(string gesture)
        {
            int pos = detectedGestures.Items.Add(string.Format("{0} : {1}", gesture, DateTime.Now));

            detectedGestures.SelectedIndex = pos;

            PerformGestureActions(gesture);

        }

        void CloseGestureDetector()
        {
            if (circleGestureRecognizer == null)
                return;

            using (Stream recordStream = File.Create(circleKBPath))
            {
                circleGestureRecognizer.SaveState(recordStream);
            }
            circleGestureRecognizer.OnGestureDetected -= OnGestureDetected;
        }

        void PerformGestureActions(String gesture)
        {
            gesture = gesture.ToLower();
            switch (gesture)
            {
                case "circle":
                    TakeOffLandDrone();
                break;

                case "lefttoright":
                MoveDroneRight();
                break;

               case "righttoleft":
                MoveDroneLeft();
                break;

               case "fronttoback":
                MoveDroneBack();
                break;

               case "backtofront":
                MoveDroneForward();
                break;

               case "downtoup":
                MoveDroneUp();
                break;

               case "uptodown":
                MoveDroneDown();
                break;

                default:
                    break;
            }
            
        }
    }
}
