using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Hardware;
using Android.Graphics.Drawables;
using Task.ServiceLayer;
using Android.Net;

namespace Task
{
    [Activity(Label = "Color Pattern", MainLauncher = true, Icon = "@drawable/icon",ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : Activity, Android.Hardware.ISensorEventListener
    {

      #region Declaration
      RelativeLayout SingleViewBacgroundview;
      SensorManager sensorMgr;
      bool hasUpdated = false;
      DateTime lastUpdate;
      float last_x = 0.0f;
      float last_y = 0.0f;
      float last_z = 0.0f;
     
      const int ShakeDetectionTimeLapse = 250;
      const double ShakeThreshold = 500;
      Random myRandomColorGenerator;
     
      private long lastTouchTime = -1;
      WebserviceSync webServer = new WebserviceSync();
      #endregion

      protected override void OnCreate(Bundle bundle)
      {
          base.OnCreate(bundle);

          // Set our view from the "main" layout resource
          SetContentView(Resource.Layout.Main);

          sensorMgr = (SensorManager)GetSystemService(SensorService);// initialize the SensorManager 


          myRandomColorGenerator = new Random();//random initialization

          SingleViewBacgroundview = FindViewById<RelativeLayout>(Resource.Id.SingleViewBacgroundview);




          SingleViewBacgroundview.SetOnTouchListener(new OnTouchListener(this));



      }

        public class OnTouchListener : Java.Lang.Object,View.IOnTouchListener
        {
            MainActivity _context;
            int xPosition, yPosition;
            private int _xDelta;
            private int _yDelta;
            
            public OnTouchListener(MainActivity context)
            {
                _context = context;
            }
            public bool OnTouch(View view, MotionEvent even)
            {
                switch (even.Action)
                {
                    case MotionEventActions.Down:

                        xPosition = (int)even.GetX();
                        yPosition = (int)even.GetY();

                        RelativeLayout.LayoutParams relativeLayoutParams = new RelativeLayout.LayoutParams(
                                RelativeLayout.LayoutParams.WrapContent,
                                RelativeLayout.LayoutParams.WrapContent);
                   
                     ImageView  generateView = new ImageView(_context);

                     if (_context.myRandomColorGenerator.Next() % 2 == 0)
                    {
                        /* Generate the  view for Square */
                        relativeLayoutParams.SetMargins(xPosition, yPosition, 0, 0);
                        generateView.LayoutParameters = relativeLayoutParams;
                        generateView.LayoutParameters.Width = 100;
                        generateView.LayoutParameters.Height = 100;
                        generateView.Tag = "S";
                        generateView.SetImageResource(Resource.Drawable.squareimg);
                        new LoadingImgeFromServerAsync(_context, generateView, "S", "NEW", ((ViewGroup)view)).Execute();
                    }
                    else
                    {
                        /* Generate the  view for circle */

                        relativeLayoutParams.SetMargins(xPosition, yPosition, 0, 0);
                        generateView.LayoutParameters = relativeLayoutParams;
                        generateView.LayoutParameters.Width = 100;
                        generateView.LayoutParameters.Height = 100;
                        generateView.Tag = "C";
                        generateView.SetImageResource(Resource.Drawable.circleimg);
                        new LoadingImgeFromServerAsync(_context, generateView, "C", "NEW", ((ViewGroup)view)).Execute();

                    }

                    generateView.Touch += generateView_Touch;

                        break;


                    case MotionEventActions.Move:
                        

                        break;
                    default:
                        return false;
                }
                return true;
            }

            void generateView_Touch(object sender, View.TouchEventArgs e)
            {
                  
                ImageView iv=(ImageView)sender;
                int X = (int)e.Event.RawX;
                int Y = (int)e.Event.RawY;
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                        RelativeLayout.LayoutParams lParams = (RelativeLayout.LayoutParams)(((View)sender)).LayoutParameters;
                        _xDelta = X - lParams.LeftMargin;
                        _yDelta = Y - lParams.TopMargin;
                        break;
                    case MotionEventActions.Move:
                        RelativeLayout.LayoutParams layoutParams = (RelativeLayout.LayoutParams)(((View)sender)).LayoutParameters;
                        layoutParams.LeftMargin = X - _xDelta;
                        layoutParams.TopMargin = Y - _yDelta;
                        layoutParams.RightMargin = -250;
                        layoutParams.BottomMargin = -250;
                        (((View)sender)).LayoutParameters = layoutParams;
                        break;
                    case MotionEventActions.Up:
                       
                             iv.PerformClick();
                        break;
        
                   
                }
            }
     
        
        }

       


        protected override void OnResume()
        {
            base.OnResume();

            sensorMgr.RegisterListener(this, sensorMgr.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Ui); 
        }

        protected override void OnPause()
        {
            base.OnPause();

            sensorMgr.UnregisterListener(this);
        }


        /// <summary>
        /// Applying Color for ImageView
        /// </summary>
        /// <param name="v"></param>
        /// <param name="hexColor"></param>
        /// <param name="imgType"></param>
        /// <param name="isNewtWorkAvaiable"></param>
        /// <returns></returns>
        public Drawable ApplyHexColor(View v, String hexColor,String imgType,bool isNewtWorkAvaiable)
        {
          
           
            ImageView img= (ImageView)v;
              Drawable  background = img.Drawable;

            try
            {

                if (isNewtWorkAvaiable == true && hexColor != "")
                {
                    ((GradientDrawable)background).SetColor(Color.ParseColor("#" + hexColor));
                    ((GradientDrawable)background).SetStroke(1,Color.ParseColor("#" + hexColor));
                }
                else
                {
                    if (imgType == "S")
                    {

                        img.SetImageResource(Resource.Drawable.squareimg);
                        background = img.Drawable;
                        ((GradientDrawable)background).SetColor(RandomColourGenerator());

                    }
                    else
                    {
                        ((GradientDrawable)background).SetColor(RandomColourGenerator());
                        ((GradientDrawable)background).SetStroke(1, RandomColourGenerator());
                    }
                    
                }

            }
            catch (Exception ex)
            {

            }
            return background;
        }

        public Color RandomColourGenerator()
        {
           
            try
            {
                Random rnd = new Random();
                 return Color.Argb(255, rnd.Next(256), rnd.Next(256), rnd.Next(256));
            }
            catch(Exception ex)
            {

            }
            return Color.White;
        }

        /// <summary>
        /// Remove all views by shaking a device 
        /// </summary>
        /// <param name="sensor"></param>
        /// <param name="accuracy"></param>
        #region Android.Hardware.ISensorEventListener implementation

        public void OnAccuracyChanged(Android.Hardware.Sensor sensor, Android.Hardware.SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(Android.Hardware.SensorEvent e)
        {
            if (e.Sensor.Type == Android.Hardware.SensorType.Accelerometer)
            {
                float x = e.Values[0];
                float y = e.Values[1];
                float z = e.Values[2];

                DateTime curTime = System.DateTime.Now;
                if (hasUpdated == false)
                {
                    hasUpdated = true;
                    lastUpdate = curTime;
                    last_x = x;
                    last_y = y;
                    last_z = z;
                }
                else
                {
                    if ((curTime - lastUpdate).TotalMilliseconds > ShakeDetectionTimeLapse)
                    {
                        float diffTime = (float)(curTime - lastUpdate).TotalMilliseconds;
                        lastUpdate = curTime;
                        float total = x + y + z - last_x - last_y - last_z;
                        float speed = Math.Abs(total) / diffTime * 10000;

                        if (speed > ShakeThreshold)
                        {
                            Toast.MakeText(this, "shake detected w/ speed: " + speed, ToastLength.Short).Show();

                            SingleViewBacgroundview.RemoveAllViews();
                        }

                        last_x = x;
                        last_y = y;
                        last_z = z;
                    }
                }
            }
        }
        #endregion

        #region Loading data from webservice
        /// <summary>
        /// Asynchronous call for retriving data from server
        /// </summary>
        public class LoadingImgeFromServerAsync : AsyncTask<Java.Lang.Void, Java.Lang.Void, Java.Lang.Void>
        {
            private ProgressDialog _progressDialog;
           
           
            Bitmap bitmapImage = null;
            private MainActivity _context;
            private ImageView _imageView;
          
            private ViewGroup _viewGroup;
            String _Tag, OutPutString, _From;

        

            public LoadingImgeFromServerAsync(MainActivity _context, ImageView imageView,String Tag,String From,ViewGroup viewGroup)
            {
                
                this._context = _context;
                this._imageView = imageView;
                this._Tag = Tag;
               
                this._viewGroup = viewGroup;
                this._From = From;
            }

            protected override void OnPreExecute()
            {
                base.OnPreExecute();

                try
                {
                    /* To show the progress bar control */
                    _progressDialog = new ProgressDialog(_context);
                    _progressDialog.SetCancelable(true);
                    _progressDialog.SetCanceledOnTouchOutside(false);
                    _progressDialog.Show();
                    /*end of showing progress bar */
                }
                catch (Exception ex)
                {
                   
                }
            }

            protected override Java.Lang.Void RunInBackground(params Java.Lang.Void[] @params)
            {
                try
                {
                     if(_Tag =="S")
                         bitmapImage = _context.webServer.GetImageURLFromURL("http://www.colourlovers.com/api/patterns/random");
                     else
                         OutPutString = _context.webServer.GetHexcodeURLFromURL("http://www.colourlovers.com/api/colors/random");
                }
                catch (Exception ex)
                {
                    
                }
                return null;
            }

            protected override void OnPostExecute(Java.Lang.Void result)
            {
                base.OnPostExecute(result);
                try
                {
                    if (_Tag == "S")
                    {
                        if (bitmapImage != null)
                        {
                            _imageView.SetImageBitmap(bitmapImage);
                            
                        }
                        else
                        {
                            _context.ApplyHexColor(_imageView, "", "S", true);
                        }

                        if (_From == "NEW")
                        {
                            _imageView.Click += _context._imageView_Click;
                            _viewGroup.AddView(_imageView);
                        }

                    }
                    else
                    {
                        if (OutPutString != "ServerError")
                        {

                            _context.ApplyHexColor(_imageView, OutPutString, "C", true);
                        }
                        else if (OutPutString == "ServerError")
                        {

                            _context.ApplyHexColor(_imageView, "", "C", true);

                        }
                        if (_From == "NEW")
                        {
                            _imageView.Click += _context._imageView_Click;
                            _viewGroup.AddView(_imageView);
                        }
                    }

                        
                }
                catch (Exception ex)
                {
                   
                }
                _progressDialog.Dismiss();
            }
            
        }
        #endregion

        /// <summary>
        /// Double tab for update imageView by pattern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _imageView_Click(object sender, EventArgs e)
        {
            try
            {
                ImageView selectedImageview = (ImageView)sender;
                long thisTime = Java.Lang.JavaSystem.CurrentTimeMillis();
                if (thisTime - lastTouchTime < 250)
                {
                    if (selectedImageview.Tag + "" == "S")
                    {
                        if (isNetWorkAvailable())
                        {
                            new LoadingImgeFromServerAsync(this, selectedImageview, "S", "UPDATE", null).Execute();
                        }
                        else
                        {
                            ApplyHexColor(selectedImageview, "", "S", false);
                        }
                    }
                    else
                    {

                        if (isNetWorkAvailable())
                        {
                            new LoadingImgeFromServerAsync(this, selectedImageview, "C", "UPDATE", null).Execute();
                        }
                        else
                        {
                            ApplyHexColor(selectedImageview, "", "C", false);
                        }
                    }

                    lastTouchTime = -1;
                }
                else
                {
                    // too slow
                    lastTouchTime = thisTime;
                }
            }
            catch (Exception ex)
            {

            }
        }

        #region Network
        /// <summary>
        /// Check Network Connection
        /// </summary>
        /// <returns></returns>
        public bool isNetWorkAvailable()
        {
            try
            {
                var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
                var activeConnection = connectivityManager.ActiveNetworkInfo;
                if ((activeConnection != null) && activeConnection.IsConnected)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        #endregion
    }
}

