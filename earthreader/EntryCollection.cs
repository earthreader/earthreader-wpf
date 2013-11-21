using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
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

		private void EntryButton_KeyDownBlock(object sender, System.Windows.Input.KeyEventArgs e) { e.Handled = true; }
		private void EntryButton_Click(object sender, RoutedEventArgs e) {
			EntryItem item = (EntryItem)(sender as Button).Tag;
			RefreshFocusEntry(item);

			//MessageBox.Show(string.Format("{0} - {1}\n{2} - {3}", item.ID, item.Title, item.Feed, FeedDictionary[item.Feed].Title));
		}

		private void RefreshFocusEntry(EntryItem item) {
			Grid gridbase = item.EntryBaseGrid;
			Button entrybutton = (Button)gridbase.Children[0];

			if (!ReadDictionary.ContainsKey(item.ID)) {
				ReadDictionary.Add(item.ID, true);
				Grid gridContent = (Grid)entrybutton.Content;

				(gridContent.Children[1] as TextBlock).FontWeight = FontWeights.Normal;
			}

			if (LastSelectedGrid != null) {
				if (LastSelectedGrid.Children.Count > 2) {
					LastSelectedGrid.Children.RemoveAt(2);
					((Button)LastSelectedGrid.Children[0]).Visibility = Visibility.Visible;
				}
			}

			entrybutton.Visibility = Visibility.Collapsed;
			StackPanel stack = MakeFocusedEntryItem(item);
			gridbase.Children.Add(stack);
			LastSelectedGrid = gridbase;
			LastSelectedEntryID = item.ID;
			buttonAdd.Focus();
		}

		
		private Grid MakeEntryButton(EntryItem item, bool isFeed) {
			Grid gridBase = new Grid() {
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
			};
			Button button = new Button() {
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
			};

			if (!isFeed) {
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150), });
			} else {
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0), });
			}
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150), });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto), });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star), });


			TextBlock txtTime = new TextBlock() {
				Width = 120,
				Text = item.Time.ToString(), Margin = new Thickness(10, 0, 10, 0), HorizontalAlignment = HorizontalAlignment.Left,
				FontSize = 13.33,
				VerticalAlignment = VerticalAlignment.Center,
			};
			grid.Children.Add(txtTime);
			Grid.SetColumn(txtTime, 1);

			TextBlock txtTitle = new TextBlock() {
				Text = item.Title, Margin = new Thickness(10, 0, 10, 0), HorizontalAlignment = HorizontalAlignment.Left,
				FontSize = 16,
				VerticalAlignment = VerticalAlignment.Center,
			};
			if (!ReadDictionary.ContainsKey(item.ID)) { txtTitle.FontWeight = FontWeights.Bold; }
			grid.Children.Add(txtTitle);
			Grid.SetColumn(txtTitle, 2);

			if (!isFeed) {
				TextBlock txtCategory = new TextBlock() {
					Text = FeedDictionary[item.Feed].Title, Margin = new Thickness(20, 0, 15, 0),
					TextTrimming = TextTrimming.CharacterEllipsis,
					HorizontalAlignment = HorizontalAlignment.Left,
					FontSize = 16,
					VerticalAlignment = VerticalAlignment.Center,
				};
				grid.Children.Add(txtCategory);
				Grid.SetColumn(txtCategory, 0);
			}

			TextBlock txtSummary = new TextBlock() {
				Text = item.Summary, Margin = new Thickness(10, 0, 10, 0), HorizontalAlignment = HorizontalAlignment.Left,
				FontSize = 16,
				Foreground = Brushes.DarkGray,
				VerticalAlignment = VerticalAlignment.Center,
			};
			grid.Children.Add(txtSummary);
			Grid.SetColumn(txtSummary, 3);

			Rectangle rect = new Rectangle() {
				Height = 1, Fill = Brushes.LightGray,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};

			button.Content = grid;
			gridBase.Children.Add(button);
			gridBase.Children.Add(rect);

			button.Click += EntryButton_Click;
			button.PreviewKeyDown += EntryButton_KeyDownBlock;

			return gridBase;
		}

		private StackPanel MakeFocusedEntryItem(EntryItem item) {
			StackPanel stack = new StackPanel();

			Button buttonClose = new Button() { Background = Brushes.Transparent, Height = 30, Cursor = Cursors.Hand, HorizontalContentAlignment = HorizontalAlignment.Stretch };
			Grid gridClose = new Grid() { Height = 30, Background = Brushes.Transparent };
			Image imageClose = new Image() {
				Source = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/btnClose_Entry.png")),
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Right,
				Margin = new Thickness(3, 3, 13, 3),
				Width = 15, Height = 15,
			};
			gridClose.Children.Add(imageClose);
			buttonClose.Content = gridClose;
			buttonClose.Click += (o, e) => {
				LastSelectedGrid.Children.RemoveAt(2);
				((Button)LastSelectedGrid.Children[0]).Visibility = Visibility.Visible;
			};

			Button buttonTitle = new Button() { Background = Brushes.Transparent, Cursor = Cursors.Hand, HorizontalContentAlignment = HorizontalAlignment.Stretch, };
			TextBlock txtTitle = new TextBlock() {
				Text = item.Title, FontSize = 25, Margin = new Thickness(30, 0, 30, 0),
				//Background = Brushes.Yellow, 
				Tag = item.URL,
			};
			buttonTitle.Content = txtTitle;
			buttonTitle.Click += (o, e) => {
				Button button = (o as Button);
				TextBlock txt = (TextBlock)button.Content;
				string url = (string)txt.Tag;
				//MessageBox.Show(txt.Text + "\n\n" + txt.Text.Replace(Environment.NewLine,""));
				Process.Start(url);
			};

			WebBrowser wb = new WebBrowser() {
				Margin = new Thickness(30, 20, 30, 30), VerticalAlignment = System.Windows.VerticalAlignment.Stretch, Focusable = false,
			};
			wb.LoadCompleted += wb_LoadCompleted;
			wb.NavigateToString(@"<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'></head>" + item.Content);

			TextBlock txtContent = new TextBlock() {
				Text = HtmlRemoval.StripTagsCharArray(item.Content), Margin = new Thickness(30, 20, 30, 30),
				FontSize = 16,
				TextWrapping = TextWrapping.Wrap,
				//Background = Brushes.Tomato,
			};


			stack.Children.Add(buttonClose);
			stack.Children.Add(buttonTitle);
			stack.Children.Add(txtContent);
			//stack.Children.Add(wb);

			return stack;
		}

		void wb_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e) {


			string script = "document.body.style.overflow ='hidden'";
			WebBrowser wb = (WebBrowser)sender;
			mshtml.HTMLDocument htmlDoc = wb.Document as mshtml.HTMLDocument;
			if (htmlDoc != null && htmlDoc.body != null) {
				mshtml.IHTMLElement2 body = (mshtml.IHTMLElement2)htmlDoc.body;
				wb.Height = body.scrollHeight;
			}

			wb.InvokeScript("execScript", new Object[] { script, "JavaScript" });
		}
	}

	public class EntryItem {
		public string Title, Summary, Content, URL;
		public int ID, Feed;
		public DateTime Time;
		public bool isRead;
		public Grid EntryBaseGrid;
	}
}
