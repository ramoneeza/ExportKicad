using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.IO;
using System.Windows;

namespace Rop.Kicad
{
	public struct Rectangle
	{
		public Point Location;
		public Size Size;
		public Rectangle(double x,double y,double w,double h):this(new Point(x,y),new Size(w,h))
		{
		}
		public Rectangle(Point location,Size size)
		{
			Location = location;
			Size = size;
		}
	}
	public struct Size
	{
		public double Width;
		public double Height;
		public Size(double w,double h)
		{
			Width = w;
			Height = h;
		}
	}
	public struct At
	{
		public double X;
		public double Y;
		public double R;
		public At(double x,double y,double r)
		{
			X = x;
			Y = y;
			R = r;
		}
		public At(double x, double y):this(x,y,0)
		{
		}
		public At(double[] v)
		{
			switch (v.Length)
			{
				case 0:
					X = 0;Y = 0;R = 0;break;
				case 1:
					X = v[0];Y = 0;R = 0;break;
				case 2:
					X = v[0]; Y = v[1]; R = 0; break;
				default:
					X = v[0]; Y = v[1]; R = v[2]; break;
			}
		}
		public Point Point() => new Point(X, Y);
		public Double Radianes => Math.PI*R / 180.0;
	}

    public struct Drill
    {
        public double Size;
        public Point Offset;
        public override string ToString()
        {
            if ((Offset.X == 0) && (Offset.Y == 0))
                return $"{(int)Size * 1000}";
            else
                return $"{(int)Size * 1000}_{(int)Offset.X * 1000}_{(int)Offset.Y * 1000}";
        }
        public Drill(double s,Point off)
        {
            Size = s;
            Offset = off;
        }
        public static implicit operator double(Drill d) => d.Size;
    }

    public class PcbFile
	{
		public string Version { get; set; }
		public string Host { get;  set; }
		public GeneralSection General { get; set; }
		public Dictionary<string,LayerSection> Layers { get; set; } 

		public SetupSection Setup { get; set; }

		public NetSection[] Nets { get; set; }
		public Dictionary<string,NetSection> NetsStr { get; set; }
		public Dictionary<string,Net_Class> NetsClass { get; set; }
		public Module[] Modules { get; set; }
		public IDwgElement[] Dibujo { get; set; }

		public PcbFile(List l)
		{
			Version =Convert.ToString(l.FindValue("version"));
			Host = Convert.ToString(l.FindValue("host"));
			General = l.FindCVValue<GeneralSection>();
			Layers = LayersSection.GlobalLayers;
			Setup = l.FindCVValue<SetupSection>();
			Nets = l.FindCVAllValues<NetSection>().ToArray();
			Modules = l.FindCVAllValues<Module>().ToArray();
			Dibujo = l.FindCVAllValues<IDwgElement>().ToArray();
		}

		// CLASES
		public class GeneralSection
		{
			public int Links;
			public int No_Connects;
			public Rectangle Area;
			public double Thickness;
			public int Drawings;
			public int Tracks;
			public int Zones;
			public int Modules;
			public int Nets;
		}
		public class LayerSection
		{
			public int Number;
			public string Name;
			public string Type;
		}
		public class SetupSection
		{
			public double last_trace_width;
			public double trace_clearance;
			public double zone_clearance;
			public bool zone_45_only;
			public double trace_min;
			public double segment_width;
			public double edge_width;
			public double via_size;
			public double via_drill;
			public double via_min_size;
			public double via_min_drill;
			public double uvia_size;
			public double uvia_drill;
			public bool uvias_allowed;
			public double uvia_min_size;
			public double uvia_min_drill;
			public double pcb_text_width;
			public Size pcb_text_size;
			public double mod_edge_width;
			public Size mod_text_size;
			public double mod_text_width;
			public Size pad_size;

