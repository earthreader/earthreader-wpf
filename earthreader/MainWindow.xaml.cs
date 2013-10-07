using System;
using System.Collections.Generic;
using System.Linq;
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

namespace earthreader {
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {

			InitializeComponent();

			buttonMinimize.Click += (o, e) => Microsoft.Windows.Shell.SystemCommands.MinimizeWindow(Window.GetWindow(this));
			buttonMaximize.Click += (o, e) => Microsoft.Windows.Shell.SystemCommands.MaximizeWindow(Window.GetWindow(this));
			buttonRestore.Click += (o, e) => Microsoft.Windows.Shell.SystemCommands.RestoreWindow(Window.GetWindow(this));
			buttonClose.Click += (o, e) => Microsoft.Windows.Shell.SystemCommands.CloseWindow(Window.GetWindow(this));
			this.StateChanged += MainWindow_StateChanged;

			this.MouseMove += MainWindow_MouseMove;
			this.PreviewMouseDown += (o, e) => isMouseDown = true;
			this.PreviewMouseUp += (o, e) => isMouseDown = false;

			buttonCategoryAccept.Click += (o, e) => {
				if (textboxCategoryInput.Text == "") {
					ShowMessage("Category's name can't be empty");
					return;
				}

				dictFeedItem.Add(nCount, new FeedItem() {
					ID = nCount, Caption = textboxCategoryInput.Text, Count = 0, ParentID = nNowID,
					IsFeed = false, URL = "", Children = new List<int>(), 
					Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconCategory.png")),
				});
				dictFeedItem[nNowID].Children.Add(nCount);

				Button buttonItem = CustomControl.GetFeedItemButton(dictFeedItem[nCount], "C" + nCount, 0);
				buttonItem.Click += buttonFeedItem_Click;
				stackNow.Children.Add(buttonItem);

				nCount++;
				buttonAdd.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			};
			buttonFeedAccept.Click += (o, e) => {
				if (textboxFeedInput.Text == "") {
					ShowMessage("Enter feed url");
					return;
				}
			};

			// Add root
			dictFeedItem.Add(0, new FeedItem() {
				ID = 0, Caption = "all feeds", Count = 1, ParentID = 0,
				IsFeed = true, URL = "", Children = new List<int>(),
				Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconFeed.png")),
			});

			RefreshFeedList(0, false);
		}

		
		bool isScrollVisible = false, isMouseDown, isAddWindowMode;
		int nFeedlistWidth = 250, nCount = 1;
		Dictionary<int, FeedItem> dictFeedItem = new Dictionary<int, FeedItem>();

