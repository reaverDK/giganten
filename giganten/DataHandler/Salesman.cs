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
		public Salesman(String[] entry) {
			Name = entry[3];
			AddEntry(entry);
		}

		public void AddEntry(String[] entry) {

		}
	}
}
