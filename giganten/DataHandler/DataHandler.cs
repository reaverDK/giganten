using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Nea;

namespace giganten {
	public class DataHandler {
		public bool shouldstop = false;

		private const int fieldcount = 20;
		private const int datefield = 2;
		List<YearInfo> years = new List<YearInfo>();

		public void LoadFile(string file, StartUpWindow startwindow) {
			bool success = true;
			try {
				NeaReader reader = new NeaReader(new StreamReader(file));
				reader.ReadLine();
				while (reader.Peek() != -1 && !shouldstop) {
					ProcessEntry(reader);
				}
				reader.Close();
			}
			catch (Exception e) {
				success = false;
			}
			if(!shouldstop)
				startwindow.Dispatcher.BeginInvoke(new Action(() => { startwindow.FinishedLoading(success); }));
		}

		private void ProcessEntry(NeaReader reader) {
			String[] fields = new String[fieldcount];
			for (int i = 0; i < fieldcount; i++) {
				fields[i] = reader.ReadUntilAny(";");
				if (reader.Peek() == -1) return;
			}
			int month;
			int year;
			try {
				String smonth = fields[datefield].Substring(3, 2);
				String syear = fields[datefield].Substring(6, 2);
				month = int.Parse(smonth) - 1;
				year = int.Parse(syear) + 2000;
				if (month < 4)
					year--;
			}
			catch (Exception e) {
				return;
			}
			YearInfo y = GetYear(year);
			if (y == null) {
				y = new YearInfo(year);
				years.Add(y);
			}
			y.AddEntry(month, fields);
			
		}

		public YearInfo GetYear(int year) {
			foreach (YearInfo y in years) {
				if (y.Year == year)
					return y;
			}
			return null;
		}
	}
}