			public double pad_drill;
			public double pad_to_mask_clearance;
			public Point aux_axis_origin;
			public Point grid_origin;
			public string visible_elements;
			public PcbPlotParamsSection PcbPlotParams;
		}
		public class PcbPlotParamsSection {
			public string layerselection;
			public bool usegerberextensions;
			public bool excludeedgelayer;
			public double linewidth;
			public bool plotframeref;
			public bool viasonmask;
			public int mode;
			public bool useauxorigin;
			public int hpglpennumber;
			public int hpglpenspeed;
			public int hpglpendiameter;
			public int hpglpenoverlay;
			public bool psnegative;
			public bool psa4output;
			public bool plotreference;
			public bool plotvalue;
			public bool plotinvisibletext;
			public bool padsonsilk;
			public bool subtractmaskfromsilk;
			public int outputformat;
			public bool mirror;
			public int drillshape;
			public double scaleselection;
			public string outputdirectory;
		}
		public class NetSection:ISimplify
		{
			public int number;
			public string name;
			public object Simplify(Type t)
			{
				return Convert.ChangeType(number, t);
			}
		}
		public class Net_Class
		{
			public string Name;
			public string Description;
			public double clearance;
			public double trace_width;
			public double via_dia;
			public double via_drill;
			public double uvia_dia;
			public double uvia_drill;
			public List<string> Nets;
    	}
		public class Module
		{
			public string Name;
			public string Layer;
			public long Tedit;
			public long Tstamp;
			public At At;
			public string Descr;
			public string Tags;
			public string path;
			public string attr;
			public List<IDwgElement> Drawing;
		}
		public class LayersSection
		{
			public static Dictionary<string,LayerSection> GlobalLayers { get; } = new Dictionary<string, LayerSection>(StringComparer.InvariantCultureIgnoreCase);
			public static IEnumerable<LayerSection> FindLayer(string mask)
			{
				var p = mask.IndexOf('*');
				if (p==-1) return GlobalLayers.Values.Where(g => g.Name.Equals(mask, StringComparison.InvariantCultureIgnoreCase));
				mask = mask.Substring(p + 1);
				return GlobalLayers.Values.Where(g => g.Name.EndsWith(mask, StringComparison.InvariantCultureIgnoreCase));
			}
			public List<LayerSection> Layers { get; set; }
		}

		// DIBUJO
		public class Segment : IDwgElement
		{
			public Point Start;
			public Point End;
			public double Width;
			public string layer;
			public string[] DwgLayers => new[] { layer };
			public int net;
			public long status;
			public long tstamp;
			public double DistanceStart(Point pto) => PointHelper.DistanceSqr(pto, Start);
			public double DistanceEnd(Point pto) => PointHelper.DistanceSqr(pto, End);
			public double Distance(Point pto)=>Math.Min(DistanceStart(pto), DistanceEnd(pto));
			public void Swap()
			{
				var p = Start;
				Start = End;
				End = Start;
			}
			public static Segment Closest(Point pto,IEnumerable<Segment> segments)
			{
				var mindistance = double.MaxValue;
				Segment res = null;
				foreach(var s in segments)
				{
					if (s.Distance(pto) >= mindistance) continue;
					if (s.DistanceEnd(pto) < s.DistanceStart(pto)) s.Swap();
					res = s;
				}
				return res;
			}
		}
		public class Gr_Line:IDwgElement
		{
			public Point Start;
			public Point End;
			public double angle;
			public double Width;
			public string layer;
			public string[] DwgLayers => new[] { layer };
		}
		
		public class Fp_Line : IDwgElement
		{
			public Point Start;
			public Point End;
			public string Layer;
			public string[] DwgLayers => new[] { Layer };
			public double Width;
		}
		public class Fp_Pad : IDwgElement
		{
			public int Number;
			public string Type;
			public string Shape;
			public At At;
			public LayersSection Layers;
			public string[] DwgLayers => Layers.Layers.Select(l => l.Name).ToArray();
			public Drill Drill;
			public Size Size;
			public int Net;
			public string Key => $"P_{Shape}_{(int)(Size.Width*1000)}_{(int)(Size.Height*1000)}_{Drill}";
		}
		public class Via : IDwgElement
		{
			public At At;
			public LayersSection Layers;
			public string[] DwgLayers => Layers.Layers.Select(l => l.Name).ToArray();
			public Double Drill;
			public Size Size;
			public int Net;
		}
		public class Fp_Circle : IDwgElement
		{
			public Point Center;
			public Point End;
			public string Layer;
			public string[] DwgLayers => new[] { Layer };
			public double Width;
		}

