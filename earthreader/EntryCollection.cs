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
		List<EntryItem> EntryCollection = new List<EntryItem>();
		Grid LastSelectedGrid = null;
		int LastSelectedEntryID = -1;

		private void RefreshEntry(int nID) {
			NowFeedIndex = nID;
			LastSelectedGrid = null;
			LastSelectedEntryID = -1;
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

			EntryCollection = new List<EntryItem>();
			foreach (int ID in L) {
				foreach (EntryItem EItem in FeedDictionary[ID].Contents) {
					EntryCollection.Add(EItem);
				}
			}
			EntryCollection.Sort(new FeedParser.mysortByValue());
			EntryCollection.Reverse();

			for (int i = 0; i < EntryCollection.Count; i++) {
				EntryItem EItem = EntryCollection[i];
				/*
				TextBlock txt = new TextBlock() {
					Text = EItem.Time.ToString() + " : " + EItem.Feed + " : " + EItem.Title,
				};
				 */ 
				Grid gridbase = MakeEntryButton(EItem, FeedDictionary[nID].IsFeed);
				EItem.EntryBaseGrid = gridbase;
				EntryStack.Children.Add(gridbase);
			}

			ScrollEntry.ScrollToTop();
		}

		private void EntryButton_KeyDownBlock(object sender, System.Windows.Input.KeyEventArgs e) {
			e.Handled = true;
		}

		private void EntryButton_Click(object sender, RoutedEventArgs e) {
			EntryItem item = (EntryItem)(sender as Button).Tag;
			RefreshFocusEntry(item);

			//MessageBox.Show(string.Format("{0} - {1}\n{2} - {3}", item.ID, item.Title, item.Feed, FeedDictionary[item.Feed].Title));
		}

		private void RefreshFocusEntry(EntryItem item) {
			Grid gridbase = item.EntryBaseGrid;
			Button entrybutton = (Button)gridbase.Children[0];

			if (LastSelectedGrid != null) {
				if (LastSelectedGrid.Children.Count > 2) {
					LastSelectedGrid.Children.RemoveAt(2);
					((Button)LastSelectedGrid.Children[0]).Visibility = Visibility.Visible;
				}
			}

			entrybutton.Visibility = Visibility.Collapsed;
			Rectangle rect = new Rectangle() { Height = 500, Fill = Brushes.Transparent };
			gridbase.Children.Add(rect);
			LastSelectedGrid = gridbase;
			LastSelectedEntryID = item.ID;
		}





		private Grid MakeEntryButton(EntryItem item, bool isFeed) {
			Grid gridBase = new Grid() {
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
			};
			Button button = new Button() {
				//Background = Brushes.Orange, 
				Background = Brushes.Transparent,
				Height = 40,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Tag = item,
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
					Text = FeedDictionary[item.Feed].Title, Margin = new Thickness(20, 0, 15, 0),
					TextTrimming = TextTrimming.CharacterEllipsis,
					HorizontalAlignment = HorizontalAlignment.Left,
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
				Height = 1, Fill = Brushes.Black,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};
			//Grid.SetColumnSpan(rect, 3);

			button.Content = grid;
			gridBase.Children.Add(button);
			gridBase.Children.Add(rect);

			button.Click += EntryButton_Click;
			button.PreviewKeyDown += EntryButton_KeyDownBlock;

			return gridBase;
		}
	}

	public class EntryItem {
		public string Title, Summary, Content, URL;
		public int ID, Feed;
		public DateTime Time;
		public Grid EntryBaseGrid;
	}
}
