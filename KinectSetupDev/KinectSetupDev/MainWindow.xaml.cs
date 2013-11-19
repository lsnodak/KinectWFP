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
using Microsoft.Kinect; //I added this manually.
using Microsoft.Kinect.Toolkit;
using System.IO; //Needed to reference this first

namespace KinectSetupDev
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private KinectSensorChooser _sensorChooser = new KinectSensorChooser();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.SensorChooserUI.KinectSensorChooser = _sensorChooser;
            //_sensorChooser.KinectChanged += new EventHandler<KinectChangedEventArgs>(_sensorChooser_KinectChanged);
            //_sensorChooser.Start();

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.Image.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            //if (null == this.sensor)
            //{
            //    this.statusBarText.Text = Properties.Resources.NoKinectReady;
            //}


        }

        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        void _sensorChooser_KinectChanged(object sender, KinectChangedEventArgs e)
        {
            KinectSensor oldSensor = e.OldSensor;

            StopKinect(oldSensor);

            KinectSensor newSensor = e.NewSensor;

            //Had to manually add this if statement to ensure it doesn't crash
            if (newSensor != null)
            {
                newSensor.ColorStream.Enable();
                newSensor.DepthStream.Enable();
                newSensor.SkeletonStream.Enable();

                //newSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(_sensor_AllFramesReady);
                try
                {
                    newSensor.Start();
                }
                catch (System.IO.IOException)
                {
                    _sensorChooser.TryResolveConflict();
                }
            }
        }

        void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //throw new NotImplementedException();

            /*using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null) return;

                byte[] pixels = new byte[colorFrame.PixelDataLength];

                colorFrame.CopyPixelDataTo(pixels);

                int stride = colorFrame.Width * 4;
                image1.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            }*/
        }

        void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                //sensor.AudioSource.Stop(); //Commented this out because the audio source never got started
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(_sensorChooser.Kinect);
        }
    }
}
