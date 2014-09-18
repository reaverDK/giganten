using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nea;

namespace giganten {
	class DataHandler {
		private const int fieldcount = 20;
		private const int datefield = 2;
		List<String> files = new List<string>();
		List<YearInfo> years = new List<YearInfo>();

		public bool AddFile(string file) {
			try {
				ProcessFile(file);
				files.Add(file);
			}
			catch (Exception e) {
				return false;
			}
			return true;
		}

		private void ProcessFile(string file) {
			NeaReader reader = new NeaReader(file);
			while (reader.Peek() != -1) {
				ProcessEntry(reader);
			}
			reader.Close();
		}

		private void ProcessEntry(NeaReader reader) {
			String[] fields = new String[fieldcount];
			for (int i = 0; i < fieldcount; i++) {
				fields[i] = reader.ReadUntilAny(";,");
				if (reader.Peek() == -1) return;
			}
			int month;
			int year;
			try {
				String smonth = fields[datefield].Substring(3, 2);
				String syear = fields[datefield].Substring(6, 2);
				month = int.Parse(smonth);
				year = int.Parse(syear);
			}
			catch (Exception e) {
				
			}
		}
	}
}
