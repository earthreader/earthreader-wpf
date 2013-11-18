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

			nFromIndex = FeedDictionary[NowCategoryViewID].Children.IndexOf(nMouseDownID);

			//textTemp.Text = nMouseDownID.ToString();
		}

		// Moving event : MouseMove

		Point pointMouseMove;
		private void Feedlist_OrderChange(object sender, MouseEventArgs e) {
			if (!isMouseDown || nMouseDownID < 0 || isDialogMode) { return; }
			pointMouseMove = e.GetPosition(this);

			if (Math.Max(Math.Abs(pointMouseDown.X - pointMouseMove.X), Math.Abs(pointMouseDown.Y - pointMouseMove.Y)) >= 10 && !isMoving) {
				nMouseMovingID = nMouseDownID;
				isMoving = true;

				textNowMoving.Text = FeedDictionary[nMouseDownID].Title;
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
				if (NowCategoryViewID == 0) { dFixedHeight = 60; }

				double pointRelate = pointAbsolute - dFixedHeight;

				int nHoverIndex = (int)pointRelate / 40;
				nHoverIndex = Math.Max(0, nHoverIndex);
				nHoverIndex = Math.Min(FeedDictionary[NowCategoryViewID].Children.Count - 1, nHoverIndex);

				int nDivideHeight = 10;
				if (FeedDictionary[FeedDictionary[NowCategoryViewID].Children[nHoverIndex]].IsFeed) { nDivideHeight = 20; }

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
					if (FeedDictionary[FeedDictionary[NowCategoryViewID].Children[nHoverIndex]].IsFeed) { return; }
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
			int TagSelected = FeedDictionary[NowCategoryViewID].Children[nFromIndex];
			int TagToward = FeedDictionary[NowCategoryViewID].Children[(nToIndex - 1) / 3];

			int nOffset = 1;
			if (NowCategoryViewID != 0) { nOffset = 2; }

			if (nToIndex == 0) {
				// send to parent

				FeedDictionary[FeedDictionary[NowCategoryViewID].ParentID].Children.Add(TagSelected);
				FeedDictionary[TagSelected].ParentID = FeedDictionary[NowCategoryViewID].ParentID;

				FeedDictionary[NowCategoryViewID].Children.Remove(TagSelected);
				stackNow.Children.RemoveAt(nFromIndex + nOffset);

				RefreshEntry(NowCategoryViewID);

			} else if (nToIndex % 3 == 2) {
				// put to other category

				// Itself
				if (TagSelected == TagToward) { return; }

				FeedDictionary[TagToward].Children.Add(TagSelected);
				FeedDictionary[TagSelected].ParentID = TagToward;

				FeedDictionary[NowCategoryViewID].Children.Remove(TagSelected);
				stackNow.Children.RemoveAt(nFromIndex + nOffset);

			} else {
				int nAbsToIndex = (nToIndex - 1) / 3;
				if (nFromIndex == nAbsToIndex) { return; }

				if (nToIndex % 3 == 0) { nAbsToIndex++; }

				int nBeforeOffset = 0;
				if (nFromIndex < nAbsToIndex) { nBeforeOffset = 1; }

				FeedDictionary[NowCategoryViewID].Children.RemoveAt(nFromIndex);
				FeedDictionary[NowCategoryViewID].Children.Insert(nAbsToIndex - nBeforeOffset, TagSelected);

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
	}
}
