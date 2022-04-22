using System;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Essentials;
using System.Net.Http;
using Newtonsoft.Json;
using Skate_Tracker.JsonTransferObjects;
using System.Threading;

namespace Skate_Tracker
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            //Base variables
            bool isPaused = false;
            Position MostRecentPosition = new Position(0, 0);
            int currentJourneyID = 4;

            //Client for posts to api
            HttpClient client = new HttpClient();

            //Prevent the screen from entering sleep mode and etc
            DeviceDisplay.KeepScreenOn = true;

            //Create main start and stop button for tracking
            Button StartJourney = new Button()
            {
                Text = "Start",
                TextColor = Color.Snow,
                BackgroundColor = Color.FromHex("#28eb35"),
                CornerRadius = 5,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End
            };

            Button StopJourney = new Button()
            {
                Text = "Stop",
                TextColor = Color.Snow,
                BackgroundColor = Color.OrangeRed,
                CornerRadius = 5,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End
            };

            Button PauseUnpauseJourney = new Button()
            {
                Text = "Pauze",
                TextColor = Color.Snow,
                BackgroundColor = Color.FromHex("#ff7700"),
                CornerRadius = 5,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
            };

            //Grid for buttons
            Grid grid = new Grid()
            {
                IsVisible = false,
                RowDefinitions =
                {
                    new RowDefinition {Height = new GridLength(2, GridUnitType.Auto) },
                    new RowDefinition()
                }
            };
            grid.Children.Add(StopJourney, 1, 0);
            grid.Children.Add(PauseUnpauseJourney, 0, 0);

            //Create map 
            Xamarin.Forms.Maps.Map map = new Xamarin.Forms.Maps.Map()
            {
                IsShowingUser = true,
            };
            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(0, 0), Distance.FromKilometers(40000)));

            //Polyline settings, creates polyline and draws it on map
            Polyline route = new Polyline()
            {
                StrokeWidth = 15,
                Geopath = { }
            };
            map.MapElements.Add(route);

            //Initialize timer + timer settings
            System.Timers.Timer timer = new System.Timers.Timer(5000);
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;

            //Events for clicks on button
            StartJourney.Clicked += StartTracking;
            StopJourney.Clicked += StopTracking;
            PauseUnpauseJourney.Clicked += PauseOrUnpause;

            //Start and stops all tracking functions and updates UI
            async void StartTracking(object sender, EventArgs args)
            {
                PostJourneyAndGetID();
                timer.Start();
                StartJourney.IsVisible = false;
                grid.IsVisible = true;

            }

            async void StopTracking(object sender, EventArgs args)
            {
                timer.Stop();
                isPaused = false;
                PauseUnpauseJourney.Text = "Pause";
                grid.IsVisible = false;
                StartJourney.BackgroundColor = Color.FromHex("#28eb35");
                StartJourney.IsVisible = true;
            }

            async void PauseOrUnpause(object sender, EventArgs args)
            {
                if (!isPaused)
                {
                    timer.Stop();
                    PauseUnpauseJourney.Text = "Unpause";
                    isPaused = true;
                }
                else
                {
                    timer.Start();
                    PauseUnpauseJourney.Text = "Pause";
                    isPaused = false;
                }
            }

            async void Timer_Elapsed(object sender, EventArgs e)
            {
                await GetCurrentLocation();
            }


            void UpdateMap(Position position, double speed)
            {
                Color color;
                if (speed < 2.8f)
                {
                    color = Color.Green;
                }
                else if (speed >= 2.8f && speed < 5.54f)
                {
                    color = Color.LightGreen;
                }
                else if (speed >= 5.54f && speed < 8.33f)
                {
                    color = Color.Yellow;
                }
                else if (speed >= 8.33f && speed < 11.11f)
                {
                    color = Color.Orange;
                }
                else
                {
                    color = Color.Red;
                }

                if (MostRecentPosition != new Position(0, 0))
                {
                    map.MapElements.Add(new Polyline()
                    {
                        Geopath = { MostRecentPosition, position },
                        StrokeWidth = 15,
                        StrokeColor = color
                    });
                    MostRecentPosition = position;
                }
                else
                {
                    MostRecentPosition = position;
                }
                map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMeters(100)));
            }

            //Gets and updates users current location
            async Task GetCurrentLocation()
            {
                try
                {
                    var location = await Geolocation.GetLocationAsync();
                    if (location.Speed != null)
                    {
                        Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}, Speed: {location.Speed}, Time: {DateTime.Now}");
                        Device.BeginInvokeOnMainThread(() => UpdateMap(new Position(location.Latitude, location.Longitude), (double)location.Speed));
                        await PostPosition(location);
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

            async Task PostPosition(Location location)
            {
                Uri uri = new Uri("https://i461941core.venus.fhict.nl/api/Skate/SendPosition");

                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    string json = JsonConvert.SerializeObject(new PositionDataObject() { JourneyID = currentJourneyID, Latitude = location.Latitude, Longitude = location.Longitude, Speed = (float)location.Speed, TimeStamp = DateTime.Now });
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(uri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Succesfully posted location data to api");
                    }
                    else
                    {
                        Console.WriteLine(response.StatusCode.ToString(), response.Content);
                    }
                }
            }

            async Task PostJourneyAndGetID()
            {
                string name = await DisplayPromptAsync("Enter journey name", "Leave empty if u don't want to name your journey");
                Uri uri = new Uri("https://i461941core.venus.fhict.nl/api/Skate/AddJourney/");
                Console.WriteLine(name);
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    string json = JsonConvert.SerializeObject(new JourneyDataObject() { Name = name, StartTime = DateTime.Now });
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(uri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        currentJourneyID = Convert.ToInt32(response.Content.ReadAsStringAsync().Result);
                        Console.WriteLine($"Succesfully created journey ({name}) with id: {currentJourneyID}");
                    }
                    else
                    {
                        Console.WriteLine(response.StatusCode.ToString(), response.Content);
                    }
                }
            }

            //Add content to page
            Content = new StackLayout()
            {
                Children =
                {
                map,
                    StartJourney,
                    grid
                }
            };

        }
    }
}
