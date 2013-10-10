using System;
using System.Collections.Generic;
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
		public MainWindow() {

			InitializeComponent();

			// Controlbox events

			buttonMinimize.Click += (o, e) => Microsoft.Windows.Shell.SystemCommands.MinimizeWindow(Window.GetWindow(this));
			buttonMaximize.Click += (o, e) => Microsoft.Windows.Shell.SystemCommands.MaximizeWindow(Window.GetWindow(this));
			buttonRestore.Click += (o, e) => Microsoft.Windows.Shell.SystemCommands.RestoreWindow(Window.GetWindow(this));
			buttonClose.Click += (o, e) => Microsoft.Windows.Shell.SystemCommands.CloseWindow(Window.GetWindow(this));

			// Etc events

			this.StateChanged += MainWindow_StateChanged;

			this.MouseMove += MainWindow_MouseMove;
			this.PreviewMouseMove += Feedlist_OrderChange;
			this.PreviewMouseUp += Feedlist_CompleteReorder;

			this.PreviewMouseDown += (o, e) => isMouseDown = true;

			textboxInput.KeyDown += (o, e) => {
				if (e.Key == Key.Enter) {
					buttonAddAccept.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
				}
			};

			// Autodiscovery timer trigger (default:3sec)

			textboxInput.TextChanged += (o, e) => {
				if (textboxInput.Text == "") { return; }
				strGlobalLoadingURL = textboxInput.Text;

				dtm.Stop();
				dtm.Tag = (string)textboxInput.Text;

				dtm.Tick += dtm_Tick;
				dtm.Start();
			};

			// Category adding process.

			buttonAddAccept.Click += (o, e) => {
				if (textboxInput.Text == "") {
					ShowMessage("Category's name can't be empty", 3);
					return;
				}

				dictFeedItem.Add(nCount, new FeedItem() {
					ID = nCount, Caption = textboxInput.Text, Count = 0, ParentID = nNowID,
					IsFeed = false, URL = "", Children = new List<int>(), 
					Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconCategory.png")),
				});
				dictFeedItem[nNowID].Children.Add(nCount);

				Button buttonItem = CustomControl.GetFeedItemButton(dictFeedItem[nCount], "C" + nCount, 0);
				buttonItem.Click += buttonFeedItem_Click;
				buttonItem.PreviewMouseDown += buttonFeedItem_PreviewMouseDown;

				foreach (MenuItem mItem in buttonItem.ContextMenu.Items) {
					mItem.Click += buttonFeedItemContext_Click;
				}

				stackNow.Children.Add(buttonItem);

				nCount++;
				buttonAdd.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			};

			// Add root

			dictFeedItem.Add(0, new FeedItem() {
				ID = 0, Caption = "all feeds", Count = 1, ParentID = 0,
				IsFeed = true, URL = "", Children = new List<int>(),
				Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconAll.png")),
			});

			// Test code (30 categories)

			for (int i = 0; i < 30; i++) {
				dictFeedItem.Add(nCount, new FeedItem() {
					ID = nCount, Caption = "Folder " + i.ToString("00"), Count = 0, ParentID = 0,
					IsFeed = false, URL = "", Children = new List<int>(),
					Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconCategory.png")),
				});
				dictFeedItem[0].Children.Add(nCount);
				nCount++;
			}

			RefreshFeedList(0, false);
		}

		// Feed & category add window animation

		private void gridAddBackCover_MouseDown(object sender, MouseButtonEventArgs e) { buttonAdd.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }
		private void buttonAdd_Click(object sender, RoutedEventArgs e) {
			if (isAddWindowMode) {
				gridAddWindow.IsHitTestVisible = false;
				gridAddBackCover.IsHitTestVisible = false;
			} else {
				gridAddWindow.IsHitTestVisible = true;
				gridAddBackCover.IsHitTestVisible = true;
				gridAlert.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(new Thickness(0), TimeSpan.FromMilliseconds(0)));

				textboxInput.Text = "";
				stackListAutoDiscovery.Children.Clear();
				textboxInput.Focus();
			}
			isAddWindowMode = !isAddWindowMode;

			int nParameter = isAddWindowMode ? 1 : 0;
			Storyboard sb = new Storyboard();
			DoubleAnimation da = new DoubleAnimation(nParameter, TimeSpan.FromMilliseconds(250));
			ThicknessAnimation ta = new ThicknessAnimation(new Thickness(0, 0, 0, -90 * (1 - nParameter)), TimeSpan.FromMilliseconds(250)) {
				BeginTime = TimeSpan.FromMilliseconds(100 * nParameter),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut }
			};

			Storyboard.SetTarget(da, gridAddWindow); Storyboard.SetTargetProperty(da, new PropertyPath(Grid.OpacityProperty));
			Storyboard.SetTarget(ta, gridAddInnerWindow); Storyboard.SetTargetProperty(ta, new PropertyPath(Grid.MarginProperty));

			sb.Children.Add(da); sb.Children.Add(ta);
			sb.Begin(this);
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

		// Autodiscovery sample code

		string strGlobalLoadingURL = "";
		DispatcherTimer dtm = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1500), IsEnabled = false };
		private async void dtm_Tick(object sender, EventArgs e) {
			((DispatcherTimer)sender).Stop();
			if (!isAddWindowMode) { return; }

			string strURL = (string)((DispatcherTimer)sender).Tag;

			if (strURL != strGlobalLoadingURL) { return; }

			stackListAutoDiscovery.Children.Clear();
			ShowMessage("Searching...", 20);
			
			Task<List<FeedCandidateList>> httpTask = AutoDiscovery.GetCandidateFeeds(strURL);
			List<FeedCandidateList> listCd = await httpTask;

			stackListAutoDiscovery.Children.Clear();

			if (strURL != strGlobalLoadingURL) { return; }
			if (listCd.Count == 0) {
				ShowMessage(string.Format("{0} feed detected", listCd.Count), 3);
				return;
			}

			foreach (FeedCandidateList fcd in listCd) {
				Button buttonCandidate = CustomControl.GetFeedCandidateButton(fcd.Title, fcd.URL);
				buttonCandidate.Click += buttonFeedCandidate_Click;
				stackListAutoDiscovery.Children.Add(buttonCandidate);
			}
			ShowMessage("", 0);
		}

		// Autodiscovery alert message function

		private void ShowMessage(string message, double dTimeout) {
			Storyboard sb = new Storyboard();

			if (dTimeout > 0) {
				ThicknessAnimation ta1 = new ThicknessAnimation(new Thickness(0, 0, 0, 50), TimeSpan.FromMilliseconds(200)) {
					BeginTime = TimeSpan.FromMilliseconds(100),
					EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
				};
				textMessage.Text = message;
				Storyboard.SetTarget(ta1, gridAlert);
				Storyboard.SetTargetProperty(ta1, new PropertyPath(Grid.MarginProperty));
				sb.Children.Add(ta1);
			}

			ThicknessAnimation ta2 = new ThicknessAnimation(new Thickness(0), TimeSpan.FromMilliseconds(200)) {
				BeginTime = TimeSpan.FromSeconds(dTimeout),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
			};

			Storyboard.SetTarget(ta2, gridAlert);
			Storyboard.SetTargetProperty(ta2, new PropertyPath(Grid.MarginProperty));

			sb.Children.Add(ta2);
			sb.Begin(this);
		}

		// Autodiscovery result click event (add feed process)

		private void buttonFeedCandidate_Click(object sender, RoutedEventArgs e) {
			KeyValuePair<string, string> kvp = (KeyValuePair<string, string>)((Button)sender).Tag;
			string strCaption = kvp.Key, strURL = kvp.Value;

			dictFeedItem.Add(nCount, new FeedItem() {
				ID = nCount, Caption = strCaption, Count = 0, ParentID = nNowID,
				IsFeed = true, URL = strURL,
				Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconFeed.png")),
			});
			dictFeedItem[nNowID].Children.Add(nCount);

			Button buttonItem = CustomControl.GetFeedItemButton(dictFeedItem[nCount], "F" + nCount, 0);
			buttonItem.Click += buttonFeedItem_Click;
			buttonItem.PreviewMouseDown += buttonFeedItem_PreviewMouseDown;

			foreach (MenuItem mItem in buttonItem.ContextMenu.Items) {
				mItem.Click += buttonFeedItemContext_Click;
			}

			stackNow.Children.Add(buttonItem);

			nCount++;
			buttonAdd.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
		}


		bool isScrollVisible = false, isMouseDown, isAddWindowMode, isDialogMode;
		int nFeedlistWidth = 250, nCount = 1;
		Dictionary<int, FeedItem> dictFeedItem = new Dictionary<int, FeedItem>();

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

		// Refresh feedlist by ID number
		#region Refresh feedlist by ID number

		bool isFirstView = true; int nNowID = 0;
		StackPanel stackNow; AniScrollViewer scrollNow;
		private void RefreshFeedList(int nID, bool isBack) {
			if (isFeedlistAnimating) { return; }

			scrollNow = isFirstView ? scrollFeedlist1 : scrollFeedlist2;

			if (!isBack) { dictFeedItem[nNowID].ScrollOffset = Math.Min(scrollNow.ScrollableHeight, scrollNow.VerticalOffset); }

			nNowID = nID;
			isFirstView = !isFirstView;

			StackPanel stackPrev = isFirstView ? stackFeedlist2 : stackFeedlist1;
			StackPanel stackNext = isFirstView ? stackFeedlist1 : stackFeedlist2;
			scrollNow = isFirstView ? scrollFeedlist1 : scrollFeedlist2;

			stackNow = stackNext;
			stackNext.Children.Clear();

			double dMargin = nID > 0 ? 0 : 20;

			Button buttonRoot = CustomControl.GetFeedItemButton(dictFeedItem[nID], "A" + nID, dMargin);
			buttonRoot.Click += buttonFeedItem_Click;
			buttonRoot.PreviewMouseDown += buttonFeedItem_PreviewMouseDown;

			stackNext.Children.Add(buttonRoot);

			if (nID > 0) {
				FeedItem fItem = new FeedItem() {
					ID = 0, Caption = "back to parent", Count = 0,
					IsFeed = true, URL = "", Children = new List<int>(),
					Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconBack.png")),
				};
				Button buttonBack = CustomControl.GetFeedItemButton(fItem, "B" + dictFeedItem[nID].ParentID, 20);
				buttonBack.Click += buttonFeedItem_Click;
				buttonBack.PreviewMouseDown += buttonFeedItem_PreviewMouseDown;
				stackNext.Children.Add(buttonBack);
			}

			foreach (int fItemTag in dictFeedItem[nID].Children) {
				string strTag = "C";
				if (dictFeedItem[fItemTag].IsFeed) { strTag = "F"; }

				Button buttonItem = CustomControl.GetFeedItemButton(dictFeedItem[fItemTag], strTag + fItemTag, 0);
				buttonItem.Click += buttonFeedItem_Click;
				buttonItem.PreviewMouseDown += buttonFeedItem_PreviewMouseDown;

				foreach (MenuItem mItem in buttonItem.ContextMenu.Items) {
					mItem.Click += buttonFeedItemContext_Click;
				}

				stackNext.Children.Add(buttonItem);
			}

			if (isBack) {
				scrollNow.ScrollToVerticalOffset(dictFeedItem[nID].ScrollOffset);
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
			};
			sb.Begin(this);
		}

		#endregion

		#region Feeditem moving event

		// Moving event : PreviewMouseDown

		int nMouseDownID = -1, nMouseMovingID = -1; Point pointMouseDown; bool isMoving;
		private void buttonFeedItem_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			if (isDialogMode) { return; }
			string strTag = (string)((Button)sender).Tag;
			isMoving = false;
			nToIndex = -1;

			if (strTag[0] == 'A' || strTag[0] == 'B') { return; }

			nMouseDownID = Convert.ToInt32(strTag.Substring(1));
			pointMouseDown = e.GetPosition(this);

			nFromIndex = dictFeedItem[nNowID].Children.IndexOf(nMouseDownID);

			//textTemp.Text = nMouseDownID.ToString();
		}

		// Moving event : MouseMove

		Point pointMouseMove;
		private void Feedlist_OrderChange(object sender, MouseEventArgs e) {
			if (!isMouseDown || nMouseDownID < 0 || isDialogMode) { return; }
			pointMouseMove = e.GetPosition(this);

			if (Math.Max(Math.Abs(pointMouseDown.X - pointMouseMove.X), Math.Abs(pointMouseDown.Y - pointMouseMove.Y)) >= 5 && !isMoving) {
				nMouseMovingID = nMouseDownID;
				isMoving = true;

				textNowMoving.Text = dictFeedItem[nMouseDownID].Caption;
				gridMoveStatus.Visibility = Visibility.Visible;
				rectMovePosition.Width = widthFeedlist.Width.Value;
			}

			if (isMoving) { FeedlistMouseMoveEvent(); }
		}

		bool isCornerScrollingDelay; int nFromIndex, nToIndex;
		private void FeedlistMouseMoveEvent() {
			if (!isMouseDown || isDialogMode) { return; }

			if (pointMouseMove.X < 0 || pointMouseMove.X > widthFeedlist.Width.Value || pointMouseMove.Y < 0 || pointMouseMove.Y > this.ActualHeight) {
				nToIndex = -1;
				gridMoveStatus.Visibility = Visibility.Collapsed;
				return;
			} else {
				gridMoveStatus.Visibility = Visibility.Visible;
			}

			gridNowMoving.Margin = new Thickness(pointMouseMove.X - 100, pointMouseMove.Y - 70, 0, 0);

			if (pointMouseMove.Y <= 50) {
				rectMovePosition.Visibility = Visibility.Collapsed;

				if (isCornerScrollingDelay) { return; }
				isCornerScrollingDelay = true;
				DelayTimer(250, "isCornerScrollingDelay");

				scrollNow.BeginAnimation(AniScrollViewer.CurrentVerticalOffsetProperty,
					new DoubleAnimation(scrollNow.VerticalOffset, Math.Max(scrollNow.VerticalOffset - 150, 0), TimeSpan.FromMilliseconds(200)) {
						EasingFunction = new ExponentialEase() { Exponent = 1, EasingMode = EasingMode.EaseOut },
					});
			} else if (pointMouseMove.Y >= this.ActualHeight - 50) {
				rectMovePosition.Visibility = Visibility.Collapsed;

				if (isCornerScrollingDelay) { return; }
				isCornerScrollingDelay = true;
				DelayTimer(250, "isCornerScrollingDelay");

				scrollNow.BeginAnimation(AniScrollViewer.CurrentVerticalOffsetProperty,
							new DoubleAnimation(scrollNow.VerticalOffset, Math.Min(scrollNow.VerticalOffset + 150, scrollNow.ScrollableHeight), TimeSpan.FromMilliseconds(200)) {
								EasingFunction = new ExponentialEase() { Exponent = 1, EasingMode = EasingMode.EaseOut },
							});
			} else {
				rectMovePosition.Visibility = Visibility.Visible;

				double pointAbsolute = scrollNow.VerticalOffset + pointMouseMove.Y - 50;		// 50 is height of titlebar
				double dFixedHeight = 100;
				if (nNowID == 0) { dFixedHeight = 60; }

				double pointRelate = pointAbsolute - dFixedHeight;

				int nHoverIndex = (int)pointRelate / 40;
				nHoverIndex = Math.Max(0, nHoverIndex);
				nHoverIndex = Math.Min(dictFeedItem[nNowID].Children.Count - 1, nHoverIndex);

				int nDivideHeight = 10;
				if (dictFeedItem[dictFeedItem[nNowID].Children[nHoverIndex]].IsFeed) { nDivideHeight = 20; }

				double dRectPosition = 0;

				// Mod 3 Case
				// 0 == |--------------*|
				// 1 == |*--------------|
				// 2 == |-------*-------|

				if (dFixedHeight == 100 && pointAbsolute >= 40 && pointAbsolute <= 80) {
					// Back Position
					nToIndex = 0;
					dRectPosition = 40;
					rectMovePosition.Height = 40;
					rectMovePosition.Margin = new Thickness(0, dRectPosition, 0, 0);
					return;
				}

				if (pointRelate < nHoverIndex * 40 + nDivideHeight) {
					textTemp.Text = nHoverIndex + "번째의 앞에";

					nToIndex = nHoverIndex * 3 + 1;

					dRectPosition = nHoverIndex * 40 + dFixedHeight - 1;
					rectMovePosition.Height = 2;

				} else if (pointRelate >= (nHoverIndex + 1) * 40 - nDivideHeight) {
					textTemp.Text = nHoverIndex + "번째의 뒤에";

					nToIndex = nHoverIndex * 3 + 3;

					dRectPosition = (nHoverIndex + 1) * 40 + dFixedHeight - 1;
					rectMovePosition.Height = 2;

				} else {
					if (dictFeedItem[dictFeedItem[nNowID].Children[nHoverIndex]].IsFeed) { return; }
					textTemp.Text = nHoverIndex + "의 안에";

					nToIndex = nHoverIndex * 3 + 2;

					dRectPosition = nHoverIndex * 40 + dFixedHeight;
					rectMovePosition.Height = 40;
				}

				dRectPosition -= scrollNow.VerticalOffset;

				rectMovePosition.Margin = new Thickness(0, dRectPosition, 0, 0);
			}
		}

		// Moving event : Mouse up, complete moving

		private void Feedlist_CompleteReorder(object sender, MouseButtonEventArgs e) {
			if (nMouseDownID < 0 || isDialogMode) { return; }
			isMouseDown = false;
			nMouseMovingID = nMouseDownID = -1;
			gridMoveStatus.Visibility = Visibility.Collapsed;

			if (nToIndex < 0) { return; }

			//textTemp.Text = string.Format("Result : {0} -> {1}", nFromIndex, nToIndex);
			int nSelectedTag = dictFeedItem[nNowID].Children[nFromIndex];
			int nTowardTag = dictFeedItem[nNowID].Children[(nToIndex - 1) / 3];

			int nOffset = 1;
			if (nNowID != 0) { nOffset = 2; }

			if (nToIndex == 0) {
				// send to parent

				dictFeedItem[dictFeedItem[nNowID].ParentID].Children.Add(nSelectedTag);
				dictFeedItem[nSelectedTag].ParentID = dictFeedItem[nNowID].ParentID;

				dictFeedItem[nNowID].Children.Remove(nSelectedTag);
				stackNow.Children.RemoveAt(nFromIndex + nOffset);

			} else if (nToIndex % 3 == 2) {
				// put to other category

				// Itself
				if (nSelectedTag == nTowardTag) { return; }

				dictFeedItem[nTowardTag].Children.Add(nSelectedTag);
				dictFeedItem[nSelectedTag].ParentID = nTowardTag;

				dictFeedItem[nNowID].Children.Remove(nSelectedTag);
				stackNow.Children.RemoveAt(nFromIndex + nOffset);

			} else {
				int nAbsToIndex = (nToIndex - 1) / 3;
				if (nFromIndex == nAbsToIndex) { return; }

				if (nToIndex % 3 == 0) { nAbsToIndex++; }

				int nBeforeOffset = 0;
				if (nFromIndex < nAbsToIndex) { nBeforeOffset = 1; }

				dictFeedItem[nNowID].Children.RemoveAt(nFromIndex);
				dictFeedItem[nNowID].Children.Insert(nAbsToIndex - nBeforeOffset, nSelectedTag);

				Button buttonMove = (Button)stackNow.Children[nFromIndex + nOffset];
				stackNow.Children.RemoveAt(nFromIndex + nOffset);
				stackNow.Children.Insert(nAbsToIndex - nBeforeOffset + nOffset, buttonMove);
			}
		}

		// Scroll event by cornering cursor

		private void DelayTimer(double time, string idTag) {
			DispatcherTimer timerDelay = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(time), IsEnabled = true, Tag = idTag };
			timerDelay.Tick += timerDelay_Tick;
		}

		private void timerDelay_Tick(object sender, EventArgs e) {
			string id = (string)((DispatcherTimer)sender).Tag;

			switch (id) {
				case "isCornerScrollingDelay":
					isCornerScrollingDelay = false;
					((DispatcherTimer)sender).Stop();
					break;
			}
			FeedlistMouseMoveEvent();
		}

		#endregion

		#region Feed item context menu

		private void buttonFeedItemContext_Click(object sender, RoutedEventArgs e) {
			ShowCommonDialog((string)((MenuItem)sender).Tag);
		}

		char strDialogType = 'N'; int nDialogID = -1;
		private void ShowCommonDialog(string strArgs) {
			strDialogType = strArgs[0];
			nDialogID = Convert.ToInt32(strArgs.Substring(1));

			switch (strDialogType) {
				case 'R':
					// Rename
					textFormType.Text = "Rename item";
					textboxTitle.Text = "";
					textboxTitle.Tag = dictFeedItem[nDialogID].Caption;

					textboxTitle.Visibility = Visibility.Visible;
					textDialogMessage.Visibility = Visibility.Collapsed;

					textboxTitle.Focus();
					break;

				case 'D':
					// Delete
					textFormType.Text = "Question";
					textDialogMessage.Text = string.Format("{0} 지울거임", dictFeedItem[nDialogID].Caption);

					if (!dictFeedItem[nDialogID].IsFeed) {
						textDialogMessage.Text += "\n카테고리의 경우 내부까지 전부 지워짐";
					}

					textboxTitle.Visibility = Visibility.Collapsed;
					textDialogMessage.Visibility = Visibility.Visible;
					break;
			}

			isDialogMode = true;
			gridDialog.IsHitTestVisible = true;
			gridDialog.BeginAnimation(Grid.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(150)));
		}

		private void buttonPopupOK_Click(object sender, RoutedEventArgs e) {
			switch (strDialogType) {
				case 'R':

					if (textboxTitle.Text == "") {
						ShowTopMessage("Title can't be empty");
						textboxTitle.Focus(); return;
					}

					dictFeedItem[nDialogID].Caption = textboxTitle.Text;

					break;

				case 'D':
					// Delete

					int nOffset = 1;
					if (nNowID != 0) { nOffset = 2; }

					int nIndex = dictFeedItem[nNowID].Children.IndexOf(nDialogID);
					stackNow.Children.RemoveAt(nIndex + nOffset);

					dictFeedItem[nNowID].Children.Remove(nDialogID);

					Queue<int> q = new Queue<int>();
					q.Enqueue(nDialogID);

					int nVertex;
					for (; q.Count > 0; ) {
						nVertex = q.Dequeue();
						if (!dictFeedItem[nVertex].IsFeed) {
							foreach (int i in dictFeedItem[nVertex].Children) {
								q.Enqueue(i);
							}
						}

						dictFeedItem.Remove(nVertex);
					}

					break;
			}

			buttonPopupCancel.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			isDialogMode = false;
		}

		private void gridDialogBackCover_MouseDown(object sender, MouseButtonEventArgs e) { buttonPopupCancel.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }
		private void buttonPopupCancel_Click(object sender, RoutedEventArgs e) {
			isDialogMode = false;
			gridDialog.IsHitTestVisible = false;
			gridDialog.BeginAnimation(Grid.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(150)));
		}

		private void ShowTopMessage(string strMessage) {
			textTopMessage.Text = strMessage;

			Storyboard sb = new Storyboard();
			DoubleAnimation da1 = new DoubleAnimation(1, 1, TimeSpan.FromMilliseconds(2500));
			Storyboard.SetTarget(da1, textTopMessage);
			Storyboard.SetTargetProperty(da1, new PropertyPath(TextBlock.OpacityProperty));

			DoubleAnimation da2 = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
			Storyboard.SetTarget(da2, textTopMessage);
			Storyboard.SetTargetProperty(da2, new PropertyPath(TextBlock.OpacityProperty));
			da2.BeginTime = TimeSpan.FromMilliseconds(1500);

			sb.Children.Add(da1);
			sb.Children.Add(da2);
			sb.Begin(this);
		}

		#endregion

		private void buttonFeedItem_Click(object sender, RoutedEventArgs e) {
			string strTag = (string)((Button)sender).Tag;
			int nID = Convert.ToInt32(strTag.Substring(1));

			if (isMoving) { return; }

			switch (strTag[0]) {
				case 'A':
					// all feeds

					break;
				case 'B':
					// back to parent
					RefreshFeedList(nID, true);

					break;
				case 'C':
					// category
					RefreshFeedList(nID, false);

					break;
				default:
					// feed

					break;
			}

			//MessageBox.Show(nID.ToString() + "\n" + widthFeedlist.Width);
		}
	}
}