		public class Effects
		{
			public Font Font;
			public string Justify;
		}
		public class Font
		{
			public Size Size;
			public double Thickness;
		}
        
        //(fp_text reference REF** (at 0 -3.5) (layer F.SilkS) hide	(effects (font (size 1 1) (thickness 0.15)))   )
        public class Fp_Text : IDwgElement
		{
			public At At;
			public string Layer;
			public bool _Hide;
			public string Type;
			public string Text;
			public Effects Effects;
			public string[] DwgLayers => new[] { Layer };

		}

		public List<IDwgElement> Pcb;

	}

	public class PcbFileTokens
	{
		[FactoryName("area")]
		public class RectangleToken:ComplexToken<Rectangle>
		{
			public RectangleToken(List l) : base(l.Name)
			{
				var v = l.ValuesDouble;
				Value = new Rectangle(v[0], v[1], v[2], v[3]);
			}
		}
		[FactoryName("start", "center","end","aux_axis_origin","grid_origin","offset")]
		public class PointToken : ComplexToken<Point>
		{
			public PointToken(List l) : base(l.Name)
			{
				var vs = l.Values;
				if (vs[0] is xyz d)
				{
					Value = d.Point;
				}
				else
				{
					var v = l.ValuesDouble;
					Value = new Point(v[0], v[1]);
				}
			}
		}
		[FactoryName("at")]
		public class AtToken : ComplexToken<At>
		{
			public AtToken(List l) : base(l.Name)
			{
				var vs = l.Values;
				if (vs[0] is xyz d)
				{
					Value =new At(d.X,d.Y);
				}
				else
				{
					Value = new At(l.ValuesDouble);
				}
			}
		}
		[FactoryName("tedit","tstamp", "status")]
		public class HexToken : ComplexToken<long>
		{
			public HexToken(List l) : base(l.Name)
			{
				var vs = l.ValuesStr;
				var s=vs[0]??"";
				try
				{
					Value = long.Parse(s, System.Globalization.NumberStyles.HexNumber);
				}
				catch
				{
					Value = 0;
				}
			}
		}

		[FactoryName("size","pcb_text_size", "mod_text_size", "pad_size")]
		public class SizeToken : ComplexToken<Size>
		{
			public SizeToken(List l) : base(l.Name)
			{
				var vs = l.Values;
				if (vs[0] is xyz d)
				{
					Value = d.Size;
				}
				else
				{
					var v = l.ValuesDouble;
					if (v.Length == 2)
						Value = new Size(v[0], v[1]);
					else
						Value = new Size(v[0], v[0]);
				}
			}
		}
		public struct xyz
		{
			public double X;
			public double Y;
			public double Z;
			public Point Point => new Point(X, Y);
			public Size Size => new Size(X, Y);
			public xyz(double x,double y,double z)
			{
				X = x;
				Y = y;
				Z = z;
			}
		}

		[FactoryName("xyz")]
		public class Xyztoken : ComplexToken<xyz>
		{
			public Xyztoken(List l) : base(l.Name)
			{
				var v = l.ValuesDouble;
				Value = new xyz(v[0], v[1], v[2]);
			}
		}
        
        [FactoryName("drill")]
        public class Drilltoken : ComplexToken<Drill>
        {
            public Drilltoken(List l) : base(l.Name)
            {
                var v = l.Values;
                var o = new Point();
                if (v.Length > 1) o = (Point)v[1];
                Value = new Drill(Convert.ToDouble(v[0]), o);
            }
        }

        [FactoryName("general")]
		public class GeneralToken : ClassToken<PcbFile.GeneralSection>{	public GeneralToken(List l) : base(l) { }}

		[FactoryName("setup")]
		public class SetupSection : ClassToken<PcbFile.SetupSection>{public SetupSection(List l) : base(l) { }}

		[FactoryName("PcbPlotParams")]
		public class PcbPlotParamsSection : ClassToken<PcbFile.PcbPlotParamsSection>{public PcbPlotParamsSection(List l) : base(l){}}

		[FactoryName("Net")]
		public class Net : ClassToken<PcbFile.NetSection>
		{
			public Net(List l) : base(l)
			{
				var tokens = l.Values;
				this.Value.number = (int)(tokens[0]);
				this.Value.name = (tokens.Length>1)?(string)(tokens[1]):null;
			}
		}
        

