using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;

namespace Rop.Kicad
{
	/// <summary>
	/// Token Static PART
	/// </summary>
	public partial class Token
	{
		#region PRIVATE
		private static Dictionary<Type, MethodInfo> DicFactory = new Dictionary<Type, MethodInfo>();
		private static MethodInfo GetFactory<U>() where U : Token
		{
			var tu = typeof(U);
			if (DicFactory.TryGetValue(tu, out MethodInfo m))
				return m;
			else
			{
				m = tu.GetMethod("Factory", BindingFlags.Static | BindingFlags.Public);
				DicFactory.Add(tu, m);
				return m;
			}
		}
		#endregion
		/// <summary>
		/// String to Base Tokens (Excluded LIST and Complex Types)
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static Token GlobalFactory(string s)
		{
			var c = s[0];
			if (c == '"') return StringValue.Factory(s);
			if (s == "nil") return Nil.Factory();
			if (Factory<IntValue>(s, out var i)) return i;
			if (Factory<DoubleValue>(s, out var f)) return f;
			if (Factory<BoolValue>(s, out var b)) return b;
			return Atom.Factory(s);
		}
		/// <summary>
		/// Factory S to Type U
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="s"></param>
		/// <returns></returns>
		public static U Factory<U>(string s) where U : Token
		{
			var f = GetFactory<U>();
			return (U)(f?.Invoke(null, new object[] { s }));
		}
		/// <summary>
		/// TryFactory S to Type U
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="s"></param>
		/// <param name="outvalue"></param>
		/// <returns></returns>
		public static bool Factory<U>(string s, out U outvalue) where U : Token
		{
			outvalue = Factory<U>(s);
			return (outvalue != null);
		}
	}
	/// <summary>
	/// Token Abstract PART
	/// </summary>
	public partial class Token
	{
		public string Name { get; protected set; }
		public object ObjectValue { get; protected set; }
		public virtual string StrValue => ObjectValue?.ToString();
		public override string ToString() => $"<{Name}>:{StrValue}";
		public Token(string name, object value)
		{
			Name = name;
			ObjectValue = value;
		}
	}

	#region BASE TOKENs
	public class Atom : Token
	{
		public Atom(string s) : base(s, s) { }
		public override string ToString() => $"ATOM({Name})";
		public static Token Factory(string s) => new Atom(s);

	}
	public class Nil : Token
	{
		protected Nil() : base("nil", null) { }
		public override string ToString() => "NIL";
		public static Token Factory() => new Nil();
	}
	/// <summary>
	/// Generic Token Encapsulates Base Type
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Token<T> : Token
	{

		public T Value
		{
			get { return (T)ObjectValue; }
			protected set { ObjectValue = value; }
		}
		public Token(T v) : base(typeof(T).Name, v) { }
		public override string ToString() => $"<{base.Name}>:{StrValue}";
	}

