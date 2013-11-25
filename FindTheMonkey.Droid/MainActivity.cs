using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using RadiusNetworks.IBeaconAndroid;
//using Region = RadiusNetworks.IBeaconAndroid.Region;
using Color = Android.Graphics.Color;

namespace FindTheMonkey.Droid
{
	[Activity(Label = "Find The Monkey", MainLauncher = true)]
	public class MainActivity : Activity, IBeaconConsumer
	{
		private const string UUID = "e4C8A4FCF68B470D959F29382AF72CE7";
		private const string monkeyId = "Monkey";

		View _view;
		IBeaconManager _iBeaconManager;
		MonitorNotifier _monitorNotifier;
		RangeNotifier _rangeNotifier;
		Region _monitoringRegion;
		Region _rangingRegion;
		TextView _text;
		int _previousProximity;

		public MainActivity()
		{
			_iBeaconManager = IBeaconManager.GetInstanceForApplication(this);

			_monitorNotifier = new MonitorNotifier();
			_rangeNotifier = new RangeNotifier();

			_monitoringRegion = new Region(monkeyId, UUID, null, null);
			_rangingRegion = new Region(monkeyId, UUID, null, null);
		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.Main);

			_view = FindViewById<RelativeLayout>(Resource.Id.findTheMonkeyView);
			_text = FindViewById<TextView>(Resource.Id.monkeyStatusLabel);

			_iBeaconManager.Bind(this);

			_monitorNotifier.EnterRegionComplete += EnteredRegion;
			_monitorNotifier.ExitRegionComplete += ExitedRegion;

			_rangeNotifier.DidRangeBeaconsInRegionComplete += RangingBeaconsInRegion;
		}

		void EnteredRegion(object sender, MonitorEventArgs e)
		{
		}

		void ExitedRegion(object sender, MonitorEventArgs e)
		{
		}

		void RangingBeaconsInRegion(object sender, RangeEventArgs e)
		{
			if (e.Beacons.Count > 0)
			{
				var beacon = e.Beacons.FirstOrDefault();
				var message = string.Empty;

				switch((ProximityType)beacon.Proximity)
				{
					case ProximityType.Immediate:
						UpdateDisplay("You found the monkey!", Color.Green);
						break;
					case ProximityType.Near:
						UpdateDisplay("You're getting warmer", Color.Yellow);
						break;
					case ProximityType.Far:
						UpdateDisplay("You're freezing cold", Color.Blue);
						break;
					case ProximityType.Unknown:
						UpdateDisplay("I'm not sure how close you are to the monkey", Color.Red);
						break;
				}

				_previousProximity = beacon.Proximity;
			}
		}

		#region IBeaconConsumer impl
		public void OnIBeaconServiceConnect()
		{
			_iBeaconManager.SetMonitorNotifier(_monitorNotifier);
			_iBeaconManager.SetRangeNotifier(_rangeNotifier);

			_iBeaconManager.StartMonitoringBeaconsInRegion(_monitoringRegion);
			_iBeaconManager.StartRangingBeaconsInRegion(_rangingRegion);
		}
		#endregion

		private void UpdateDisplay(string message, Color color)
		{
			RunOnUiThread(() =>
			{
				_text.Text = message;
				_view.SetBackgroundColor(color);
			});
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			_monitorNotifier.EnterRegionComplete -= EnteredRegion;
			_monitorNotifier.ExitRegionComplete -= ExitedRegion;

			_rangeNotifier.DidRangeBeaconsInRegionComplete -= RangingBeaconsInRegion;

			_iBeaconManager.StopMonitoringBeaconsInRegion(_monitoringRegion);
			_iBeaconManager.StopRangingBeaconsInRegion(_rangingRegion);
			_iBeaconManager.UnBind(this);
		}
	}
}