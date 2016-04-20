﻿using System.IO;
using System.Threading.Tasks;

using Plugin.Media;
using Plugin.Media.Abstractions;

using Microsoft.ProjectOxford.Emotion;

using Xamarin.Forms;
using System;
using Microsoft.ProjectOxford.Emotion.Contract;
using Xamarin;

namespace FaceOff
{
	public class PictureViewModel : BaseViewModel
	{
		#region Constant Fields
		readonly string[] _emotionStrings = { "Anger", "Contempt", "Disgust", "Fear", "Happiness", "Neutral", "Sadness", "Surprise" };
		#endregion

		#region Fields
		ImageSource _photo1ImageSource, _photo2ImageSource;
		string _scoreButton1Text, _scoreButton2Text;
		bool _isTakeLeftPhotoButtonEnabled = true;
		bool _isTakeRightPhotoButtonEnabled = true;
		string _pageTitle;
		int _emotionNumber;
		bool _isCalculatingPhoto1Score, _isCalculatingPhoto2Score, _image1IsVertical, _image2IsVertical;
		bool _isScore1ButtonEnabled, _isScore2ButtonEnabled, _isScore1ButtonVisable, _isScore2ButtonVisable;
		string _photo1Results, _photo2Results;
		#endregion

		#region Constructors
		public PictureViewModel()
		{
			SetEmotion();

			TakePhoto1ButtonPressed = new Command(async () =>
			{
				Insights.Track(InsightsConstants.PhotoButton1Tapped);

				var imageMediaFile = await GetMediaFileFromCamera("FaceOff", "PhotoImage1");
				if (imageMediaFile == null)
					return;

				IsTakeLeftPhotoButtonEnabled = false;

				ScoreButton1Text = "Calculating Score";
				IsScore1ButtonVisable = true;

				Photo1ImageSource = ImageSource.FromStream(() =>
				{
					return GetPhotoStream(imageMediaFile, false);
				});

				if (!_image1IsVertical)
				{
					EnsureAndroidImageIsVertical(270, 1);
					_image1IsVertical = true;
				}

				IsCalculatingPhoto1Score = true;

				var emotionArray = await GetEmotionResultsFromMediaFile(imageMediaFile, false);
				ScoreButton1Text = $"Score: {GetPhotoEmotionScore(emotionArray, 0)}";
				_photo1Results = GetStringOfAllPhotoEmotionScores(emotionArray, 0);

				IsCalculatingPhoto1Score = false;
				IsScore1ButtonEnabled = true;

				imageMediaFile.Dispose();
			});

			TakePhoto2ButtonPressed = new Command(async () =>
			{
				Insights.Track(InsightsConstants.PhotoButton2Tapped);

				var imageMediaFile = await GetMediaFileFromCamera("FaceOff", "PhotoImage2");
				if (imageMediaFile == null)
					return;

				IsTakeRightPhotoButtonEnabled = false;

				ScoreButton2Text = "Calculating Score";
				IsScore2ButtonVisable = true;

				Photo2ImageSource = ImageSource.FromStream(() =>
				{
					return GetPhotoStream(imageMediaFile, false);
				});

				if (!_image2IsVertical)
				{
					EnsureAndroidImageIsVertical(270, 2);
					_image2IsVertical = true;
				}

				IsCalculatingPhoto2Score = true;

				var emotionArray = await GetEmotionResultsFromMediaFile(imageMediaFile, false);
				ScoreButton2Text = $"Score: {GetPhotoEmotionScore(emotionArray, 0)}";
				_photo2Results = GetStringOfAllPhotoEmotionScores(emotionArray, 0);

				IsCalculatingPhoto2Score = false;
				IsScore2ButtonEnabled = true;

				imageMediaFile.Dispose();
			});

			ResetButtonPressed = new Command(() =>
			{
				Insights.Track(InsightsConstants.ResetButtonTapped);

				SetEmotion();

				Photo1ImageSource = null;
				Photo2ImageSource = null;

				IsTakeLeftPhotoButtonEnabled = true;
				IsTakeRightPhotoButtonEnabled = true;

				ScoreButton1Text = null;
				ScoreButton2Text = null;

				IsScore1ButtonEnabled = false;
				IsScore2ButtonEnabled = false;

				IsScore1ButtonVisable = false;
				IsScore2ButtonVisable = false;
			});
		}
		#endregion

		#region Properties
		public Command TakePhoto1ButtonPressed { get; protected set; }
		public Command TakePhoto2ButtonPressed { get; protected set; }
		public Command ResetButtonPressed { get; protected set; }
		public Command SubmitButtonPressed { get; protected set; }