	public class IntValue : Token<int>
	{
		public IntValue(int v) : base(v) { }
		public static IntValue Factory(string s)
		{
            if ((s == "-") || (s == "+")) return null;
			if (!_reg.IsMatch(s)) return null;
			return new IntValue(int.Parse(s));
		}
		private static Regex _reg = new Regex(@"^(\+|\-|\d)?\d*$", RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled);
	}
	public class DoubleValue : Token<double>
	{
		public DoubleValue(double v) : base(v)
		{
		}
		public static DoubleValue Factory(string s)
		{
			if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double r))
				return new DoubleValue(r);
			else
				return null;
		}
	}
	public class BoolValue : Token<bool>
	{
		public BoolValue(bool v) : base(v)
		{
		}
		public static BoolValue Factory(string s)
		{
			bool? res = null;
			if (s == "true") res = true;
			if (s == "yes") res = true;
			if (s == "false") res = false;
			if (s == "no") res = false;
			return (res.HasValue) ? new BoolValue(res.Value) : null;
		}
	}

	public class StringValue : Token<string>
	{
		public StringValue(string v) : base(v) { }
		public static StringValue Factory(string s)
		{
			if (!_reg.IsMatch(s)) return null;
			return new StringValue(s.Substring(1, s.Length - 2));
		}
		private static Regex _reg = new Regex(@"^\""[^\""]*\""$", RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled);
	}
	#endregion
	#region COMPLEX VALUE STATIC
	/// <summary>
	/// Attribute to Map NAMED LIST to Complex Types
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class FactoryNameAttribute : Attribute
	{
		public string[] Name { get; set; }
		public FactoryNameAttribute(params string[] name) => Name = name;
	}
	/// <summary>
	/// Register of ComplexTypes
	/// </summary>
	public static class ComplexValue
	{
#region PRIVATE
		private static Dictionary<string, Type> factorydic = new Dictionary<string, Type>(StringComparer.CurrentCultureIgnoreCase);
		private static Type GetListType(string n)
		{
			return ((n != null) && (factorydic.TryGetValue(n, out var value))) ? value : null;
		}
		private static string[] GetComplexNames(Type t)
		{
			return t.GetCustomAttribute<FactoryNameAttribute>()?.Name ?? new string[] {};
		}
#endregion
		public static void Register<T>() where T : class
		{
			var tipo = typeof(T);
			foreach (var t in tipo.GetNestedTypes())
			{
				foreach (var name in GetComplexNames(t))
				{
					try
					{
						factorydic.Add(name, t);
					}
					catch 
					{
						throw new Exception($"Complex Type Register Error ({name})");
					}
				}
			}
		}
		public static Token Factory(List l)
		{
			var cv = GetListType(l.Name);
			if (cv == null) return null;
			return (Token)Activator.CreateInstance(cv, new object[] { l });
		}
	}
	/// <summary>
	/// Interface... Complex Value can be Simplified to Base one.
	/// </summary>
	public interface ISimplify
	{
		object Simplify(Type t);
	}
	#endregion
	#region COMPLEX TOKEN ABSTRACTS
	/// <summary>
	/// Abstract Token of type Struct
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ComplexToken<T> : Token<T>
	{
		public ComplexToken(string n, T v) : base(v) { Name = n; }
		public ComplexToken(string n) : this(n, default(T)) { }
	}
	/// <summary>
	/// Abstract Token of class Type
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ClassToken<T> : ComplexToken<T> where T : class, new()
	{
		public ClassToken(List l) : base(l.Name, new T())
		{
			l?.MapTo(this.Value);
		}
	}
	#endregion
	#region LIST TOKEN
	/// <summary>
	/// List is a Token of List of Tokens.(Named or not)
	/// </summary>
	public class List : Token<List<Token>>
	{
		public List(List<Token> ts) : base(ts)
		{
			Name = "";
			if (ts == null) return;
			if (ts.Count == 0) return;
			if (Value[0] is Atom a)
				Name = a.Name;
		}
		public List() : this(null) { }
		/// <summary>
		/// Is Empty List
		/// </summary>
		public bool IsNil => (Value == null);
		/// <summary>
		/// All Values (Excluded name)
		/// </summary>
		public object[] Values => Tokens.Select(t => t.ObjectValue).ToArray();
		/// <summary>
		/// All Values as String (Excluded name)
		/// </summary>
		public string[] ValuesStr => Tokens.Select(t => Convert.ToString(t.ObjectValue)).ToArray();
		/// <summary>
		/// All Values as Double (Excluded name)
		/// </summary>
		public double[] ValuesDouble => Tokens.Select(t => Convert.ToDouble(t.ObjectValue)).ToArray();
		/// <summary>
		/// All Values as Int (Excluded name)
		/// </summary>
		public int[] ValuesInt => Tokens.Select(t => Convert.ToInt32(t.ObjectValue)).ToArray();
		/// <summary>
		/// All Values as Tokens (Excluded name)
		/// </summary>
		public IEnumerable<Token> Tokens
		{
			get
			{
				if (IsNil) return null;
				var sk = IsNamed() ? 1 : 0;
				return Value.Skip(sk);
			}
		}
		public override string ToString()
		{
			if (IsNil) return "nil";
			var fs = string.Join(" ", ValuesStr);
			return $"({fs})";
		}
		/// <summary>
		/// Check if this list has certain name
		/// </summary>
		/// <param name="s">Name to check</param>
		/// <returns>True if this list has this name</returns>
		public bool IsNamed(string s) => Name.Equals(s, StringComparison.CurrentCultureIgnoreCase);
		public bool IsNamed() => !string.IsNullOrEmpty(Name);
		/// <summary>
		/// Find a Named List in this List
		/// </summary>
		/// <param name="v">Name of sublist</param>
		/// <returns>The list has a name</returns>
		public List FindList(string v)
		{
			return Tokens.FirstOrDefault(t => (t is List l) && (l.IsNamed(v))) as List;
		}
		/// <summary>
		/// Find All named sublist.
		/// </summary>
		/// <param name="v">Name of sublist</param>
		/// <returns></returns>
		public IEnumerable<List> FindAllList(string v)
		{
			return Tokens.OfType<List>().Where(a => a.IsNamed(v));
		}
		/// <summary>
		/// Add a Token at end
		/// </summary>
		/// <param name="t">Token to Add</param>
		public void Add(Token t)
		{
			if (Value == null)
			{
				Value = new List<Token>();
				if (t is Atom a) Name = a.Name;
			}
			Value.Add(t);
		}
		/// <summary>
		/// Find Complex Token of certain name
		/// </summary>
		/// <param name="v">Name of Complex Token</param>
		/// <returns></returns>
		public Token FindCV(string v)
		{
			return Tokens.FirstOrDefault(tok => tok.Name.Equals(v, StringComparison.InvariantCultureIgnoreCase));
		}
		/// <summary>
		/// Find All Complex Tokens of certain name
		/// </summary>
		/// <param name="v">Name of complex tokens</param>
		/// <returns></returns>
		public IEnumerable<Token> FindAllCV(string v)
		{
			return Tokens.Where(tok => tok.Name.Equals(v, StringComparison.InvariantCultureIgnoreCase));
		}
		/// <summary>
		/// Get Nth element of de list (INCLUDED Name)
		/// </summary>
		/// <param name="n">Index</param>
		/// <returns>Token at index</returns>
		public Token GetNth(int n)
		{
			if (n < 0 || (n >= Value.Count)) return null;
			return Value[n];
		}
		/// <summary>
		/// Find Value of Named Sublist.
		/// If Sublist has one Value return this value.
		/// else Return an array of values. 
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public object FindValue(string v)
		{
			var l = FindList(v);
			if (l == null) return null;
			var vs = l.Values;
			if (vs.Length == 1)
				return vs[0];
			else
				return vs;
		}
		/// <summary>
		/// Map list to Type T
		/// </summary>
		/// <typeparam name="T">Generic type. Class</typeparam>
		/// <param name="clase">Class to FillUp</param>
		public void MapTo<T>(T clase) where T : class
		{
			var pro = typeof(T).GetProperties(System.Reflection.BindingFlags.Public);
			foreach (var p in pro)
			{
				var n = p.Name;
				var v = FindValue(n);
				if (v == null) v = FindCV(n)?.ObjectValue;
				if (v == null) continue;
				p.SetValue(clase, v);
			}
			var fi = typeof(T).GetFields();
			foreach (var p in fi)
			{
				var n = p.Name;
				var v = FindValue(n);
				if (v == null)
					v = FindCV(n)?.ObjectValue;
				if (v == null) continue;
				if (p.FieldType != v.GetType())
				{
					switch (v)
					{
						case IConvertible cvt:
							v = Convert.ChangeType(v, p.FieldType); break;
						case ISimplify simp:
							v = simp.Simplify(p.FieldType);
							break;
                        case Drill d:
                            v = d.Size;
                            break;
						default:
							throw new Exception("Imposible conversion");
					}
				}
				p.SetValue(clase, v);
			}
		}
		/// <summary>
		/// Map to Struct
		/// </summary>
		/// <typeparam name="T">Struct type</typeparam>
		/// <param name="stru">Reference to Struct Type to fillup</param>
		public void MapTo<T>(ref T stru) where T : struct
		{
			T r = new T();
			var pro = typeof(T).GetFields();
			var boxed = (object)stru;
			for (var f = 0; f < pro.Length; f++)
			{
				var v = GetNth(f).ObjectValue;
				pro[f].SetValue(boxed, v);
			}
			stru = (T)boxed;
		}
		public T MapTo<T>() where T : struct
		{
			T r = new T();
			MapTo(ref r);
			return r;
		}
		/// <summary>
		/// Find Complex Value
		/// </summary>
		/// <typeparam name="T">Complex type to find</typeparam>
		/// <returns>Default(T) if not found</returns>
		public T FindCVValue<T>()
		{
			var ts = Tokens.FirstOrDefault(t => t.ObjectValue?.GetType() == typeof(T));
			if (ts != null) return (T)ts.ObjectValue;
			return default(T);
		}
		/// <summary>
		/// Find all Complex Values of T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IEnumerable<T> FindCVAllValues<T>()
		{
			return Tokens.Where(t => (t.ObjectValue != null) && (t.ObjectValue is T)).Select(t => (T)t.ObjectValue);
		}
	}
	/// <summary>
	/// Parser of a List
	/// </summary>
	public class ListParser
	{
		public TextReader BaseStream { get; private set; }
		/// <summary>
		/// Peek a Char without consuming it
		/// </summary>
		/// <returns>Next Char</returns>
		private char Peek()
		{
			var c = BaseStream.Peek();
			return (c < 0) ? (char)0 : (char)c;
		}
		/// <summary>
		/// Read a Char
		/// </summary>
		/// <returns>Char readen</returns>
		private char Read()
		{
			var c = BaseStream.Read();
			return (c < 0) ? (char)0 : (char)c;
		}
		/// <summary>
		/// Peek a char (skip whites) 
		/// </summary>
		/// <returns>Next char no white</returns>
		private char PeekNoWhite()
		{
			while (true)
			{
				var c = Peek();
				if ((c <= 0) || (c > 32)) return c;
				Read();
			}
		}
		/// <summary>
		/// Read string Element, ready to token conversion
		/// </summary>
		/// <returns>String readen</returns>
		private string ReadElement()
		{
			var c = PeekNoWhite();
			if ((c == ')') || (c == '(') || (c == 0)) return Read().ToString();
			var str = new List<Char>();
			if (c != '"')
			{
				while (true)
				{
					str.Add(Read());
					c = Peek();
					if ((c <= 32) || (c == ')') || (c == '(')) return new string(str.ToArray());
				}
			}
			else
			{
				while (true)
				{
					str.Add(Read());
					c = Peek();
					if (c == '"')
					{
						str.Add(Read());
						return new string(str.ToArray());
					}
					if (c == 0)
					{
						Error = "No end string";
						return null;
					}
				}
			}
		}

		public string Error { get; private set; } = "";
		public List Decode(System.IO.TextReader stream)
		{
			BaseStream = stream;
			var res = ReadList();
			var p = PeekNoWhite();
			if (p != 0) { Error = "No End List"; return null; }
			return res as List;
		}
		/// <summary>
		/// Read a List
		/// </summary>
		/// <returns></returns>
		public Token ReadList()
		{
			var e = ReadElement();
			if (e != "(") { Error = "No Start List"; return null; }
			var lst = new List();
			while (true)
			{
				var c = PeekNoWhite();
				if (c == ')') break;
				if (c == '(')
				{
					var l = ReadList();
					var cv = ComplexValue.Factory(l as List);
					if (cv == null)
						lst.Add(l);
					else
						lst.Add(cv);
				}
				else
				{
					e = ReadElement();
					lst.Add(Token.GlobalFactory(e));
				}
			}
			e = ReadElement();
			if (e != ")") { Error = "No End List"; return null; }
			return lst;
		}
	}
	#endregion
}
