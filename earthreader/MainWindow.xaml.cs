﻿using System;
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
		Dictionary<int, bool> ReadDictionary = new Dictionary<int, bool>();
		Dictionary<int, bool> StarDictionary = new Dictionary<int, bool>();
		//Dictionary<int, EntryItem> dictEntry = new Dictionary<int, EntryItem>();
		int NowFeedIndex = -1;

		public MainWindow() { InitializeComponent(); }

		public void windowMain_Loaded(object sender, RoutedEventArgs e) {
			ButtonEvent();

			// Add root

			FeedDictionary.Add(0, new FeedItem() {
				ID = 0, Title = "all feeds", Count = 0, ParentID = 0,
				IsFeed = false, URL = "", Children = new List<int>(),
				Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconAll.png")),
			});

			// Test code (30 categories)

			for (int i = 0; i < 0; i++) {
				FeedDictionary.Add(nCount, new FeedItem() {
					ID = nCount, Title = "Folder " + i.ToString("00"), Count = 0, ParentID = 0,
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
				if (isDialogMode) { return; }
				bool isKeyHandled = true;
				int EntryIndex = -1;

				switch (ex.Key) {
					case Key.Up:
						ScrollEntry.ScrollToVerticalOffset(ScrollEntry.VerticalOffset - 40);
						break;
					case Key.Down:
						ScrollEntry.ScrollToVerticalOffset(ScrollEntry.VerticalOffset + 40);
						break;
					case Key.J:
						//textTemp.Text += LastSelectedEntryID.ToString();
						EntryIndex = GetPositionByIndex(LastSelectedEntryID);

						if (EntryIndex >= EntryCollection.Count - 1) { return; }
						if (EntryIndex < 0 && EntryCollection.Count == 0) { return; }

						RefreshFocusEntry(EntryCollection[EntryIndex + 1]);
						ScrollEntry.ScrollToVerticalOffset(EntryIndex * 40 + 10);
						break;
					case Key.K:
						EntryIndex = GetPositionByIndex(LastSelectedEntryID);

						if (EntryIndex <= 0) { return; }

						RefreshFocusEntry(EntryCollection[EntryIndex - 1]);
						ScrollEntry.ScrollToVerticalOffset((EntryIndex - 2) * 40 + 10);
						break;
					case Key.O: 
						EntryIndex = GetPositionByIndex(LastSelectedEntryID);
						textTemp.Text = EntryIndex.ToString();

						if (EntryIndex < 0) { return; }

						Process.Start(EntryCollection[EntryIndex].URL);
						break;
					default:
						isKeyHandled = false;
						break;
				}	
				e.Handled = isKeyHandled;
				//textTemp.Text = isKeyHandled.ToString();
			};
		}

		private int GetPositionByIndex(int EntryID) {
			if (EntryCollection.Count == 0) { return -1; }

			int EntryIndex = EntryCollection.FindIndex(x => x.ID == EntryID);
			return EntryIndex;
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


			RefreshEntry(NowFeedIndex);
		}

		private void buttonEntryOption_Click(object sender, RoutedEventArgs e) {
			Point targetPoint = buttonEntryOption.PointToScreen(new Point(0, 0));

			contextMenu.PlacementRectangle = new Rect(targetPoint, new Size(buttonEntryOption.Width, buttonEntryOption.Height));
			contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;

			contextMenu.HorizontalOffset = 10;
			contextMenu.VerticalOffset = 50;

			contextMenu.IsOpen = true;
		}

		bool isHelpVisible = false;
		private void gridHelpCover_MouseDown(object sender, MouseButtonEventArgs e) { buttonEntryHelp.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }
		private void buttonEntryHelp_Click(object sender, RoutedEventArgs e) {
			isHelpVisible = !isHelpVisible;

			gridHelpCover.Visibility = isHelpVisible ? Visibility.Visible : Visibility.Collapsed;
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