        [FactoryName("Net_class")]
		public class Net_Class : ClassToken<PcbFile.Net_Class>
		{
			public Net_Class(List l):base(l)
			{
				this.Value.Name = (string)l.GetNth(0).ObjectValue;
				this.Value.Description = (string)l.GetNth(1).ObjectValue;
				var adds = l.FindAllList("add_net");
				this.Value.Nets = new List<string>();
				foreach(var a in adds)
				{
					this.Value.Nets.Add((string)a.Values[0]);
				}
			}
		}
		[FactoryName("Module")]
		public class Module_Class : ClassToken<PcbFile.Module>
		{
			public Module_Class(List l):base(l)
			{
				this.Value.Name = (string)l.GetNth(1).ObjectValue;
				var adds = l.FindCVAllValues<IDwgElement>();
				this.Value.Drawing = adds.ToList();
			}
		}
		[FactoryName("Layers")]
		public class LayersSection : ClassToken<PcbFile.LayersSection>
		{
			public LayersSection(List l):base(l)
			{
				this.Value.Layers = new List<PcbFile.LayerSection>();
				foreach(var e in l.Tokens)
				{
					if (e is List ll)
					{
						var v = ll.Values;
						var ls = new PcbFile.LayerSection() { Number = (int)v[0], Name = (string)v[1], Type = (string)v[2] };
						if (!PcbFile.LayersSection.GlobalLayers.ContainsKey(ls.Name))
							PcbFile.LayersSection.GlobalLayers.Add(ls.Name,ls);
					}
					else
					{
						var n = e.ObjectValue as string;
						var ls = PcbFile.LayersSection.FindLayer(n);
						this.Value.Layers.AddRange(ls);
					}
				}

			}
		}

		[FactoryName("effects")]
		public class Effects : ClassToken<PcbFile.Effects>{	public Effects(List l):base(l){	}}
		
		[FactoryName("font")]
		public class Font : ClassToken<PcbFile.Font>{public Font(List l):base(l){}}
		
		// DIBUJO

		[FactoryName("segment")]
		public class Segment : ClassToken<PcbFile.Segment>{	public Segment(List l) : base(l){}}
		[FactoryName("gr_line")]
		public class Gr_Line : ClassToken<PcbFile.Gr_Line>{	public Gr_Line(List l) : base(l){}}

		[FactoryName("fp_line")]
		public class Fp_Line : ClassToken<PcbFile.Fp_Line>{	public Fp_Line(List l) : base(l){}}

		[FactoryName("fp_pad")]
		public class Fp_Pad : ClassToken<PcbFile.Fp_Pad>{public Fp_Pad(List l) : base(l){}	}

		[FactoryName("via")]
		public class Via: ClassToken<PcbFile.Via>{	public Via(List l) : base(l){}}

		[FactoryName("fp_circle")]
		public class Fp_Circle : ClassToken<PcbFile.Fp_Circle>{	public Fp_Circle(List l) : base(l){	}}

		[FactoryName("pad")]
		public class Pad : ClassToken<PcbFile.Fp_Pad>{
			public Pad(List l) : base(l)
			{
				var vs=l.Values;
				this.Value.Number = Convert.ToInt32(vs[0]);
				this.Value.Type = Convert.ToString(vs[1]);
				this.Value.Shape = Convert.ToString(vs[2]);
			}
		}
		[FactoryName("fp_text")]
		public class Fp_Text : ClassToken<PcbFile.Fp_Text>
		{
			public Fp_Text(List l):base(l){
				this.Value._Hide = l.ValuesStr.Contains("hide");
				this.Value.Type = l.ValuesStr[0];
				this.Value.Text = l.ValuesStr[1];
			}
		}
	}

	public class PcbFileParser
	{
		public PcbFileParser()
		{
		}
		private List _Parse(string file)
		{
			ListParser parser = new ListParser();
			return parser.Decode(new StreamReader(file));
		}
		public PcbFile Parse(string file)
		{
			List l = _Parse(file);
			if (l.Name != "kicad_pcb") return null;
			var p = new PcbFile(l);
			return p;
		}
		static PcbFileParser()
		{
			ComplexValue.Register<PcbFileTokens>();
		}
	}
}
