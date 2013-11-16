using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace earthreader {
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window {

		bool isScrollVisible = false, isMouseDown, isAddWindowMode, isDialogMode;
		int nFeedlistWidth = 250, nCount = 1;
		Dictionary<int, FeedItem> FeedDictionary = new Dictionary<int, FeedItem>();
		//Dictionary<int, EntryItem> dictEntry = new Dictionary<int, EntryItem>();
		int NowFeedIndex = -1;

		public MainWindow() { InitializeComponent(); }

		public void windowMain_Loaded(object sender, RoutedEventArgs e) {
			ButtonEvent();

			// Add root

			FeedDictionary.Add(0, new FeedItem() {
				ID = 0, Caption = "all feeds", Count = 1, ParentID = 0,
				IsFeed = false, URL = "", Children = new List<int>(),
				Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconAll.png")),
			});

			// Test code (30 categories)

			for (int i = 0; i < 5; i++) {
				FeedDictionary.Add(nCount, new FeedItem() {
					ID = nCount, Caption = "Folder " + i.ToString("00"), Count = 0, ParentID = 0,
					IsFeed = false, URL = "", Children = new List<int>(),
					Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconCategory.png")),
				});
				FeedDictionary[0].Children.Add(nCount);
				nCount++;
			}

			MakeNewFeedTestData("tokyotosho.info/rss.php", 0);
			MakeNewFeedTestData("nyaa.eu/?page=rss", 0);

			RefreshFeedList(0, false);

			this.PreviewKeyDown += (o, ex) => {
				if (isAddWindowMode) { return; }
				bool isKeyHandled = true;

				switch (ex.Key) {
					case Key.Up:
						break;
					case Key.Down:
						break;
					case Key.J:
						break;
					case Key.K:

						break;
					default:
						isKeyHandled = false;
						break;
				}
				e.Handled = isKeyHandled;
				textTemp.Text = isKeyHandled.ToString();
			};
		}

		private async void MakeNewFeedTestData(string URL, int Category) {
			Task<List<FeedCandidateItem>> httpTask = AutoDiscovery.GetCandidateFeeds(URL);
			List<FeedCandidateItem> listCd = await httpTask;

			if (listCd.Count == 0) { return; }

			MakeNewFeed(listCd[0], Category);
		}

		// Controlbox event

		private void MainWindow_StateChanged(object sender, EventArgs e) {
			if (this.WindowState == WindowState.Normal) {
				buttonRestore.Visibility = Visibility.Collapsed;
				buttonMaximize.Visibility = Visibility.Visible;
				gridMain.Margin = new Thickness(0);
			} else {
				buttonRestore.Visibility = Visibility.Visible;
				buttonMaximize.Visibility = Visibility.Collapsed;
				gridMain.Margin = new Thickness(5);
			}
		}

		// Read mode select event

		string strReadmode = "all";
		private void buttonModeSelect_Click(object sender, RoutedEventArgs e) {
			strReadmode = (string)((Button)sender).Tag;
			int nMode = 0;

			switch (strReadmode) {
				case "all": nMode = 0; break;
				case "unread": nMode = 1; break;
				case "starred": nMode = 2; break;
			}

			imageModeAll.Visibility = nMode == 0 ? Visibility.Visible : Visibility.Collapsed;
			imageModeUnread.Visibility = nMode == 1 ? Visibility.Visible : Visibility.Collapsed;
			imageModeStarred.Visibility = nMode == 2 ? Visibility.Visible : Visibility.Collapsed;

			buttonModeAll.Visibility = nMode != 0 ? Visibility.Visible : Visibility.Collapsed;
			buttonModeUnread.Visibility = nMode != 1 ? Visibility.Visible : Visibility.Collapsed;
			buttonModeStarred.Visibility = nMode != 2 ? Visibility.Visible : Visibility.Collapsed;
		}

		private void buttonEntryOption_Click(object sender, RoutedEventArgs e) {
			Point targetPoint = buttonEntryOption.PointToScreen(new Point(0, 0));

			contextMenu.PlacementRectangle = new Rect(targetPoint, new Size(buttonEntryOption.Width, buttonEntryOption.Height));
			contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;

			contextMenu.HorizontalOffset = 10;
			contextMenu.VerticalOffset = 50;

			contextMenu.IsOpen = true;
		}

		/*
		private void RefreshEntryList(int nID) {
			if (!dictFeedItem[nID].IsFeed) { return; }

			nShowingEntryIndex = -1;
			NowFeedIndex = nID;

			dictEntry.Clear();
			foreach (EntryItem eItem in dictFeedItem[nID].Contents) {
				eItem.SummaryVisibility = Visibility.Visible;
				eItem.ContentVisibility = Visibility.Collapsed;
				eItem.ContentView = "";

				dictEntry.Add(eItem.Tag, eItem);
			}
			listEntry.DataContext = dictFeedItem[NowFeedIndex].Contents;

			if (dictFeedItem[NowFeedIndex].Contents.Count > 0) {
				listEntry.ScrollIntoView(listEntry.Items[0]);
			}
		}
		 */

		private void TextBlockEntryTitle_MouseDown(object sender, MouseButtonEventArgs e) {
			e.Handled = true;
			TextBlock txt = sender as TextBlock;
			try {
				Process.Start((string)txt.Tag);
			} catch { }
		}

		int nShowingEntryIndex = -1;
		private void ButtonEntryItem_Click(object sender, RoutedEventArgs e) {
			ExpandEntryItem((int)((Button)sender).Tag, false);
		}

		private void ExpandEntryItem(int nID, bool Focus) {
			/*
			if (nShowingEntryIndex >= 0) {
				dictEntry[nShowingEntryIndex].ContentView = "";
				dictEntry[nShowingEntryIndex].ContentVisibility = Visibility.Collapsed;
				dictEntry[nShowingEntryIndex].SummaryVisibility = Visibility.Visible;
			}

			if (!dictEntry.ContainsKey(nID)) {
				nShowingEntryIndex = -1;
				return;
			}

			nShowingEntryIndex = nID;
			dictEntry[nShowingEntryIndex].ContentVisibility = Visibility.Visible;
			dictEntry[nShowingEntryIndex].SummaryVisibility = Visibility.Collapsed;

			dictEntry[nShowingEntryIndex].ContentView = dictEntry[nShowingEntryIndex].Content;
			 */ 
		}

		private void ButtonEntryItemClose_Click(object sender, RoutedEventArgs e) {
			/*
			nShowingEntryIndex = -1;
			int nIdx = (int)((Button)sender).Tag;

			dictEntry[nIdx].ContentVisibility = Visibility.Collapsed;
			dictEntry[nIdx].SummaryVisibility = Visibility.Visible;
			 */ 
		}

		private void Button_PreviewMouseDown_1(object sender, MouseButtonEventArgs e) {
			/*
			nShowingEntryIndex = -1;
			int nIdx = (int)((Button)sender).Tag;

			dictEntry[nIdx].ContentVisibility = Visibility.Collapsed;
			dictEntry[nIdx].SummaryVisibility = Visibility.Visible;
			 */ 
		}
	}
}
