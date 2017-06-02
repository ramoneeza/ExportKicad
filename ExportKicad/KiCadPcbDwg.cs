using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorelDRAW;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;

namespace Rop.Kicad
{
	public interface IDwgElement
	{
		
		string[] DwgLayers { get;}
	}
	public static class PointHelper
	{
		public static double DistanceSqr(Point p1,Point p2)
		{
			var dx = p1.X - p2.X;
			var dy = p1.Y - p2.Y;
			return (dx * dx) + (dy * dy);
		}
		public static double Distance(Point p1, Point p2) => Math.Sqrt(DistanceSqr(p1, p2));
		public static bool Equals(Point p1,Point p2)
		{
			var d = DistanceSqr(p1, p2);
			return d < 0.001;
		}
		public static bool EqualsAny(params (Point,Point)[] args)
		{
			return args.Any(a => Equals(a.Item1, a.Item2));
		}
		public static bool EqualsAll(params (Point, Point)[] args)
		{
			return args.All(a => Equals(a.Item1, a.Item2));
		}
		public static bool EqualsNone(params (Point, Point)[] args)
		{
			return !EqualsAny(args);
		}
	}
	public class CorelDrawPaint
	{
		private static Dictionary<string, System.Windows.Media.Color> layercolors = new Dictionary<string, System.Windows.Media.Color>(StringComparer.CurrentCultureIgnoreCase)
		{
			["F.Cu"] = System.Windows.Media.Colors.Red,
			["In1.Cu"] = System.Windows.Media.Colors.Yellow,
			["In2.Cu"] = System.Windows.Media.Colors.Magenta,
			["In3.Cu"] = System.Windows.Media.Colors.Red,
			["In4.Cu"] = System.Windows.Media.Colors.DarkCyan,
			["In5.Cu"] = System.Windows.Media.Colors.Green,
			["In6.Cu"] = System.Windows.Media.Colors.Blue,
			["In7.Cu"] = System.Windows.Media.Colors.Gray,
			["In8.Cu"] = System.Windows.Media.Colors.Magenta,
			["In9.Cu"] = System.Windows.Media.Colors.Gray,
			["In10.Cu"] = System.Windows.Media.Colors.Magenta,
			["In11.Cu"] = System.Windows.Media.Colors.Red,
			["In12.Cu"] = System.Windows.Media.Colors.Brown,
			["In13.Cu"] = System.Windows.Media.Colors.Gray,
			["In14.Cu"] = System.Windows.Media.Colors.Blue,
			["In15.Cu"] = System.Windows.Media.Colors.Green,
			["In16.Cu"] = System.Windows.Media.Colors.Red,
			["In17.Cu"] = System.Windows.Media.Colors.Yellow,
			["In18.Cu"] = System.Windows.Media.Colors.Magenta,
			["In19.Cu"] = System.Windows.Media.Colors.Red,
			["In20.Cu"] = System.Windows.Media.Colors.DarkCyan,
			["In21.Cu"] = System.Windows.Media.Colors.Green,
			["In22.Cu"] = System.Windows.Media.Colors.Blue,
			["In23.Cu"] = System.Windows.Media.Colors.Gray,
			["In24.Cu"] = System.Windows.Media.Colors.Magenta,
			["In25.Cu"] = System.Windows.Media.Colors.Gray,
			["In26.Cu"] = System.Windows.Media.Colors.Magenta,
			["In27.Cu"] = System.Windows.Media.Colors.Red,
			["In28.Cu"] = System.Windows.Media.Colors.Brown,
			["In29.Cu"] = System.Windows.Media.Colors.Gray,
			["In30.Cu"] = System.Windows.Media.Colors.Blue,
			["B.Cu"] = System.Windows.Media.Colors.Green,
			["B.Adhes"] = System.Windows.Media.Colors.Blue,
			["F.Adhes"] = System.Windows.Media.Colors.Magenta,
			["B.Paste"] = System.Windows.Media.Colors.DarkCyan,
			["F.Paste"] = System.Windows.Media.Colors.Red,
			["B.SilkS"] = System.Windows.Media.Colors.Magenta,
			["F.SilkS"] = System.Windows.Media.Colors.DarkCyan,
			["B.Mask"] = System.Windows.Media.Colors.Brown,
			["F.Mask"] = System.Windows.Media.Colors.Magenta,
			["Dwgs.User"] = System.Windows.Media.Colors.Gray,
			["Cmts.User"] = System.Windows.Media.Colors.Blue,
			["Eco1.User"] = System.Windows.Media.Colors.Green,
			["Eco2.User"] = System.Windows.Media.Colors.Yellow,
			["Edge.Cuts"] = System.Windows.Media.Colors.Yellow,
			["Margin"] = System.Windows.Media.Colors.Magenta,
			["B.CrtYd"] = System.Windows.Media.Colors.Yellow,
			["F.CrtYd"] = System.Windows.Media.Colors.Gray,
			["B.Fab"] = System.Windows.Media.Colors.Red,
			["F.Fab"] = System.Windows.Media.Colors.Yellow,
			["F.PAD"]=System.Windows.Media.Colors.Black,
			["B.PAD"] = System.Windows.Media.Colors.Black,
			["PAD"] = System.Windows.Media.Colors.Black
		};
		public struct LayerData
		{
			public CorelDRAW.Page Page { get; private set; }
			public string Name;
			private Layer _layer;
			public Layer Layer
			{
				get
				{
					if ((_layer == null)||(_layer.Name!=Name)) CreateLayer();
					return _layer;
				}
			}
			public CorelDRAW.Color Color { get; set; }
			public void SetColor(System.Windows.Media.Color color)
			{
				Color.RGBAssign(color.R, color.G, color.B);
			}
			private void CreateLayer()
			{
				_layer = Page.Layers.Find(Name);
				if (_layer==null)
				_layer = Page.CreateLayer(Name);
			}
			public LayerData(CorelDRAW.Page page,string name)
			{
				Page = page;
				Name = name;
				_layer = null;
				Color = page.Application.CreateColor();
				CreateLayer();
				SetColor(CorelDrawPaint.layercolors[name]);
			}
		}
		public Dictionary<string, LayerData> LayerColorsCD = new Dictionary<string, LayerData>();
		public LayerData GetLayerData(string name)
		{
			if (LayerColorsCD.ContainsKey(name)) return LayerColorsCD[name];
			var nc = new LayerData(Page, name);
			LayerColorsCD[name] = nc;
			return nc;
		}
		public class DwgSegment
		{
			public string Layer { get;}
			public int Net { get; }
			public double Width { get; }
			public List<List<Point>> Segments { get; } = new List<List<Point>>();
			public (string, int, double) Key => (Layer, Net, Width);
			public DwgSegment((string, int, double) k)
			{
				Layer = k.Item1;
				Net = k.Item2;
				Width = k.Item3;
			}
			public int Reduce()
			{
				var count = 0;
				var fe = new List<List<Point>>(Segments);
				Segments.Clear();
				void NewPath(List<Point> path)
				{
					Segments.Add(path);
					fe.Remove(path);
				}
				(Point,Point) Extremes(List<Point> path)
				{
					return (path[0],path[path.Count - 1]);
				}
				bool TryAppend(List<Point> path,List<Point> candidate)
				{
					var pathext = Extremes(path);
					var candext = Extremes(candidate);
					if (PointHelper.EqualsNone((pathext.Item1,candext.Item1),(pathext.Item1,candext.Item2),(pathext.Item2,candext.Item1),(pathext.Item2,candext.Item2))) return false;
					if (PointHelper.Equals(pathext.Item1,candext.Item2))
					{
						path.InsertRange(0, candidate.Take(candidate.Count-1));
						fe.Remove(candidate);
						return true;
					}
					if (PointHelper.Equals(pathext.Item2,candext.Item1))
					{
						path.AddRange(candidate.Skip(1));
						fe.Remove(candidate);
						return true;
					}
					candidate.Reverse();
					candext = Extremes(candidate);
					if (PointHelper.Equals(pathext.Item1,candext.Item2))
					{
						path.InsertRange(0, candidate.Take(candidate.Count-1));
						fe.Remove(candidate);
						return true;
					}
					if (PointHelper.Equals(pathext.Item2,candext.Item1))
					{
						path.AddRange(candidate.Skip(1));
						fe.Remove(candidate);
						return true;
					}
					throw new Exception("No puedo añadir");
				}
				while (fe.Count > 0)
				{
					var candidate = fe[0];
					var flag = false;
					foreach(var s in Segments)
					{
						if (TryAppend(s, candidate))
						{
							flag = true;
							count++;
							break;
						}
					}
					if (!flag) NewPath(candidate);
				}
				return count;
			}
		}
		
