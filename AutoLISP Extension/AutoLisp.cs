using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Converts a <see cref="TypedValue"/> to <see cref="int"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// Acceptable LispDataTypes: Int16, Int32
        /// </remarks>
        /// <returns><see cref="int"/></returns>
        /// <exception cref="System.Exception"/>
        public static int LispToInt(TypedValue value)
        {
            if ((value.TypeCode == (short)LispDataType.Int16)
                || (value.TypeCode == (short)LispDataType.Int32))
                return Convert.ToInt32(value.Value);

            throw new System.Exception($"invalid argument type <{typeof(int)}>: <{value.Value.GetType()}> {value.Value}");
        }

        /// <summary>
        /// Converts a <see cref="TypedValue"/> to <see cref="double"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// Acceptable LispDataTypes: Angle, Double, Int16, Int32
        /// </remarks>
        /// <returns><see cref="double"/></returns>
        /// <exception cref="System.Exception"/>
        public static double LispToDouble(TypedValue value)
        {
            if ((value.TypeCode == (short)LispDataType.Angle)
                || (value.TypeCode == (short)LispDataType.Double)
                || (value.TypeCode == (short)LispDataType.Int16)
                || (value.TypeCode == (short)LispDataType.Int32))
                return Convert.ToDouble(value.Value);
            else
                throw new System.Exception($"invalid argument type <{typeof(double)}>: <{value.Value.GetType()}> {value.Value}");
        }

        /// <summary>
        /// Converts a <see cref="TypedValue"/> to <see cref="ObjectId"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// Acceptable LispDataTypes: ObjectId
        /// </remarks>
        /// <returns><see cref="ObjectId"/></returns>
        /// <exception cref="System.Exception"/>
        public static ObjectId LispToObjectId(TypedValue value)
        {
            if (value.TypeCode == (short)LispDataType.ObjectId)
                return (value.Value as ObjectId?).Value;

            throw new System.Exception($"invalid argument type <{typeof(ObjectId)}>: <{value.Value.GetType()}> {value.Value}");
        }

        /// <summary>
        /// Converts a <see cref="TypedValue"/> to <see cref="Point3d"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// Acceptable LispDataTypes: Point3d
        /// </remarks>
        /// <returns><see cref="Point3d"/></returns>
        /// <exception cref="System.Exception"/>
        public static Point3d LispToPoint3d(TypedValue value)
        {
            if (value.TypeCode == (short)LispDataType.Point3d)
                return (value.Value as Point3d?).Value;

            throw new System.Exception($"invalid argument type <{typeof(Point3d)}>: <{value.Value.GetType()}> {value.Value}");
        }

        /// <summary>
        /// Converts a <see cref="TypedValue"/> to <see cref="Point2d"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// Acceptable LispDataTypes: Point2d
        /// </remarks>
        /// <returns><see cref="Point2d"/></returns>
        /// <exception cref="System.Exception"/>
        public static Point2d LispToPoint2d(TypedValue value)
        {
            if (value.TypeCode == (short)LispDataType.Point2d)
                return (value.Value as Point2d?).Value;

            throw new System.Exception($"invalid argument type <{typeof(Point2d)}>: <{value.Value.GetType()}> {value.Value}");
        }

        /// <summary>
        /// Converts a <see cref="TypedValue"/> to <see cref="string"/>. The member <see cref="TypeCode"/> uses the <see cref="LispDataType"/> enum.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// Acceptable LispDataTypes: Text
        /// </remarks>
        /// <returns><see cref="string"/></returns>
        /// <exception cref="System.Exception"/>
        public static string LispToString(TypedValue value)
        {
            if (value.TypeCode == (short)LispDataType.Text)
                return value.Value.ToString();

            throw new System.Exception($"invalid argument type <{typeof(string)}>: <{value.Value.GetType()}> {value.Value}");
        }

        /// <summary>
        /// Invoke a lisp error with message from .NET
        /// </summary>
        /// <param name="message"></param>
        public static void ThrowLispError(string message)
        {
            try
            {
                using (ResultBuffer args = new ResultBuffer())
                {
                    args.Add(new TypedValue((int)LispDataType.Text, "ThrowLispError"));
                    args.Add(new TypedValue((int)LispDataType.Text, message));
                    Application.Invoke(args);
                }
            }
            catch (System.Exception e)
            {
                _ = e;
            }
        }
    }
}