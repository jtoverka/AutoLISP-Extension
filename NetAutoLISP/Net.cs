using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using NetDBX;
using System.Collections.Generic;
using System.IO;

namespace NetAutoLISP
{
	public static class Net
	{
		#region Fields

		private static readonly Dictionary<int, Database> databaseDictionaryField = new();
		private static int databaseCounterField = 1;

		#endregion

		#region Database Functions

		[LispFunction("ADN-9C1EC30A-C5EC-40D3-A03D-B2932A76440D")]
		public static ResultBuffer CreateDatabase()
		{
			ResultBuffer result = new();
			bool success = true;
			int index = databaseCounterField++;

			try
			{
				Database database = new(true, true);
				databaseDictionaryField[index] = database;
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
				success = false;
			}

			if (success)
				result.Add(new TypedValue((int)LispDataType.Int32, index));
			else
				result.Add(new TypedValue((int)LispDataType.Nil));

			return result;
		}

		[LispFunction("ADN-9CF301B0-559C-4A51-8C77-1D100613C9DF")]
		public static ResultBuffer OpenDatabase(ResultBuffer buffer)
		{
			ResultBuffer result = new();
			bool success = true;
			int index = databaseCounterField++;

			try
			{
				List<TypedValue> input = AutoLisp.HandleLispArguments(buffer, 1, 1);
				string filepath = AutoLisp.LispToString(input[0]);

				Database database = new(false, true);
				database.ReadDwgFile(filepath, FileShare.ReadWrite, true, "");

				databaseDictionaryField[index] = database;
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
				success = false;
			}

			if (success)
				result.Add(new TypedValue((int)LispDataType.Int32, index));
			else
				result.Add(new TypedValue((int)LispDataType.Nil));

			return result;
		}

		[LispFunction("ADN-E89476C7-535B-4E79-9B74-1A5E98F801F6")]
		public static ResultBuffer SaveDatabase(ResultBuffer buffer)
		{
			ResultBuffer result = new();
			bool success = true;

			try
			{
				List<TypedValue> input = AutoLisp.HandleLispArguments(buffer, 2, 3);
				int databaseId = AutoLisp.LispToInt(input[0]);
				string filepath = AutoLisp.LispToString(input[1]);

				DwgVersion version = DwgVersion.Current;
				if (input.Count > 2)
					version = (DwgVersion)AutoLisp.LispToInt(input[2]);

				if (!databaseDictionaryField.ContainsKey(databaseId))
					throw new System.Exception($"Database id not found: {databaseId}");

				if (File.Exists(filepath))
					File.Delete(filepath);

				if (File.Exists(filepath))
					throw new IOException($"Could not overwrite file: {filepath}");

				Database database = databaseDictionaryField[databaseId];
				try
				{
					database.SaveAs(filepath, version);
				}
				catch
				{
					throw new System.Exception($"Could not save to '{filepath}'");
				}
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
				success = false;
			}

			if (success)
				result.Add(new TypedValue((int)LispDataType.T_atom));
			else
				result.Add(new TypedValue((int)LispDataType.Nil));

			return result;
		}

		[LispFunction("ADN-76349A8D-AF85-44A3-A32E-0D7C68E99806")]
		public static ResultBuffer CloseDatabase(ResultBuffer buffer)
		{
			ResultBuffer result = new();
			bool success = true;

			try
			{
				List<TypedValue> input = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(input[0]);

				if (databaseDictionaryField.ContainsKey(databaseId))
				{
					Database database = databaseDictionaryField[databaseId];
					databaseDictionaryField.Remove(databaseId);
					database.Dispose();
				}
				else
				{
					success = false;
				}
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
				success = false;
			}

			if (success)
				result.Add(new TypedValue((int)LispDataType.T_atom));
			else
				result.Add(new TypedValue((int)LispDataType.Nil));

			return result;
		}

		[LispFunction("ADN-85F89A70-BEA5-4EE8-BCAA-7D8FDD8091DE")]
		public static ResultBuffer AlignDatabaseText(ResultBuffer buffer)
		{
			ResultBuffer result = new();
			bool success = true;
			
			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				Database database = databaseDictionaryField[databaseId];
				using Transaction transaction = database.TransactionManager.StartTransaction();

				using BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

				using WorkingDatabaseSwitcher switcher = new(database);

				foreach (ObjectId blockId in blockTable)
				{
					using BlockTableRecord blockTableRecord = transaction.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
					
					foreach (ObjectId entityId in blockTableRecord)
					{
						string className = entityId.ObjectClass.Name;

						if (className.Equals("AcDbBlockReference"))
						{
							using BlockReference reference = transaction.GetObject(entityId, OpenMode.ForRead) as BlockReference;
							AttributeCollection collection = reference.AttributeCollection;

							if (collection.Count > 0)
								reference.UpgradeOpen();
							else
								continue;


							foreach (ObjectId attributeId in collection)
							{
								using AttributeReference attribute = transaction.GetObject(attributeId, OpenMode.ForWrite) as AttributeReference;
								attribute.AdjustAlignment(database);
							}
						}
						else if (className.Equals("AcDbText"))
						{
							using DBText text = transaction.GetObject(entityId, OpenMode.ForWrite) as DBText;
							text.AdjustAlignment(database);
						}
					}
				}

				transaction.Commit();
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
				success = false;
			}

			if (success)
				result.Add(new TypedValue((int)LispDataType.T_atom));
			else
				result.Add(new TypedValue((int)LispDataType.Nil));

			return result;
		}

		#endregion
	}
}
