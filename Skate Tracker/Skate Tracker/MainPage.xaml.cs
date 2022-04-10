using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Skate_Tracker
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            Geocoder geoCoder = new Geocoder();
            Button StartStopJourney = new Button()
            {
                Text = "Start",
                TextColor = Color.Snow,
                BackgroundColor = Color.FromHex("#ff7700"),
                CornerRadius = 5,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End
            };

            Map map = new Map()
            {
                IsShowingUser = true
            };
        
            Position currentPosition = new Position(51.565429, 5.375261);

            map.MoveToRegion(MapSpan.FromCenterAndRadius(currentPosition, Distance.FromMeters(100)));
            Polyline route = new Polyline()
            {
                StrokeColor = Color.FromHex("#ff7700"),
                StrokeWidth = 5,
                Geopath =
                {
                    currentPosition,
                    new Position(52, 5.30),
                    new Position(53, 6)
                }
            };
            map.MapElements.Add(route);

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
