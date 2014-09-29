using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace giganten {
	public class MonthInfo {
		List<Salesman> Salesmen = new List<Salesman>();

		public MonthInfo(String[] entry) {
			Salesmen.Add(new Salesman(entry));
		}

		public void AddEntry(String[] entry) {
			Salesman man = GetSalesman(entry[3]);
			if (man != null) {
				man.AddEntry(entry);
			}
			else {
				Salesmen.Add(new Salesman(entry));
			}
		}

		public Salesman GetSalesman(String name) {
			foreach (Salesman sm in Salesmen) {
				if (sm.Name == name)
					return sm;
			}
			return null;
		}

		public String[] GetSalesmen() {
			String[] result = new String[Salesmen.Count];
			for (int i = 0; i < Salesmen.Count; i++) {
				result[i] = Salesmen[i].Name;
			}
			return result;
		}
	}
}