		private void MainWindow_MouseMove(object sender, MouseEventArgs e) {
			if (isAddWindowMode) {
				ScrollBar s = scrollFeedlist.Template.FindName("PART_VerticalScrollBar", scrollFeedlist) as ScrollBar;
				s.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(0)));
				isScrollVisible = false;
				return;
			}
			
			if (isScrollVisible && e.GetPosition(this).X > nFeedlistWidth + 10 && !isMouseDown) {
				isScrollVisible = false;

				ScrollBar s = scrollFeedlist.Template.FindName("PART_VerticalScrollBar", scrollFeedlist) as ScrollBar;
				s.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(150)));

			} else if (!isScrollVisible && e.GetPosition(this).X <= nFeedlistWidth + 10) {
				isScrollVisible = true;

				ScrollBar s = scrollFeedlist.Template.FindName("PART_VerticalScrollBar", scrollFeedlist) as ScrollBar;
				s.BeginAnimation(ScrollBar.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(150)));
			}
		}

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

		private void gridAddBackCover_MouseDown(object sender, MouseButtonEventArgs e) { buttonAdd.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }
		private void buttonAdd_Click(object sender, RoutedEventArgs e) {
			if (isAddWindowMode) {
				gridAddBackCover.IsHitTestVisible = false;
			} else {
				gridAddBackCover.IsHitTestVisible = true;
				gridMessage.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(new Thickness(0), TimeSpan.FromMilliseconds(0)));

				textboxFeedInput.Text = textboxCategoryInput.Text = "";
				stackListAutoDiscovery.Children.Clear();
				textboxFeedInput.Focus();
			}
			isAddWindowMode = !isAddWindowMode;

			int nParameter = isAddWindowMode ? 1 : 0;
			Storyboard sb = new Storyboard();
			DoubleAnimation da = new DoubleAnimation(nParameter, TimeSpan.FromMilliseconds(250));
			ThicknessAnimation ta = new ThicknessAnimation(new Thickness(0, 0, 0, -90 * (1 - nParameter)), TimeSpan.FromMilliseconds(250)) {
				BeginTime = TimeSpan.FromMilliseconds(150 * nParameter),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut }
			};

			Storyboard.SetTarget(da, gridAddWindow); Storyboard.SetTargetProperty(da, new PropertyPath(Grid.OpacityProperty));
			Storyboard.SetTarget(ta, gridAddInnerWindow); Storyboard.SetTargetProperty(ta, new PropertyPath(Grid.MarginProperty));

			sb.Children.Add(da); sb.Children.Add(ta);
			sb.Begin(this);
		}


		private void ShowMessage(string message) {
			textMessage.Text = message;
			Storyboard sb = new Storyboard();
			ThicknessAnimation ta1 = new ThicknessAnimation(new Thickness(0, -30, 0, 0), TimeSpan.FromMilliseconds(200)) {
				BeginTime = TimeSpan.FromMilliseconds(100),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
			};
			ThicknessAnimation ta2 = new ThicknessAnimation(new Thickness(0), TimeSpan.FromMilliseconds(200)) {
				BeginTime = TimeSpan.FromSeconds(3),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
			};

			Storyboard.SetTarget(ta1, gridMessage); Storyboard.SetTarget(ta2, gridMessage);
			Storyboard.SetTargetProperty(ta1, new PropertyPath(Grid.MarginProperty));
			Storyboard.SetTargetProperty(ta2, new PropertyPath(Grid.MarginProperty));

			sb.Children.Add(ta1); sb.Children.Add(ta2);
			sb.Begin(this);
		}


		bool isFirstView = true; int nNowID = 0;
		StackPanel stackNow;
		private void RefreshFeedList(int nID, bool isBack) {
			if (isFeedlistAnimating) { return; }

			nNowID = nID;
			isFirstView = !isFirstView;

			StackPanel stackPrev = isFirstView ? stackFeedlist2 : stackFeedlist1;
			StackPanel stackNext = isFirstView ? stackFeedlist1 : stackFeedlist2;
			stackNow = stackNext;
			stackNext.Children.Clear();
			
			Button buttonRoot = CustomControl.GetFeedItemButton(dictFeedItem[nID], "A" + nID, 0);
			buttonRoot.Click += buttonFeedItem_Click;
			stackNext.Children.Add(buttonRoot);

			if (nID > 0) {
				FeedItem fItem = new FeedItem() {
					ID = 0, Caption = "back to parent", Count = 0,
					IsFeed = true, URL = "", Children = new List<int>(),
					Favicon = new BitmapImage(new Uri("pack://application:,,,/earthreader;component/Resources/iconBack.png")),
				};
				Button buttonBack = CustomControl.GetFeedItemButton(fItem, "B" + dictFeedItem[nID].ParentID, 20);
				buttonBack.Click += buttonFeedItem_Click;
				stackNext.Children.Add(buttonBack);
			}

			foreach (int fItemTag in dictFeedItem[nID].Children) {
				string strTag = "C";
				if (dictFeedItem[fItemTag].IsFeed) { strTag = "F"; }

				Button buttonItem = CustomControl.GetFeedItemButton(dictFeedItem[fItemTag], strTag + fItemTag, 0);
				buttonItem.Click += buttonFeedItem_Click;
				stackNext.Children.Add(buttonItem);
			}

			scrollFeedlist.ScrollToTop();
			AnimateFeedlist(isBack);
		}

		bool isFeedlistAnimating = false;
		private void AnimateFeedlist(bool isBack) {
			isFeedlistAnimating = true;

			StackPanel stackPrev = isFirstView ? stackFeedlist2 : stackFeedlist1;
			StackPanel stackNext = isFirstView ? stackFeedlist1 : stackFeedlist2;
			Thickness tn = new Thickness(-100, 0, 0, 0);
			
			if (!isBack) {
				tn = new Thickness(widthFeedlist.Width.Value, 0, 0, 0);
				gridFeedlist.Children.Remove(stackNext);
				gridFeedlist.Children.Add(stackNext);
			} else {
				gridFeedlist.Children.Remove(stackNext);
				gridFeedlist.Children.Insert(0, stackNext);
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
			sb.Completed += delegate(object sender, EventArgs e) { isFeedlistAnimating = false; };
			sb.Begin(this);
		}

		private void buttonFeedItem_Click(object sender, RoutedEventArgs e) {
			string strTag = (string)((Button)sender).Tag;
			int nID = Convert.ToInt32(strTag.Substring(1));

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