		public Page Page { get; set; }
		public Document Doc { get; set; }
		public Application App { get; set; }
		public bool SkipDrillSize { get; internal set; }
		public bool MarkBigDrill { get; internal set; }

		public Matrix PageMatrix;
		public void DrawPcb(PcbFile pcb)
		{
			App = new CorelDRAW.Application();
			App.Visible = true;
			Doc=App.Application.CreateDocument();
			Doc.Unit = cdrUnit.cdrMillimeter;
			
			Doc.DataFields.Add("Net");
			Doc.DataFields.Add("NetWidth");
			var sa4 = App.PageSizes["A4"];
			var sa4w = App.ConvertUnits(sa4.Width, cdrUnit.cdrInch, Doc.Unit);
			var sa4h = App.ConvertUnits(sa4.Height, cdrUnit.cdrInch, Doc.Unit);
			Doc.MasterPage.SetSize(sa4w, sa4h);
			Page = Doc.ActivePage;
			PageMatrix = new Matrix(1, 0, 0, -1, 0, sa4h);
			var dwg = new Dictionary<(string,int,double),DwgSegment>();
			Page.CreateLayer("B.PAD");
			foreach (var l in pcb.Layers.Values.OrderBy(k=>k.Name.StartsWith("B")?"0"+k.Name:"1"+k.Name))
			{
				var nl = Page.CreateLayer(l.Name);
			}
			Page.CreateLayer("PAD");
			Page.CreateLayer("F.PAD");

			foreach (var f in pcb.Dibujo.OfType<PcbFile.Segment>())
			{
				var k = (f.layer, f.net, f.Width);
				if (!dwg.ContainsKey(k)) dwg.Add(k, new DwgSegment(k));
				dwg[k].Segments.Add(new List<Point>() { f.Start, f.End });
			}
			foreach(Layer l in Page.Layers)
			{
				var lname = l.Name;
				foreach (var s in dwg.Values.Where(s => s.Layer == lname))
				{
					int sc = -1;
					while(sc!=0)
						sc=s.Reduce();
					DrawSegment(l, s);
				}
			}
			foreach (var f in pcb.Dibujo)
			{
				if (f is PcbFile.Segment) continue;
				DrawElement(f.DwgLayers, PageMatrix, f);
			}

			foreach (var mod in pcb.Modules)
			{
				var figuras = new Dictionary<string, List<Shape>>();
				var centro = mod.At;
				var m = Matrix.Identity;
				m.Rotate(-centro.R);
				m.Translate(centro.X,centro.Y);
				m.Append(PageMatrix);
				foreach (var d in mod.Drawing)
				{
					switch (d) {
						case PcbFile.Via via:
							DrawVia(m, via);
							break;
						case PcbFile.Fp_Pad pad:
							DrawPad(m, pad);
							break;
						default:
						var ls = d.DwgLayers;
						foreach (var l in ls)
						{
								var sh = DrawModuleElement(l, m, d);
								if (sh == null) continue;
								if (!figuras.ContainsKey(l)) figuras[l] = new List<Shape>();
								figuras[l].Add(sh);
						}
							break;
					}
				}
				foreach(var l in figuras.Keys)
				{
					var laco = GetLayerData(l);
					var arr = figuras[l].ToArray<object>();
					var rng = Doc.CreateShapeRangeFromArray(arr);
					laco.Layer.Activate();
					var fnsh = rng.Group();
					fnsh.Outline.Color.CopyAssign(laco.Color);
				}

			}
			
		}
		private void DrawElement(string[] layers,Matrix m,IDwgElement d)
		{
			
			Shape sh;
			switch (d) {
				case PcbFile.Via via:
					sh=DrawVia(m, via);
					break;
				case PcbFile.Gr_Line grline:
					foreach (var l in layers)
					{
						var laco = GetLayerData(l);
						sh = DrawGrLine(laco, m, grline);
					}
					break;
			}
		}

