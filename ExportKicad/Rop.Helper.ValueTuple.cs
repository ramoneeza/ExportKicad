using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rop.Helper
{
	public static class ValueTupleHelper
	{
		public static object[] ToArray<T1>(this ValueTuple<T1> tuple) => new object[] { tuple.Item1 };
		public static object[] ToArray<T1,T2>(this ValueTuple<T1,T2> tuple) => new object[] { tuple.Item1,tuple.Item2 };
		public static object[] ToArray<T1,T2,T3>(this ValueTuple<T1,T2,T3> tuple) => new object[] { tuple.Item1,tuple.Item2,tuple.Item3 };
		public static object[] ToArray<T1,T2,T3,T4>(this ValueTuple<T1,T2,T3,T4> tuple) => new object[] { tuple.Item1,tuple.Item2,tuple.Item3,tuple.Item4 };
		public static object[] ToArray<T1, T2, T3, T4,T5>(this ValueTuple<T1, T2, T3, T4,T5> tuple) => new object[] { tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4 ,tuple.Item5};
		public static object[] ToArray<T1, T2, T3, T4,T5,T6>(this ValueTuple<T1, T2, T3, T4,T5,T6> tuple) => new object[] { tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4,tuple.Item5,tuple.Item6 };
		public static object[] ToArray<T1, T2, T3, T4,T5,T6,T7>(this ValueTuple<T1, T2, T3, T4,T5,T6,T7> tuple) => new object[] { tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4,tuple.Item5,tuple.Item6,tuple.Item7 };

		public static ValueTuple<T1> Shorten<T1, T2>(this ValueTuple<T1, T2> tuple) => new ValueTuple<T1>(tuple.Item1);
		public static ValueTuple<T1,T2> Shorten<T1, T2,T3>(this ValueTuple<T1, T2,T3> tuple) => new ValueTuple<T1,T2>(tuple.Item1,tuple.Item2);
		public static ValueTuple<T1,T2,T3> Shorten<T1, T2,T3,T4>(this ValueTuple<T1, T2,T3,T4> tuple) => new ValueTuple<T1,T2,T3>(tuple.Item1,tuple.Item2,tuple.Item3);
		public static ValueTuple<T1, T2, T3,T4> Shorten<T1, T2,T3, T4,T5>(this ValueTuple<T1, T2,T3,T4,T5> tuple) => new ValueTuple<T1,T2,T3,T4>(tuple.Item1, tuple.Item2, tuple.Item3,tuple.Item4);
		public static ValueTuple<T1, T2, T3, T4,T5> Shorten<T1, T2, T3, T4, T5,T6>(this ValueTuple<T1, T2, T3, T4, T5,T6> tuple) => new ValueTuple<T1, T2, T3, T4,T5>(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4,tuple.Item5);
		public static ValueTuple<T1, T2, T3, T4,T5,T6> Shorten<T1, T2, T3, T4, T5,T6,T7>(this ValueTuple<T1, T2, T3, T4, T5,T6,T7> tuple) => new ValueTuple<T1, T2, T3, T4,T5,T6>(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4,tuple.Item5,tuple.Item6);

		public static ValueTuple<T1,T2> Expand<T1, T2>(this ValueTuple<T1> tuple,T2 add=default(T2)) => new ValueTuple<T1,T2>(tuple.Item1,add);
		public static ValueTuple<T1, T2,T3> Expand<T1, T2, T3>(this ValueTuple<T1, T2> tuple, T3 add = default(T3)) => new ValueTuple<T1, T2,T3>(tuple.Item1, tuple.Item2,add);
		public static ValueTuple<T1, T2, T3,T4> Expand<T1, T2, T3, T4>(this ValueTuple<T1, T2, T3> tuple, T4 add = default(T4)) => new ValueTuple<T1, T2, T3,T4>(tuple.Item1, tuple.Item2, tuple.Item3,add);
		public static ValueTuple<T1, T2, T3, T4,T5> Expand<T1, T2, T3, T4, T5>(this ValueTuple<T1, T2, T3, T4> tuple, T5 add = default(T5)) => new ValueTuple<T1, T2, T3, T4,T5>(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, add);
		public static ValueTuple<T1, T2, T3, T4, T5,T6> Expand<T1, T2, T3, T4, T5, T6>(this ValueTuple<T1, T2, T3, T4, T5> tuple, T6 add = default(T6)) => new ValueTuple<T1, T2, T3, T4, T5,T6>(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, add);
		public static ValueTuple<T1, T2, T3, T4, T5, T6,T7> Expand<T1, T2, T3, T4, T5, T6, T7>(this ValueTuple<T1, T2, T3, T4, T5, T6> tuple, T7 add = default(T7)) => new ValueTuple<T1, T2, T3, T4, T5, T6,T7>(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, add);

		public static ValueTuple<T1, T2> Join<T1, T2>(this ValueTuple<T1> tuple1,ValueTuple<T2> tuple2) => new ValueTuple<T1, T2>(tuple1.Item1, tuple2.Item1);
		public static ValueTuple<T1, T2,T3> Join<T1, T2,T3>(this ValueTuple<T1,T2> tuple1, ValueTuple<T3> tuple2) => new ValueTuple<T1, T2,T3>(tuple1.Item1, tuple1.Item2,tuple2.Item1);
		public static ValueTuple<T1, T2, T3,T4> Join<T1, T2, T3,T4>(this ValueTuple<T1, T2,T3> tuple1, ValueTuple<T4> tuple2) => new ValueTuple<T1, T2, T3,T4>(tuple1.Item1, tuple1.Item2, tuple1.Item3,tuple2.Item1);
		public static ValueTuple<T1, T2, T3, T4,T5> Join<T1, T2, T3, T4,T5>(this ValueTuple<T1, T2, T3,T4> tuple1, ValueTuple<T5> tuple2) => new ValueTuple<T1, T2, T3, T4,T5>(tuple1.Item1, tuple1.Item2, tuple1.Item3, tuple1.Item4,tuple2.Item1);
		public static ValueTuple<T1, T2, T3, T4,T5,T6> Join<T1, T2, T3, T4,T5,T6>(this ValueTuple<T1, T2, T3,T4,T5> tuple1, ValueTuple<T6> tuple2) => new ValueTuple<T1, T2, T3, T4,T5,T6>(tuple1.Item1, tuple1.Item2, tuple1.Item3, tuple1.Item4,tuple1.Item5,tuple2.Item1);
		public static ValueTuple<T1, T2, T3, T4, T5, T6,T7> Join<T1, T2, T3, T4, T5, T6,T7>(this ValueTuple<T1, T2, T3, T4, T5,T6> tuple1, ValueTuple<T7> tuple2) => new ValueTuple<T1, T2, T3, T4, T5, T6,T7>(tuple1.Item1, tuple1.Item2, tuple1.Item3, tuple1.Item4, tuple1.Item5, tuple1.Item6,tuple2.Item1);

		public static ValueTuple<T1, T2, T3> Join<T1, T2, T3>(this ValueTuple<T1> tuple1, ValueTuple<T2,T3> tuple2) => new ValueTuple<T1, T2, T3>(tuple1.Item1, tuple2.Item1, tuple2.Item2);
		public static ValueTuple<T1, T2, T3,T4> Join<T1, T2, T3,T4>(this ValueTuple<T1,T2> tuple1, ValueTuple<T3, T4> tuple2) => new ValueTuple<T1, T2, T3,T4>(tuple1.Item1, tuple1.Item2,tuple2.Item1, tuple2.Item2);
		public static ValueTuple<T1, T2, T3, T4,T5> Join<T1, T2, T3, T4,T5>(this ValueTuple<T1, T2,T3> tuple1, ValueTuple<T4, T5> tuple2) => new ValueTuple<T1, T2, T3, T4,T5>(tuple1.Item1, tuple1.Item2, tuple1.Item3,tuple2.Item1, tuple2.Item2);
		public static ValueTuple<T1, T2, T3, T4, T5,T6> Join<T1, T2, T3, T4, T5,T6>(this ValueTuple<T1, T2, T3,T4> tuple1, ValueTuple<T5, T6> tuple2) => new ValueTuple<T1, T2, T3, T4, T5,T6>(tuple1.Item1, tuple1.Item2, tuple1.Item3,tuple1.Item4, tuple2.Item1, tuple2.Item2);
		public static ValueTuple<T1, T2, T3, T4, T5, T6,T7> Join<T1, T2, T3, T4, T5, T6,T7>(this ValueTuple<T1, T2, T3, T4,T5> tuple1, ValueTuple<T6, T7> tuple2) => new ValueTuple<T1, T2, T3, T4, T5, T6,T7>(tuple1.Item1, tuple1.Item2, tuple1.Item3, tuple1.Item4,tuple1.Item5,tuple2.Item1, tuple2.Item2);

		public static ValueTuple<T1, T2, T3, T4> Join<T1, T2, T3, T4>(this ValueTuple<T1> tuple1, ValueTuple<T2,T3, T4> tuple2) => new ValueTuple<T1, T2, T3, T4>(tuple1.Item1, tuple2.Item1, tuple2.Item2, tuple2.Item3);
		public static ValueTuple<T1, T2, T3, T4, T5> Join<T1, T2, T3, T4, T5>(this ValueTuple<T1, T2> tuple1, ValueTuple<T3,T4, T5> tuple2) => new ValueTuple<T1, T2, T3, T4, T5>(tuple1.Item1, tuple1.Item2, tuple2.Item1, tuple2.Item2, tuple2.Item3);
		public static ValueTuple<T1, T2, T3, T4, T5, T6> Join<T1, T2, T3, T4, T5, T6>(this ValueTuple<T1, T2, T3> tuple1, ValueTuple<T4,T5, T6> tuple2) => new ValueTuple<T1, T2, T3, T4, T5, T6>(tuple1.Item1, tuple1.Item2, tuple1.Item3, tuple2.Item1, tuple2.Item2, tuple2.Item3);
		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7> Join<T1, T2, T3, T4, T5, T6, T7>(this ValueTuple<T1, T2, T3, T4> tuple1, ValueTuple<T5,T6, T7> tuple2) => new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(tuple1.Item1, tuple1.Item2, tuple1.Item3, tuple1.Item4, tuple2.Item1, tuple2.Item2, tuple2.Item3);

		public static ValueTuple<T1, T2, T3, T4, T5> Join<T1, T2, T3, T4, T5>(this ValueTuple<T1> tuple1, ValueTuple<T2,T3, T4, T5> tuple2) => new ValueTuple<T1, T2, T3, T4, T5>(tuple1.Item1, tuple2.Item1, tuple2.Item2, tuple2.Item3, tuple2.Item4);
		public static ValueTuple<T1, T2, T3, T4, T5, T6> Join<T1, T2, T3, T4, T5, T6>(this ValueTuple<T1, T2> tuple1, ValueTuple<T3,T4, T5, T6> tuple2) => new ValueTuple<T1, T2, T3, T4, T5, T6>(tuple1.Item1, tuple1.Item2, tuple2.Item1, tuple2.Item2, tuple2.Item3, tuple2.Item4);
		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7> Join<T1, T2, T3, T4, T5, T6, T7>(this ValueTuple<T1, T2, T3> tuple1, ValueTuple<T4,T5, T6, T7> tuple2) => new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(tuple1.Item1, tuple1.Item2, tuple1.Item3, tuple2.Item1, tuple2.Item2, tuple2.Item3, tuple2.Item4);

		public static ValueTuple<T1, T2, T3, T4, T5, T6> Join<T1, T2, T3, T4, T5, T6>(this ValueTuple<T1> tuple1, ValueTuple<T2,T3, T4, T5, T6> tuple2) => new ValueTuple<T1, T2, T3, T4, T5, T6>(tuple1.Item1, tuple2.Item1, tuple2.Item2, tuple2.Item3, tuple2.Item4, tuple2.Item5);
		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7> Join<T1, T2, T3, T4, T5, T6, T7>(this ValueTuple<T1, T2> tuple1, ValueTuple<T3,T4, T5, T6, T7> tuple2) => new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(tuple1.Item1, tuple1.Item2, tuple2.Item1, tuple2.Item2, tuple2.Item3, tuple2.Item4, tuple2.Item5);

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7> Join<T1, T2, T3, T4, T5, T6, T7>(this ValueTuple<T1> tuple1, ValueTuple<T2,T3, T4, T5, T6, T7> tuple2) => new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(tuple1.Item1, tuple2.Item1, tuple2.Item2, tuple2.Item3, tuple2.Item4, tuple2.Item5, tuple2.Item6);

		public static T ToStruct<T>(this object[] values) where T : struct
		{
			var fields = typeof(T).GetFields();
			if (fields.Length < values.Length) throw new ArgumentException("Big Value Tuple");
			T res = new T();
			for (var f = 0; f < fields.Length;f++)
				fields[f].SetValue(res,values[f]);
			return res;
		}
		private static Dictionary<Type, (Dictionary<string, FieldInfo>, Dictionary<string, PropertyInfo>)> mapdic = new Dictionary<Type, (Dictionary<string, FieldInfo>, Dictionary<string, PropertyInfo>)>();
		public static T ToStruct<T>((string key,object value)[] keyvalues) where T : struct
		{
			(Dictionary<string, FieldInfo> fields, Dictionary<string, PropertyInfo> props) data;
			if (!mapdic.TryGetValue(typeof(T), out data))
			{
				data = (typeof(T).GetFields().ToDictionary(f => f.Name, StringComparer.CurrentCultureIgnoreCase),
					 typeof(T).GetProperties().ToDictionary(f => f.Name, StringComparer.CurrentCultureIgnoreCase));
				mapdic[typeof(T)] = data;
			}
			T res = new T();
			foreach(var kv in keyvalues)
			{
				data.fields.TryGetValue(kv.key, out var f);
				data.props.TryGetValue(kv.key, out var p);
				f?.SetValue(res, kv.value);
				p?.SetValue(res, kv.value);
			}
			return res;
		}

		public static T ToStruct<T, T1>(this ValueTuple<T1> tuple) where T : struct => tuple.ToArray().ToStruct<T>();
		public static T ToStruct<T, T1,T2>(this ValueTuple<T1,T2> tuple) where T : struct => tuple.ToArray().ToStruct<T>();
		public static T ToStruct<T, T1,T2,T3>(this ValueTuple<T1,T2,T3> tuple) where T : struct => tuple.ToArray().ToStruct<T>();
		public static T ToStruct<T, T1,T2,T3,T4>(this ValueTuple<T1,T2,T3,T4> tuple) where T : struct => tuple.ToArray().ToStruct<T>();
		public static T ToStruct<T, T1,T2,T3,T4,T5>(this ValueTuple<T1,T2,T3,T4,T5> tuple) where T : struct => tuple.ToArray().ToStruct<T>();
		public static T ToStruct<T, T1,T2,T3,T4,T5,T6>(this ValueTuple<T1,T2,T3,T4,T5,T6> tuple) where T : struct => tuple.ToArray().ToStruct<T>();
		public static T ToStruct<T, T1,T2,T3,T4,T5,T6,T7>(this ValueTuple<T1,T2,T3,T4,T5,T6,T7> tuple) where T : struct => tuple.ToArray().ToStruct<T>();
	}
}
