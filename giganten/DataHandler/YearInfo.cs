using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace giganten {
	public class YearInfo {
		public readonly int Year;
		public MonthInfo this[int index] {
			get { return months[index]; }
		}

		MonthInfo[] months = new MonthInfo[12];

		public YearInfo(int year) {
			Year = year;
		}

		public void AddEntry(int month, String[] entry) {
			if (months[month] == null)
				months[month] = new MonthInfo(entry);
			else
				months[month].AddEntry(entry);
		}

		public String[] GetSalesmen() {
			List<String> result = new List<string>();
			foreach (MonthInfo mi in months) {
				if(mi!=null)
				foreach (String sm in mi.GetSalesmen()) {
					if (!result.Contains(sm))
						result.Add(sm);
				}
			}
			return result.ToArray();
		}
	}
}
