using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace giganten {
	class YearInfo {
		public readonly int Year;
		public MonthInfo this[int index] {
			get { return months[index]; }
		}

		MonthInfo[] months = new MonthInfo[12];

		public YearInfo(int year) {
			Year = year;
		}

		public void AddMonth(MonthInfo month) {
			months[month.Index] = month;
		}
	}
}
