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
	public partial class MainWindow {
		private void ButtonEvent() {
			// Controlbox events

			buttonMinimize.Click += (o, e) => SystemCommands.MinimizeWindow(Window.GetWindow(this));
			buttonMaximize.Click += (o, e) => SystemCommands.MaximizeWindow(Window.GetWindow(this));
			buttonRestore.Click += (o, e) => SystemCommands.RestoreWindow(Window.GetWindow(this));
			buttonClose.Click += (o, e) => SystemCommands.CloseWindow(Window.GetWindow(this));

			// Etc events

			this.StateChanged += MainWindow_StateChanged;

			this.MouseMove += MainWindow_MouseMove;
			this.PreviewMouseMove += Feedlist_OrderChange;
			this.PreviewMouseUp += Feedlist_CompleteReorder;

			this.PreviewMouseDown += (o, e) => isMouseDown = true;

			textboxCategoryInput.KeyDown += (o, e) => {
				if (e.Key == Key.Enter) {
					buttonCategoryAddAccept.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
				}
			};

			textboxFeedInput.KeyDown += (o, e) => {
				if (e.Key == Key.Enter) {
					buttonFeedAccept.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
				}
			};

			buttonFeedAccept.Click += buttonFeedAccept_Click;
			buttonCategoryAddAccept.Click += buttonCategoryAddAccept_Click;
		}

		// Scrollbar visibility by cursor position

		private void MainWindow_MouseMove(object sender, MouseEventArgs e) {
			if (isAddWindowMode) {
				ScrollBar s1 = scrollFeedlist1.Template.FindName("PART_VerticalScrollBar", scrollFeedlist1) as ScrollBar;
				s1.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(0)));
				ScrollBar s2 = scrollFeedlist2.Template.FindName("PART_VerticalScrollBar", scrollFeedlist2) as ScrollBar;
				s2.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(0)));
				isScrollVisible = false;
				return;
			}

			if (isScrollVisible && e.GetPosition(this).X > nFeedlistWidth + 10 && !isMouseDown) {
				isScrollVisible = false;

				ScrollBar s1 = scrollFeedlist1.Template.FindName("PART_VerticalScrollBar", scrollFeedlist1) as ScrollBar;
				s1.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(150)));
				ScrollBar s2 = scrollFeedlist2.Template.FindName("PART_VerticalScrollBar", scrollFeedlist2) as ScrollBar;
				s2.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(150)));

			} else if (!isScrollVisible && e.GetPosition(this).X <= nFeedlistWidth + 10) {
				isScrollVisible = true;

				ScrollBar s1 = scrollFeedlist1.Template.FindName("PART_VerticalScrollBar", scrollFeedlist1) as ScrollBar;
				s1.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(150)));
				ScrollBar s2 = scrollFeedlist2.Template.FindName("PART_VerticalScrollBar", scrollFeedlist2) as ScrollBar;
				s2.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(150)));
			}
		}

		private async void buttonFeedAccept_Click(object sender, RoutedEventArgs e) {
			if (!isAddWindowMode) { return; }

			string strURL = textboxFeedInput.Text;

			stackListAutoDiscovery.Children.Clear();
			ShowMessage("Searching...", 20);

			Task<List<FeedCandidateItem>> httpTask = AutoDiscovery.GetCandidateFeeds(strURL);
			List<FeedCandidateItem> listCd = await httpTask;

			stackListAutoDiscovery.Children.Clear();

			if (listCd.Count == 0) {
				ShowMessage(string.Format("{0} feed detected", listCd.Count), 3);
				return;
			}

			foreach (FeedCandidateItem fcd in listCd) {
				Button buttonCandidate = CustomControl.GetFeedCandidateButton(fcd);
				buttonCandidate.Click += buttonFeedCandidate_Click;
				stackListAutoDiscovery.Children.Add(buttonCandidate);
			}
			ShowMessage("", 0);
		}

		// Autodiscovery result click event (add feed process)

		private void buttonFeedCandidate_Click(object sender, RoutedEventArgs e) {
			FeedCandidateItem fci = (FeedCandidateItem)((Button)sender).Tag;
			buttonAdd.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			MakeNewFeed(fci, NowCategoryViewID);
		}

		private void MakeNewFeed(FeedCandidateItem fci, int Category) {
			string strCaption = fci.Title, strURL = fci.URL;

			FeedDictionary.Add(nCount, new FeedItem() {
				ID = nCount, Caption = strCaption, Count = 0, ParentID = Category,
				IsFeed = true, URL = strURL,
				Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconFeed.png")),
				Contents = FeedParser.Parser(fci.Source, fci.Title),
			});
			FeedDictionary[Category].Children.Add(nCount);

			Button buttonItem = CustomControl.GetFeedItemButton(FeedDictionary[nCount], "F" + nCount, 0);
			buttonItem.Click += buttonFeedItem_Click;
			buttonItem.PreviewMouseDown += buttonFeedItem_PreviewMouseDown;

			foreach (MenuItem mItem in buttonItem.ContextMenu.Items) {
				mItem.Click += buttonFeedItemContext_Click;
			}

			nCount++;
			if (NowCategoryViewID == Category) {
				stackNow.Children.Add(buttonItem);
				scrollNow.ScrollToEnd();

				RefreshEntry(NowCategoryViewID);
			}
		}

		private void buttonCategoryAddAccept_Click(object sender, RoutedEventArgs e) {
			if (textboxCategoryInput.Text == "") {
				ShowMessage("Category's name can't be empty", 3);
				return;
			}

			FeedDictionary.Add(nCount, new FeedItem() {
				ID = nCount, Caption = textboxCategoryInput.Text, Count = 0, ParentID = NowCategoryViewID,
				IsFeed = false, URL = "", Children = new List<int>(),
				Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconCategory.png")),
			});
			FeedDictionary[NowCategoryViewID].Children.Add(nCount);

			Button buttonItem = CustomControl.GetFeedItemButton(FeedDictionary[nCount], "C" + nCount, 0);
			buttonItem.Click += buttonFeedItem_Click;
			buttonItem.PreviewMouseDown += buttonFeedItem_PreviewMouseDown;

			foreach (MenuItem mItem in buttonItem.ContextMenu.Items) {
				mItem.Click += buttonFeedItemContext_Click;
			}

			stackNow.Children.Add(buttonItem);

			nCount++;
			buttonAdd.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

			scrollNow.ScrollToEnd();
		}

		// Feed & category add window animation
		private void gridAddBackCover_MouseDown(object sender, MouseButtonEventArgs e) { buttonAdd.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }
		private void buttonAddOption_Click(object sender, RoutedEventArgs e) {
			if (isAddWindowMode) {
				gridAddWindow.IsHitTestVisible = false;
				gridAddBackCover.IsHitTestVisible = false;
			} else {
				gridAddWindow.IsHitTestVisible = true;
				gridAddBackCover.IsHitTestVisible = true;
				gridAlert.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(new Thickness(0, 0, 0, 130), TimeSpan.FromMilliseconds(0)));

				textboxCategoryInput.Text = "";
				textboxFeedInput.Text = "";
				stackListAutoDiscovery.Children.Clear();
				textboxCategoryInput.Focus();
			}
			isAddWindowMode = !isAddWindowMode;

			int nParameter = isAddWindowMode ? 1 : 0;
			Storyboard sb = new Storyboard();
			DoubleAnimation da = new DoubleAnimation(nParameter, TimeSpan.FromMilliseconds(250));
			ThicknessAnimation ta = new ThicknessAnimation(new Thickness(0, 0, 0, -130 * (1 - nParameter)), TimeSpan.FromMilliseconds(250)) {
				BeginTime = TimeSpan.FromMilliseconds(100 * nParameter),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut }
			};

			Storyboard.SetTarget(da, gridAddWindow); Storyboard.SetTargetProperty(da, new PropertyPath(Grid.OpacityProperty));
			Storyboard.SetTarget(ta, gridAddInnerWindow); Storyboard.SetTargetProperty(ta, new PropertyPath(Grid.MarginProperty));

			sb.Children.Add(da); sb.Children.Add(ta);
			sb.Begin(this);
		}

		// Refresh feedlist by ID number
		#region Refresh feedlist by ID number

		bool isFirstView = true; int NowCategoryViewID = 0, nNowEntryViewID = 0;
		StackPanel stackNow; AniScrollViewer scrollNow;
		private void RefreshFeedList(int nID, bool isBack) {
			if (isFeedlistAnimating) { return; }

			scrollNow = isFirstView ? scrollFeedlist1 : scrollFeedlist2;

			if (!isBack) { FeedDictionary[NowCategoryViewID].ScrollOffset = Math.Min(scrollNow.ScrollableHeight, scrollNow.VerticalOffset); }

			NowCategoryViewID = nID;
			isFirstView = !isFirstView;

			StackPanel stackPrev = isFirstView ? stackFeedlist2 : stackFeedlist1;
			StackPanel stackNext = isFirstView ? stackFeedlist1 : stackFeedlist2;
			scrollNow = isFirstView ? scrollFeedlist1 : scrollFeedlist2;

			stackNow = stackNext;
			stackNext.Children.Clear();

			double dMargin = nID > 0 ? 0 : 20;

			Button buttonRoot = CustomControl.GetFeedItemButton(FeedDictionary[nID], "A" + nID, dMargin);
			buttonRoot.Click += buttonFeedItem_Click;
			buttonRoot.PreviewMouseDown += buttonFeedItem_PreviewMouseDown;

			stackNext.Children.Add(buttonRoot);

			if (nID > 0) {
				FeedItem fItem = new FeedItem() {
					ID = 0, Caption = "back to parent", Count = 0,
					IsFeed = true, URL = "", Children = new List<int>(),
					Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconBack.png")),
				};
				Button buttonBack = CustomControl.GetFeedItemButton(fItem, "B" + FeedDictionary[nID].ParentID, 20);
				buttonBack.Click += buttonFeedItem_Click;
				buttonBack.PreviewMouseDown += buttonFeedItem_PreviewMouseDown;
				stackNext.Children.Add(buttonBack);
			}

			foreach (int fItemTag in FeedDictionary[nID].Children) {
				string strTag = "C";
				if (FeedDictionary[fItemTag].IsFeed) { strTag = "F"; }

				Button buttonItem = CustomControl.GetFeedItemButton(FeedDictionary[fItemTag], strTag + fItemTag, 0);
				buttonItem.Click += buttonFeedItem_Click;
				buttonItem.PreviewMouseDown += buttonFeedItem_PreviewMouseDown;

				foreach (MenuItem mItem in buttonItem.ContextMenu.Items) {
					mItem.Click += buttonFeedItemContext_Click;
				}

				stackNext.Children.Add(buttonItem);
			}

			if (isBack) {
				scrollNow.ScrollToVerticalOffset(FeedDictionary[nID].ScrollOffset);
			} else {
				scrollNow.ScrollToTop();
			}

			AnimateFeedlist(isBack);
		}

		bool isFeedlistAnimating = false;
		private void AnimateFeedlist(bool isBack) {
			isFeedlistAnimating = true;

			StackPanel stackPrev = isFirstView ? stackFeedlist2 : stackFeedlist1;
			StackPanel stackNext = isFirstView ? stackFeedlist1 : stackFeedlist2;
			ScrollViewer scrollPrev = isFirstView ? scrollFeedlist2 : scrollFeedlist1;
			ScrollViewer scrollNext = isFirstView ? scrollFeedlist1 : scrollFeedlist2;
			Thickness tn = new Thickness(-100, 0, 0, 0);

			stackNext.Visibility = Visibility.Visible;
			scrollPrev.IsHitTestVisible = false;
			scrollNext.IsHitTestVisible = true;

			if (!isBack) {
				tn = new Thickness(widthFeedlist.Width.Value, 0, 0, 0);
				gridFeedlist.Children.Remove(scrollNow);
				gridFeedlist.Children.Add(scrollNow);
			} else {
				gridFeedlist.Children.Remove(scrollNow);
				gridFeedlist.Children.Insert(0, scrollNow);
			}

			int nBack = isBack ? 1 : 0;

			Storyboard sb = new Storyboard();
			ThicknessAnimation taPrev = new ThicknessAnimation(new Thickness(-100 + nBack * (widthFeedlist.Width.Value + 100), 0, 0, 0), TimeSpan.FromMilliseconds(350)) {
				BeginTime = TimeSpan.FromMilliseconds(150),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut }
			};
			ThicknessAnimation taPreS = new ThicknessAnimation(tn, TimeSpan.FromMilliseconds(0));
			ThicknessAnimation taNext = new ThicknessAnimation(tn, new Thickness(0), TimeSpan.FromMilliseconds(350)) {
				BeginTime = TimeSpan.FromMilliseconds(150),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut }
			};

			Storyboard.SetTarget(taPrev, stackPrev); Storyboard.SetTarget(taNext, stackNext); Storyboard.SetTarget(taPreS, stackNext);
			Storyboard.SetTargetProperty(taPrev, new PropertyPath(StackPanel.MarginProperty));
			Storyboard.SetTargetProperty(taPreS, new PropertyPath(StackPanel.MarginProperty));
			Storyboard.SetTargetProperty(taNext, new PropertyPath(StackPanel.MarginProperty));

			sb.Children.Add(taPrev); sb.Children.Add(taPreS); sb.Children.Add(taNext);
			sb.Completed += delegate(object sender, EventArgs e) {
				isFeedlistAnimating = false;
				stackPrev.Visibility = Visibility.Collapsed;

				ScrollBar s1 = scrollNext.Template.FindName("PART_VerticalScrollBar", scrollNext) as ScrollBar;
				s1.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(150)));

				RefreshEntry(NowCategoryViewID);
			};
			sb.Begin(this);
		}

		#endregion

		// Autodiscovery alert message function

		private void ShowMessage(string message, double dTimeout) {
			Storyboard sb = new Storyboard();

			if (dTimeout > 0) {
				ThicknessAnimation ta1 = new ThicknessAnimation(new Thickness(0, -30, 0, 0), TimeSpan.FromMilliseconds(200)) {
					BeginTime = TimeSpan.FromMilliseconds(100),
					EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
				};
				textMessage.Text = message;
				Storyboard.SetTarget(ta1, gridAlert);
				Storyboard.SetTargetProperty(ta1, new PropertyPath(Grid.MarginProperty));
				sb.Children.Add(ta1);
			}

			ThicknessAnimation ta2 = new ThicknessAnimation(new Thickness(0, 0, 0, 0), TimeSpan.FromMilliseconds(200)) {
				BeginTime = TimeSpan.FromSeconds(dTimeout),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
			};

			Storyboard.SetTarget(ta2, gridAlert);
			Storyboard.SetTargetProperty(ta2, new PropertyPath(Grid.MarginProperty));

			sb.Children.Add(ta2);
			sb.Begin(this);
		}
	}
}
