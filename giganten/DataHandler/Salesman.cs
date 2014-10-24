using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//0: Afd
//1: BilagsNr
//2: Dato
//3: SK
//4: Varenummer
//5: Beskrivelse
//6: Antal
//7: Salgspris
//8: KostPris
//9: Bto.%
//10: Bto.kr.
//11: Rab%
//12: KGM
//13: M‘rke
//14: MVA%
//15: Kundenummer
//16: Kundetype
//17: Uge
//18: EksternNr
//19: Serienr.

namespace giganten {
	public class Salesman {
		public readonly String Name;
		public readonly Dictionary<string, int> KGMs = new Dictionary<string, int>();
		public int TotalSales { get; private set; }
		public double Omsaetning { get; private set; }
		public double Indtjening { get; private set; }

		public Salesman(String[] entry) {
			Name = entry[3];
			Omsaetning = 0;
			Indtjening = 0;
			AddEntry(entry);
		}

		public void AddEntry(String[] entry) {
			double antal = 0;
			double salg = 0;
			double kost = 0;
			double.TryParse(entry[6], out antal);
			if (antal >= 0) {
				double.TryParse(entry[7], out salg);
				double.TryParse(entry[8], out kost);

				Omsaetning += antal * (Math.Abs(salg) + Math.Abs(kost));
				Indtjening += antal * (salg - kost);

				string kgm = entry[12];
				if (KGMs.ContainsKey(kgm)) {
					KGMs[kgm]++;
				}
				else {
					KGMs.Add(kgm, 1);
				}
				TotalSales++;
			}
		}

		public double PercentOfTotal(String[] list) {
			int count = 0;
			foreach (KeyValuePair<String,int> kgm in KGMs) {
				foreach (String comp in list) {
					if (comp == kgm.Key) {
						count += kgm.Value;
						break;
					}
				}
			}
			return ((double)count)/(double)TotalSales;
		}
	}
}
