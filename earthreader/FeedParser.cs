using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace earthreader {
	public class FeedParser {
		private static int nCount = 0;
		public static List<EntryItem> Parser(string strXML, int FeedID) {
			XmlReader reader = XmlReader.Create(new StringReader(strXML));
			SyndicationFeed feed = SyndicationFeed.Load(reader);

			List<EntryItem> listEntry = new List<EntryItem>();
			string Content;

			foreach (SyndicationItem item in feed.Items) {
				EntryItem ctm = new EntryItem();

				ctm.Title = item.Title.Text.Replace(Environment.NewLine, " ");
				ctm.Title = ctm.Title.Replace((char)10, ' ').Trim();
				ctm.URL = item.Links[0].Uri.OriginalString;
				ctm.Content = item.Summary.Text;
				Content = HtmlRemoval.StripTagsCharArray(ctm.Content);

				ctm.Content = ctm.Content.Replace(Environment.NewLine, ((char)10).ToString());
				ctm.Content = ctm.Content.Replace(((char)10).ToString(), Environment.NewLine);

				ctm.ID = nCount; nCount++;
                

				ctm.Summary = Content.Replace(Environment.NewLine, " ").Trim();
				ctm.Summary = ctm.Summary.Replace((char)10, ' ').Trim();
				if (ctm.Summary.Length > 200) {
					ctm.Summary = ctm.Summary.Substring(200);
				}

				try { ctm.Time = item.PublishDate.DateTime; } catch { ctm.Time = new DateTime(); }
				ctm.Feed = FeedID;

				listEntry.Add(ctm);
			}

			listEntry.Sort(new mysortByValue());
			return listEntry;
		}

		public class mysortByValue : IComparer<EntryItem> {
			public int Compare(EntryItem arg1, EntryItem arg2) {
				if (arg1.Time == arg2.Time) { return 0; }
				return arg1.Time > arg2.Time ? 1 : -1;
			}
		}
	}
}