		public event EventHandler RotateImage;
		public event EventHandler DisplayAlert;

		public ImageSource Photo1ImageSource
		{
			get
			{
				return _photo1ImageSource;
			}
			set
			{
				_photo1ImageSource = value;
				OnPropertyChanged("Photo1ImageSource");
			}
		}

		public ImageSource Photo2ImageSource
		{
			get
			{
				return _photo2ImageSource;
			}
			set
			{
				_photo2ImageSource = value;
				OnPropertyChanged("Photo2ImageSource");
			}
		}

		public bool IsPhotoImage1Enabled
		{
			get { return !IsTakeLeftPhotoButtonEnabled; }
		}
		public bool IsPhotoImage2Enabled
		{
			get { return !IsTakeRightPhotoButtonEnabled; }
		}

		public bool IsTakeLeftPhotoButtonEnabled
		{
			get
			{
				return _isTakeLeftPhotoButtonEnabled;
			}
			set
			{
				_isTakeLeftPhotoButtonEnabled = value;
				OnPropertyChanged("IsTakeLeftPhotoButtonEnabled");
				OnPropertyChanged("IsPhotoImage1Enabled");
			}
		}

		public bool IsTakeRightPhotoButtonEnabled
		{
			get
			{
				return _isTakeRightPhotoButtonEnabled;
			}
			set
			{
				_isTakeRightPhotoButtonEnabled = value;
				OnPropertyChanged("IsTakeRightPhotoButtonEnabled");
				OnPropertyChanged("IsPhotoImage2Enabled");
			}
		}

		public string PageTitle
		{
			get
			{
				return _pageTitle;
			}
			set
			{
				_pageTitle = value;
				OnPropertyChanged("PageTitle");
			}
		}

		public string ScoreButton1Text
		{
			get
			{
				return _scoreButton1Text;
			}
			set
			{
				_scoreButton1Text = value;
				OnPropertyChanged("ScoreButton1Text");
			}
		}

		public string ScoreButton2Text
		{
			get
			{
				return _scoreButton2Text;
			}
			set
			{
				_scoreButton2Text = value;
				OnPropertyChanged("ScoreButton2Text");
			}
		}

		public bool IsCalculatingPhoto1Score
		{
			get
			{
				return _isCalculatingPhoto1Score;
			}
			set
			{
				_isCalculatingPhoto1Score = value;
				OnPropertyChanged("IsCalculatingPhoto1Score");
				OnPropertyChanged("IsResetButtonEnabled");
			}
		}

		public bool IsCalculatingPhoto2Score
		{
			get
			{
				return _isCalculatingPhoto2Score;
			}
			set
			{
				_isCalculatingPhoto2Score = value;
				OnPropertyChanged("IsCalculatingPhoto2Score");
				OnPropertyChanged("IsResetButtonEnabled");
			}
		}

		public bool IsResetButtonEnabled
		{
			get
			{
				return !(IsCalculatingPhoto2Score || IsCalculatingPhoto1Score);
			}
		}

		public bool IsScore1ButtonEnabled
		{
			get
			{
				return _isScore1ButtonEnabled;
			}
			set
			{
				_isScore1ButtonEnabled = value;
				OnPropertyChanged("IsScore1ButtonEnabled");
			}
		}

		public bool IsScore2ButtonEnabled
		{
			get
			{
				return _isScore2ButtonEnabled;
			}
			set
			{
				_isScore2ButtonEnabled = value;
				OnPropertyChanged("IsScore2ButtonEnabled");
			}
		}

		public bool IsScore1ButtonVisable
		{
			get
			{
				return _isScore1ButtonVisable;
			}
			set
			{
				_isScore1ButtonVisable = value;
				OnPropertyChanged("IsScore1ButtonVisable");
			}
		}

		public bool IsScore2ButtonVisable
		{
			get
			{
				return _isScore2ButtonVisable;
			}
			set
			{
				_isScore2ButtonVisable = value;
				OnPropertyChanged("IsScore2ButtonVisable");
			}
		}

		#endregion

		#region Methods
		Stream GetPhotoStream(MediaFile mediaFile, bool disposeMediaFile)
		{
			var stream = mediaFile.GetStream();

			if (disposeMediaFile)
				mediaFile.Dispose();

			return stream;
		}

		async Task<MediaFile> GetMediaFileFromCamera(string directory, string filename)
		{
			await CrossMedia.Current.Initialize();

			if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
			{
				//Todo Handle case when no camera is available
				return null;
			}

			var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
			{
				Directory = directory,
				Name = filename,
				DefaultCamera = CameraDevice.Front
			});

