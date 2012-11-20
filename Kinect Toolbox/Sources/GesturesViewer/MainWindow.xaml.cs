using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Kinect.Toolbox;
using Kinect.Toolbox.Record;
using System.IO;
using Microsoft.Kinect;
using Microsoft.Win32;
using Kinect.Toolbox.Voice;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using ARDrone.Hud;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AviationInstruments;
using ARDrone.Control;
using ARDrone.Capture;
using ARDrone.Hud;
using ARDrone.Input;
using ARDrone.Input.Utils;
using ARDrone.Control.Commands;
using ARDrone.Control.Data;
using ARDrone.Control.Events;
using Emgu.CV;
using Emgu.CV.Structure;

using ARDrone.Control;
using ARDrone.Control.Commands;
using ARDrone.Control.Data;
using ARDrone.Control.Events;
using ARDrone.Detection;
using ARDrone.Input;
using ARDrone.Input.Utils;

using Kinect.Toolbox;
using Kinect.Toolbox.Record;

namespace GesturesViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        KinectSensor kinectSensor;

        SwipeGestureDetector swipeGestureRecognizer;
        TemplatedGestureDetector circleGestureRecognizer;
        readonly ColorStreamManager colorManager = new ColorStreamManager();
        readonly DepthStreamManager depthManager = new DepthStreamManager();
        AudioStreamManager audioManager;
        SkeletonDisplayManager skeletonDisplayManager;
        readonly ContextTracker contextTracker = new ContextTracker();
        EyeTracker eyeTracker;
        ParallelCombinedGestureDetector parallelCombinedGestureDetector;
        readonly AlgorithmicPostureDetector algorithmicPostureRecognizer = new AlgorithmicPostureDetector();
        TemplatedPostureDetector templatePostureDetector;
        private bool recordNextFrameForPosture;
        bool displayDepth;

        string circleKBPath;
        string letterT_KBPath;

        KinectRecorder recorder;
        KinectReplay replay;

        BindableNUICamera nuiCamera;

        private Skeleton[] skeletons;

        VoiceCommander voiceCommander;

        private DroneControl droneControl;
        private DroneConfig currentDroneConfig;
        private HudConfig currentHudConfig;
        private DispatcherTimer timerStatusUpdate;
        private DispatcherTimer timerVideoUpdate;
        private DispatcherTimer timerHudStatusUpdate;
        private VideoRecorder videoRecorder;
        private SnapshotRecorder snapshotRecorder;
        private InstrumentsManager instrumentsManager;
        private HudInterface hudInterface;
        private ARDrone.Input.InputManager inputManager;
        private Dictionary<String, DateTime> booleanInputFadeout;
        private Boolean droneInAir;
        private String currentStatusMsg = "";
        private int count = 3;

        public MainWindow()
        {
            InitializeDroneControl();
            InitializeComponent();

            InitializeOtherComponents();

           
        }

        private void TryConnectDrone()
        {
            while (!droneControl.IsConnected && count-- > 0)
            {
                Connect();
                if (!droneControl.IsConnected)
                {
                    Thread.Sleep(5000);

                }
            }
        }

        private void droneControl_Error_Async(object sender, DroneErrorEventArgs e)
        {
            //this.BeginInvoke(new DroneErrorEventHandler(droneControl_Error_Sync), sender, e);
        }

        private void droneControl_Error_Sync(object sender, DroneErrorEventArgs e)
        {
            //HandleError(e);
        }

        private void droneControl_ConnectionStateChanged_Async(object sender, DroneConnectionStateChangedEventArgs e)
        {
            //this.BeginInvoke(new DroneConnectionStateChangedEventHandler(droneControl_ConnectionStateChanged_Sync), sender, e);
        }

        private void droneControl_ConnectionStateChanged_Sync(object sender, DroneConnectionStateChangedEventArgs e)
        {
            // HandleConnectionStateChange(e);
        }

        private void InitializeOtherComponents()
        {
            InitializeDroneControlEventHandlers();

            InitializeTimers();
            InitializeInputManager();

            InitializeAviationControls();
            InitializeHudInterface();

            InitializeRecorders();
        }

        private void InitializeDroneControlEventHandlers()
        {
            droneControl.Error += droneControl_Error_Async;
            droneControl.ConnectionStateChanged += droneControl_ConnectionStateChanged_Async;
        }

        private void InitializeDroneControl()
        {
            currentDroneConfig = new DroneConfig();
            currentDroneConfig.Load();

            InitializeDroneControl(currentDroneConfig);
        }

        private void InitializeDroneControl(DroneConfig droneConfig)
        {
            droneControl = new DroneControl(droneConfig);
        }

        private void InitializeTimers()
        {
            timerStatusUpdate = new DispatcherTimer();
            timerStatusUpdate.Interval = new TimeSpan(0, 0, 1);
            timerStatusUpdate.Tick += new EventHandler(timerStatusUpdate_Tick);

            timerHudStatusUpdate = new DispatcherTimer();
            timerHudStatusUpdate.Interval = new TimeSpan(0, 0, 0, 0, 50);
            timerHudStatusUpdate.Tick += new EventHandler(timerHudStatusUpdate_Tick);

            timerVideoUpdate = new DispatcherTimer();
            timerVideoUpdate.Interval = new TimeSpan(0, 0, 0, 0, 50);
            timerVideoUpdate.Tick += new EventHandler(timerVideoUpdate_Tick);
        }

        private void timerStatusUpdate_Tick(object sender, EventArgs e)
        {
            //UpdateStatus();
        }

        private void timerHudStatusUpdate_Tick(object sender, EventArgs e)
        {
            //UpdateHudStatus();
        }

        private void timerVideoUpdate_Tick(object sender, EventArgs e)
        {
            //SetNewVideoImage();
        }

        private void InitializeInputManager()
        {
            /*
             inputManager = new ARDrone.Input.InputManager(Utility.GetWindowHandle(this));
             inputManager.SwitchInputMode(Input.InputManager.InputMode.ControlInput);

             inputManager.NewInputState += inputManager_NewInputState;
             inputManager.NewInputDevice += inputManager_NewInputDevice;
             inputManager.InputDeviceLost += inputManager_InputDeviceLost;
             */
            booleanInputFadeout = new Dictionary<String, DateTime>();
        }

        private void InitializeAviationControls()
        {
            instrumentsManager = new InstrumentsManager(droneControl);
            //instrumentsManager.addInstrument(this.attitudeControl);
            //instrumentsManager.addInstrument(this.altimeterControl);
            //instrumentsManager.addInstrument(this.headingControl);
            instrumentsManager.startManage();
        }

        private void InitializeHudInterface()
        {
            currentHudConfig = new HudConfig();
            currentHudConfig.Load();

            InitializeHudInterface(currentHudConfig);
        }

        private void InitializeHudInterface(HudConfig hudConfig)
        {
            HudConstants hudConstants = new HudConstants(droneControl.FrontCameraFieldOfViewDegrees);

            hudInterface = new HudInterface(hudConfig, hudConstants);
        }

        private void InitializeRecorders()
        {
            videoRecorder = new VideoRecorder();
            snapshotRecorder = new SnapshotRecorder();

            //videoRecorder.CompressionComplete += new EventHandler(videoRecorder_CompressionComplete);
            //videoRecorder.CompressionError += new System.IO.ErrorEventHandler(videoRecorder_CompressionError);
        }

        private void Connect()
        {
            if (droneControl.IsConnected) { return; }

            currentStatusMsg = "Connecting to the Drone...";
            droneControl.ConnectToDroneNetworkAndDrone();
            //UpdateUISync("Connecting to the drone");
        }

        private void Disconnect()
        {
            if (!droneControl.IsConnected) { return; }

            if (droneInAir)
                Land();

            //timerVideoUpdate.Stop();
            currentStatusMsg = "Disconnecting from Drone...";
            droneControl.Disconnect();
            //UpdateUISync("Disconnecting from the drone");
        }

        private void Takeoff()
        {
            if (droneInAir)
                return;

            Command takeOffCommand = new FlightModeCommand(DroneFlightMode.TakeOff);

            if (!droneControl.IsCommandPossible(takeOffCommand))
                return;
            currentStatusMsg = "Initiating Drone take off..";
            droneControl.SendCommand(takeOffCommand);
            //Thread.Sleep(5000);
            droneInAir = true;
            //UpdateUIAsync("Taking off");
        }

        private void Land()
        {
            if (!droneInAir)
                return;

            Command landCommand = new FlightModeCommand(DroneFlightMode.Land);

            if (!droneControl.IsCommandPossible(landCommand))
                return;

            currentStatusMsg = "Initialing Drone Landing...";
            droneControl.SendCommand(landCommand);
            //this.DrawText(skeleton, drawingContext, "Landing");
        }

        void Kinects_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (kinectSensor == null)
                    {
                        kinectSensor = e.Sensor;
                        Initialize();
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (kinectSensor == e.Sensor)
                    {
                        Clean();
                        System.Windows.MessageBox.Show("Kinect was disconnected");
                    }
                    break;
                case KinectStatus.NotReady:
                    break;
                case KinectStatus.NotPowered:
                    if (kinectSensor == e.Sensor)
                    {
                        Clean();
                        System.Windows.MessageBox.Show("Kinect is no more powered");
                    }
                    break;
                default:
                    System.Windows.MessageBox.Show("Unhandled Status: " + e.Status);
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            circleKBPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"data\circleKB.save");
            letterT_KBPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"data\t_KB.save");

            try
            {
                //listen to any status change for Kinects
                KinectSensor.KinectSensors.StatusChanged += Kinects_StatusChanged;

                //loop through all the Kinects attached to this PC, and start the first that is connected without an error.
                foreach (KinectSensor kinect in KinectSensor.KinectSensors)
                {
                    if (kinect.Status == KinectStatus.Connected)
                    {
                        kinectSensor = kinect;
                        break;
                    }
                }

                if (KinectSensor.KinectSensors.Count == 0)
                    System.Windows.MessageBox.Show("No Kinect found");
                else
                    Initialize();

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void Initialize()
        {
            if (kinectSensor == null)
                return;

            audioManager = new AudioStreamManager(kinectSensor.AudioSource);
            audioBeamAngle.DataContext = audioManager;

            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectSensor.ColorFrameReady += kinectRuntime_ColorFrameReady;

            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            kinectSensor.DepthFrameReady += kinectSensor_DepthFrameReady;

            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
                                                   {
                                                 Smoothing = 0.5f,
                                                 Correction = 0.5f,
                                                 Prediction = 0.5f,
                                                 JitterRadius = 0.05f,
                                                 MaxDeviationRadius = 0.04f
                                             });
            kinectSensor.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;

            swipeGestureRecognizer = new SwipeGestureDetector();
            swipeGestureRecognizer.OnGestureDetected += OnGestureDetected;

            skeletonDisplayManager = new SkeletonDisplayManager(kinectSensor, kinectCanvas);

            kinectSensor.Start();

            LoadCircleGestureDetector();
            LoadLetterTPostureDetector();

            nuiCamera = new BindableNUICamera(kinectSensor);

            elevationSlider.DataContext = nuiCamera;

            voiceCommander = new VoiceCommander("record", "stop");
            voiceCommander.OrderDetected += voiceCommander_OrderDetected;

            StartVoiceCommander();

            kinectDisplay.DataContext = colorManager;

            parallelCombinedGestureDetector = new ParallelCombinedGestureDetector();
            parallelCombinedGestureDetector.OnGestureDetected += OnGestureDetected;
            parallelCombinedGestureDetector.Add(swipeGestureRecognizer);
            parallelCombinedGestureDetector.Add(circleGestureRecognizer);
        }

        void kinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if (replay != null && !replay.IsFinished)
                return;

            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;

                if (recorder != null && ((recorder.Options & KinectRecordOptions.Depth) != 0))
                {
                    recorder.Record(frame);
                }

                if (!displayDepth)
                    return;

                depthManager.Update(frame);
            }
        }

        void kinectRuntime_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            if (replay != null && !replay.IsFinished)
                return;

            using (var frame = e.OpenColorImageFrame())
            {
                if (frame == null)
                    return;

                if (recorder != null && ((recorder.Options & KinectRecordOptions.Color) != 0))
                {
                    recorder.Record(frame);
                }

                if (displayDepth)
                    return;

                colorManager.Update(frame);
            }
        }

        void kinectRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (replay != null && !replay.IsFinished)
                return;

            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                if (recorder != null && ((recorder.Options & KinectRecordOptions.Skeletons) != 0))
                    recorder.Record(frame);

                frame.GetSkeletons(ref skeletons);

                if (skeletons.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
                    return;

                ProcessFrame(frame);
            }
        }

        void ProcessFrame(ReplaySkeletonFrame frame)
        {
            Dictionary<int, string> stabilities = new Dictionary<int, string>();
            foreach (var skeleton in frame.Skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;

                //if (eyeTracker == null)
                //    eyeTracker = new EyeTracker(kinectSensor);

                //eyeTracker.Track(skeleton);

                contextTracker.Add(skeleton.Position.ToVector3(), skeleton.TrackingId);
                stabilities.Add(skeleton.TrackingId, contextTracker.IsStableRelativeToCurrentSpeed(skeleton.TrackingId) ? "Stable" : "Non stable");
                if (!contextTracker.IsStableRelativeToCurrentSpeed(skeleton.TrackingId))
                    continue;

                //if (eyeTracker.IsLookingToSensor.HasValue && eyeTracker.IsLookingToSensor == false)
                //    continue;

                foreach (Joint joint in skeleton.Joints)
                {
                    if (joint.TrackingState != JointTrackingState.Tracked)
                        continue;

                    if (joint.JointType == JointType.HandRight)
                    {
                        circleGestureRecognizer.Add(joint.Position, kinectSensor);
                    }
                    else if (joint.JointType == JointType.HandLeft)
                    {
                        swipeGestureRecognizer.Add(joint.Position, kinectSensor);
                        if (controlMouse.IsChecked == true)
                            MouseController.Current.SetHandPosition(kinectSensor, joint, skeleton);
                    }
                }

                algorithmicPostureRecognizer.TrackPostures(skeleton);
                templatePostureDetector.TrackPostures(skeleton);

                if (recordNextFrameForPosture)
                {
                    templatePostureDetector.AddTemplate(skeleton);
                    recordNextFrameForPosture = false;
                }
            }

            skeletonDisplayManager.Draw(frame.Skeletons, seatedMode.IsChecked == true);

            stabilitiesList.ItemsSource = stabilities;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Clean();
        }

        private void Clean()
        {
            if (swipeGestureRecognizer != null)
            {
                swipeGestureRecognizer.OnGestureDetected -= OnGestureDetected;
            }

            if (audioManager != null)
            {
                audioManager.Dispose();
                audioManager = null;
            }

            if (parallelCombinedGestureDetector != null)
            {
                parallelCombinedGestureDetector.Remove(swipeGestureRecognizer);
                parallelCombinedGestureDetector.Remove(circleGestureRecognizer);
                parallelCombinedGestureDetector = null;
            }

            CloseGestureDetector();

            ClosePostureDetector();

            if (voiceCommander != null)
            {
                voiceCommander.OrderDetected -= voiceCommander_OrderDetected;
                voiceCommander.Stop();
                voiceCommander = null;
            }

            if (recorder != null)
            {
                recorder.Stop();
                recorder = null;
            }

            if (eyeTracker != null)
            {
                eyeTracker.Dispose();
                eyeTracker = null;
            }

            if (kinectSensor != null)
            {
                kinectSensor.DepthFrameReady -= kinectSensor_DepthFrameReady;
                kinectSensor.SkeletonFrameReady -= kinectRuntime_SkeletonFrameReady;
                kinectSensor.ColorFrameReady -= kinectRuntime_ColorFrameReady;
                kinectSensor.Stop();
                kinectSensor = null;
            }
        }

        private void replayButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog { Title = "Select filename", Filter = "Replay files|*.replay" };

            if (openFileDialog.ShowDialog() == true)
            {
                if (replay != null)
                {
                    replay.SkeletonFrameReady -= replay_SkeletonFrameReady;
                    replay.ColorImageFrameReady -= replay_ColorImageFrameReady;
                    replay.Stop();
                }
                Stream recordStream = File.OpenRead(openFileDialog.FileName);

                replay = new KinectReplay(recordStream);

                replay.SkeletonFrameReady += replay_SkeletonFrameReady;
                replay.ColorImageFrameReady += replay_ColorImageFrameReady;
                replay.DepthImageFrameReady += replay_DepthImageFrameReady;

                replay.Start();
            }
        }

        void replay_DepthImageFrameReady(object sender, ReplayDepthImageFrameReadyEventArgs e)
        {
            if (!displayDepth)
                return;

            depthManager.Update(e.DepthImageFrame);
        }

        void replay_ColorImageFrameReady(object sender, ReplayColorImageFrameReadyEventArgs e)
        {
            if (displayDepth)
                return;

            colorManager.Update(e.ColorImageFrame);
        }

        void replay_SkeletonFrameReady(object sender, ReplaySkeletonFrameReadyEventArgs e)
        {
            ProcessFrame(e.SkeletonFrame);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            displayDepth = !displayDepth;

            if (displayDepth)
            {
                viewButton.Content = "View Color";
                kinectDisplay.DataContext = depthManager;
            }
            else
            {
                viewButton.Content = "View Depth";
                kinectDisplay.DataContext = colorManager;
            }
        }

        private void nearMode_Checked_1(object sender, RoutedEventArgs e)
        {
            if (kinectSensor == null)
                return;

            kinectSensor.DepthStream.Range = DepthRange.Near;
            kinectSensor.SkeletonStream.EnableTrackingInNearRange = true;
        }

        private void nearMode_Unchecked_1(object sender, RoutedEventArgs e)
        {
            if (kinectSensor == null)
                return;

            kinectSensor.DepthStream.Range = DepthRange.Default;
            kinectSensor.SkeletonStream.EnableTrackingInNearRange = false;
        }

        private void seatedMode_Checked_1(object sender, RoutedEventArgs e)
        {
            if (kinectSensor == null)
                return;

            kinectSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
        }

        private void seatedMode_Unchecked_1(object sender, RoutedEventArgs e)
        {
            if (kinectSensor == null)
                return;

            kinectSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
        }

        private void TakeOffLandDrone()
        {
            if (!droneControl.IsConnected)
            {
                TryConnectDrone();
            }
            if(droneControl.IsConnected)
            {
                if (droneInAir)
                    Land();
                else
                    Takeoff();
            }
        }

        private void MoveDroneForward()
        {
            if (droneInAir)
            {
                Navigate(0, 0, 1, 0);
            }
        }

        private void MoveDroneBackward()
        {
            if (droneInAir)
            {
                Navigate(0, 0, -1, 0);
            }
        }

        private void Navigate(float roll, float pitch, float yaw, float gaz)
        {
            FlightMoveCommand flightMoveCommand = new FlightMoveCommand(roll, pitch, yaw, gaz);

            if (droneControl.IsCommandPossible(flightMoveCommand))
                droneControl.SendCommand(flightMoveCommand);
        }

    }
}