		private Shape DrawGrLine(LayerData laco, Matrix m, PcbFile.Gr_Line grline)
		{
			var gp0 = m.Transform(grline.Start);
			var gp1 = m.Transform(grline.End);
			var sh = laco.Layer.CreateLineSegment(gp0.X, gp0.Y, gp1.X, gp1.Y);
			sh.Outline.Color.CopyAssign(laco.Color);
			sh.Outline.Width = grline.Width;
			return sh;
		}

		private Shape DrawVia(Matrix m, PcbFile.Via via)
		{
			var l = GetLayerData("PAD");
			var at = via.At;
			var p0 = m.Transform(at.Point());
			var sh = l.Layer.CreateEllipse2(p0.X, p0.Y, via.Size.Width / 2, via.Size.Height / 2);
			sh.Fill.ApplyUniformFill(l.Color);
			sh.Outline.Width = 0;
			var sh2 = l.Layer.CreateEllipse2(p0.X, p0.Y, via.Drill / 2, via.Drill / 2);
			sh2.Fill.ApplyUniformFill(App.CreateColor("White"));
			sh2.Outline.Width = 0;
			var r = Doc.CreateShapeRangeFromArray(sh, sh2);
			sh = r.Group();
			return sh;
		}
		private double textmult = 1.5;
		private Shape DrawModuleElement(string l, Matrix m, IDwgElement d)
		{
			var laco = GetLayerData(l);
			var la = laco.Layer;
			Shape sh=null;
			switch (d)
			{
				case PcbFile.Fp_Line line:
					var p0 = m.Transform(line.Start);
					var p1 = m.Transform(line.End);
					sh = la.CreateLineSegment(p0.X,p0.Y,p1.X,p1.Y);
					//sh.Outline.Color.CopyAssign(laco.Color);
					sh.Outline.Width = line.Width;
					break;
				case PcbFile.Fp_Circle circle:
					var c0 = m.Transform(circle.Center);
					var dis = Math.Sqrt(PointHelper.DistanceSqr(circle.Center, circle.End));
					sh = la.CreateEllipse2(c0.X, c0.Y, dis);
					//sh.Outline.Color.CopyAssign(laco.Color);
					sh.Outline.Width = circle.Width;
					break;
				//case PcbFile.Fp_Pad pad:
				//	DrawPad(m, pad);
				//	break;
				case PcbFile.Gr_Line grline:
					var gp0 = m.Transform(grline.Start);
					var gp1 = m.Transform(grline.End);
					sh = la.CreateLineSegment(gp0.X, gp0.Y, gp1.X, gp1.Y);
					//sh.Outline.Color.CopyAssign(laco.Color);
					sh.Outline.Width = grline.Width;
					break;
				case PcbFile.Fp_Text texto:
					if ((!texto._Hide)&&(texto.Type!="user")&&(!string.IsNullOrWhiteSpace(texto.Text)))
					{
						var pto = m.Transform(texto.At.Point());
						var th = texto.Effects.Font.Thickness;
						var sz = texto.Effects.Font.Size;
						var fsz = textmult*App.ConvertUnits(sz.Height, cdrUnit.cdrMillimeter, cdrUnit.cdrPoint);
						sh = la.CreateArtisticText(pto.X, pto.Y, texto.Text, Size: (float)fsz, Font: "Arial Rounded MT Bold", Alignment: cdrAlignment.cdrCenterAlignment);
						sh.Fill.ApplyUniformFill(laco.Color);
						sh.Outline.Width = 0;
						if (texto.Effects.Justify == "mirror")
						{
							sh.Flip(cdrFlipAxes.cdrFlipHorizontal);
						}
						var r = texto.At.R;
						if (r > 90) r -= 180;
						sh.Rotate(-r);
					}
					break;
			}
			return sh;
		}
		private Shape GroupShapes(params Shape[] parameters)
		{
			var r = Doc.CreateShapeRangeFromArray(parameters.Where(p=>p!=null).ToArray<object>());
			return r.Group();
		}
		private Shape CircleByDiameter(Layer layer,Point pto,double diameter, CorelDRAW.Color fillcolor =null,double outline=0,CorelDRAW.Color outlinecolor=null,bool outlineback=false)
		{
			var finalradius = diameter/2;
			var sh2 = layer.CreateEllipse2(pto.X, pto.Y, finalradius);
			if (fillcolor != null)
				sh2.Fill.ApplyUniformFill(fillcolor);
			else
				sh2.Fill.ApplyNoFill();
			sh2.Outline.Width = outline;
			if (outline > 0)
				sh2.Outline.Color = outlinecolor;
			sh2.Outline.BehindFill = outlineback;
			return sh2;
		}
		private Shape FillCircleByDiameter(Layer layer, Point pto, double diameter,string fillcolor)
		{
			return CircleByDiameter(layer, pto, diameter, App.CreateColor(fillcolor));
		}


