using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Assignment2
{
    
    public partial class MainWindow : Window
    {
        private XDocument document;
        private string url;
        private Dictionary<string, string> feedUrls = new Dictionary<string, string>();
        private Dictionary<string, DateTime> titleDates = new Dictionary<string, DateTime>();
        private Thickness spacing = new Thickness(5);
        private HttpClient http = new HttpClient();
        private string feedName;
        // We will need these as instance variables to access in event handlers.
        private TextBox addFeedTextBox;
        private Button addFeedButton;
        private ComboBox selectFeedComboBox;
        private Button loadArticlesButton;
        private StackPanel articlePanel;

        private string allFeeds = "All Feeds";

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        private void Start()
        {
            // Window options
            Title = "Feed Reader";
            Width = 800;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Scrolling
            var root = new ScrollViewer();
            root.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Content = root;

            // Main grid
            var grid = new Grid();
            root.Content = grid;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var addFeedLabel = new Label
            {
                Content = "Feed URL:",
                Margin = spacing
            };
            grid.Children.Add(addFeedLabel);

            addFeedTextBox = new TextBox
            {
                Margin = spacing,
                Padding = spacing
            };
            grid.Children.Add(addFeedTextBox);
            Grid.SetColumn(addFeedTextBox, 1);

            addFeedButton = new Button
            {
                Content = "Add Feed",
                Margin = spacing,
                Padding = spacing
            };
            grid.Children.Add(addFeedButton);
            Grid.SetColumn(addFeedButton, 2);
            addFeedButton.Click += AddFeedButton_Click;

            var selectFeedLabel = new Label
            {
                Content = "Select Feed:",
                Margin = spacing
            };
            grid.Children.Add(selectFeedLabel);
            Grid.SetRow(selectFeedLabel, 1);

            selectFeedComboBox = new ComboBox
            {
                Margin = spacing,
                Padding = spacing,
                IsEditable = false
            };
            grid.Children.Add(selectFeedComboBox);
            Grid.SetRow(selectFeedComboBox, 1);
            Grid.SetColumn(selectFeedComboBox, 1);
            selectFeedComboBox.Items.Add(allFeeds);
            selectFeedComboBox.SelectedItem = allFeeds;

            loadArticlesButton = new Button
            {
                Content = "Load Articles",
                Margin = spacing,
                Padding = spacing
            };
            grid.Children.Add(loadArticlesButton);
            Grid.SetRow(loadArticlesButton, 1);
            Grid.SetColumn(loadArticlesButton, 2);
            loadArticlesButton.Click += LoadArticlesButton_Click;

            articlePanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = spacing
            };
            grid.Children.Add(articlePanel);
            Grid.SetRow(articlePanel, 2);
            Grid.SetColumnSpan(articlePanel, 3);

            // These are just placeholders.
            // Replace them with your own code that shows actual articles.
           
        }

        private async void LoadArticlesButton_Click(object sender, RoutedEventArgs e)
        {
            feedName = selectFeedComboBox.SelectedItem.ToString();
            articlePanel.Children.Clear();
            //TODO lägg till all feeds
            if (selectFeedComboBox.SelectedItem.ToString() == "All Feeds")
            {
                titleDates.Clear();
                foreach(var feed in feedUrls)
                {
                    url = feed.Value;
                    document = await LoadDocumentAsync(url);
                    feedName = document.Descendants("title").First().Value;
                    string[] titles = document.Descendants("title").Skip(2).Select(t => t.Value).ToArray();
                    string[] dates = document.Descendants("pubDate").Select(t => t.Value).ToArray();
                    for(int i = 0; i < 5; i++)
                    {
                        titleDates.Add(titles[i], DateTime.ParseExact(dates[i].Substring(0, 25), "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture));

                    }

                }
                titleDates.OrderBy(d => d.Value);
                Print(feedUrls.Count * 5);

            }
            else
            {
                url = feedUrls[selectFeedComboBox.SelectedItem.ToString()];
                document = await LoadDocumentAsync(url);
                titleDates.Clear();
                // Get all titles as an array of strings.
                string[] allTitles = document.Descendants("title").Skip(2).Select(t => t.Value).ToArray();
                string[] allDates = document.Descendants("pubDate").Select(t => t.Value).ToArray();
                for (int i = 0; i < 5; i++)
                {
                    titleDates.Add(allTitles[i], DateTime.ParseExact(allDates[i].Substring(0, 25), "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture));
                    
                }
                Print(5);
            }
        }

        private async void AddFeedButton_Click(object sender, RoutedEventArgs e)
        {
            addFeedButton.IsEnabled = false;

            url = addFeedTextBox.Text;

            document = await LoadDocumentAsync(url);

            // Get the title of the xmlFile as a string.
            feedName = document.Descendants("title").First().Value;

            if (feedUrls.ContainsKey(feedName))
            {
                MessageBox.Show("Error");
                addFeedButton.IsEnabled = true;
                return;
            }
            else
            {
                selectFeedComboBox.SelectedItem = feedName;
                selectFeedComboBox.Items.Add(feedName);

                feedUrls.Add(feedName, url);
            }

            addFeedButton.IsEnabled = true;
        }

        private void Print(int count)
        {


            for (int i = 0; i < count; i++)
            {
                var articleBox = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = spacing
                };
                articlePanel.Children.Add(articleBox);

                var articleTitle = new TextBlock
                {
                    Text = Convert.ToString(titleDates.ElementAt(i).Value+ "  -  " + titleDates.ElementAt(i).Key),
                    FontWeight = FontWeights.Bold,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                articleBox.Children.Add(articleTitle);

                var articleWebsite = new TextBlock
                {
                    Text = feedName
                };
                articleBox.Children.Add(articleWebsite);
            }
        }

        private async Task<XDocument> LoadDocumentAsync(string url)
        {
            // This is just to simulate a slow/large data transfer and make testing easier.
            // Remove it if you want to.
            //await Task.Delay(5000);
            var response = await http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var feed = XDocument.Load(stream);
            return feed;
        }
    }
}