			return file;
		}

		async Task<Emotion[]> GetEmotionResultsFromMediaFile(MediaFile mediaFile, bool disposeMediaFile)
		{
			if (mediaFile == null)
				return null;

			try
			{
				var emotionClient = new EmotionServiceClient(CognitiveServicesConstants.EmotionApiKey);

				using (var handle = Insights.TrackTime(InsightsConstants.CalculateEmotion))
				{
					return await emotionClient.RecognizeAsync(GetPhotoStream(mediaFile, disposeMediaFile));
				}
			}
			catch (Exception e)
			{
				Insights.Report(e);
				return null;
			}
		}

		int GetRandomNumberForEmotion()
		{
			int randomNumber;

			do
			{
				var rnd = new Random();
				randomNumber = rnd.Next(0, _emotionStrings.Length);
			} while (randomNumber == _emotionNumber);

			return randomNumber;
		}

		void SetPageTitle(int emotionNumber)
		{
			PageTitle = _emotionStrings[emotionNumber];
		}

		void SetEmotion()
		{
			_emotionNumber = GetRandomNumberForEmotion();
			SetPageTitle(_emotionNumber);
		}

		string GetPhotoEmotionScore(Emotion[] emotionResults, int emotionResultNumber)
		{
			var errorMessage = "Error, No Face Detected";
			float rawEmotionScore;

			if (emotionResults == null || emotionResults.Length < 1)
				return errorMessage;

			try
			{
				switch (_emotionNumber)
				{
					case 0:
						rawEmotionScore = emotionResults[emotionResultNumber].Scores.Anger;
						break;
					case 1:
						rawEmotionScore = emotionResults[emotionResultNumber].Scores.Contempt;
						break;
					case 2:
						rawEmotionScore = emotionResults[emotionResultNumber].Scores.Disgust;
						break;
					case 3:
						rawEmotionScore = emotionResults[emotionResultNumber].Scores.Fear;
						break;
					case 4:
						rawEmotionScore = emotionResults[emotionResultNumber].Scores.Happiness;
						break;
					case 5:
						rawEmotionScore = emotionResults[emotionResultNumber].Scores.Neutral;
						break;
					case 6:
						rawEmotionScore = emotionResults[emotionResultNumber].Scores.Sadness;
						break;
					case 7:
						rawEmotionScore = emotionResults[emotionResultNumber].Scores.Surprise;
						break;
					default:
						return errorMessage;
				}

				var emotionScoreAsPercentage = ConvertFloatToPercentage(rawEmotionScore);

				return emotionScoreAsPercentage;
			}
			catch (Exception e)
			{
				Insights.Report(e);
				return errorMessage;
			}
		}

		string GetStringOfAllPhotoEmotionScores(Emotion[] emotionResults, int emotionResultNumber)
		{
			string allEmotionsString = "";

			allEmotionsString += $"Anger: {ConvertFloatToPercentage(emotionResults[emotionResultNumber].Scores.Anger)}\n";
			allEmotionsString += $"Disgust: {ConvertFloatToPercentage(emotionResults[emotionResultNumber].Scores.Disgust)}\n";
			allEmotionsString += $"Fear: {ConvertFloatToPercentage(emotionResults[emotionResultNumber].Scores.Fear)}\n";
			allEmotionsString += $"Happiness: {ConvertFloatToPercentage(emotionResults[emotionResultNumber].Scores.Happiness)}\n";
			allEmotionsString += $"Neutral: {ConvertFloatToPercentage(emotionResults[emotionResultNumber].Scores.Neutral)}\n";
			allEmotionsString += $"Sadness: {ConvertFloatToPercentage(emotionResults[emotionResultNumber].Scores.Sadness)}\n";
			allEmotionsString += $"Surprise: {ConvertFloatToPercentage(emotionResults[emotionResultNumber].Scores.Surprise)}\n";

			return allEmotionsString;
		}


		string ConvertFloatToPercentage(float floatToConvert)
		{
			return floatToConvert.ToString("#0.##%");

		}

		void EnsureAndroidImageIsVertical(int degreesOfClockwiseRotation, int imageNumberToRotate)
		{
			if (Device.OS == TargetPlatform.Android)
			{
				var parameters = new RotatableImageParameters
				{
					DegreesOfClockwiseRotation = degreesOfClockwiseRotation,
					ImageNumberToRotate = imageNumberToRotate

				};
				RotateImage(parameters, new EventArgs());
			}
		}

		#endregion
	}
}