		private HashSet<string> Symbols = new HashSet<string>();
		private bool CreatePadSymbol(LayerData la,Point pto, PcbFile.Fp_Pad pad,out Shape sh)
		{
			Debug.Assert(la.Layer.Name.EndsWith("PAD"));
			var key = pad.Key;
			sh = null;
			if (Symbols.Contains(key)) return false;
			if ((pad.Shape == "circle") || ((pad.Shape == "oval") && (pad.Size.Width == pad.Size.Height)))
			{
				sh = CreateCirclePadSymbol(la.Layer,pto,pad);
				
			}
			else
			{
				if (pad.Shape == "oval")
				{
					sh = CreateOvalPadSymbol(la.Layer,pto,pad);
					
				}
				else
				{
					sh=CreateRectPadSymbol(la.Layer,pto,pad);
				}
			}
			sh.Fill.ApplyUniformFill(la.Color);
			sh.Outline.Width = 0;
			if (pad.Drill > 0)
			{
                var ptodrill = pto;
                ptodrill.X += pad.Drill.Offset.X;
                ptodrill.Y -= pad.Drill.Offset.Y;
                if (!SkipDrillSize)
				{
					var sh2 = FillCircleByDiameter(la.Layer,ptodrill,pad.Drill,"White");
					sh = GroupShapes(sh, sh2);
				}
				else
				{
					var sh2= FillCircleByDiameter(la.Layer, ptodrill, 0.5, "White");
					Shape sh3 = null;
					if (MarkBigDrill&&(pad.Drill>1))
					{
						sh3 = CircleByDiameter(la.Layer, ptodrill, pad.Drill - 0.3, null, 0.2, App.CreateColor("White"), true);
					}
					sh = GroupShapes(sh, sh2, sh3);
				}
			}
			sh = sh.ConvertToSymbol(key);
			Symbols.Add(key);
			return true;
		}
		private Shape CreateCirclePadSymbol(Layer la, Point pto,PcbFile.Fp_Pad pad)
		{
			var sz = pad.Size;
			return la.CreateEllipse2(pto.X,pto.Y, sz.Width / 2, sz.Height / 2);
		}
		private Shape CreateRectPadSymbol(Layer la, Point pto,PcbFile.Fp_Pad pad)
		{
			var sz = pad.Size;
			return la.CreateRectangle2(pto.X- sz.Width / 2,pto.Y - sz.Height / 2, sz.Width, sz.Height, 0, 0, 0, 0);
		}
		private Shape CreateOvalPadSymbol(Layer la, Point pto,PcbFile.Fp_Pad pad)
		{
			var sz = pad.Size;
			var cc = Math.Max(sz.Width, sz.Height);
			var sp1 = la.CreateEllipse2(pto.X, pto.Y, cc / 2, cc / 2);
			var sp2= la.CreateRectangle2(pto.X-sz.Width / 2,pto.Y-sz.Height / 2, sz.Width, sz.Height, 0, 0, 0, 0);
			var res=sp2.Intersect(sp1, false, false);
			return res;
		}

