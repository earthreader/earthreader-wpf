using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace earthreader {
	public partial class MainWindow {
		int LastEntryIndex = -1;
		private void RefreshEntry(int nID) {
			NowFeedIndex = nID;
			EntryStack.Children.Clear();

			Queue<int> Q = new Queue<int>();
			List<int> L = new List<int>();
			Q.Enqueue(nID);

			int FeedID;
			for (; Q.Count > 0; ) {
				FeedID = Q.Dequeue();
				if (FeedDictionary[FeedID].IsFeed) {
					L.Add(FeedID);
				} else {
					foreach (int Child in FeedDictionary[FeedID].Children) {
						Q.Enqueue(Child);
					}
				}
			}

			if (L.Count == 0) { return; }

			List<EntryItem> EntryCollection = new List<EntryItem>();
			foreach (int ID in L) {
				foreach (EntryItem EItem in FeedDictionary[ID].Contents) {
					EntryCollection.Add(EItem);
				}
			}
			EntryCollection.Sort(new FeedParser.mysortByValue());
			EntryCollection.Reverse();

			foreach (EntryItem EItem in EntryCollection) {
				TextBlock txt = new TextBlock() {
					Text = EItem.Time.ToString() + " : " + EItem.Feed + " : " + EItem.Title,
				};
				Button button = MakeEntryButton(EItem, FeedDictionary[nID].IsFeed);
				EntryStack.Children.Add(button);
			}

			ScrollEntry.ScrollToTop();
			//ScrollEntry.BeginAnimation(AniScrollViewer.CurrentVerticalOffsetProperty, new DoubleAnimation(scrollNow.VerticalOffset, 0, TimeSpan.FromMilliseconds(200)));
		}

		private Button MakeEntryButton(EntryItem item, bool isFeed) {
			Button button = new Button() {
				//Background = Brushes.Orange, 
				Background = Brushes.Transparent,
				Height = 40, 
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};
			button.PreviewKeyDown += (o, e) => e.Handled = true;

			Grid grid = new Grid() {
				Height = 40, 
				HorizontalAlignment = HorizontalAlignment.Stretch,
				//Background = Brushes.Pink
			};

			if (!isFeed) {
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150), });
				TextBlock txtCategory = new TextBlock() {
					Text = item.Feed, Margin = new Thickness(20, 0, 15, 0),
					TextTrimming = TextTrimming.CharacterEllipsis,
					HorizontalAlignment = HorizontalAlignment.Left,
					//Background = Brushes.Red,
					FontSize = 16,
					VerticalAlignment = VerticalAlignment.Center,
				};
				grid.Children.Add(txtCategory);
				Grid.SetColumn(txtCategory, 0);
			} else {
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0), });
			}
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150), });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150, GridUnitType.Star), });


			TextBlock txtTime = new TextBlock() {
				Width = 120,
				Text = item.Time.ToString(), Margin = new Thickness(10, 0, 10, 0), HorizontalAlignment = HorizontalAlignment.Left,
				//Background = Brushes.Green,
				FontSize = 13.33,
				VerticalAlignment = VerticalAlignment.Center,
			};
			grid.Children.Add(txtTime);
			Grid.SetColumn(txtTime, 1);

			TextBlock txtSummary = new TextBlock() {
				Text = item.Summary, Margin = new Thickness(10, 0, 10, 0), HorizontalAlignment = HorizontalAlignment.Left,
				FontSize = 16,
				//Background = Brushes.Blue,
				VerticalAlignment = VerticalAlignment.Center,
			};
			grid.Children.Add(txtSummary);
			Grid.SetColumn(txtSummary, 2);

			Rectangle rect = new Rectangle() {
				Height = 1, Fill = Brushes.LightGray,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};
			grid.Children.Add(rect);
			Grid.SetColumnSpan(rect, 3);

			button.Content = grid;

			return button;
		}
	}

	public struct EntryItem {
		public string Title, Summary, Content, URL, Feed;
		public int ID;
		public DateTime Time;
	}
}
