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
					textboxTitle.Text = FeedDictionary[nDialogID].Title;
					textboxTitle.Tag = FeedDictionary[nDialogID].Title;

					textboxTitle.Visibility = Visibility.Visible;
					textDialogMessage.Visibility = Visibility.Collapsed;

					textboxTitle.Focus();
					break;

				case 'D':
					// Delete
					textFormType.Text = "Question";
					textDialogMessage.Text = string.Format("{0} 지울거임", FeedDictionary[nDialogID].Title);

					if (!FeedDictionary[nDialogID].IsFeed) {
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

					FeedDictionary[nDialogID].Title = textboxTitle.Text;

					break;

				case 'D':
					// Delete

					int nOffset = 1;
					if (NowCategoryViewID != 0) { nOffset = 2; }

					int nIndex = FeedDictionary[NowCategoryViewID].Children.IndexOf(nDialogID);
					stackNow.Children.RemoveAt(nIndex + nOffset);

					FeedDictionary[NowCategoryViewID].Children.Remove(nDialogID);

					Queue<int> q = new Queue<int>();
					q.Enqueue(nDialogID);

					int nVertex;
					for (; q.Count > 0; ) {
						nVertex = q.Dequeue();
						if (!FeedDictionary[nVertex].IsFeed) {
							foreach (int i in FeedDictionary[nVertex].Children) {
								q.Enqueue(i);
							}
						}

						FeedDictionary.Remove(nVertex);
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
					RefreshEntry(NowCategoryViewID);
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
					RefreshEntry(nID);
					break;
			}

			//MessageBox.Show(nID.ToString() + "\n" + widthFeedlist.Width);
		}
	}
}
