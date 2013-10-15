using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace earthreader {
	public class FileClass {
		static string ffFeedlist = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\RSSData\Feedlist.txt";
		static string ffFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\RSSData\";

		public static void SaveFeedlist(Dictionary<int, FeedItem> dictFeedItem) {
			if (!Directory.Exists(ffFolder)) {
				Directory.CreateDirectory(ffFolder);
			}

			string strAllData = "", strLineData;
			int nID = 0, nCount = 0;
			Dictionary<int, int> dictSaveIndex = new Dictionary<int, int>();
			Queue<int> q = new Queue<int>();
			q.Enqueue(0);

			string strFaviconUri;
			int nParentID;

			for (; q.Count > 0; ) {
				nID = q.Dequeue();

				if (nID == 0) {
					strFaviconUri = "iconAll.png";
				} else if (dictFeedItem[nID].IsFeed) {
					strFaviconUri = "iconFeed.png";
				} else {
					strFaviconUri = "iconCategory.png";
				}

				nParentID = dictSaveIndex[dictFeedItem[nID].ParentID];
				dictSaveIndex.Add(nID, nCount);
				nCount++;

				strLineData = string.Format("{0}#DataSplit#{1}#DataSplit#{2}#DataSplit#{3}#DataSplit#{4}#DataSplit#{5}", dictFeedItem[nID].Caption, dictFeedItem[nID].URL, dictFeedItem[nID].IsFeed, nParentID, strFaviconUri);
				strAllData += strLineData + "\n";

				if (dictFeedItem[nID].IsFeed) { continue; }

				foreach (int nChildID in dictFeedItem[nID].Children) {
					q.Enqueue(nChildID);
				}
			}

			dictSaveIndex.Clear();
		}
	}
}
