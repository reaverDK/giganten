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
	}
}