		private void DrawPad(Matrix m,PcbFile.Fp_Pad pad)
		{
			var cat = pad.At;
			var c0 = m.Transform(cat.Point());
			var sz = pad.Size;
			Shape sh = null;
			Shape sh2 = null;
			LayerData laco;
			if (pad.Type != "smd")
			{
				laco = GetLayerData("PAD");
			}
			else
			{
				if (pad.DwgLayers[0][0] == 'B')
					laco = GetLayerData("B.PAD");
				else
					laco = GetLayerData("F.PAD");
			}
			if (!CreatePadSymbol(laco,c0,pad,out sh))
				sh = laco.Layer.CreateSymbol(c0.X, c0.Y, pad.Key);
			//if (pad.Drill > 0)
			//{
			//	sh2 = laco.Layer.CreateEllipse2(c0.X, c0.Y, pad.Drill / 2);
			//	sh2.Fill.ApplyUniformFill(App.CreateColor("White"));
			//	sh2.Outline.Width = 0;
			//}
			sh.Rotate(-pad.At.R);
			sh.Name = pad.Key;
			if (!sh.Layer.Name.EndsWith("PAD"))
			{
				sh.Layer = laco.Layer;
				if (!sh.Layer.Name.EndsWith("PAD"))
					throw new Exception("Pad equivocado");
			}
		}

