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
		public static ObservableCollection<EntryItem> Parser(string strXML, string strCaption) {
			XmlReader reader = XmlReader.Create(new StringReader(strXML));
			SyndicationFeed feed = SyndicationFeed.Load(reader);

			ObservableCollection<EntryItem> listEntry = new ObservableCollection<EntryItem>();

			foreach (SyndicationItem item in feed.Items) {
				EntryItem ctm = new EntryItem();

				ctm.Title = item.Title.Text;
				ctm.URL = item.Links[0].Uri.OriginalString;
				ctm.Content = HtmlRemoval.StripTagsCharArray(item.Summary.Text);
				ctm.Tag = nCount; nCount++;

				ctm.Summary = ctm.Content.Replace(Environment.NewLine, " ").Trim();
				ctm.Summary = ctm.Summary.Replace((char)10, ' ').Trim();
				if (ctm.Summary.Length > 200) {
					ctm.Summary = ctm.Summary.Substring(200);
				}


				try { ctm.Time = item.PublishDate.DateTime.ToString(); } catch { ctm.Time = ""; }
				ctm.Category = strCaption;

				listEntry.Add(ctm);
			}
			return listEntry;
		}
	}
}
