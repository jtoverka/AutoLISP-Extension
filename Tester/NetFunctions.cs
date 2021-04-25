using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using NetDBX;

namespace Tester
{
    public static class NetFunctions
    {
        [LispFunction("ADN-f894e51b-3905-4fe0-aef1-743aa83db0b9")]
        public static void Test(ResultBuffer buffer)
        {
            try
            {
                List<TypedValue> args = AutoLisp.HandleLispArguments(buffer, 1, 1);

                Point3d point = AutoLisp.LispToPoint3d(args[0]);
                ObjectId id = AutoLisp.LispToObjectId(args[0]);
            }
            catch (System.Exception e)
            {
                AutoLisp.ThrowLispError(e.Message);
            }
        }
    }
}