		private void DrawSegment(Layer layer,DwgSegment s)
		{
			var lc = new CorelDRAW.Color();
			var pc = layercolors[layer.Name];
			lc.RGBAssign(pc.R, pc.G, pc.B);
			var m = PageMatrix;
			foreach (var p in s.Segments)
			{
				var linea = DrawCurve(layer, m, p.ToArray());
				linea.Outline.Width = s.Width;
				var c = new CorelDRAW.Color();
				linea.Outline.Color.CopyAssign(lc);
				linea.Outline.LineJoin = cdrOutlineLineJoin.cdrOutlineRoundLineJoin;
				linea.Outline.LineCaps = cdrOutlineLineCaps.cdrOutlineRoundLineCaps;
				linea.ObjectData["net"].Value = s.Net;
				linea.ObjectData["netwidth"].Value = s.Width;
			}
		}

		private Shape DrawCurve(Layer layer,Matrix matrix, Point[] points)
		{
			var curva = App.CreateCurve(Doc);
			var cvtpto = points.ToArray();
			matrix.Transform(cvtpto);
			var pto = cvtpto[0];
			var sh = curva.CreateSubPath(pto.X, pto.Y);
			foreach (var p in cvtpto.Skip(1))
			{
				sh.AppendLineSegment(p.X, p.Y);
			}
			return layer.CreateCurve(curva);
		}
		
	}

}
