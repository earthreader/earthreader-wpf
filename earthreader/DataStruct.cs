using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace earthreader {
	public class FeedItem : INotifyPropertyChanged {
		public bool IsFeed;
		public string URL;
		public int ID, ParentID;
		public double ScrollOffset = 0;
		public List<int> Children;
		public BitmapImage Favicon;

		public List<EntryItem> Contents;

		private string _caption;
		public string Caption {
			get { return _caption; }
			set {
				_caption = value;
				OnPropertyChanged("Caption");
			}
		}

		private int _count;
		public int Count {
			get { return _count; }
			set {
				_count = value;
				if (_count == 0) {
					_opacity = 0;
				} else { Opacity = 1; }

				if (_count > 999) {
					_noticountstring = "999+";
				} else { _noticountstring = _count.ToString(); }
				OnPropertyChanged("Count");
			}
		}

		private double _opacity;
		public double Opacity {
			get { return _opacity; }
			set { _opacity = value; }
		}

		private string _noticountstring;
		public string NotiCount {
			get { return _noticountstring; }
			set {
				_noticountstring = value;
				OnPropertyChanged("NotiCount");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	/*
	public class EntryItem : INotifyPropertyChanged, IComparable {

		public EntryItem() { }

		public int CompareTo(object obj) {
			EntryItem ei = obj as EntryItem;
			if (ei == null) {
				throw new ArgumentException("NULL");
			}
			return this.Time.CompareTo(ei.Time);
		}

		private string sTitle;
		public string Title {
			get { return sTitle; }
			set {
				sTitle = value;
				OnPropertyChanged("Title");
			}
		}

		private int sTag;
		public int Tag {
			get { return sTag; }
			set {
				sTag = value;
				OnPropertyChanged("Tag");
			}
		}

        private int sTagPrev;
        public int TagPrev
        {
            get { return sTagPrev; }
            set
            {
                sTagPrev = value;
                OnPropertyChanged("TagPrev");
            }
        }

        private int sTagNext;
        public int TagNext
        {
            get { return sTagNext; }
            set
            {
                sTagNext = value;
                OnPropertyChanged("TagNext");
            }
        }

		private string sTime;
		public string Time {
			get { return sTime; }
			set {
				sTime = value;
				OnPropertyChanged("Time");
			}
		}

		private string sContentView;
		public string ContentView {
			get { return sContentView; }
			set {
				sContentView = value;
				OnPropertyChanged("ContentView");
			}
		}

		private string sContent;
		public string Content {
			get { return sContent; }
			set {
				sContent = value;
				OnPropertyChanged("Content");
			}
		}

		private string sSummary;
		public string Summary {
			get { return sSummary; }
			set {
				sSummary = value;
				OnPropertyChanged("Summary");
			}
		}

		private string sURL;
		public string URL {
			get { return sURL; }
			set {
				sURL = value;
				OnPropertyChanged("URL");
			}
		}


		public object ShallowCopy() {
			return this.MemberwiseClone();
		}

		private string sCategory;
		public string Category {
			get { return sCategory; }
			set {
				sCategory = value;
				OnPropertyChanged("Category");
			}
		}

		private Visibility sContentVisibility = Visibility.Collapsed;
		public Visibility ContentVisibility {
			get { return sContentVisibility; }
			set {
				sContentVisibility = value;
				OnPropertyChanged("ContentVisibility");
			}
		}

		private Visibility sSummaryVisibility = Visibility.Visible;
		public Visibility SummaryVisibility {
			get { return sSummaryVisibility; }
			set {
				sSummaryVisibility = value;
				OnPropertyChanged("SummaryVisibility");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	 */ 
}
