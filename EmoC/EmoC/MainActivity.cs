using Android.App;
using Android.Widget;
using Android.OS;
using Microsoft.ProjectOxford.Emotion;
using System;
using Android.Content;
using Java.IO;
using Android.Provider;
using Android.Runtime;
using Microsoft.ProjectOxford.Emotion.Contract;

namespace EmoC
{
	[Activity(Label = "EmoC", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		EmotionServiceClient emotionServiceClient = new EmotionServiceClient(myKey);
		private ImageView _imageView;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			RequestWindowFeature(Android.Views.WindowFeatures.NoTitle);
			SetContentView(Resource.Layout.Main);
			CameraHelper.CreateDirectoryForPictures();
			Button emoCameraButton = FindViewById<Button>(Resource.Id.emoButton);
			emoCameraButton.Click += OpenCamera;
			_imageView = FindViewById<ImageView>(Resource.Id.image);
		}

		private void OpenCamera(object sender, EventArgs e)
		{
			var intent = new Intent(MediaStore.ActionImageCapture);
			CameraHelper._file = new File(CameraHelper._dir, string.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
			intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(CameraHelper._file));
			StartActivityForResult(intent, 0);
		}

		protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			try
			{
				var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
				var contentUri = Android.Net.Uri.FromFile(CameraHelper._file);
				mediaScanIntent.SetData(contentUri);
				SendBroadcast(mediaScanIntent);
				var height = Resources.DisplayMetrics.HeightPixels;
				var width = _imageView.Height;
				CameraHelper.bitmap = CameraHelper.LoadAndResizeBitmap(CameraHelper._file.Path, width, height);
				if (CameraHelper.bitmap != null)
				{
					_imageView.SetImageBitmap(CameraHelper.bitmap);
					CameraHelper.bitmap = null;
				}
				Upload();
			}
			catch (Exception e)
			{
				TextView textView = FindViewById<TextView>(Resource.Id.textView);
				textView.Text = e.Message;
			}
			GC.Collect();
		}

		private async void Upload()
		{
			Emotion[] x = null;
			TextView textView = FindViewById<TextView>(Resource.Id.textView);
			try
			{
				textView.Text = "Start";
				using (System.IO.Stream imageStream = System.IO.File.OpenRead(CameraHelper._file.Path))
				{
					x = await emotionServiceClient.RecognizeAsync(imageStream);
					textView.Text = "Anger: " + Convert.ToString(x[0].Scores.Anger) + System.Environment.NewLine;
					textView.Text += "Contempt: " + Convert.ToString(x[0].Scores.Contempt) + System.Environment.NewLine;
					textView.Text += "Disgust: " + Convert.ToString(x[0].Scores.Disgust) + System.Environment.NewLine;
					textView.Text += "Fear: " + Convert.ToString(x[0].Scores.Fear) + System.Environment.NewLine;
					textView.Text += "Happiness: " + Convert.ToString(x[0].Scores.Happiness) + System.Environment.NewLine;
					textView.Text += "Neutral: " + Convert.ToString(x[0].Scores.Neutral) + System.Environment.NewLine;
					textView.Text += "Sadness: " + Convert.ToString(x[0].Scores.Sadness) + System.Environment.NewLine;
					textView.Text += "Surprise: " + Convert.ToString(x[0].Scores.Surprise) + System.Environment.NewLine;
				}
			}
			catch (Exception e)
			{
				textView.Text = e.Message;
			}
			//Toast.MakeText(this, "Finish", ToastLength.Long).Show();
			//Toast.MakeText(this, x[0].Scores.Neutral.ToString(), ToastLength.Long).Show();
		}
	}
}


