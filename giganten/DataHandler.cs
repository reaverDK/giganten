using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nea;

namespace giganten {
	class DataHandler {
		List<String> files = new List<string>();

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
			NeaReader r = new NeaReader(file);
			String text = r.ReadUntilAny(";,");
		}
	}
}
