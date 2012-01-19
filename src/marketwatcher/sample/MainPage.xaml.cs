using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using marketwatcher;

namespace sample
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ProgressIndicator progressIndicator;
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            SystemTray.SetIsVisible(this, true);
            SystemTray.SetOpacity(this, 0.5);

            progressIndicator = new ProgressIndicator { IsVisible = false, IsIndeterminate = true, Text = "loading reviews" };
            SystemTray.SetProgressIndicator(this, progressIndicator);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var fetcher = new Fetcher();

            var reviews = new ObservableCollection<Review>();

            Items.ItemsSource = reviews;

            progressIndicator.IsVisible = true;

            fetcher.FetchReviewsForApp(this.Appid.Text)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(x =>
                               {
                                   foreach (var review in x)
                                   {
                                       reviews.Add(review);
                                   }
                               },
                               ex => Debug.WriteLine("error"),
                               () => progressIndicator.IsVisible = false);

        }
    }
}