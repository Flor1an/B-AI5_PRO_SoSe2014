
using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using System.Windows.Threading;
using LightBuzz.Vitruvius;

namespace ProSoSe2014
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        bool glued = false;
        public MainWindow()
        {
            InitializeComponent();


            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
        }

        bool closing = false;
        const int skeletonCount = 6;
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];
        GestureController _gestureController;
        const int zoomScale = 50;
        const int zoomScaleBackground = zoomScale * 20;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);

        }

        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor old = (KinectSensor)e.OldValue;

            StopKinect(old);

            KinectSensor sensor = (KinectSensor)e.NewValue;

            if (sensor == null)
            {
                return;
            }


            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.0f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            sensor.SkeletonStream.Enable(parameters);

            sensor.SkeletonStream.Enable();

            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);

            sensor.EnableAllStreams();

            sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;


            _gestureController = new GestureController(GestureType.All);
            _gestureController.GestureRecognized += GestureController_GestureRecognized;




            try
            {
                sensor.Start();
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser1.AppConflictOccurred();
            }


        }



        //start
        void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    //canvas.ClearSkeletons();
                    //tblHeights.Text = string.Empty;

                    var skeletons = frame.Skeletons().Where(s => s.TrackingState == SkeletonTrackingState.Tracked);

                    foreach (var skeleton in skeletons)
                    {
                        if (skeleton != null)
                        {
                            // Update skeleton gestures.
                            _gestureController.Update(skeleton);

                            // Draw skeleton.
                            //canvas.DrawSkeleton(skeleton);

                            // Display user height.
                            //tblHeights.Text += string.Format("\nUser {0}: {1}cm", skeleton.TrackingId, skeleton.Height());
                        }
                    }
                }
            }
        }
        //end

        void GestureController_GestureRecognized(object sender, GestureEventArgs e)
        {
            // Display the gesture type.
            txtLog.Text += e.Name + "\n";

            // Do something according to the type of the gesture.
            switch (e.Name)
            {
                //case GestureType.JoinedHands.ToString():
                //    break;
                //case GestureType.Menu:
                //    break;
                //case GestureType.SwipeDown:
                //    break;
                //case GestureType.SwipeLeft:
                //    break;
                //case GestureType.SwipeRight:
                //    break;
                //case GestureType.SwipeUp:
                //    break;
                //case GestureType.WaveLeft:
                //    break;
                //case GestureType.WaveRight:
                //    break;
                case "ZoomIn"://GestureType.ZoomIn:
                    zooming(true);

                    break;
                //case GestureType.ZoomOut:
                //    //Target.Width -= 10;
                //    //Target.Height -= 10;

                //    //Key.Width -= 10;
                //    //Key.Height -= 10;
                //    break;
                case "ZoomOut"://GestureType.ZoomIn:
                    zooming(false);


                    break;
                default:
                    break;
            }
        }
        //end

        void zooming(bool zoomIn)
        {
            int x = 1;
            if (!zoomIn)
            {
                x = -1;
            }
          //  fadezooming(Target, zoomScale * x);
            try
            {

          
            Target.Width += zoomScale * x;
            Target.Height += zoomScale * x;
         

            Key.Width += zoomScale * x;
            Key.Height += zoomScale * x;

            //fadezooming(cristalImage,  (cristalImage.Height + zoomScaleBackground * x));

            //cristalImage.Width += zoomScaleBackground * x;
            //Canvas.SetLeft(cristalImage, Canvas.GetLeft(cristalImage) - ((zoomScaleBackground * x) / 2));

            //cristalImage.Height += zoomScaleBackground * x;
            //Canvas.SetTop(cristalImage, Canvas.GetTop(cristalImage) - ((zoomScaleBackground * x) / 2));

            //Canvas.SetLeft(Target, Canvas.GetLeft(Target) - zoomScale * x);
            //Canvas.SetLeft(Key, Canvas.GetLeft(Key) - zoomScale * x);
            }
            catch (Exception)
            {

            }
        }

        //async void fadezooming(Image control, double finalSize)
        //{


        //    while (control.Width<finalSize || control.Height<finalSize)
        //    {
        //        control.Width += 10;
        //        control.Height += 10;


        //    }
        //}

        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }

            //Get a skeleton
            Skeleton first = GetFirstSkeleton(e);

            if (first == null)
            {
                return;
            }



            //set scaled position
            //ScalePosition(headImage, first.Joints[JointType.Head]);
            ScalePosition(leftEllipse, first.Joints[JointType.HandLeft]);
            ScalePosition(rightEllipse, first.Joints[JointType.HandRight]);

            GetCameraPoint(first, e);

        }

        void GetCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
        {

            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null ||
                    kinectSensorChooser1.Kinect == null)
                {
                    return;
                }

                // DepthImagePoint headpoint = CoordinateMapper.MapSkeletonPointToDepthPoint(first.Joints[JointType.Head].Position, DepthImageFormat.Resolution640x480Fps30);


                //Map a joint location to a point on the depth map
                //head
                DepthImagePoint headDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);
                //left hand
                DepthImagePoint leftDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.HandLeft].Position);
                //right hand
                DepthImagePoint rightDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.HandRight].Position);


                //Map a depth point to a point on the color image
                //head
                ColorImagePoint headColorPoint = depth.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y, ColorImageFormat.RgbResolution1280x960Fps12);
                //left hand
                ColorImagePoint leftColorPoint = depth.MapToColorImagePoint(leftDepthPoint.X, leftDepthPoint.Y, ColorImageFormat.RgbResolution1280x960Fps12);
                //right hand
                ColorImagePoint rightColorPoint = depth.MapToColorImagePoint(rightDepthPoint.X, rightDepthPoint.Y, ColorImageFormat.RgbResolution1280x960Fps12);


                //Set location
                CameraPosition(headImage, headColorPoint);
                CameraPosition(leftEllipse, leftColorPoint);
                CameraPosition(rightEllipse, rightColorPoint);

                hPosText.Text = "x:" + headColorPoint.X + " | y:" + headColorPoint.Y;
                lhPosText.Text = "x:" + leftColorPoint.X + " | y:" + leftColorPoint.Y;
                rhPosText.Text = "x:" + rightColorPoint.X + " | y:" + rightColorPoint.Y;



                if (glued == true)
                {
                    CameraPosition(Key, leftColorPoint);
                }

                if (glued == false && isOnKey(leftColorPoint))
                {
                    glued = true;
                }
                else if (glued == true && isOnTarget(leftColorPoint))
                {
                    Target.Fill = new SolidColorBrush(Colors.GreenYellow);

                    dispatcherTimer.Start();
                }
                else
                {
                    SolidColorBrush r = new SolidColorBrush(Colors.Red);
                    r.Opacity = 0.5;
                    Target.Fill = r;

                    dispatcherTimer.Stop();
                    countdown = 0;
                }
            }
        }

        bool isOnTarget(ColorImagePoint hidPosition)
        {
            double targetFromLeft = Canvas.GetLeft(Target);
            double targetFromTop = Canvas.GetTop(Target);

            double targetWidth = Target.Width;
            double targetHeight = Target.Height;


            if ((hidPosition.X > targetFromLeft && hidPosition.X < targetFromLeft + targetWidth) && (hidPosition.Y > targetFromTop && hidPosition.Y < targetFromTop + targetHeight))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        bool isOnKey(ColorImagePoint hidPosition)
        {
            double keyFromLeft = Canvas.GetLeft(Key);
            double keyFromTop = Canvas.GetTop(Key);

            double keyWidth = Key.Width;
            double keyHeight = Key.Height;


            if ((hidPosition.X > keyFromLeft && hidPosition.X < keyFromLeft + keyWidth) && (hidPosition.Y > keyFromTop && hidPosition.Y < keyFromTop + keyHeight))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        int countdown = 0;
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            countdown++;
            if (countdown >= 5)
            {
                dispatcherTimer.Stop();
                countdown = 0;
                changeKeyAndTargetPosition();
                glued = false;
            }
            targetInfo.Text = countdown + "on Target";
        }

        void changeKeyAndTargetPosition()
        {
            Random r = new Random();

            Canvas.SetLeft(Target, r.Next(1280 - (int)Target.Width));
            Canvas.SetTop(Target, r.Next(960 - (int)Target.Height));

            Canvas.SetLeft(Key, r.Next(1280 - (int)Target.Width));
            Canvas.SetTop(Key, r.Next(960 - (int)Target.Height));
        }




        Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }


                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                //get the first tracked skeleton
                Skeleton first = (from s in allSkeletons
                                  where s.TrackingState == SkeletonTrackingState.Tracked
                                  select s).FirstOrDefault();

                return first;

            }
        }

        private void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }


                }
            }
        }

        private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            //Divide by 2 for width and height so point is right in the middle 
            // instead of in top/left corner
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);

        }

        private void ScalePosition(FrameworkElement element, Joint joint)
        {
            //convert the value to X/Y
            //Joint scaledJoint = joint.ScaleTo(1280, 720); 

            //convert & scale (.3 = means 1/3 of joint distance)
            Joint scaledJoint = joint.ScaleTo(1280, 720, .3f, .3f);

            Canvas.SetLeft(element, scaledJoint.Position.X);
            Canvas.SetTop(element, scaledJoint.Position.Y);

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
            StopKinect(kinectSensorChooser1.Kinect);
        }


    }
}
