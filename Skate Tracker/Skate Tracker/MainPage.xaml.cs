using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Essentials;
using System.Threading;
using System.Timers;

namespace Skate_Tracker
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            bool isTracking = false;

            //Prevent the screen from entering sleep mode and etc
            DeviceDisplay.KeepScreenOn = true;

            //Create main start and stop button for tracking
            Button StartStopJourney = new Button()
            {
                Text = "Start",
                TextColor = Color.Snow,
                BackgroundColor = Color.FromHex("#ff7700"),
                CornerRadius = 5,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End
            };

            //Create map 
            Xamarin.Forms.Maps.Map map = new Xamarin.Forms.Maps.Map()
            {
                IsShowingUser = true,
            };
            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(0,0), Distance.FromKilometers(40000)));

            //Polyline settings, creates polyline and draws it on map
            Polyline route = new Polyline()
            {
                StrokeColor = Color.FromHex("#ff7700"),
                StrokeWidth = 15,
                Geopath = {}
            };
            map.MapElements.Add(route);

            //Initialize timer + timer settings
            System.Timers.Timer timer = new System.Timers.Timer(5000);
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;

            //On main button for tracking clicked
            StartStopJourney.Clicked += StartStopTracking;

            //Start and stops all tracking functions and updates UI
            async void StartStopTracking(object sender, EventArgs args)
            {
                if (!isTracking)
                {
                    isTracking = true;
                    StartStopJourney.Text = "Stop";
                    timer.Start();
                }
                else
                {
                    isTracking = false;
                    StartStopJourney.Text = "Start";
                    timer.Stop();
                }
            }

            void Timer_Elapsed(object sender, EventArgs e)
            {
                GetCurrentLocation(); 
            }

            Position oldPosition;
            void UpdateMap(Position position, double speed)
            {
                route.Geopath.Add(position);
                map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMeters(100)));
            }

            //Gets and updates users current location
            async Task GetCurrentLocation()
            {
                try
                {
                    var location = await Geolocation.GetLocationAsync();
                    if (location != null)
                    {
                        Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}, Speed: {location.Speed}, Time: {DateTime.Now}");
                        Device.BeginInvokeOnMainThread(() => UpdateMap(new Position(location.Latitude, location.Longitude), (double)location.Speed));
                    }
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    Console.Write(fnsEx);
                }
                catch (FeatureNotEnabledException fneEx)
                {
                    Console.Write(fneEx);
                }
                catch (PermissionException pEx)
                {
                    Console.Write(pEx);
                }
                catch (Exception Ex)
                {
                    Console.Write(Ex);
                }
            }

            async void PostPosition()
            {

            }
            //Add content to page
            Content = new StackLayout()
            {
                Children =
                {
                    map,
                    StartStopJourney
                }
            };
        }
    }
}
