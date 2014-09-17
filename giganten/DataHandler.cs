using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nea;

namespace giganten {
	class DataHandler {
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
				ProcessEntry(reader, 4);
			}
			reader.Close();
		}

		private void ProcessEntry(NeaReader reader, int fieldcount) {
			String[] fields = new String[fieldcount];
			for (int i = 0; i < fieldcount; i++) {
				fields[i] = reader.ReadUntilAny(";,");
				if (reader.Peek() == -1) return;
			}
		}
	}
}
