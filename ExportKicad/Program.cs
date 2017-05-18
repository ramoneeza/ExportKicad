using Rop.Kicad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportKicad
{
	class Program
	{
		static int Main(string[] args)
		{
			var a = args[0];
			var d = new Rop.Kicad.PcbFileParser();
			var r = d.Parse(a);
			var c = new CorelDrawPaint();
			c.DrawPcb(r);
			return 0;
		}
	}
}
