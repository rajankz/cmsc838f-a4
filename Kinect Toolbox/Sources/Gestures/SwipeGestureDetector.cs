using System;
using Microsoft.Kinect;

namespace Kinect.Toolbox
{
    public class SwipeGestureDetector : GestureDetector
    {
        public float SwipeMinimalLength {get;set;}
        public float SwipeMaximalHeight {get;set;}
        public int SwipeMininalDuration {get;set;}
        public int SwipeMaximalDuration {get;set;}

        public SwipeGestureDetector(int windowSize = 20)
            : base(windowSize)
        {
            SwipeMinimalLength = 0.4f;
            SwipeMaximalHeight = 0.2f;
            SwipeMininalDuration = 250;
            SwipeMaximalDuration = 1500;
        }

        protected bool ScanPositions(Func<Vector3, Vector3, bool> heightFunction, Func<Vector3, Vector3, bool> directionFunction, 
            Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        {
            int start = 0;

            for (int index = 1; index < Entries.Count - 1; index++)
            {
                if (!heightFunction(Entries[0].Position, Entries[index].Position) || !directionFunction(Entries[index].Position, Entries[index + 1].Position))
                {
                    start = index;
                }

                if (lengthFunction(Entries[index].Position, Entries[start].Position))
                {
                    double totalMilliseconds = (Entries[index].Time - Entries[start].Time).TotalMilliseconds;
                    if (totalMilliseconds >= minTime && totalMilliseconds <= maxTime)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void LookForGesture()
        {
            /*
            // Swipe to right
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight, // Height
                (p1, p2) => p2.X - p1.X > -0.01f, // Progression to right
                (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
                SwipeMininalDuration, SwipeMaximalDuration)) // Duration
            {
                RaiseGestureDetected("SwipeToRight");
                return;
            }

            // Swipe to left
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight,  // Height
                (p1, p2) => p2.X - p1.X < 0.01f, // Progression to right
                (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
                SwipeMininalDuration, SwipeMaximalDuration))// Duration
            {
                RaiseGestureDetected("SwipeToLeft");
                return;
            }

            */

            // From left to right
            if (ScanPositions ((p1, p2) => Math.Abs(p2.Y - p1.Y) <0.20f, 
               (p1, p2) => p2.X - p1.X> - 0.01f,
               (p1, p2 ) =>   Math.Abs(p2.X - p1.X)> 0.2f, 250, 2500))
            {
                RaiseGestureDetected ("LeftToRight");
                return;
            }

            // from right to left
            if (ScanPositions ((p1, p2) => Math.Abs(p2.Y - p1.Y) <0.20f, 
               (p1, p2) => p2.X - p1.X <0.01f, (p1, p2) => 
               Math.Abs(p2.X - p1.X)> 0.2f, 250, 2500))
            {
                RaiseGestureDetected ("RightToLeft");
                return;
            }

            // From down to up
            if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < 0.20f,
               (p1, p2) => p2.Y - p1.Y > -0.01f,
               (p1, p2) => Math.Abs(p2.Y - p1.Y) > 0.2f, 250, 2500))
            {
                RaiseGestureDetected("DownToUp");
                return;
            }

            // from up to down
            if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < 0.20f,
               (p1, p2) => p2.Y - p1.Y < 0.01f, (p1, p2) =>
               Math.Abs(p2.Y - p1.Y) > 0.2f, 250, 2500))
            {
                RaiseGestureDetected("UpToDown");
                return;
            }

            // From back to front
            if (ScanPositions (
                (p1, p2) => Math.Abs (p2.Y - p1.Y) <0.15f, 
               (p1, p2) => p2.Z - p1.Z <0.01f, 
               (p1, p2) => 
                Math.Abs(p2.Z - p1.Z)> 0.2f, 250, 2500))
            {
                RaiseGestureDetected ("BackToFront");
                return;
            }

            // from front to back
            if (ScanPositions(
                (p1, p2) => Math.Abs(p2.Y - p1.Y) <0.15f, 
               (p1, p2) => p2.Z - p1.Z>-0.04f, 
               (p1, p2 ) 
                 => Math.Abs (p2.Z - p1.Z)> 0.4f, 250, 2500))
            {
                RaiseGestureDetected ("FrontToBack");
                return;
            }

        }
    }
}