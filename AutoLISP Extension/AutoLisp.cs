using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetDBX
{
	/// <summary>
	/// Provides functionality to AutoLisp for .NET
	/// </summary>
	public static class AutoLisp
	{
		/// <summary>
		/// Converts a <see cref="ResultBuffer"/> to <see cref="List{TypedValue}"/>. If the Lisp arguments are below the minimum or above the maximum required, an exception will be thrown.
		/// </summary>
		/// <param name="arguments"></param>
		/// <param name="minimum"></param>
		/// <param name="maximum"></param>
		/// <returns><see cref="List{TypedValue}"/></returns>
		/// <exception cref="System.Exception"/>
		public static List<TypedValue> HandleLispArguments(ResultBuffer arguments, int minimum, int maximum)
		{
			if (arguments == null)
				throw new System.Exception("too few arguments");

			List<TypedValue> Arguments = new List<TypedValue>(arguments.AsArray());

			if (Arguments.Count < minimum)
				throw new System.Exception("too few arguments");
			else if (Arguments.Count > maximum)
				throw new System.Exception("too many arguments");

			return Arguments;
		}

		#region TypedValue Constants

		// From: http://www.theswamp.org/index.php?topic=56274.msg601709#msg601709
		// Credit: Gile
		// Region: TypedValue Constants

		/// <summary>
		/// Gets the value LispDataType.Nil
		/// </summary>
		public static TypedValue Nil => new TypedValue((short)LispDataType.Nil);

		/// <summary>
		/// Gets the value LispDataType.T_atom
		/// </summary>
		public static TypedValue T => new TypedValue((short)LispDataType.T_atom);

		/// <summary>
		/// Gets the value LispDataType.ListBegin
		/// </summary>
		public static TypedValue ListBegin => new TypedValue((short)LispDataType.ListBegin);

		/// <summary>
		/// Gets the value LispDataType.ListEnd
		/// </summary>
		public static TypedValue ListEnd => new TypedValue((short)LispDataType.ListEnd);

		/// <summary>
		/// Gets the value LispDataType.DottedPair
		/// </summary>
		public static TypedValue DottedPair => new TypedValue((short)LispDataType.DottedPair);

		/// <summary>
		/// Gets the value LispDataType.Void
		/// </summary>
		public static TypedValue Void => new TypedValue((short)LispDataType.Void);

		#endregion

		#region Type .NET -> TypedValue

		// From: http://www.theswamp.org/index.php?topic=56274.msg601709#msg601709
		// Credit: Gile
		// Region: Type .NET -> TypedValue

		/// <summary>
		/// Gets the value as a TypedValue.
		/// </summary>
		/// <param name="i">Instance of Int32 which the method applies to.</param>
		/// <returns>The value as TypedValue.</returns>
		public static TypedValue GetLispValue(this int i) => new TypedValue((short)LispDataType.Int32, i);

		/// <summary>
		/// Gets the value as a TypedValue.
		/// </summary>
		/// <param name="d">Instance of Double which the method applies to.</param>
		/// <returns>The value as TypedValue.</returns>
		public static TypedValue GetLispValue(this double d) => new TypedValue((short)LispDataType.Double, d);

		/// <summary>
		/// Gets the value as a TypedValue.
		/// </summary>
		/// <param name="s">Instance of String which the method applies to.</param>
		/// <returns>The value as TypedValue.</returns>
		public static TypedValue GetLispValue(this string s) => new TypedValue((short)LispDataType.Text, s);

		/// <summary>
		/// Gets the value as a TypedValue.
		/// </summary>
		/// <param name="id">Instance of ObjectId which the method applies to.</param>
		/// <returns>The value as TypedValue.</returns>
		public static TypedValue GetLispValue(this ObjectId id) => new TypedValue((short)LispDataType.ObjectId, id);

		/// <summary>
		/// Gets the value as a TypedValue.
		/// </summary>
		/// <param name="ss">Instance of SelectionSet which the method applies to.</param>
		/// <returns>The value as TypedValue.</returns>
		public static TypedValue GetLispValue(this SelectionSet ss) => new TypedValue((short)LispDataType.SelectionSet, ss);

		/// <summary>
		/// Gets the value as a TypedValue
		/// </summary>
		/// <param name="point">Instance of Point3d which the method applies to.</param>
		/// <returns>The value as TypedValue.</returns>
		public static TypedValue GetLispValue(this Point3d point) => new TypedValue((short)LispDataType.Point3d, point);

		/// <summary>
		/// Gets the value as a TypedValue
		/// </summary>
		/// <param name="point">Instance of Point2d which the method applies to.</param>
		/// <returns>The value as TypedValue.</returns>
		public static TypedValue GetLispValue(this Point2d point) => new TypedValue((short)LispDataType.Point2d, point);

		#endregion

		#region TypedValue -> Type .NET

		/// <summary>
		/// Converts a <see cref="TypedValue"/> to <see cref="short"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
		/// </summary>
		/// <param name="value"></param>
		/// <remarks>
		/// Acceptable LispDataTypes: Int16
		/// </remarks>
		/// <returns><see cref="short"/></returns>
		/// <exception cref="ArgumentException"/>
		public static short LispToShort(TypedValue value)
		{
			if (value.TypeCode == (short)LispDataType.Int16)
				return Convert.ToInt16(value.Value);

			throw new ArgumentException($"invalid argument type <{typeof(short)}>: <{value.Value.GetType()}> {value.Value}");
		}

		/// <summary>
		/// Converts a <see cref="TypedValue"/> to <see cref="int"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
		/// </summary>
		/// <param name="value"></param>
		/// <remarks>
		/// Acceptable LispDataTypes: Int16, Int32
		/// </remarks>
		/// <returns><see cref="int"/></returns>
		/// <exception cref="ArgumentException"/>
		public static int LispToInt(TypedValue value)
		{
			if ((value.TypeCode == (short)LispDataType.Int16)
			 || (value.TypeCode == (short)LispDataType.Int32))
				return Convert.ToInt32(value.Value);

			throw new ArgumentException($"invalid argument type <{typeof(int)}>: <{value.Value.GetType()}> {value.Value}");
		}

		/// <summary>
		/// Converts a <see cref="TypedValue"/> to <see cref="double"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
		/// </summary>
		/// <param name="value"></param>
		/// <remarks>
		/// Acceptable LispDataTypes: Angle, Orientation, Double, Int16, Int32
		/// </remarks>
		/// <returns><see cref="double"/></returns>
		/// <exception cref="ArgumentException"/>
		public static double LispToDouble(TypedValue value)
		{
			if ((value.TypeCode == (short)LispDataType.Angle)
				|| (value.TypeCode == (short)LispDataType.Orientation)
				|| (value.TypeCode == (short)LispDataType.Double)
				|| (value.TypeCode == (short)LispDataType.Int16)
				|| (value.TypeCode == (short)LispDataType.Int32))
				return Convert.ToDouble(value.Value);
			else
				throw new ArgumentException($"invalid argument type <{typeof(double)}>: <{value.Value.GetType()}> {value.Value}");
		}

		/// <summary>
		/// Converts a <see cref="TypedValue"/> to <see cref="ObjectId"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
		/// </summary>
		/// <param name="value"></param>
		/// <remarks>
		/// Acceptable LispDataTypes: ObjectId
		/// </remarks>
		/// <returns><see cref="ObjectId"/></returns>
		/// <exception cref="ArgumentException"/>
		public static ObjectId LispToObjectId(TypedValue value)
		{
			if (value.TypeCode == (short)LispDataType.ObjectId)
				return (value.Value as ObjectId?).Value;

			throw new ArgumentException($"invalid argument type <{typeof(ObjectId)}>: <{value.Value.GetType()}> {value.Value}");
		}

		/// <summary>
		/// Converts a <see cref="TypedValue"/> to <see cref="Point3d"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
		/// </summary>
		/// <param name="value"></param>
		/// <remarks>
		/// Acceptable LispDataTypes: Point3d
		/// </remarks>
		/// <returns><see cref="Point3d"/></returns>
		/// <exception cref="ArgumentException"/>
		public static Point3d LispToPoint3d(TypedValue value)
		{
			if (value.TypeCode == (short)LispDataType.Point3d)
				return (value.Value as Point3d?).Value;

			if (value.TypeCode == (short)LispDataType.Point2d)
			{
				Point2d pt = (Point2d)value.Value;
				return new Point3d(pt.X, pt.Y, 0.0);
			}

			throw new ArgumentException($"invalid argument type <{typeof(Point3d)}>: <{value.Value.GetType()}> {value.Value}");
		}

		/// <summary>
		/// Converts a <see cref="TypedValue"/> to <see cref="Point2d"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
		/// </summary>
		/// <param name="value"></param>
		/// <remarks>
		/// Acceptable LispDataTypes: Point2d
		/// </remarks>
		/// <returns><see cref="Point2d"/></returns>
		/// <exception cref="ArgumentException"/>
		public static Point2d LispToPoint2d(TypedValue value)
		{
			if (value.TypeCode == (short)LispDataType.Point2d)
				return (value.Value as Point2d?).Value;

			throw new ArgumentException($"invalid argument type <{typeof(Point2d)}>: <{value.Value.GetType()}> {value.Value}");
		}

		/// <summary>
		/// Converts a <see cref="TypedValue"/> to <see cref="string"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
		/// </summary>
		/// <param name="value"></param>
		/// <remarks>
		/// Acceptable LispDataTypes: Text
		/// </remarks>
		/// <returns><see cref="string"/></returns>
		/// <exception cref="ArgumentException"/>
		public static string LispToString(TypedValue value)
		{
			if (value.TypeCode == (short)LispDataType.Text)
				return value.Value.ToString();

			throw new ArgumentException($"invalid argument type <{typeof(string)}>: <{value.Value.GetType()}> {value.Value}");
		}

		/// <summary>
		/// Converts a <see cref="TypedValue"/> to <see cref="SelectionSet"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
		/// </summary>
		/// <param name="value"></param>
		/// <remarks>
		/// Acceptable LispDataTypes: SelectionSet
		/// </remarks>
		/// <returns><see cref="SelectionSet"/></returns>
		/// <exception cref="ArgumentException"/>
		public static SelectionSet LispToSelectionSet(TypedValue value)
		{
			if (value.TypeCode == (short)LispDataType.SelectionSet)
				return (SelectionSet)value.Value;

			throw new ArgumentException($"invalid argument type <{typeof(SelectionSet)}>: <{value.Value.GetType()}> {value.Value}");
		}

		/// <summary>
		/// Evaluates whether <see cref="TypedValue"/> is Nil. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Returns true if the TypeCode equals LispDataType.Nil; otherwise, false.</returns>
		public static bool IsNil(this TypedValue value) => value.TypeCode == (short)LispDataType.Nil;

		/// <summary>
		/// Evaluates whether <see cref="TypedValue"/> is T. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Returns true if the TypeCode equals LispDataType.T; otherwise, false.</returns>
		public static bool IsT(this TypedValue value) => value.TypeCode == (short)LispDataType.T_atom;

		/// <summary>
		/// Converts a <see cref="TypedValue[]"/> with list begin and list end to <see cref="List{TypedValue}"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
		/// </summary>
		/// <param name="values"></param>
		/// <param name="index"></param>
		/// <returns><see cref="List{TypedValue}"/></returns>
		/// <exception cref="ArgumentException"/>
		public static List<TypedValue> LispToList(this TypedValue[] values, int index)
		{
			if (values[index].IsNil())
				return null;

			if (values[index].TypeCode != (short)LispDataType.ListBegin)
				throw new ArgumentException($"invalid argument type <{typeof(List<TypedValue>)}>: <{values[index].Value.GetType()}> {values[index].Value}");

			int count = 1;
			return values
					.Skip(index + 1)
					.TakeWhile(v =>
					{
						if (v.TypeCode == (short)LispDataType.ListBegin)
							count++;
						else if (v.TypeCode == (short)LispDataType.ListEnd)
							count--;
						return count != 0;
					}).ToList<TypedValue>();
		}

		/// <summary>
		/// Converts a <see cref="List{TypedValue}"/> to a <see cref="Dictionary{string, string}"/>.
		/// </summary>
		/// <param name="values"></param>
		/// <returns><see cref="Dictionary{string, string}"/></returns>
		/// <exception cref="ArgumentException"/>
		public static Dictionary<string, string> LispToDictionary(this List<TypedValue> values)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();

			if (values.Count == 0)
				throw new ArgumentException("invalid association list");

			if (values.Count == 1)
			{
				if (values[0].IsNil())
					return dictionary;

				throw new ArgumentException("invalid association list");
			}	

			if (values[0].TypeCode != (short)LispDataType.ListBegin)
				throw new ArgumentException("invalid association list");

			if (values[values.Count - 1].TypeCode != (short)LispDataType.ListEnd)
				throw new ArgumentException("invalid association list");

			values.RemoveAt(0);
			values.RemoveAt(values.Count - 1);

			string key = null;
			string replace = null;

			for (int i = 0; i < values.Count; i++)
			{
				TypedValue value = values[i];

				if (value.TypeCode == (short)LispDataType.ListBegin)
					continue;
				if (value.TypeCode == (short)LispDataType.DottedPair)
					continue;
				else if (value.TypeCode == (short)LispDataType.ListEnd)
				{
					if (key == null)
						continue;

					if (replace == null)
						replace = "";

					dictionary[key] = replace;

					key = null;
					replace = null;
				}
				else if (key == null)
					key = LispToString(value);
				else if (replace == null)
					replace = LispToString(value);
			}

			return dictionary;
		}

		#endregion

		#region Error Handling

		/// <summary>
		/// Invoke a lisp error with message from .NET
		/// </summary>
		/// <param name="message"></param>
		public static void ThrowLispError(string message)
		{
			try
			{
				// Must include LispError.VLX in AutoCAD
				using (ResultBuffer args = new ResultBuffer())
				{
					args.Add(new TypedValue((int)LispDataType.Text, "ThrowLispError"));
					args.Add(new TypedValue((int)LispDataType.Text, message));
					Application.Invoke(args);
				}
			}
			catch (System.Exception e)
			{
				// For Debugging purposes
				_ = e;
			}
		}

		#endregion
	}
}