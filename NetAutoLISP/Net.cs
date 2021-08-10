using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using NetDBX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAutoLISP
{
	public static class Net
	{
		#region Fields

		private static readonly Dictionary<int, Database> databaseDictionaryField = new();
		private static int databaseCounterField = 1;

		#endregion

		#region Database Functions

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

		#endregion

		#region Database Tables

		private static ResultBuffer GetTable(int databaseId, DatabaseTable table)
		{
			ResultBuffer result = new();
			ObjectId tableId = new();
			bool success = true;

			try
			{
				if (!databaseDictionaryField.ContainsKey(databaseId))
					throw new System.Exception($"Database id not found: {databaseId}");

				Database database = databaseDictionaryField[databaseId];

				switch (table)
				{
					case DatabaseTable.Block:
						tableId = database.BlockTableId;
						break;
					case DatabaseTable.DimStyle:
						tableId = database.DimStyleTableId;
						break;
					case DatabaseTable.Layer:
						tableId = database.LayerTableId;
						break;
					case DatabaseTable.Linetype:
						tableId = database.LinetypeTableId;
						break;
					case DatabaseTable.RegApp:
						tableId = database.RegAppTableId;
						break;
					case DatabaseTable.TextStyle:
						tableId = database.TextStyleTableId;
						break;
					case DatabaseTable.Ucs:
						tableId = database.UcsTableId;
						break;
					case DatabaseTable.Viewport:
						tableId = database.ViewportTableId;
						break;
					case DatabaseTable.View:
						tableId = database.ViewTableId;
						break;
				}
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
				success = false;
			}

			if (success)
				result.Add(new TypedValue((int)LispDataType.ObjectId, tableId));
			else
				result.Add(new TypedValue((int)LispDataType.Nil));

			return result;
		}

		public static ResultBuffer AddTableRecord(ResultBuffer buffer)
		{
			ResultBuffer result = new();

			try
			{

			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
			}

			return result;
		}

		#endregion

		#region Block Table Functions

		public static ResultBuffer GetBlockTable(ResultBuffer buffer)
		{
			ResultBuffer result = new();

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				result = GetTable(databaseId, DatabaseTable.Block);
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
			}

			return result;
		}

		public static ResultBuffer AddBlock(ResultBuffer buffer)
		{
			ResultBuffer result = new();
			ObjectId blockTableRecordId = new();
			bool success = true;

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 2, 2);
				ObjectId blockTableId = AutoLisp.LispToObjectId(inputs[0]);
				string name = AutoLisp.LispToString(inputs[1]);

				Database database = blockTableId.Database;
				Transaction transaction = database.TransactionManager.StartTransaction();

				BlockTable blockTable = transaction.GetObject(blockTableId, OpenMode.ForWrite) as BlockTable;
				BlockTableRecord blockTableRecord = new()
				{
					Name = name,
					
				};

				blockTableRecordId = blockTable.Add(blockTableRecord);
				transaction.AddNewlyCreatedDBObject(blockTableRecord, true);

				transaction.Commit();
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
				success = false;
			}

			if (success)
				result.Add(new TypedValue((int)LispDataType.ObjectId, blockTableRecordId));
			else
				result.Add(new TypedValue((int)LispDataType.Nil));

			return result;
		}

		#endregion

		#region DimStyle Table Functions

		public static ResultBuffer GetDimStyleTable(ResultBuffer buffer)
		{
			ResultBuffer result = new();

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				result = GetTable(databaseId, DatabaseTable.DimStyle);
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
			}

			return result;
		}

		#endregion

		#region Layer Table Functions

		public static ResultBuffer GetLayerTable(ResultBuffer buffer)
		{
			ResultBuffer result = new();

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				result = GetTable(databaseId, DatabaseTable.Layer);
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
			}

			return result;
		}


		#endregion

		#region Linetype Table Functions

		public static ResultBuffer GetLinetypeTable(ResultBuffer buffer)
		{
			ResultBuffer result = new();

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				result = GetTable(databaseId, DatabaseTable.Linetype);
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
			}

			return result;
		}

		#endregion

		#region RegApp Table Functions

		public static ResultBuffer GetRegAppTable(ResultBuffer buffer)
		{
			ResultBuffer result = new();

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				result = GetTable(databaseId, DatabaseTable.RegApp);
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
			}

			return result;
		}

		#endregion

		#region TextStyle Table Functions

		public static ResultBuffer GetTextStyleTable(ResultBuffer buffer)
		{
			ResultBuffer result = new();

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				result = GetTable(databaseId, DatabaseTable.TextStyle);
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
			}

			return result;
		}

		#endregion

		#region Ucs Table Functions

		public static ResultBuffer GetUcsTable(ResultBuffer buffer)
		{
			ResultBuffer result = new();

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				result = GetTable(databaseId, DatabaseTable.Ucs);
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
			}

			return result;
		}

		#endregion

		#region Viewport Table Functions

		public static ResultBuffer GetViewportTable(ResultBuffer buffer)
		{
			ResultBuffer result = new();

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				result = GetTable(databaseId, DatabaseTable.Viewport);
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
			}

			return result;
		}

		#endregion

		#region View Table Functions

		public static ResultBuffer GetViewTable(ResultBuffer buffer)
		{
			ResultBuffer result = new();

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				result = GetTable(databaseId, DatabaseTable.View);
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
			}

			return result;
		}

		#endregion

		#region Methods

		public static void AddMethod(ResultBuffer buffer) { }

		public static void Add3DFaceMethod(ResultBuffer buffer) { }

		public static void Add3DMeshMethod(ResultBuffer buffer) { }

		public static void Add3DPolyMethod(ResultBuffer buffer) { }

		public static void AddArcMethod(ResultBuffer buffer) { }

		public static void AddAttributeMethod(ResultBuffer buffer) { }
		
		public static void AddBoxMethod(ResultBuffer buffer) { }

		public static void AddCircleMethod(ResultBuffer buffer) { }

		public static void AddConeMethod(ResultBuffer buffer) { }

		public static void AddCylinderMethod(ResultBuffer buffer) { }

		public static void AddDim3PointAngularMethod(ResultBuffer buffer) { }

		public static void AddDimAlignedMethod(ResultBuffer buffer) { }

		public static void AddDimAngularMethod(ResultBuffer buffer) { }

		public static void AddDimArcMethod(ResultBuffer buffer) { }

		public static void AddDimDiametricMethod(ResultBuffer buffer) { }

		public static void AddDimOrdinateMethod(ResultBuffer buffer) { }

		public static void AddDimRadialMethod(ResultBuffer buffer) { }

		public static void AddDimRadialLargeMethod(ResultBuffer buffer) { }

		public static void AddDimRotatedMethod(ResultBuffer buffer) { }

		public static void AddEllipseMethod(ResultBuffer buffer) { }

		public static void AddEllipticalConeMethod(ResultBuffer buffer) { }

		public static void AddEllipticalCylinderMethod(ResultBuffer buffer) { }

		public static void AddExtrudedSolidMethod(ResultBuffer buffer) { }

		public static void AddExtrudedSolidAlongPathMethod(ResultBuffer buffer) { }

		public static void AddFitPointMethod(ResultBuffer buffer) { }

		public static void AddHatchMethod(ResultBuffer buffer) { }

		public static void AddItemsMethod(ResultBuffer buffer) { }

		public static void AddLeaderMethod(ResultBuffer buffer) { }

		public static void AddLeaderLineMethod(ResultBuffer buffer) { }

		public static void AddLeaderLineExMethod(ResultBuffer buffer) { }
		
		public static void AddLightWeightPolylineMethod(ResultBuffer buffer) { }
		
		public static void AddLineMethod(ResultBuffer buffer) { }

		public static void AddMInsertBlockMethod(ResultBuffer buffer) { }

		public static void AddMLeaderMethod(ResultBuffer buffer) { }

		public static void AddMLineMethod(ResultBuffer buffer) { }

		public static void AddMTextMethod(ResultBuffer buffer) { }

		public static void AddObjectMethod(ResultBuffer buffer) { }

		public static void AddPointMethod(ResultBuffer buffer) { }

		public static void AddPolyfaceMeshMethod(ResultBuffer buffer) { }

		public static void AddPolylineMethod(ResultBuffer buffer) { }

		public static void AddPViewportMethod(ResultBuffer buffer) { }

		public static void AddRasterMethod(ResultBuffer buffer) { }

		public static void AddRayMethod(ResultBuffer buffer) { }

		public static void AddRegionMethod(ResultBuffer buffer) { }

		public static void AddRevolvedSolidMethod(ResultBuffer buffer) { }

		public static void AddSectionMethod(ResultBuffer buffer) { }

		public static void AddSeparatorMethod(ResultBuffer buffer) { }

		public static void AddShapeMethod(ResultBuffer buffer) { }

		public static void AddSolidMethod(ResultBuffer buffer) { }

		public static void AddSphereMethod(ResultBuffer buffer) { }

		public static void AddSplineMethod(ResultBuffer buffer) { }

		public static void AddTableMethod(ResultBuffer buffer) { }

		public static void AddTextMethod(ResultBuffer buffer) { }

		public static void AddToleranceMethod(ResultBuffer buffer) { }

		public static void AddTorusMethod(ResultBuffer buffer) { }

		public static void AddTraceMethod(ResultBuffer buffer) { }

		public static void AddVertexMethod(ResultBuffer buffer) { }

		public static void AddWedgeMethod(ResultBuffer buffer) { }

		public static void AddXLineMethod(ResultBuffer buffer) { }

		public static void AddXRecordMethod(ResultBuffer buffer) { }

		public static void AppendInnerLoopMethod(ResultBuffer buffer) { }

		public static void AppendItemsMethod(ResultBuffer buffer) { }

		public static void AppendOuterLoopMethod(ResultBuffer buffer) { }

		public static void AppendVertexMethod(ResultBuffer buffer) { }

		public static void ArrayPolarMethod(ResultBuffer buffer) { }

		public static void ArrayRectangularMethod(ResultBuffer buffer) { }

		public static void AttachExternalReferenceMethod(ResultBuffer buffer) { }

		public static void BindMethod(ResultBuffer buffer) { }

		public static void BlockMethod(ResultBuffer buffer) { }

		public static void BooleanMethod(ResultBuffer buffer) { }

		public static void CheckInterferenceMethod(ResultBuffer buffer) { }

		public static void ClearMethod(ResultBuffer buffer) { }

		public static void ClearSubSectionMethod(ResultBuffer buffer) { }

		public static void ClearTableStyleOverridesMethod(ResultBuffer buffer) { }

		public static void ClipBoundaryMethod(ResultBuffer buffer) { }

		public static void CloseMethod(ResultBuffer buffer) { }

		public static void ConvertToAnonymousBlockMethod(ResultBuffer buffer) { }

		public static void ConvertToStaticBlockMethod(ResultBuffer buffer) { }

		public static void CopyMethod(ResultBuffer buffer) { }

		public static void CopyFromMethod(ResultBuffer buffer) { }

		public static void CopyObjectsMethod(ResultBuffer buffer) { }

		public static void CopyProfileMethod(ResultBuffer buffer) { }

		public static void CreateCellStyleMethod(ResultBuffer buffer) { }

		public static void CreateCellStyleFromStyleMethod(ResultBuffer buffer) { }

		public static void CreateContentMethod(ResultBuffer buffer) { }

		public static void CreateJogMethod(ResultBuffer buffer) { }

		public static void CreateTypedArrayMethod(ResultBuffer buffer) { }

		public static void DeleteMethod(ResultBuffer buffer) { }

		public static void DeleteCellContentMethod(ResultBuffer buffer) { }

		public static void DeleteCellStyleMethod(ResultBuffer buffer) { }

		public static void DeleteColumnsMethod(ResultBuffer buffer) { }

		public static void DeleteConfigurationMethod(ResultBuffer buffer) { }

		public static void DeleteContentMethod(ResultBuffer buffer) { }

		public static void DeleteFitPointMethod(ResultBuffer buffer) { }

		public static void DeleteProfileMethod(ResultBuffer buffer) { }

		public static void DeleteRowsMethod(ResultBuffer buffer) { }

		public static void DetachMethod(ResultBuffer buffer) { }

		public static void DisplayMethod(ResultBuffer buffer) { }

		public static void DisplayPlotPreviewMethod(ResultBuffer buffer) { }

		public static void DockMethod(ResultBuffer buffer) { }

		public static void ElevateOrderMethod(ResultBuffer buffer) { }

		public static void EnableMergeAllMethod(ResultBuffer buffer) { }

		public static void EndUndoMarkMethod(ResultBuffer buffer) { }

		public static void EraseMethod(ResultBuffer buffer) { }

		public static void EvalMethod(ResultBuffer buffer) { }

		public static void EvaluateMethod(ResultBuffer buffer) { }

		public static void ExplodeMethod(ResultBuffer buffer) { }

		public static void ExportMethod(ResultBuffer buffer) { }

		public static void ExportProfileMethod(ResultBuffer buffer) { }

		public static void FieldCodeMethod(ResultBuffer buffer) { }

		public static void FloatMethod(ResultBuffer buffer) { }

		public static void FormatValueMethod(ResultBuffer buffer) { }

		public static void GenerateLayoutMethod(ResultBuffer buffer) { }

		public static void GenerateSectionGeometryMethod(ResultBuffer buffer) { }

		public static void GenerateUsageDataMethod(ResultBuffer buffer) { }

		public static void GetAcadStateMethod(ResultBuffer buffer) { }

		public static void GetAlignmentMethod(ResultBuffer buffer) { }

		public static void GetAlignment2Method(ResultBuffer buffer) { }

		public static void GetAllProfileNamesMethod(ResultBuffer buffer) { }

		public static void GetAttachmentPointMethod(ResultBuffer buffer) { }

		public static void GetAttributesMethod(ResultBuffer buffer) { }

		public static void GetAutoScaleMethod(ResultBuffer buffer) { }

		public static void GetAutoScale2Method(ResultBuffer buffer) { }

		public static void GetBackgroundColorMethod(ResultBuffer buffer) { }

		public static void GetBackgroundColor2Method(ResultBuffer buffer) { }

		public static void GetBackgroundColorNoneMethod(ResultBuffer buffer) { }

		public static void GetBitmapsMethod(ResultBuffer buffer) { }

		public static void GetBlockAttributeValueMethod(ResultBuffer buffer) { }

		public static void GetBlockAttributeValue2Method(ResultBuffer buffer) { }

		public static void GetBlockRotationMethod(ResultBuffer buffer) { }

		public static void GetBlockScaleMethod(ResultBuffer buffer) { }

		public static void GetBlockTableRecordIdMethod(ResultBuffer buffer) { }

		public static void GetBlockTableRecordId2Method(ResultBuffer buffer) { }

		public static void GetBoundingBoxMethod(ResultBuffer buffer) { }

		public static void GetBreakHeightMethod(ResultBuffer buffer) { }

		public static void GetBulgeMethod(ResultBuffer buffer) { }

		public static void GetCanonicalMediaNamesMethod(ResultBuffer buffer) { }

		public static void GetCellAlignmentMethod(ResultBuffer buffer) { }

		public static void GetCellBackgroundColorMethod(ResultBuffer buffer) { }

		public static void GetCellBackgroundColorNoneMethod(ResultBuffer buffer) { }

		public static void GetCellClassMethod(ResultBuffer buffer) { }

		public static void GetCellContentColorMethod(ResultBuffer buffer) { }

		public static void GetCellContentColor2Method(ResultBuffer buffer) { }

		public static void GetCellDataTypeMethod(ResultBuffer buffer) { }

		public static void GetCellExtentsMethod(ResultBuffer buffer) { }

		public static void GetCellFormatMethod(ResultBuffer buffer) { }

		public static void GetCellGridColorMethod(ResultBuffer buffer) { }

		public static void GetCellGridLineWeightMethod(ResultBuffer buffer) { }

		public static void GetCellGridVisibilityMethod(ResultBuffer buffer) { }

		public static void GetCellStateMethod(ResultBuffer buffer) { }

		public static void GetCellStyleMethod(ResultBuffer buffer) { }

		public static void GetCellStyleOverridesMethod(ResultBuffer buffer) { }

		public static void GetCellStylesMethod(ResultBuffer buffer) { }

		public static void GetCellTextHeightMethod(ResultBuffer buffer) { }

		public static void GetCellTextStyleMethod(ResultBuffer buffer) { }

		public static void GetCellTypeMethod(ResultBuffer buffer) { }

		public static void GetCellValueMethod(ResultBuffer buffer) { }

		public static void GetColorMethod(ResultBuffer buffer) { }

		public static void GetColor2Method(ResultBuffer buffer) { }

		public static void GetColumnNameMethod(ResultBuffer buffer) { }

		public static void GetColumnWidthMethod(ResultBuffer buffer) { }

		public static void GetConstantAttributesMethod(ResultBuffer buffer) { }

		public static void GetContentColorMethod(ResultBuffer buffer) { }

		public static void GetContentColor2Method(ResultBuffer buffer) { }

		public static void GetContentLayoutMethod(ResultBuffer buffer) { }

		public static void GetContentTypeMethod(ResultBuffer buffer) { }

		public static void GetControlPointMethod(ResultBuffer buffer) { }

		public static void GetCornerMethod(ResultBuffer buffer) { }

		public static void GetCustomByIndexMethod(ResultBuffer buffer) { }

		public static void GetCustomByKeyMethod(ResultBuffer buffer) { }

		public static void GetCustomDataMethod(ResultBuffer buffer) { }

		public static void GetCustomScaleMethod(ResultBuffer buffer) { }

		public static void GetDataFormatMethod(ResultBuffer buffer) { }

		public static void GetDataTypeMethod(ResultBuffer buffer) { }

		public static void GetDataType2Method(ResultBuffer buffer) { }

		public static void GetDistanceMethod(ResultBuffer buffer) { }

		public static void GetDoglegDirectionMethod(ResultBuffer buffer) { }

		public static void GetDynamicBlockPropertiesMethod(ResultBuffer buffer) { }

		public static void GetEntityMethod(ResultBuffer buffer) { }

		public static void GetExtensionDictionaryMethod(ResultBuffer buffer) { }

		public static void GetFieldIdMethod(ResultBuffer buffer) { }

		public static void GetFieldId2Method(ResultBuffer buffer) { }

		public static void GetFitPointMethod(ResultBuffer buffer) { }

		public static void GetFontMethod(ResultBuffer buffer) { }

		public static void GetFormatMethod(ResultBuffer buffer) { }

		public static void GetFormat2Method(ResultBuffer buffer) { }

		public static void GetFormulaMethod(ResultBuffer buffer) { }

		public static void GetFullDrawOrderMethod(ResultBuffer buffer) { }

		public static void GetGridColorMethod(ResultBuffer buffer) { }

		public static void GetGridColor2Method(ResultBuffer buffer) { }

		public static void GetGridDoubleLineSpacingMethod(ResultBuffer buffer) { }

		public static void GetGridLineStyleMethod(ResultBuffer buffer) { }

		public static void GetGridLinetypeMethod(ResultBuffer buffer) { }

		public static void GetGridLineWeightMethod(ResultBuffer buffer) { }

		public static void GetGridLineWeight2Method(ResultBuffer buffer) { }

		public static void GetGridSpacingMethod(ResultBuffer buffer) { }

		public static void GetGridVisibilityMethod(ResultBuffer buffer) { }

		public static void GetGridVisibility2Method(ResultBuffer buffer) { }

		public static void GetHasFormulaMethod(ResultBuffer buffer) { }

		public static void GetInvisibleEdgeMethod(ResultBuffer buffer) { }

		public static void GetIsCellStyleInUseMethod(ResultBuffer buffer) { }

		public static void GetIsMergeAllEnabledMethod(ResultBuffer buffer) { }

		public static void GetKeywordMethod(ResultBuffer buffer) { }

		public static void GetLeaderIndexMethod(ResultBuffer buffer) { }

		public static void GetLeaderLineIndexesMethod(ResultBuffer buffer) { }

		public static void GetLeaderLineVerticesMethod(ResultBuffer buffer) { }

		public static void GetLiveSectionMethod(ResultBuffer buffer) { }

		public static void GetLocaleMediaNameMethod(ResultBuffer buffer) { }

		public static void GetLoopAtMethod(ResultBuffer buffer) { }

		public static void GetMarginMethod(ResultBuffer buffer) { }

		public static void GetMinimumColumnWidthMethod(ResultBuffer buffer) { }

		public static void GetMinimumRowHeightMethod(ResultBuffer buffer) { }

		public static void GetNameMethod(ResultBuffer buffer) { }

		public static void GetObjectMethod(ResultBuffer buffer) { }

		public static void GetObjectIdStringMethod(ResultBuffer buffer) { }

		public static void GetOrientationMethod(ResultBuffer buffer) { }

		public static void GetOverrideMethod(ResultBuffer buffer) { }

		public static void GetPaperMarginsMethod(ResultBuffer buffer) { }

		public static void GetPaperSizeMethod(ResultBuffer buffer) { }

		public static void GetPlotDeviceNamesMethod(ResultBuffer buffer) { }

		public static void GetPlotStyleTableNamesMethod(ResultBuffer buffer) { }

		public static void GetProjectFilePathMethod(ResultBuffer buffer) { }

		public static void GetRelativeDrawOrderMethod(ResultBuffer buffer) { }

		public static void GetRemoteFileMethod(ResultBuffer buffer) { }

		public static void GetRotationMethod(ResultBuffer buffer) { }

		public static void GetRowHeightMethod(ResultBuffer buffer) { }

		public static void GetRowTypeMethod(ResultBuffer buffer) { }

		public static void GetScaleMethod(ResultBuffer buffer) { }

		public static void GetSectionTypeSettingsMethod(ResultBuffer buffer) { }

		public static void GetSnapSpacingMethod(ResultBuffer buffer) { }

		public static void GetSubEntityMethod(ResultBuffer buffer) { }

		public static void GetTextHeightMethod(ResultBuffer buffer) { }

		public static void GetTextHeight2Method(ResultBuffer buffer) { }

		public static void GetTextRotationMethod(ResultBuffer buffer) { }

		public static void GetTextStringMethod(ResultBuffer buffer) { }

		public static void GetTextStyleMethod(ResultBuffer buffer) { }

		public static void GetTextStyle2Method(ResultBuffer buffer) { }

		public static void GetTextStyleIdMethod(ResultBuffer buffer) { }

		public static void GetUCSMatrixMethod(ResultBuffer buffer) { }

		public static void GetUniqueCellStyleNamesMethod(ResultBuffer buffer) { }

		public static void GetUniqueSectionNameMethod(ResultBuffer buffer) { }

		public static void GetValueMethod(ResultBuffer buffer) { }

		public static void GetVariableMethod(ResultBuffer buffer) { }

		public static void GetVertexCountMethod(ResultBuffer buffer) { }

		public static void GetWeightMethod(ResultBuffer buffer) { }

		public static void GetWidthMethod(ResultBuffer buffer) { }

		public static void GetWindowToPlotMethod(ResultBuffer buffer) { }

		public static void GetXDataMethod(ResultBuffer buffer) { }

		public static void GetXRecordDataMethod(ResultBuffer buffer) { }

		public static void HitTestMethod(ResultBuffer buffer) { }

		public static void ImportMethod(ResultBuffer buffer) { }

		public static void InsertBlockMethod(ResultBuffer buffer) { }

		public static void InsertColumnsMethod(ResultBuffer buffer) { }

		public static void InsertColumnsAndInheritMethod(ResultBuffer buffer) { }

		public static void InsertLoopAtMethod(ResultBuffer buffer) { }

		public static void InsertRowsMethod(ResultBuffer buffer) { }

		public static void InsertRowsAndInheritMethod(ResultBuffer buffer) { }

		public static void IntersectWithMethod(ResultBuffer buffer) { }

		public static void IsContentEditableMethod(ResultBuffer buffer) { }

		public static void IsEmptyMethod(ResultBuffer buffer) { }

		public static void IsFormatEditableMethod(ResultBuffer buffer) { }

		public static void IsMergeAllEnabledMethod(ResultBuffer buffer) { }

		public static void IsMergedCellMethod(ResultBuffer buffer) { }

		public static void IsRemoteFileMethod(ResultBuffer buffer) { }

		public static void IsURLMethod(ResultBuffer buffer) { }

		public static void ItemMethod(ResultBuffer buffer) { }

		public static void LoadMethod(ResultBuffer buffer) { }

		public static void LoadShapeFileMethod(ResultBuffer buffer) { }

		public static void MergeCellsMethod(ResultBuffer buffer) { }

		public static void MirrorMethod(ResultBuffer buffer) { }

		public static void Mirror3dMethod(ResultBuffer buffer) { }

		public static void MoveMethod(ResultBuffer buffer) { }

		public static void MoveAboveMethod(ResultBuffer buffer) { }

		public static void MoveBelowMethod(ResultBuffer buffer) { }

		public static void MoveContentMethod(ResultBuffer buffer) { }

		public static void MoveToBottomMethod(ResultBuffer buffer) { }

		public static void MoveToTopMethod(ResultBuffer buffer) { }

		public static void NewMethod(ResultBuffer buffer) { }

		public static void NumCustomInfoMethod(ResultBuffer buffer) { }

		public static void OffsetMethod(ResultBuffer buffer) { }

		public static void OpenMethod(ResultBuffer buffer) { }

		public static void PurgeAllMethod(ResultBuffer buffer) { }

		public static void PurgeFitDataMethod(ResultBuffer buffer) { }

		public static void PutRemoteFileMethod(ResultBuffer buffer) { }

		public static void RecomputeTableBLockMethod(ResultBuffer buffer) { }

		public static void RemoveMethod(ResultBuffer buffer) { }

		public static void RemoveAllOverridesMethod(ResultBuffer buffer) { }

		public static void RemoveCustomByIndexMethod(ResultBuffer buffer) { }

		public static void RemoveCustomByKeyMethod(ResultBuffer buffer) { }

		public static void RemoveItemsMethod(ResultBuffer buffer) { }

		public static void RemoveLeaderMethod(ResultBuffer buffer) { }

		public static void RemoveLeaderLineMethod(ResultBuffer buffer) { }

		public static void RemoveVertexMethod(ResultBuffer buffer) { }

		public static void RenameMethod(ResultBuffer buffer) { }

		public static void RenameCellStyleMethod(ResultBuffer buffer) { }

		public static void ReplaceMethod(ResultBuffer buffer) { }

		public static void ResetBlockMethod(ResultBuffer buffer) { }

		public static void ResetCellValueMethod(ResultBuffer buffer) { }

		public static void RestoreMethod(ResultBuffer buffer) { }


		public static void ReverseMethod(ResultBuffer buffer) { }

		public static void RotateMethod(ResultBuffer buffer) { }

		public static void Rotate3DMethod(ResultBuffer buffer) { }

		public static void SaveMethod(ResultBuffer buffer) { }

		public static void SaveAsMethod(ResultBuffer buffer) { }

		public static void ScaleEntityMethod(ResultBuffer buffer) { }

		public static void SectionSolidMethod(ResultBuffer buffer) { }

		public static void SelectMethod(ResultBuffer buffer) { }

		public static void SelectAtPointMethod(ResultBuffer buffer) { }

		public static void SelectByPolygonMethod(ResultBuffer buffer) { }

		public static void SetAlignmentMethod(ResultBuffer buffer) { }

		public static void SetAlignment2Method(ResultBuffer buffer) { }

		public static void SetAutoScaleMethod(ResultBuffer buffer) { }

		public static void SetAutoScale2Method(ResultBuffer buffer) { }

		public static void SetBackgroundCOlorMethod(ResultBuffer buffer) { }

		public static void SetBackgroundColor2Method(ResultBuffer buffer) { }

		public static void SetBitmapsMethod(ResultBuffer buffer) { }

		public static void SetBlockAttributeValueMethod(ResultBuffer buffer) { }

		public static void SetBlockAttributeValue2Method(ResultBuffer buffer) { }

		public static void SetBlockRotationMethod(ResultBuffer buffer) { }

		public static void SetBlockScaleMethod(ResultBuffer buffer) { }

		public static void SetBlockTableRecordIdMethod(ResultBuffer buffer) { }

		public static void SetBlockTableRecordId2Method(ResultBuffer buffer) { }

		public static void SetBreakHeightMethod(ResultBuffer buffer) { }

		public static void SetBulgeMethod(ResultBuffer buffer) { }

		public static void SetCellAlignmentMethod(ResultBuffer buffer) { }

		public static void SetCellBackgroundColorMethod(ResultBuffer buffer) { }

		public static void SetCellBackgroundNoneMethod(ResultBuffer buffer) { }

		public static void SetCellClasssMethod(ResultBuffer buffer) { }

		public static void SetCellContentColorMethod(ResultBuffer buffer) { }

		public static void SetCellDataTypeMethod(ResultBuffer buffer) { }

		public static void SetCellFormatMethod(ResultBuffer buffer) { }

		public static void SetCellGridColorMethod(ResultBuffer buffer) { }

		public static void SetCellGridLineweightMethod(ResultBuffer buffer) { }

		public static void SetCellGridVisibilityMethod(ResultBuffer buffer) { }

		public static void SetCellStateMethod(ResultBuffer buffer) { }

		public static void SetCellStyleMethod(ResultBuffer buffer) { }

		public static void SetCellTextHeightMethod(ResultBuffer buffer) { }

		public static void SetCellTypeMethod(ResultBuffer buffer) { }

		public static void SetCellValueMethod(ResultBuffer buffer) { }

		public static void SetCellValueFromTextMethod(ResultBuffer buffer) { }

		public static void SetColorMethod(ResultBuffer buffer) { }

		public static void SetColor2Method(ResultBuffer buffer) { }

		public static void SetColorBookColorMethod(ResultBuffer buffer) { }

		public static void SetColumnNameMethod(ResultBuffer buffer) { }

		public static void SetColumnWidthMethod(ResultBuffer buffer) { }

		public static void SetContentColorMethod(ResultBuffer buffer) { }

		public static void SetContentColor2Method(ResultBuffer buffer) { }

		public static void SetContentLayoutMethod(ResultBuffer buffer) { }

		public static void SetControlPointMethod(ResultBuffer buffer) { }

		public static void SetCustomByIndexMethod(ResultBuffer buffer) { }

		public static void SetCustomDataMethod(ResultBuffer buffer) { }

		public static void SetCustomScaleMethod(ResultBuffer buffer) { }

		public static void SetDatabaseMethod(ResultBuffer buffer) { }

		public static void SetDataFormatMethod(ResultBuffer buffer) { }

		public static void SetDataTypeMethod(ResultBuffer buffer) { }

		public static void SetDataType2Method(ResultBuffer buffer) { }

		public static void SetDoglegDirectionMethod(ResultBuffer buffer) { }

		public static void SetFieldIdMethod(ResultBuffer buffer) { }

		public static void SetFieldId2Method(ResultBuffer buffer) { }

		public static void SetFitPointMethod(ResultBuffer buffer) { }

		public static void SetFontMethod(ResultBuffer buffer) { }

		public static void SetFormatMethod(ResultBuffer buffer) { }

		public static void SetFormat2Method(ResultBuffer buffer) { }

		public static void SetFormulaMethod(ResultBuffer buffer) { }

		public static void SetGridColorMethod(ResultBuffer buffer) { }

		public static void SetGridColor2Method(ResultBuffer buffer) { }

		public static void SetGridDoubleLineSpacingMethod(ResultBuffer buffer) { }

		public static void SetGridLineStyleMethod(ResultBuffer buffer) { }

		public static void SetGridLinetypeMethod(ResultBuffer buffer) { }

		public static void SetGridLineWeightMethod(ResultBuffer buffer) { }

		public static void SetGridLineWeight2Method(ResultBuffer buffer) { }

		public static void SetGridSPacingMethod(ResultBuffer buffer) { }

		public static void SetGridVisibilityMethod(ResultBuffer buffer) { }

		public static void SetGridVisibility2Method(ResultBuffer buffer) { }

		public static void SetInvisibleEdgeMethod(ResultBuffer buffer) { }

		public static void SetLayoutsToPlotMethod(ResultBuffer buffer) { }

		public static void SetLeaderLineVerticesMethod(ResultBuffer buffer) { }

		public static void SetMarginMethod(ResultBuffer buffer) { }

		public static void SetNamesMethod(ResultBuffer buffer) { }

		public static void SetOverrideMethod(ResultBuffer buffer) { }

		public static void SetPatternMethod(ResultBuffer buffer) { }

		public static void SetProjectFilePathMethod(ResultBuffer buffer) { }

		public static void SetRelativeDrawOrderMethod(ResultBuffer buffer) { }

		public static void SetRGBMethod(ResultBuffer buffer) { }

		public static void SetRotationMethod(ResultBuffer buffer) { }

		public static void SetRowHeightMethod(ResultBuffer buffer) { }

		public static void SetScaleMethod(ResultBuffer buffer) { }

		public static void SetSnapSpacingMethod(ResultBuffer buffer) { }

		public static void SetSubSelectionMethod(ResultBuffer buffer) { }

		public static void SetTemplateIdMethod(ResultBuffer buffer) { }

		public static void SetTextMethod(ResultBuffer buffer) { }

		public static void SetTextHeightMethod(ResultBuffer buffer) { }

		public static void SetTextHeight2Method(ResultBuffer buffer) { }

		public static void SetTextRotationMethod(ResultBuffer buffer) { }

		public static void SetTextStringMethod(ResultBuffer buffer) { }

		public static void SetTextStyleMethod(ResultBuffer buffer) { }

		public static void SetTextStyle2Method(ResultBuffer buffer) { }

		public static void SetTextStyleIdMethod(ResultBuffer buffer) { }

		public static void SetToolTipMethod(ResultBuffer buffer) { }

		public static void SetValueMethod(ResultBuffer buffer) { }

		public static void SetValueFromTextMethod(ResultBuffer buffer) { }

		public static void SetVariableMethod(ResultBuffer buffer) { }

		public static void SetViewMethod(ResultBuffer buffer) { }

		public static void SetWeightMethod(ResultBuffer buffer) { }

		public static void SetWidthMethod(ResultBuffer buffer) { }

		public static void SetXDataMethod(ResultBuffer buffer) { }

		public static void SetXRecordDataMethod(ResultBuffer buffer) { }

		public static void SliceSolidMethod(ResultBuffer buffer) { }

		public static void SplitMethod(ResultBuffer buffer) { }

		public static void StartBatchModeMethod(ResultBuffer buffer) { }

		public static void SwapOrderMethod(ResultBuffer buffer) { }

		public static void TransformByMethod(ResultBuffer buffer) { }

		public static void TranslateCoordinatesMethod(ResultBuffer buffer) { }

		public static void UnloadMethod(ResultBuffer buffer) { }

		public static void UnmergeCellsMethod(ResultBuffer buffer) { }

		public static void UpdateMethod(ResultBuffer buffer) { }

		public static void UpdateMTextAttributeMethod(ResultBuffer buffer) { }

		public static void WBlockMethod(ResultBuffer buffer) { }

		#endregion

		#region Properties

		public static ResultBuffer GetActiveDimStyleProperty(ResultBuffer buffer) 
		{
			ResultBuffer result = new();
			ObjectId objectId = new();
			bool success = true;

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				if (!databaseDictionaryField.ContainsKey(databaseId))
					throw new System.Exception($"Database id not found: {databaseId}");

				Database database = databaseDictionaryField[databaseId];

				objectId = database.Dimstyle;
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
				success = false;
			}

			if (success)
				result.Add(new TypedValue((int)LispDataType.ObjectId, objectId));
			else
				result.Add(new TypedValue((int)LispDataType.Nil));

			return result;
		}

		public static void GetActiveLayerProperty(ResultBuffer buffer) { }
		public static void GetActiveLayoutProperty(ResultBuffer buffer) { }
		public static void GetActiveLinetypeProperty(ResultBuffer buffer) { }
		public static void GetActiveMaterialProperty(ResultBuffer buffer) { }
		public static void GetActiveProfileProperty(ResultBuffer buffer) { }
		public static void GetActivePViewportProperty(ResultBuffer buffer) { }
		public static void GetActiveSelectionSetProperty(ResultBuffer buffer) { }
		public static void GetActiveSpaceProperty(ResultBuffer buffer) { }
		public static void GetActiveTextStyleProperty(ResultBuffer buffer) { }
		public static void GetActiveUCSProperty(ResultBuffer buffer) { }
		public static void GetActiveViewportProperty(ResultBuffer buffer) { }
		public static void GetADCInsertUnitsDefaultSourceProperty(ResultBuffer buffer) { }
		public static void GetADCInsertUnitsDefaultTargetProperty(ResultBuffer buffer) { }
		public static void GetAdjustForBackgroundProperty(ResultBuffer buffer) { }
		public static void GetAlignmentProperty(ResultBuffer buffer) { }
		public static void GetAlignmentPointAcquisitionProperty(ResultBuffer buffer) { }
		public static void GetAlignSpaceProperty(ResultBuffer buffer) { }
		public static void GetAllowedValuesProperty(ResultBuffer buffer) { }
		public static void GetAllowLongSymbolNamesProperty(ResultBuffer buffer) { }
		public static void GetAllowManualHeightsProperty(ResultBuffer buffer) { }
		public static void GetAllowManualPositionsProperty(ResultBuffer buffer) { }
		public static void GetAltFontFileProperty(ResultBuffer buffer) { }
		public static void GetAltitudeProperty(ResultBuffer buffer) { }
		public static void GetAltRoundDistanceProperty(ResultBuffer buffer) { }
		public static void GetAltSubUnitsFactorProperty(ResultBuffer buffer) { }
		public static void GetAltSubUnitsSuffixProperty(ResultBuffer buffer) { }
		public static void GetAltSuppressLeadingZerosProperty(ResultBuffer buffer) { }
		public static void GetAltSuppressTrailingZerosProperty(ResultBuffer buffer) { }
		public static void GetAltSuppressZeroFeetProperty(ResultBuffer buffer) { }
		public static void GetAltSuppressZeroInchesProperty(ResultBuffer buffer) { }
		public static void GetAltTabletMenuFileProperty(ResultBuffer buffer) { }
		public static void GetAltTextPrefixProperty(ResultBuffer buffer) { }
		public static void GetAltTextSuffixProperty(ResultBuffer buffer) { }
		public static void GetAltTolerancePrecisionProperty(ResultBuffer buffer) { }
		public static void GetAltToleranceSuppressLeadingZerosProperty(ResultBuffer buffer) { }
		public static void GetAltToleranceSuppressTrailingZerosProperty(ResultBuffer buffer) { }
		public static void GetAltToleranceSuppressZeroFeetProperty(ResultBuffer buffer) { }
		public static void GetAltToleranceSuppressZeroInchesProperty(ResultBuffer buffer) { }
		public static void GetAltUnitsProperty(ResultBuffer buffer) { }
		public static void GetAltUnitsFormatProperty(ResultBuffer buffer) { }
		public static void GetAltUnitsPrecisionProperty(ResultBuffer buffer) { }
		public static void GetAltUnitsScaleProperty(ResultBuffer buffer) { }
		public static void GetAngleProperty(ResultBuffer buffer) { }
		public static void GetAngleFormatProperty(ResultBuffer buffer) { }
		public static void GetAngleVertexProperty(ResultBuffer buffer) { }
		public static void GetAnnotationProperty(ResultBuffer buffer) { }
		public static void GetAnnotativeProperty(ResultBuffer buffer) { }
		public static void GetApplicationProperty(ResultBuffer buffer) { }
		public static void GetArcEndParamProperty(ResultBuffer buffer) { }
		public static void GetArcLengthProperty(ResultBuffer buffer) { }
		public static void GetArcPointProperty(ResultBuffer buffer) { }
		public static void GetArcSmoothnessProperty(ResultBuffer buffer) { }
		public static void GetArcStartParamProperty(ResultBuffer buffer) { }
		public static void GetAreaProperty(ResultBuffer buffer) { }
		public static void GetArrowhead1BlockProperty(ResultBuffer buffer) { }
		public static void GetArrowhead1TypeProperty(ResultBuffer buffer) { }
		public static void GetArrowhead2BlockProperty(ResultBuffer buffer) { }
		public static void GetArrowhead2TypeProperty(ResultBuffer buffer) { }
		public static void GetArrowheadBlockProperty(ResultBuffer buffer) { }
		public static void GetArrowheadSizeProperty(ResultBuffer buffer) { }
		public static void GetArrowheadTypeProperty(ResultBuffer buffer) { }
		public static void GetArrowSizeProperty(ResultBuffer buffer) { }
		public static void GetArrowSymbolProperty(ResultBuffer buffer) { }
		public static void GetAssociativeHatchProperty(ResultBuffer buffer) { }
		public static void GetAttachmentPointProperty(ResultBuffer buffer) { }
		public static void GetAuthorProperty(ResultBuffer buffer) { }
		public static void GetAutoAuditProperty(ResultBuffer buffer) { }
		public static void GetAutomaticPlotLogProperty(ResultBuffer buffer) { }
		public static void GetAutoSaveIntervalProperty(ResultBuffer buffer) { }
		public static void GetAutoSavePathProperty(ResultBuffer buffer) { }
		public static void GetAutoSnapApertureProperty(ResultBuffer buffer) { }
		public static void GetAutoSnapApertureSizeProperty(ResultBuffer buffer) { }
		public static void GetAutoSnapMagnetProperty(ResultBuffer buffer) { }
		public static void GetAutoSnapMarkerProperty(ResultBuffer buffer) { }
		public static void GetAutoSnapMarkerColorProperty(ResultBuffer buffer) { }
		public static void GetAutoSnapMarkerSizeProperty(ResultBuffer buffer) { }
		public static void GetAutoSnapToolTipProperty(ResultBuffer buffer) { }
		public static void GetAutoTrackingVecColorProperty(ResultBuffer buffer) { }
		public static void GetAutoTrackTooltipProperty(ResultBuffer buffer) { }
		public static void GetAxisDirectionProperty(ResultBuffer buffer) { }
		public static void GetAxisPositionProperty(ResultBuffer buffer) { }
		public static void GetBackgroundColorProperty(ResultBuffer buffer) { }
		public static void GetBackgroundFillProperty(ResultBuffer buffer) { }
		public static void GetBackgroundLinesColorProperty(ResultBuffer buffer) { }
		public static void GetBackgroundLinesHiddenLineProperty(ResultBuffer buffer) { }
		public static void GetBackgroundLinesLayerProperty(ResultBuffer buffer) { }
		public static void GetBackgroundLinesLinetypeProperty(ResultBuffer buffer) { }
		public static void GetBackgroundLinesLinetypeScaleProperty(ResultBuffer buffer) { }
		public static void GetBackgroundLinesLineweightProperty(ResultBuffer buffer) { }
		public static void GetBackgroundLinesPlotStyleNameProperty(ResultBuffer buffer) { }
		public static void GetBackgroundLinesVisibleProperty(ResultBuffer buffer) { }
		public static void GetBackwardProperty(ResultBuffer buffer) { }
		public static void GetBankProperty(ResultBuffer buffer) { }
		public static void GetBasePointProperty(ResultBuffer buffer) { }
		public static void GetBaseRadiusProperty(ResultBuffer buffer) { }
		public static void GetBatchPlotProgressProperty(ResultBuffer buffer) { }
		public static void GetBeepOnErrorProperty(ResultBuffer buffer) { }
		public static void GetBigFontFileProperty(ResultBuffer buffer) { }
		public static void GetBitFlagsProperty(ResultBuffer buffer) { }
		public static void GetBlockProperty(ResultBuffer buffer) { }
		public static void GetBlockColorProperty(ResultBuffer buffer) { }
		public static void GetBlockConnectionTypeProperty(ResultBuffer buffer) { }
		public static void GetBlockRotationProperty(ResultBuffer buffer) { }
		public static void GetBlocksProperty(ResultBuffer buffer) { }
		public static void GetBlockScaleProperty(ResultBuffer buffer) { }
		public static void GetBlockScalingProperty(ResultBuffer buffer) { }
		public static void GetBlueProperty(ResultBuffer buffer) { }
		public static void GetBookNameProperty(ResultBuffer buffer) { }
		public static void GetBottomHeightProperty(ResultBuffer buffer) { }
		public static void GetBreaksEnabledProperty(ResultBuffer buffer) { }
		public static void GetBreakSizeProperty(ResultBuffer buffer) { }
		public static void GetBreakSpacingProperty(ResultBuffer buffer) { }
		public static void GetBrightnessProperty(ResultBuffer buffer) { }
		public static void GetCanonicalMediaNameProperty(ResultBuffer buffer) { }
		public static void GetCaptionProperty(ResultBuffer buffer) { }
		public static void GetCategoryNameProperty(ResultBuffer buffer) { }
		public static void GetCenterProperty(ResultBuffer buffer) { }
		public static void GetCenterMarkSizeProperty(ResultBuffer buffer) { }
		public static void GetCenterPlotProperty(ResultBuffer buffer) { }
		public static void GetCenterPointProperty(ResultBuffer buffer) { }
		public static void GetCenterTypeProperty(ResultBuffer buffer) { }
		public static void GetCentroidProperty(ResultBuffer buffer) { }
		public static void GetCheckProperty(ResultBuffer buffer) { }
		public static void GetChordPointProperty(ResultBuffer buffer) { }
		public static void GetCircumferenceProperty(ResultBuffer buffer) { }
		public static void GetClippedProperty(ResultBuffer buffer) { }
		public static void GetClippingEnabledProperty(ResultBuffer buffer) { }
		public static void GetClosedProperty(ResultBuffer buffer) { }
		public static void GetClosed2Property(ResultBuffer buffer) { }
		public static void GetColorProperty(ResultBuffer buffer) { }
		public static void GetColorBookPathProperty(ResultBuffer buffer) { }
		public static void GetColorIndexProperty(ResultBuffer buffer) { }
		public static void GetColorMethodProperty(ResultBuffer buffer) { }
		public static void GetColorNameProperty(ResultBuffer buffer) { }
		public static void GetColorSchemeProperty(ResultBuffer buffer) { }
		public static void GetColumnsProperty(ResultBuffer buffer) { }
		public static void GetColumnSpacingProperty(ResultBuffer buffer) { }
		public static void GetColumnWidthProperty(ResultBuffer buffer) { }
		public static void GetCommandDisplayNameProperty(ResultBuffer buffer) { }
		public static void GetCommentProperty(ResultBuffer buffer) { }
		public static void GetCommentsProperty(ResultBuffer buffer) { }
		public static void GetConfigFileProperty(ResultBuffer buffer) { }
		public static void GetConfigNameProperty(ResultBuffer buffer) { }
		public static void GetConstantProperty(ResultBuffer buffer) { }
		public static void GetConstantWidthProperty(ResultBuffer buffer) { }
		public static void GetConstrainProperty(ResultBuffer buffer) { }
		public static void GetContentBlockNameProperty(ResultBuffer buffer) { }
		public static void GetContentBlockTypeProperty(ResultBuffer buffer) { }
		public static void GetContentTypeProperty(ResultBuffer buffer) { }
		public static void GetContinuousPlotLogProperty(ResultBuffer buffer) { }
		public static void GetContourLinesPerSurfaceProperty(ResultBuffer buffer) { }
		public static void GetContrastProperty(ResultBuffer buffer) { }
		public static void GetControlPointsProperty(ResultBuffer buffer) { }
		public static void GetCoordinateProperty(ResultBuffer buffer) { }
		public static void GetCoordinatesProperty(ResultBuffer buffer) { }
		public static void GetCountProperty(ResultBuffer buffer) { }
		public static void GetCreaseLevelProperty(ResultBuffer buffer) { }
		public static void GetCreaseTypeProperty(ResultBuffer buffer) { }
		public static void GetCreateBackupProperty(ResultBuffer buffer) { }
		public static void GetCurrentSectionTypeProperty(ResultBuffer buffer) { }
		public static void GetCursorSizeProperty(ResultBuffer buffer) { }
		public static void GetCurveTangencyLinesColorProperty(ResultBuffer buffer) { }
		public static void GetCurveTangencyLinesLayerProperty(ResultBuffer buffer) { }
		public static void GetCurveTangencyLinesLinetypeProperty(ResultBuffer buffer) { }
		public static void GetCurveTangencyLinesLinetypeScaleProperty(ResultBuffer buffer) { }
		public static void GetCurveTangencyLinesLineweightProperty(ResultBuffer buffer) { }
		public static void GetCurveTangencyLinesPlotStyleNameProperty(ResultBuffer buffer) { }
		public static void GetCurveTangencyLinesVisibleProperty(ResultBuffer buffer) { }
		public static void GetCustomDictionaryProperty(ResultBuffer buffer) { }
		public static void GetCustomIconPathProperty(ResultBuffer buffer) { }
		public static void GetCustomScaleProperty(ResultBuffer buffer) { }
		public static void GetCvHullDisplayProperty(ResultBuffer buffer) { }
		public static void GetDatabaseProperty(ResultBuffer buffer) { }
		public static void GetDecimalSeparatorProperty(ResultBuffer buffer) { }
		public static void GetDefaultInternetURLProperty(ResultBuffer buffer) { }
		public static void GetDefaultOutputDeviceProperty(ResultBuffer buffer) { }
		public static void GetDefaultPlotStyleForLayerProperty(ResultBuffer buffer) { }
		public static void GetDefaultPlotStyleForObjectsProperty(ResultBuffer buffer) { }
		public static void GetDefaultPlotStyleTableProperty(ResultBuffer buffer) { }
		public static void GetDefaultPlotToFilePathProperty(ResultBuffer buffer) { }
		public static void GetDegreeProperty(ResultBuffer buffer) { }
		public static void GetDegree2Property(ResultBuffer buffer) { }
		public static void GetDeltaProperty(ResultBuffer buffer) { }
		public static void GetDemandLoadARXAppProperty(ResultBuffer buffer) { }
		public static void GetDescriptionProperty(ResultBuffer buffer) { }
		public static void GetDestinationBlockProperty(ResultBuffer buffer) { }
		public static void GetDestinationFileProperty(ResultBuffer buffer) { }
		public static void GetDiameterProperty(ResultBuffer buffer) { }
		public static void GetDictionariesProperty(ResultBuffer buffer) { }
		public static void GetDimConstrDescProperty(ResultBuffer buffer) { }
		public static void GetDimConstrExpressionProperty(ResultBuffer buffer) { }
		public static void GetDimConstrFormProperty(ResultBuffer buffer) { }
		public static void GetDimConstrNameProperty(ResultBuffer buffer) { }
		public static void GetDimConstrReferenceProperty(ResultBuffer buffer) { }
		public static void GetDimConstrValueProperty(ResultBuffer buffer) { }
		public static void GetDimensionLineColorProperty(ResultBuffer buffer) { }
		public static void GetDimensionLineExtendProperty(ResultBuffer buffer) { }
		public static void GetDimensionLinetypeProperty(ResultBuffer buffer) { }
		public static void GetDimensionLineWeightProperty(ResultBuffer buffer) { }
		public static void GetDimLine1SuppressProperty(ResultBuffer buffer) { }
		public static void GetDimLine2SuppressProperty(ResultBuffer buffer) { }
		public static void GetDimLineInsideProperty(ResultBuffer buffer) { }
		public static void GetDimLineSuppressProperty(ResultBuffer buffer) { }
		public static void GetDimStylesProperty(ResultBuffer buffer) { }
		public static void GetDimTxtDirectionProperty(ResultBuffer buffer) { }
		public static void GetDirectionProperty(ResultBuffer buffer) { }
		public static void GetDirectionVectorProperty(ResultBuffer buffer) { }
		public static void GetDisplayProperty(ResultBuffer buffer) { }
		public static void GetDisplayGripsProperty(ResultBuffer buffer) { }
		public static void GetDisplayGripsWithinBlocksProperty(ResultBuffer buffer) { }
		public static void GetDisplayLayoutTabsProperty(ResultBuffer buffer) { }
		public static void GetDisplayLockedProperty(ResultBuffer buffer) { }
		public static void GetDisplayOLEScaleProperty(ResultBuffer buffer) { }
		public static void GetDisplayScreenMenuProperty(ResultBuffer buffer) { }
		public static void GetDisplayScrollBarsProperty(ResultBuffer buffer) { }
		public static void GetDisplaySilhouetteProperty(ResultBuffer buffer) { }
		public static void GetDockedVisibleLinesProperty(ResultBuffer buffer) { }
		public static void GetDockStatusProperty(ResultBuffer buffer) { }
		public static void GetDocumentProperty(ResultBuffer buffer) { }
		public static void GetDocumentsProperty(ResultBuffer buffer) { }
		public static void GetDogLeggedProperty(ResultBuffer buffer) { }
		public static void GetDoglegLengthProperty(ResultBuffer buffer) { }
		public static void GetDraftingProperty(ResultBuffer buffer) { }
		public static void GetDrawingDirectionProperty(ResultBuffer buffer) { }
		public static void GetDrawLeaderOrderTypeProperty(ResultBuffer buffer) { }
		public static void GetDrawMLeaderOrderTypeProperty(ResultBuffer buffer) { }
		public static void GetDriversPathProperty(ResultBuffer buffer) { }
		public static void GetDWFFormatProperty(ResultBuffer buffer) { }
		public static void GetEdgeExtensionDistancesProperty(ResultBuffer buffer) { }
		public static void GetEffectiveNameProperty(ResultBuffer buffer) { }
		public static void GetElevationProperty(ResultBuffer buffer) { }
		public static void GetElevationModelSpaceProperty(ResultBuffer buffer) { }
		public static void GetElevationPaperSpaceProperty(ResultBuffer buffer) { }
		public static void GetEnableProperty(ResultBuffer buffer) { }
		public static void GetEnableBlockRotationProperty(ResultBuffer buffer) { }
		public static void GetEnableBlockScaleProperty(ResultBuffer buffer) { }
		public static void GetEnableBreakProperty(ResultBuffer buffer) { }
		public static void GetEnableDoglegProperty(ResultBuffer buffer) { }
		public static void GetEnableFrameTextProperty(ResultBuffer buffer) { }
		public static void GetEnableLandingProperty(ResultBuffer buffer) { }
		public static void GetEnableStartupDialogProperty(ResultBuffer buffer) { }
		public static void GetEndAngleProperty(ResultBuffer buffer) { }
		public static void GetEndDraftAngleProperty(ResultBuffer buffer) { }
		public static void GetEndDraftMagnitudeProperty(ResultBuffer buffer) { }
		public static void GetEndParameterProperty(ResultBuffer buffer) { }
		public static void GetEndPointProperty(ResultBuffer buffer) { }
		public static void GetEndSmoothContinuityProperty(ResultBuffer buffer) { }
		public static void GetEndSmoothMagnitudeProperty(ResultBuffer buffer) { }
		public static void GetEndSubMenuLevelProperty(ResultBuffer buffer) { }
		public static void GetEndTangentProperty(ResultBuffer buffer) { }
		public static void GetEnterpriseMenuFileProperty(ResultBuffer buffer) { }
		public static void GetEntityColorProperty(ResultBuffer buffer) { }
		public static void GetEntityTransparencyProperty(ResultBuffer buffer) { }
		public static void GetExplodableProperty(ResultBuffer buffer) { }
		public static void GetExtensionLineColorProperty(ResultBuffer buffer) { }
		public static void GetExtensionLineExtendProperty(ResultBuffer buffer) { }
		public static void GetExtensionLineOffsetProperty(ResultBuffer buffer) { }
		public static void GetExtensionLineWeightProperty(ResultBuffer buffer) { }
		public static void GetExtLine1EndPointProperty(ResultBuffer buffer) { }
		public static void GetExtLine1LinetypeProperty(ResultBuffer buffer) { }
		public static void GetExtLine1PointProperty(ResultBuffer buffer) { }
		public static void GetExtLine1StartPointProperty(ResultBuffer buffer) { }
		public static void GetExtLine1SuppressProperty(ResultBuffer buffer) { }
		public static void GetExtLine2EndPointProperty(ResultBuffer buffer) { }
		public static void GetExtLine2LinetypeProperty(ResultBuffer buffer) { }
		public static void GetExtLine2PointProperty(ResultBuffer buffer) { }
		public static void GetExtLine2StartPointProperty(ResultBuffer buffer) { }
		public static void GetExtLine2SuppressProperty(ResultBuffer buffer) { }
		public static void GetExtLineFixedLenProperty(ResultBuffer buffer) { }
		public static void GetExtLineFixedLenSuppressProperty(ResultBuffer buffer) { }
		public static void GetFaceCountProperty(ResultBuffer buffer) { }
		public static void GetFadeProperty(ResultBuffer buffer) { }
		public static void GetFieldLengthProperty(ResultBuffer buffer) { }
		public static void GetFileProperty(ResultBuffer buffer) { }
		public static void GetFilesProperty(ResultBuffer buffer) { }
		public static void GetFirstSegmentAngleConstraintProperty(ResultBuffer buffer) { }
		public static void GetFitProperty(ResultBuffer buffer) { }
		public static void GetFitPointsProperty(ResultBuffer buffer) { }
		public static void GetFitToleranceProperty(ResultBuffer buffer) { }
		public static void GetFloatingRowsProperty(ResultBuffer buffer) { }
		public static void GetFlowDirectionProperty(ResultBuffer buffer) { }
		public static void GetFlyoutProperty(ResultBuffer buffer) { }
		public static void GetFontFileProperty(ResultBuffer buffer) { }
		public static void GetFontFileMapProperty(ResultBuffer buffer) { }
		public static void GetForceLineInsideProperty(ResultBuffer buffer) { }
		public static void GetForegroundLinesColorProperty(ResultBuffer buffer) { }
		public static void GetForegroundLinesEdgeTransparencyProperty(ResultBuffer buffer) { }
		public static void GetForegroundLinesFaceTransparencyProperty(ResultBuffer buffer) { }
		public static void GetForegroundLinesHiddenLineProperty(ResultBuffer buffer) { }
		public static void GetForegroundLinesLayerProperty(ResultBuffer buffer) { }
		public static void GetForegroundLinesLinetypeProperty(ResultBuffer buffer) { }
		public static void GetForegroundLinesLinetypeScaleProperty(ResultBuffer buffer) { }
		public static void GetForegroundLinesLineweightProperty(ResultBuffer buffer) { }
		public static void GetForegroundLinesPlotStyleNameProperty(ResultBuffer buffer) { }
		public static void GetForegroundLinesVisibleProperty(ResultBuffer buffer) { }
		public static void GetFoundPathProperty(ResultBuffer buffer) { }
		public static void GetFractionFormatProperty(ResultBuffer buffer) { }
		public static void GetFreezeProperty(ResultBuffer buffer) { }
		public static void GetFullCRCValidationProperty(ResultBuffer buffer) { }
		public static void GetFullNameProperty(ResultBuffer buffer) { }
		public static void GetFullScreenTrackingVectorProperty(ResultBuffer buffer) { }
		public static void GetGenerationOptionsProperty(ResultBuffer buffer) { }
		public static void GetGeoImageBrightnessProperty(ResultBuffer buffer) { }
		public static void GetGeoImageContrastProperty(ResultBuffer buffer) { }
		public static void GetGeoImageFadeProperty(ResultBuffer buffer) { }
		public static void GetGeoImageHeightProperty(ResultBuffer buffer) { }
		public static void GetGeoImagePositionProperty(ResultBuffer buffer) { }
		public static void GetGeoImageWidthProperty(ResultBuffer buffer) { }
		public static void GetGeolocateProperty(ResultBuffer buffer) { }
		public static void GetGradientAngleProperty(ResultBuffer buffer) { }
		public static void GetGradientCenteredProperty(ResultBuffer buffer) { }
		public static void GetGradientColor1Property(ResultBuffer buffer) { }
		public static void GetGradientColor2Property(ResultBuffer buffer) { }
		public static void GetGradientNameProperty(ResultBuffer buffer) { }
		public static void GetGraphicsWinLayoutBackgrndColorProperty(ResultBuffer buffer) { }
		public static void GetGraphicsWinModelBackgrndColorProperty(ResultBuffer buffer) { }
		public static void GetGreenProperty(ResultBuffer buffer) { }
		public static void GetGridOnProperty(ResultBuffer buffer) { }
		public static void GetGripColorSelectedProperty(ResultBuffer buffer) { }
		public static void GetGripColorUnselectedProperty(ResultBuffer buffer) { }
		public static void GetGripSizeProperty(ResultBuffer buffer) { }
		public static void GetGroupsProperty(ResultBuffer buffer) { }
		public static void GetHandleProperty(ResultBuffer buffer) { }
		public static void GetHasAttributesProperty(ResultBuffer buffer) { }
		public static void GetHasExtensionDictionaryProperty(ResultBuffer buffer) { }
		public static void GetHasLeaderProperty(ResultBuffer buffer) { }
		public static void GetHasSheetViewProperty(ResultBuffer buffer) { }
		public static void GetHasSubSelectionProperty(ResultBuffer buffer) { }
		public static void GetHasVpAssociationProperty(ResultBuffer buffer) { }
		public static void GetHatchObjectTypeProperty(ResultBuffer buffer) { }
		public static void GetHatchStyleProperty(ResultBuffer buffer) { }
		public static void GetHeaderSuppressedProperty(ResultBuffer buffer) { }
		public static void GetHeightProperty(ResultBuffer buffer) { }
		public static void GetHelpFilePathProperty(ResultBuffer buffer) { }
		public static void GetHelpStringProperty(ResultBuffer buffer) { }
		public static void GetHistoryProperty(ResultBuffer buffer) { }
		public static void GetHistoryLinesProperty(ResultBuffer buffer) { }
		public static void GetHorizontalTextPositionProperty(ResultBuffer buffer) { }
		public static void GetHorzCellMarginProperty(ResultBuffer buffer) { }
		public static void GetHWNDProperty(ResultBuffer buffer) { }
		public static void GetHyperlinkBaseProperty(ResultBuffer buffer) { }
		public static void GetHyperlinkDisplayCursorProperty(ResultBuffer buffer) { }
		public static void GetHyperlinksProperty(ResultBuffer buffer) { }
		public static void GetImageFileProperty(ResultBuffer buffer) { }
		public static void GetImageFrameHighlightProperty(ResultBuffer buffer) { }
		public static void GetImageHeightProperty(ResultBuffer buffer) { }
		public static void GetImageVisibilityProperty(ResultBuffer buffer) { }
		public static void GetImageWidthProperty(ResultBuffer buffer) { }
		public static void GetIncrementalSavePercentProperty(ResultBuffer buffer) { }
		public static void GetIndexProperty(ResultBuffer buffer) { }
		public static void GetIndicatorFillColorProperty(ResultBuffer buffer) { }
		public static void GetIndicatorTransparencyProperty(ResultBuffer buffer) { }
		public static void GetInsertionPointProperty(ResultBuffer buffer) { }
		public static void GetInsUnitsProperty(ResultBuffer buffer) { }
		public static void GetInsUnitsFactorProperty(ResultBuffer buffer) { }
		public static void GetIntensityColorSchemeProperty(ResultBuffer buffer) { }
		public static void GetIntersectionBoundaryColorProperty(ResultBuffer buffer) { }
		public static void GetIntersectionBoundaryDivisionLinesProperty(ResultBuffer buffer) { }
		public static void GetIntersectionBoundaryLayerProperty(ResultBuffer buffer) { }
		public static void GetIntersectionBoundaryLinetypeProperty(ResultBuffer buffer) { }
		public static void GetIntersectionBoundaryLinetypeScaleProperty(ResultBuffer buffer) { }
		public static void GetIntersectionBoundaryLineweightProperty(ResultBuffer buffer) { }
		public static void GetIntersectionBoundaryPlotStyleNameProperty(ResultBuffer buffer) { }
		public static void GetIntersectionBoundaryVisibleProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillColorProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillFaceTransparencyProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillHatchAngleProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillHatchPatternNameProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillHatchPatternTypeProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillHatchScaleProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillHatchSpacingProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillLayerProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillLinetypeProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillLinetypeScaleProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillLineweightProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillPlotStyleNameProperty(ResultBuffer buffer) { }
		public static void GetIntersectionFillVisibleProperty(ResultBuffer buffer) { }
		public static void GetInvisibleProperty(ResultBuffer buffer) { }
		public static void GetIsClonedProperty(ResultBuffer buffer) { }
		public static void GetIsDynamicBlockProperty(ResultBuffer buffer) { }
		public static void GetIsLayoutProperty(ResultBuffer buffer) { }
		public static void GetISOPenWidthProperty(ResultBuffer buffer) { }
		public static void GetIsOwnerXlatedProperty(ResultBuffer buffer) { }
		public static void GetIsPartialProperty(ResultBuffer buffer) { }
		public static void GetIsPeriodicProperty(ResultBuffer buffer) { }
		public static void GetIsPlanarProperty(ResultBuffer buffer) { }
		public static void GetIsPrimaryProperty(ResultBuffer buffer) { }
		public static void GetIsQuiescentProperty(ResultBuffer buffer) { }
		public static void GetIsRationalProperty(ResultBuffer buffer) { }
		public static void GetIssuerProperty(ResultBuffer buffer) { }
		public static void GetIsXRefProperty(ResultBuffer buffer) { }
		public static void GetItemNameProperty(ResultBuffer buffer) { }
		public static void GetJogAngleProperty(ResultBuffer buffer) { }
		public static void GetJogLocationProperty(ResultBuffer buffer) { }
		public static void GetJustificationProperty(ResultBuffer buffer) { }
		public static void GetKeyProperty(ResultBuffer buffer) { }
		public static void GetKeyboardAcceleratorProperty(ResultBuffer buffer) { }
		public static void GetKeyboardPriorityProperty(ResultBuffer buffer) { }
		public static void GetKeywordsProperty(ResultBuffer buffer) { }
		public static void GetKnotParameterizationProperty(ResultBuffer buffer) { }
		public static void GetKnotsProperty(ResultBuffer buffer) { }
		public static void GetLabelProperty(ResultBuffer buffer) { }
		public static void GetLabelBlockIdProperty(ResultBuffer buffer) { }
		public static void GetLandingGapProperty(ResultBuffer buffer) { }
		public static void GetLargeButtonsProperty(ResultBuffer buffer) { }
		public static void GetLastHeightProperty(ResultBuffer buffer) { }
		public static void GetLastSavedByProperty(ResultBuffer buffer) { }
		public static void GetLatitudeProperty(ResultBuffer buffer) { }
		public static void GetLayerProperty(ResultBuffer buffer) { }
		public static void GetLayerOnProperty(ResultBuffer buffer) { }
		public static void GetLayerPropertyOverridesProperty(ResultBuffer buffer) { }
		public static void GetLayersProperty(ResultBuffer buffer) { }
		public static void GetLayerStateProperty(ResultBuffer buffer) { }
		public static void GetLayoutProperty(ResultBuffer buffer) { }
		public static void GetLayoutCreateViewportProperty(ResultBuffer buffer) { }
		public static void GetLayoutCrosshairColorProperty(ResultBuffer buffer) { }
		public static void GetLayoutDisplayMarginsProperty(ResultBuffer buffer) { }
		public static void GetLayoutDisplayPaperProperty(ResultBuffer buffer) { }
		public static void GetLayoutDisplayPaperShadowProperty(ResultBuffer buffer) { }
		public static void GetLayoutIDProperty(ResultBuffer buffer) { }
		public static void GetLayoutsProperty(ResultBuffer buffer) { }
		public static void GetLayoutShowPlotSetupProperty(ResultBuffer buffer) { }
		public static void GetLeader1PointProperty(ResultBuffer buffer) { }
		public static void GetLeader2PointProperty(ResultBuffer buffer) { }
		public static void GetLeaderCountProperty(ResultBuffer buffer) { }
		public static void GetLeaderLengthProperty(ResultBuffer buffer) { }
		public static void GetLeaderLineColorProperty(ResultBuffer buffer) { }
		public static void GetLeaderLinetypeProperty(ResultBuffer buffer) { }
		public static void GetLeaderLineTypeIdProperty(ResultBuffer buffer) { }
		public static void GetLeaderLineWeightProperty(ResultBuffer buffer) { }
		public static void GetLeaderTypeProperty(ResultBuffer buffer) { }
		public static void GetLeftProperty(ResultBuffer buffer) { }
		public static void GetLengthProperty(ResultBuffer buffer) { }
		public static void GetLensLengthProperty(ResultBuffer buffer) { }
		public static void GetLimitsProperty(ResultBuffer buffer) { }
		public static void GetLinearScaleFactorProperty(ResultBuffer buffer) { }
		public static void GetLineSpacingDistanceProperty(ResultBuffer buffer) { }
		public static void GetLineSpacingFactorProperty(ResultBuffer buffer) { }
		public static void GetLineSpacingStyleProperty(ResultBuffer buffer) { }
		public static void GetLinetypeProperty(ResultBuffer buffer) { }
		public static void GetLinetypeGenerationProperty(ResultBuffer buffer) { }
		public static void GetLinetypesProperty(ResultBuffer buffer) { }
		public static void GetLinetypeScaleProperty(ResultBuffer buffer) { }
		public static void GetLineweightProperty(ResultBuffer buffer) { }
		public static void GetLineweightDisplayProperty(ResultBuffer buffer) { }
		public static void GetLiveSectionEnabledProperty(ResultBuffer buffer) { }
		public static void GetLoadAcadLspInAllDocumentsProperty(ResultBuffer buffer) { }
		public static void GetLocaleIDProperty(ResultBuffer buffer) { }
		public static void GetLockProperty(ResultBuffer buffer) { }
		public static void GetLockAspectRatioProperty(ResultBuffer buffer) { }
		public static void GetLockedProperty(ResultBuffer buffer) { }
		public static void GetLockPositionProperty(ResultBuffer buffer) { }
		public static void GetLogFileOnProperty(ResultBuffer buffer) { }
		public static void GetLogFilePathProperty(ResultBuffer buffer) { }
		public static void GetLongitudeProperty(ResultBuffer buffer) { }
		public static void GetLowerLeftCornerProperty(ResultBuffer buffer) { }
		public static void GetMacroProperty(ResultBuffer buffer) { }
		public static void GetMainDictionaryProperty(ResultBuffer buffer) { }
		public static void GetMaintainAssociativityProperty(ResultBuffer buffer) { }
		public static void GetMajorAxisProperty(ResultBuffer buffer) { }
		public static void GetMajorRadiusProperty(ResultBuffer buffer) { }
		public static void GetMaskProperty(ResultBuffer buffer) { }
		public static void GetMaterialProperty(ResultBuffer buffer) { }
		public static void GetMaterialsProperty(ResultBuffer buffer) { }
		public static void GetMaxActiveViewportsProperty(ResultBuffer buffer) { }
		public static void GetMaxAutoCADWindowProperty(ResultBuffer buffer) { }
		public static void GetMaxLeaderSegmentsPointsProperty(ResultBuffer buffer) { }
		public static void GetMCloseProperty(ResultBuffer buffer) { }
		public static void GetMDensityProperty(ResultBuffer buffer) { }
		public static void GetMeasurementProperty(ResultBuffer buffer) { }
		public static void GetMenuBarProperty(ResultBuffer buffer) { }
		public static void GetMenuFileProperty(ResultBuffer buffer) { }
		public static void GetMenuFileNameProperty(ResultBuffer buffer) { }
		public static void GetMenuGroupsProperty(ResultBuffer buffer) { }
		public static void GetMenusProperty(ResultBuffer buffer) { }
		public static void GetMinimumTableHeightProperty(ResultBuffer buffer) { }
		public static void GetMinimumTableWidthProperty(ResultBuffer buffer) { }
		public static void GetMinorAxisProperty(ResultBuffer buffer) { }
		public static void GetMinorRadiusProperty(ResultBuffer buffer) { }
		public static void GetMLineScaleProperty(ResultBuffer buffer) { }
		public static void GetModeProperty(ResultBuffer buffer) { }
		public static void GetModelCrosshairColorProperty(ResultBuffer buffer) { }
		public static void GetModelSpaceProperty(ResultBuffer buffer) { }
		public static void GetModelTypeProperty(ResultBuffer buffer) { }
		public static void GetModelViewProperty(ResultBuffer buffer) { }
		public static void GetMomentOfInertiaProperty(ResultBuffer buffer) { }
		public static void GetMonochromeProperty(ResultBuffer buffer) { }
		public static void GetMRUNumberProperty(ResultBuffer buffer) { }
		public static void GetMSpaceProperty(ResultBuffer buffer) { }
		public static void GetMTextAttributeProperty(ResultBuffer buffer) { }
		public static void GetMTextAttributeContentProperty(ResultBuffer buffer) { }
		public static void GetMTextBoundaryWidthProperty(ResultBuffer buffer) { }
		public static void GetMTextDrawingDirectionProperty(ResultBuffer buffer) { }
		public static void GetMVertexCountProperty(ResultBuffer buffer) { }
		public static void GetNameProperty(ResultBuffer buffer) { }
		public static void GetNameNoMnemonicProperty(ResultBuffer buffer) { }
		public static void GetNCloseProperty(ResultBuffer buffer) { }
		public static void GetNDensityProperty(ResultBuffer buffer) { }
		public static void GetNormalProperty(ResultBuffer buffer) { }
		public static void GetNotesProperty(ResultBuffer buffer) { }
		public static void GetNumberOfControlPointsProperty(ResultBuffer buffer) { }
		public static void GetNumberOfCopiesProperty(ResultBuffer buffer) { }
		public static void GetNumberOfFacesProperty(ResultBuffer buffer) { }
		public static void GetNumberOfFitPointsProperty(ResultBuffer buffer) { }
		public static void GetNumberOfLoopsProperty(ResultBuffer buffer) { }
		public static void GetNumberOfVerticesProperty(ResultBuffer buffer) { }
		public static void GetNumCellStylesProperty(ResultBuffer buffer) { }
		public static void GetNumCrossSectionsProperty(ResultBuffer buffer) { }
		public static void GetNumGuidePathsProperty(ResultBuffer buffer) { }
		public static void GetNumVerticesProperty(ResultBuffer buffer) { }
		public static void GetNVertexCountProperty(ResultBuffer buffer) { }
		public static void GetObjectIDProperty(ResultBuffer buffer) { }
		public static void GetObjectNameProperty(ResultBuffer buffer) { }
		public static void GetObjectSnapModeProperty(ResultBuffer buffer) { }
		public static void GetObjectSortByPlottingProperty(ResultBuffer buffer) { }
		public static void GetObjectSortByPSOutputProperty(ResultBuffer buffer) { }
		public static void GetObjectSortByRedrawsProperty(ResultBuffer buffer) { }
		public static void GetObjectSortByRegensProperty(ResultBuffer buffer) { }
		public static void GetObjectSortBySelectionProperty(ResultBuffer buffer) { }
		public static void GetObjectSortBySnapProperty(ResultBuffer buffer) { }
		public static void GetObliqueAngleProperty(ResultBuffer buffer) { }
		public static void GetOleItemTypeProperty(ResultBuffer buffer) { }
		public static void GetOLELaunchProperty(ResultBuffer buffer) { }
		public static void GetOlePlotQualityProperty(ResultBuffer buffer) { }
		public static void GetOLEQualityProperty(ResultBuffer buffer) { }
		public static void GetOleSourceAppProperty(ResultBuffer buffer) { }
		public static void GetOnMenuBarProperty(ResultBuffer buffer) { }
		public static void GetOpenSaveProperty(ResultBuffer buffer) { }
		public static void GetOriginProperty(ResultBuffer buffer) { }
		public static void GetOrthoOnProperty(ResultBuffer buffer) { }
		public static void GetOutputProperty(ResultBuffer buffer) { }
		public static void GetOverrideCenterProperty(ResultBuffer buffer) { }
		public static void GetOverwritePropChangedProperty(ResultBuffer buffer) { }
		public static void GetOwnerIDProperty(ResultBuffer buffer) { }
		public static void GetPageSetupOverridesTemplateFileProperty(ResultBuffer buffer) { }
		public static void GetPaperSpaceProperty(ResultBuffer buffer) { }
		public static void GetPaperUnitsProperty(ResultBuffer buffer) { }
		public static void GetParentProperty(ResultBuffer buffer) { }
		public static void GetPathProperty(ResultBuffer buffer) { }
		public static void GetPatternAngleProperty(ResultBuffer buffer) { }
		public static void GetPatternDoubleProperty(ResultBuffer buffer) { }
		public static void GetPatternNameProperty(ResultBuffer buffer) { }
		public static void GetPatternScaleProperty(ResultBuffer buffer) { }
		public static void GetPatternSpaceProperty(ResultBuffer buffer) { }
		public static void GetPatternTypeProperty(ResultBuffer buffer) { }
		public static void GetPerimeterProperty(ResultBuffer buffer) { }
		public static void GetPeriodicProperty(ResultBuffer buffer) { }
		public static void GetPickAddProperty(ResultBuffer buffer) { }
		public static void GetPickAutoProperty(ResultBuffer buffer) { }
		public static void GetPickBoxSizeProperty(ResultBuffer buffer) { }
		public static void GetPickDragProperty(ResultBuffer buffer) { }
		public static void GetPickFirstProperty(ResultBuffer buffer) { }
		public static void GetPickfirstSelectionSetProperty(ResultBuffer buffer) { }
		public static void GetPickGroupProperty(ResultBuffer buffer) { }
		public static void GetPlotProperty(ResultBuffer buffer) { }
		public static void GetPlotConfigurationsProperty(ResultBuffer buffer) { }
		public static void GetPlotHiddenProperty(ResultBuffer buffer) { }
		public static void GetPlotLegacyProperty(ResultBuffer buffer) { }
		public static void GetPlotLogFilePathProperty(ResultBuffer buffer) { }
		public static void GetPlotOriginProperty(ResultBuffer buffer) { }
		public static void GetPlotPolicyProperty(ResultBuffer buffer) { }
		public static void GetPlotRotationProperty(ResultBuffer buffer) { }
		public static void GetPlotStyleNameProperty(ResultBuffer buffer) { }
		public static void GetPlottableProperty(ResultBuffer buffer) { }
		public static void GetPlotTypeProperty(ResultBuffer buffer) { }
		public static void GetPlotViewportBordersProperty(ResultBuffer buffer) { }
		public static void GetPlotViewportsFirstProperty(ResultBuffer buffer) { }
		public static void GetPlotWithLineweightsProperty(ResultBuffer buffer) { }
		public static void GetPlotWithPlotStylesProperty(ResultBuffer buffer) { }
		public static void GetPolarTrackingVectorProperty(ResultBuffer buffer) { }
		public static void GetPositionProperty(ResultBuffer buffer) { }
		public static void GetPostScriptPrologFileProperty(ResultBuffer buffer) { }
		public static void GetPreferencesProperty(ResultBuffer buffer) { }
		public static void GetPresetProperty(ResultBuffer buffer) { }
		public static void GetPrimaryUnitsPrecisionProperty(ResultBuffer buffer) { }
		public static void GetPrincipalDirectionsProperty(ResultBuffer buffer) { }
		public static void GetPrincipalMomentsProperty(ResultBuffer buffer) { }
		public static void GetPrinterConfigPathProperty(ResultBuffer buffer) { }
		public static void GetPrinterDescPathProperty(ResultBuffer buffer) { }
		public static void GetPrinterPaperSizeAlertProperty(ResultBuffer buffer) { }
		public static void GetPrinterSpoolAlertProperty(ResultBuffer buffer) { }
		public static void GetPrinterStyleSheetPathProperty(ResultBuffer buffer) { }
		public static void GetPrintFileProperty(ResultBuffer buffer) { }
		public static void GetPrintSpoolerPathProperty(ResultBuffer buffer) { }
		public static void GetPrintSpoolExecutableProperty(ResultBuffer buffer) { }
		public static void GetProductOfInertiaProperty(ResultBuffer buffer) { }
		public static void GetProfileRotationProperty(ResultBuffer buffer) { }
		public static void GetProfilesProperty(ResultBuffer buffer) { }
		public static void GetPromptStringProperty(ResultBuffer buffer) { }
		public static void GetPropertyNameProperty(ResultBuffer buffer) { }
		public static void GetProxyImageProperty(ResultBuffer buffer) { }
		public static void GetQNewTemplateFileProperty(ResultBuffer buffer) { }
		public static void GetQuietErrorModeProperty(ResultBuffer buffer) { }
		public static void GetRadiiOfGyrationProperty(ResultBuffer buffer) { }
		public static void GetRadiusProperty(ResultBuffer buffer) { }
		public static void GetRadiusRatioProperty(ResultBuffer buffer) { }
		public static void GetReadOnlyProperty(ResultBuffer buffer) { }
		public static void GetRedProperty(ResultBuffer buffer) { }
		public static void GetRegenerateTableSuppressedProperty(ResultBuffer buffer) { }
		public static void GetRegisteredApplicationsProperty(ResultBuffer buffer) { }
		public static void GetRemoveHiddenLinesProperty(ResultBuffer buffer) { }
		public static void GetRenderSmoothnessProperty(ResultBuffer buffer) { }
		public static void GetRepeatBottomLabelsProperty(ResultBuffer buffer) { }
		public static void GetRepeatTopLabelsProperty(ResultBuffer buffer) { }
		public static void GetRevisionNumberProperty(ResultBuffer buffer) { }
		public static void GetRevolutionAngleProperty(ResultBuffer buffer) { }
		public static void GetRotationProperty(ResultBuffer buffer) { }
		public static void GetRoundDistanceProperty(ResultBuffer buffer) { }
		public static void GetRowHeightProperty(ResultBuffer buffer) { }
		public static void GetRowsProperty(ResultBuffer buffer) { }
		public static void GetRowSpacingProperty(ResultBuffer buffer) { }
		public static void GetSaveAsTypeProperty(ResultBuffer buffer) { }
		public static void GetSavedProperty(ResultBuffer buffer) { }
		public static void GetSavePreviewThumbnailProperty(ResultBuffer buffer) { }
		public static void GetScaleProperty(ResultBuffer buffer) { }
		public static void GetScaleFactorProperty(ResultBuffer buffer) { }
		public static void GetScaleHeightProperty(ResultBuffer buffer) { }
		public static void GetScaleLineweightsProperty(ResultBuffer buffer) { }
		public static void GetScaleWidthProperty(ResultBuffer buffer) { }
		public static void GetSCMCommandModeProperty(ResultBuffer buffer) { }
		public static void GetSCMDefaultModeProperty(ResultBuffer buffer) { }
		public static void GetSCMEditModeProperty(ResultBuffer buffer) { }
		public static void GetSCMTimeModeProperty(ResultBuffer buffer) { }
		public static void GetSCMTimeValueProperty(ResultBuffer buffer) { }
		public static void GetSecondPointProperty(ResultBuffer buffer) { }
		public static void GetSecondSegmentAngleConstraintProperty(ResultBuffer buffer) { }
		public static void GetSectionManagerProperty(ResultBuffer buffer) { }
		public static void GetSectionPlaneOffsetProperty(ResultBuffer buffer) { }
		public static void GetSegmentationProperty(ResultBuffer buffer) { }
		public static void GetSegmentPerPolylineProperty(ResultBuffer buffer) { }
		public static void GetSelectionProperty(ResultBuffer buffer) { }
		public static void GetSelectionSetsProperty(ResultBuffer buffer) { }
		public static void GetSerialNumberProperty(ResultBuffer buffer) { }
		public static void GetSettingsProperty(ResultBuffer buffer) { }
		public static void GetShadePlotProperty(ResultBuffer buffer) { }
		public static void GetSheetViewProperty(ResultBuffer buffer) { }
		public static void GetShortcutMenuProperty(ResultBuffer buffer) { }
		public static void GetShortCutMenuDisplayProperty(ResultBuffer buffer) { }
		public static void GetShowProperty(ResultBuffer buffer) { }
		public static void GetShowAssociativityProperty(ResultBuffer buffer) { }
		public static void GetShowClippedProperty(ResultBuffer buffer) { }
		public static void GetShowCroppedProperty(ResultBuffer buffer) { }
		public static void GetShowHistoryProperty(ResultBuffer buffer) { }
		public static void GetShowIntensityProperty(ResultBuffer buffer) { }
		public static void GetShowPlotStylesProperty(ResultBuffer buffer) { }
		public static void GetShowProxyDialogBoxProperty(ResultBuffer buffer) { }
		public static void GetShowRasterImageProperty(ResultBuffer buffer) { }
		public static void GetShowRotationProperty(ResultBuffer buffer) { }
		public static void GetShowWarningMessagesProperty(ResultBuffer buffer) { }
		public static void GetSingleDocumentModeProperty(ResultBuffer buffer) { }
		public static void GetSliceDepthProperty(ResultBuffer buffer) { }
		public static void GetSmoothnessProperty(ResultBuffer buffer) { }
		public static void GetSnapBasePointProperty(ResultBuffer buffer) { }
		public static void GetSnapOnProperty(ResultBuffer buffer) { }
		public static void GetSnapRotationAngleProperty(ResultBuffer buffer) { }
		public static void GetSolidFillProperty(ResultBuffer buffer) { }
		public static void GetSolidTypeProperty(ResultBuffer buffer) { }
		public static void GetSourceObjectsProperty(ResultBuffer buffer) { }
		public static void GetSplineFrameProperty(ResultBuffer buffer) { }
		public static void GetSplineMethodProperty(ResultBuffer buffer) { }
		public static void GetStandardScaleProperty(ResultBuffer buffer) { }
		public static void GetStandardScale2Property(ResultBuffer buffer) { }
		public static void GetStartAngleProperty(ResultBuffer buffer) { }
		public static void GetStartDraftAngleProperty(ResultBuffer buffer) { }
		public static void GetStartDraftMagnitudeProperty(ResultBuffer buffer) { }
		public static void GetStartParameterProperty(ResultBuffer buffer) { }
		public static void GetStartPointProperty(ResultBuffer buffer) { }
		public static void GetStartSmoothContinuityProperty(ResultBuffer buffer) { }
		public static void GetStartSmoothMagnitudeProperty(ResultBuffer buffer) { }
		public static void GetStartTangentProperty(ResultBuffer buffer) { }
		public static void GetStateProperty(ResultBuffer buffer) { }
		public static void GetState2Property(ResultBuffer buffer) { }
		public static void GetStatusIDProperty(ResultBuffer buffer) { }
		public static void GetStoreSQLIndexProperty(ResultBuffer buffer) { }
		public static void GetStyleNameProperty(ResultBuffer buffer) { }
		public static void GetStyleSheetProperty(ResultBuffer buffer) { }
		public static void GetStylizationProperty(ResultBuffer buffer) { }
		public static void GetSubjectProperty(ResultBuffer buffer) { }
		public static void GetSubMenuProperty(ResultBuffer buffer) { }
		public static void GetSubUnitsFactorProperty(ResultBuffer buffer) { }
		public static void GetSubUnitsSuffixProperty(ResultBuffer buffer) { }
		public static void GetSummaryInfoProperty(ResultBuffer buffer) { }
		public static void GetSupportPathProperty(ResultBuffer buffer) { }
		public static void GetSuppressLeadingZerosProperty(ResultBuffer buffer) { }
		public static void GetSuppressTrailingZerosProperty(ResultBuffer buffer) { }
		public static void GetSuppressZeroFeetProperty(ResultBuffer buffer) { }
		public static void GetSuppressZeroInchesProperty(ResultBuffer buffer) { }
		public static void GetSurfaceNormalsProperty(ResultBuffer buffer) { }
		public static void GetSurfaceTypeProperty(ResultBuffer buffer) { }
		public static void GetSurfTrimAssociativityProperty(ResultBuffer buffer) { }
		public static void GetSymbolPositionProperty(ResultBuffer buffer) { }
		public static void GetSystemProperty(ResultBuffer buffer) { }
		public static void GetTableBreakFlowDirectionProperty(ResultBuffer buffer) { }
		public static void GetTableBreakHeightProperty(ResultBuffer buffer) { }
		public static void GetTablesReadOnlyProperty(ResultBuffer buffer) { }
		public static void GetTableStyleOverridesProperty(ResultBuffer buffer) { }
		public static void GetTabOrderProperty(ResultBuffer buffer) { }
		public static void GetTagStringProperty(ResultBuffer buffer) { }
		public static void GetTaperAngleProperty(ResultBuffer buffer) { }
		public static void GetTargetProperty(ResultBuffer buffer) { }
		public static void GetTempFileExtensionProperty(ResultBuffer buffer) { }
		public static void GetTempFilePathProperty(ResultBuffer buffer) { }
		public static void GetTemplateDWGPathProperty(ResultBuffer buffer) { }
		public static void GetTemplateIdProperty(ResultBuffer buffer) { }
		public static void GetTempXRefPathProperty(ResultBuffer buffer) { }
		public static void GetTextAlignmentPointProperty(ResultBuffer buffer) { }
		public static void GetTextAlignmentTypeProperty(ResultBuffer buffer) { }
		public static void GetTextAngleTypeProperty(ResultBuffer buffer) { }
		public static void GetTextAttachmentDirectionProperty(ResultBuffer buffer) { }
		public static void GetTextAttachmentTypeProperty(ResultBuffer buffer) { }
		public static void GetTextBackgroundFillProperty(ResultBuffer buffer) { }
		public static void GetTextBottomAttachmentTypeProperty(ResultBuffer buffer) { }
		public static void GetTextColorProperty(ResultBuffer buffer) { }
		public static void GetTextDirectionProperty(ResultBuffer buffer) { }
		public static void GetTextEditorProperty(ResultBuffer buffer) { }
		public static void GetTextFillProperty(ResultBuffer buffer) { }
		public static void GetTextFillColorProperty(ResultBuffer buffer) { }
		public static void GetTextFontProperty(ResultBuffer buffer) { }
		public static void GetTextFontSizeProperty(ResultBuffer buffer) { }
		public static void GetTextFontStyleProperty(ResultBuffer buffer) { }
		public static void GetTextFrameDisplayProperty(ResultBuffer buffer) { }
		public static void GetTextGapProperty(ResultBuffer buffer) { }
		public static void GetTextGenerationFlagProperty(ResultBuffer buffer) { }
		public static void GetTextHeightProperty(ResultBuffer buffer) { }
		public static void GetTextInsideProperty(ResultBuffer buffer) { }
		public static void GetTextInsideAlignProperty(ResultBuffer buffer) { }
		public static void GetTextJustifyProperty(ResultBuffer buffer) { }
		public static void GetTextLeftAttachmentTypeProperty(ResultBuffer buffer) { }
		public static void GetTextLineSpacingDistanceProperty(ResultBuffer buffer) { }
		public static void GetTextLineSpacingFactorProperty(ResultBuffer buffer) { }
		public static void GetTextLineSpacingStyleProperty(ResultBuffer buffer) { }
		public static void GetTextMovementProperty(ResultBuffer buffer) { }
		public static void GetTextOutsideAlignProperty(ResultBuffer buffer) { }
		public static void GetTextOverrideProperty(ResultBuffer buffer) { }
		public static void GetTextPositionProperty(ResultBuffer buffer) { }
		public static void GetTextPrecisionProperty(ResultBuffer buffer) { }
		public static void GetTextPrefixProperty(ResultBuffer buffer) { }
		public static void GetTextRightAttachmentTypeProperty(ResultBuffer buffer) { }
		public static void GetTextRotationProperty(ResultBuffer buffer) { }
		public static void GetTextStringProperty(ResultBuffer buffer) { }
		public static void GetTextStyleProperty(ResultBuffer buffer) { }
		public static void GetTextStyleNameProperty(ResultBuffer buffer) { }
		public static void GetTextStylesProperty(ResultBuffer buffer) { }
		public static void GetTextSuffixProperty(ResultBuffer buffer) { }
		public static void GetTextTopAttachmentTypeProperty(ResultBuffer buffer) { }
		public static void GetTextureMapPathProperty(ResultBuffer buffer) { }
		public static void GetTextWidthProperty(ResultBuffer buffer) { }
		public static void GetTextWinBackgrndColorProperty(ResultBuffer buffer) { }
		public static void GetTextWinTextColorProperty(ResultBuffer buffer) { }
		public static void GetThicknessProperty(ResultBuffer buffer) { }
		public static void GetTimeServerProperty(ResultBuffer buffer) { }
		public static void GetTitleProperty(ResultBuffer buffer) { }
		public static void GetTitleSuppressedProperty(ResultBuffer buffer) { }
		public static void GetToleranceDisplayProperty(ResultBuffer buffer) { }
		public static void GetToleranceHeightScaleProperty(ResultBuffer buffer) { }
		public static void GetToleranceJustificationProperty(ResultBuffer buffer) { }
		public static void GetToleranceLowerLimitProperty(ResultBuffer buffer) { }
		public static void GetTolerancePrecisionProperty(ResultBuffer buffer) { }
		public static void GetToleranceSuppressLeadingZerosProperty(ResultBuffer buffer) { }
		public static void GetToleranceSuppressTrailingZerosProperty(ResultBuffer buffer) { }
		public static void GetToleranceSuppressZeroFeetProperty(ResultBuffer buffer) { }
		public static void GetToleranceSuppressZeroInchesProperty(ResultBuffer buffer) { }
		public static void GetToleranceUpperLimitProperty(ResultBuffer buffer) { }
		public static void GetToolbarsProperty(ResultBuffer buffer) { }
		public static void GetToolPalettePathProperty(ResultBuffer buffer) { }
		public static void GetTopProperty(ResultBuffer buffer) { }
		public static void GetTopHeightProperty(ResultBuffer buffer) { }
		public static void GetTopRadiusProperty(ResultBuffer buffer) { }
		public static void GetTotalAngleProperty(ResultBuffer buffer) { }
		public static void GetTotalLengthProperty(ResultBuffer buffer) { }
		public static void GetTranslateIDsProperty(ResultBuffer buffer) { }
		public static void GetTransparencyProperty(ResultBuffer buffer) { }
		public static void GetTrueColorProperty(ResultBuffer buffer) { }
		public static void GetTrueColorImagesProperty(ResultBuffer buffer) { }
		public static void GetTurnHeightProperty(ResultBuffer buffer) { }
		public static void GetTurnsProperty(ResultBuffer buffer) { }
		public static void GetTurnSlopeProperty(ResultBuffer buffer) { }
		public static void GetTwistProperty(ResultBuffer buffer) { }
		public static void GetTwistAngleProperty(ResultBuffer buffer) { }
		public static void GetTypeProperty(ResultBuffer buffer) { }
		public static void GetUCSIconAtOriginProperty(ResultBuffer buffer) { }
		public static void GetUCSIconOnProperty(ResultBuffer buffer) { }
		public static void GetUCSPerViewportProperty(ResultBuffer buffer) { }
		public static void GetUIsolineDensityProperty(ResultBuffer buffer) { }
		public static void GetUnderlayLayerOverrideAppliedProperty(ResultBuffer buffer) { }
		public static void GetUnderlayNameProperty(ResultBuffer buffer) { }
		public static void GetUnderlayVisibilityProperty(ResultBuffer buffer) { }
		public static void GetUnitProperty(ResultBuffer buffer) { }
		public static void GetUnitFactorProperty(ResultBuffer buffer) { }
		public static void GetUnitsProperty(ResultBuffer buffer) { }
		public static void GetUnitsFormatProperty(ResultBuffer buffer) { }
		public static void GetUnitsTypeProperty(ResultBuffer buffer) { }
		public static void GetUpperRightCornerProperty(ResultBuffer buffer) { }
		public static void GetUpsideDownProperty(ResultBuffer buffer) { }
		public static void GetURLProperty(ResultBuffer buffer) { }
		public static void GetURLDescriptionProperty(ResultBuffer buffer) { }
		public static void GetURLNamedLocationProperty(ResultBuffer buffer) { }
		public static void GetUsedProperty(ResultBuffer buffer) { }
		public static void GetUseEntityColorProperty(ResultBuffer buffer) { }
		public static void GetUseLastPlotSettingsProperty(ResultBuffer buffer) { }
		public static void GetUserProperty(ResultBuffer buffer) { }
		public static void GetUserCoordinateSystemsProperty(ResultBuffer buffer) { }
		public static void GetUseStandardScaleProperty(ResultBuffer buffer) { }
		public static void GetUtilityProperty(ResultBuffer buffer) { }
		public static void GetValueProperty(ResultBuffer buffer) { }
		public static void GetVBEProperty(ResultBuffer buffer) { }
		public static void GetVerifyProperty(ResultBuffer buffer) { }
		public static void GetVersionProperty(ResultBuffer buffer) { }
		public static void GetVertCellMarginProperty(ResultBuffer buffer) { }
		public static void GetVertexCountProperty(ResultBuffer buffer) { }
		public static void GetVerticalDirectionProperty(ResultBuffer buffer) { }
		public static void GetVerticalTextPositionProperty(ResultBuffer buffer) { }
		public static void GetVerticesProperty(ResultBuffer buffer) { }
		public static void GetViewingDirectionProperty(ResultBuffer buffer) { }
		public static void GetViewportDefaultProperty(ResultBuffer buffer) { }
		public static void GetViewportOnProperty(ResultBuffer buffer) { }
		public static void GetViewportsProperty(ResultBuffer buffer) { }
		public static void GetViewsProperty(ResultBuffer buffer) { }
		public static void GetViewToPlotProperty(ResultBuffer buffer) { }
		public static void GetVisibilityEdge1Property(ResultBuffer buffer) { }
		public static void GetVisibilityEdge2Property(ResultBuffer buffer) { }
		public static void GetVisibilityEdge3Property(ResultBuffer buffer) { }
		public static void GetVisibilityEdge4Property(ResultBuffer buffer) { }
		public static void GetVisibleProperty(ResultBuffer buffer) { }
		public static void GetVIsolineDensityProperty(ResultBuffer buffer) { }
		public static void GetVisualStyleProperty(ResultBuffer buffer) { }
		public static void GetVolumeProperty(ResultBuffer buffer) { }
		public static void GetWeightsProperty(ResultBuffer buffer) { }
		public static void GetWidthProperty(ResultBuffer buffer) { }
		public static void GetWindowLeftProperty(ResultBuffer buffer) { }
		public static void GetWindowStateProperty(ResultBuffer buffer) { }
		public static void GetWindowTitleProperty(ResultBuffer buffer) { }
		public static void GetWindowTopProperty(ResultBuffer buffer) { }
		public static void GetWireframeTypeProperty(ResultBuffer buffer) { }
		public static void GetWorkspacePathProperty(ResultBuffer buffer) { }
		public static void GetXEffectiveScaleFactorProperty(ResultBuffer buffer) { }
		public static void GetXRefDatabaseProperty(ResultBuffer buffer) { }
		public static void GetXRefDemandLoadProperty(ResultBuffer buffer) { }
		public static void GetXRefEditProperty(ResultBuffer buffer) { }
		public static void GetXRefFadeIntensityProperty(ResultBuffer buffer) { }
		public static void GetXRefLayerVisibilityProperty(ResultBuffer buffer) { }
		public static void GetXScaleFactorProperty(ResultBuffer buffer) { }
		public static void GetXVectorProperty(ResultBuffer buffer) { }
		public static void GetYEffectiveScaleFactorProperty(ResultBuffer buffer) { }
		public static void GetYScaleFactorProperty(ResultBuffer buffer) { }
		public static void GetYVectorProperty(ResultBuffer buffer) { }
		public static void GetZEffectiveScaleFactorProperty(ResultBuffer buffer) { }
		public static void GetZScaleFactorProperty(ResultBuffer buffer) { }


		public static void SetActiveDimStyleProperty(ResultBuffer buffer)
		{
			ObjectId objectId = new();

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 2);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				if (inputs.Count > 1)
					objectId = AutoLisp.LispToObjectId(inputs[1]);

				if (!databaseDictionaryField.ContainsKey(databaseId))
					throw new System.Exception($"database id not found: {databaseId}");

				Database database = databaseDictionaryField[databaseId];

				if (database != objectId.Database)
					throw new System.Exception($"ename not found in database");

				if (!objectId.ObjectClass.Name.Equals("AcDbDimStyleTableRecord", StringComparison.OrdinalIgnoreCase))
					throw new System.Exception($"no function definition: Net-SetActiveDimStyle");

				Transaction transaction = database.TransactionManager.StartTransaction();
				database.Dimstyle = objectId;
				transaction.Commit();
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
			}
		}
		public static void SetActiveLayerProperty(ResultBuffer buffer)
		{
			ObjectId objectId = new();

			try
			{
				List<TypedValue> inputs = AutoLisp.HandleLispArguments(buffer, 1, 1);
				int databaseId = AutoLisp.LispToInt(inputs[0]);

				if (!databaseDictionaryField.ContainsKey(databaseId))
					throw new System.Exception($"Database id not found: {databaseId}");

				Database database = databaseDictionaryField[databaseId];

				if (database != objectId.Database)
					throw new System.Exception($"Ename not found in database");

				Transaction transaction = database.TransactionManager.StartTransaction();
				database.Dimstyle = objectId;
				transaction.Commit();
			}
			catch (System.Exception e)
			{
				AutoLisp.ThrowLispError(e.Message);
			}
		}
		public static void SetActiveLayoutProperty(ResultBuffer buffer) { }
		public static void SetActiveLinetypeProperty(ResultBuffer buffer) { }
		public static void SetActiveMaterialProperty(ResultBuffer buffer) { }
		public static void SetActiveProfileProperty(ResultBuffer buffer) { }
		public static void SetActivePViewportProperty(ResultBuffer buffer) { }
		public static void SetActiveSelectionSetProperty(ResultBuffer buffer) { }
		public static void SetActiveSpaceProperty(ResultBuffer buffer) { }
		public static void SetActiveTextStyleProperty(ResultBuffer buffer) { }
		public static void SetActiveUCSProperty(ResultBuffer buffer) { }
		public static void SetActiveViewportProperty(ResultBuffer buffer) { }
		public static void SetADCInsertUnitsDefaultSourceProperty(ResultBuffer buffer) { }
		public static void SetADCInsertUnitsDefaultTargetProperty(ResultBuffer buffer) { }
		public static void SetAdjustForBackgroundProperty(ResultBuffer buffer) { }
		public static void SetAlignmentProperty(ResultBuffer buffer) { }
		public static void SetAlignmentPointAcquisitionProperty(ResultBuffer buffer) { }
		public static void SetAlignSpaceProperty(ResultBuffer buffer) { }
		public static void SetAllowedValuesProperty(ResultBuffer buffer) { }
		public static void SetAllowLongSymbolNamesProperty(ResultBuffer buffer) { }
		public static void SetAllowManualHeightsProperty(ResultBuffer buffer) { }
		public static void SetAllowManualPositionsProperty(ResultBuffer buffer) { }
		public static void SetAltFontFileProperty(ResultBuffer buffer) { }
		public static void SetAltitudeProperty(ResultBuffer buffer) { }
		public static void SetAltRoundDistanceProperty(ResultBuffer buffer) { }
		public static void SetAltSubUnitsFactorProperty(ResultBuffer buffer) { }
		public static void SetAltSubUnitsSuffixProperty(ResultBuffer buffer) { }
		public static void SetAltSuppressLeadingZerosProperty(ResultBuffer buffer) { }
		public static void SetAltSuppressTrailingZerosProperty(ResultBuffer buffer) { }
		public static void SetAltSuppressZeroFeetProperty(ResultBuffer buffer) { }
		public static void SetAltSuppressZeroInchesProperty(ResultBuffer buffer) { }
		public static void SetAltTabletMenuFileProperty(ResultBuffer buffer) { }
		public static void SetAltTextPrefixProperty(ResultBuffer buffer) { }
		public static void SetAltTextSuffixProperty(ResultBuffer buffer) { }
		public static void SetAltTolerancePrecisionProperty(ResultBuffer buffer) { }
		public static void SetAltToleranceSuppressLeadingZerosProperty(ResultBuffer buffer) { }
		public static void SetAltToleranceSuppressTrailingZerosProperty(ResultBuffer buffer) { }
		public static void SetAltToleranceSuppressZeroFeetProperty(ResultBuffer buffer) { }
		public static void SetAltToleranceSuppressZeroInchesProperty(ResultBuffer buffer) { }
		public static void SetAltUnitsProperty(ResultBuffer buffer) { }
		public static void SetAltUnitsFormatProperty(ResultBuffer buffer) { }
		public static void SetAltUnitsPrecisionProperty(ResultBuffer buffer) { }
		public static void SetAltUnitsScaleProperty(ResultBuffer buffer) { }
		public static void SetAngleProperty(ResultBuffer buffer) { }
		public static void SetAngleFormatProperty(ResultBuffer buffer) { }
		public static void SetAngleVertexProperty(ResultBuffer buffer) { }
		public static void SetAnnotationProperty(ResultBuffer buffer) { }
		public static void SetAnnotativeProperty(ResultBuffer buffer) { }
		public static void SetApplicationProperty(ResultBuffer buffer) { }
		public static void SetArcEndParamProperty(ResultBuffer buffer) { }
		public static void SetArcLengthProperty(ResultBuffer buffer) { }
		public static void SetArcPointProperty(ResultBuffer buffer) { }
		public static void SetArcSmoothnessProperty(ResultBuffer buffer) { }
		public static void SetArcStartParamProperty(ResultBuffer buffer) { }
		public static void SetAreaProperty(ResultBuffer buffer) { }
		public static void SetArrowhead1BlockProperty(ResultBuffer buffer) { }
		public static void SetArrowhead1TypeProperty(ResultBuffer buffer) { }
		public static void SetArrowhead2BlockProperty(ResultBuffer buffer) { }
		public static void SetArrowhead2TypeProperty(ResultBuffer buffer) { }
		public static void SetArrowheadBlockProperty(ResultBuffer buffer) { }
		public static void SetArrowheadSizeProperty(ResultBuffer buffer) { }
		public static void SetArrowheadTypeProperty(ResultBuffer buffer) { }
		public static void SetArrowSizeProperty(ResultBuffer buffer) { }
		public static void SetArrowSymbolProperty(ResultBuffer buffer) { }
		public static void SetAssociativeHatchProperty(ResultBuffer buffer) { }
		public static void SetAttachmentPointProperty(ResultBuffer buffer) { }
		public static void SetAuthorProperty(ResultBuffer buffer) { }
		public static void SetAutoAuditProperty(ResultBuffer buffer) { }
		public static void SetAutomaticPlotLogProperty(ResultBuffer buffer) { }
		public static void SetAutoSaveIntervalProperty(ResultBuffer buffer) { }
		public static void SetAutoSavePathProperty(ResultBuffer buffer) { }
		public static void SetAutoSnapApertureProperty(ResultBuffer buffer) { }
		public static void SetAutoSnapApertureSizeProperty(ResultBuffer buffer) { }
		public static void SetAutoSnapMagnetProperty(ResultBuffer buffer) { }
		public static void SetAutoSnapMarkerProperty(ResultBuffer buffer) { }
		public static void SetAutoSnapMarkerColorProperty(ResultBuffer buffer) { }
		public static void SetAutoSnapMarkerSizeProperty(ResultBuffer buffer) { }
		public static void SetAutoSnapToolTipProperty(ResultBuffer buffer) { }
		public static void SetAutoTrackingVecColorProperty(ResultBuffer buffer) { }
		public static void SetAutoTrackTooltipProperty(ResultBuffer buffer) { }
		public static void SetAxisDirectionProperty(ResultBuffer buffer) { }
		public static void SetAxisPositionProperty(ResultBuffer buffer) { }
		public static void SetBackgroundColorProperty(ResultBuffer buffer) { }
		public static void SetBackgroundFillProperty(ResultBuffer buffer) { }
		public static void SetBackgroundLinesColorProperty(ResultBuffer buffer) { }
		public static void SetBackgroundLinesHiddenLineProperty(ResultBuffer buffer) { }
		public static void SetBackgroundLinesLayerProperty(ResultBuffer buffer) { }
		public static void SetBackgroundLinesLinetypeProperty(ResultBuffer buffer) { }
		public static void SetBackgroundLinesLinetypeScaleProperty(ResultBuffer buffer) { }
		public static void SetBackgroundLinesLineweightProperty(ResultBuffer buffer) { }
		public static void SetBackgroundLinesPlotStyleNameProperty(ResultBuffer buffer) { }
		public static void SetBackgroundLinesVisibleProperty(ResultBuffer buffer) { }
		public static void SetBackwardProperty(ResultBuffer buffer) { }
		public static void SetBankProperty(ResultBuffer buffer) { }
		public static void SetBasePointProperty(ResultBuffer buffer) { }
		public static void SetBaseRadiusProperty(ResultBuffer buffer) { }
		public static void SetBatchPlotProgressProperty(ResultBuffer buffer) { }
		public static void SetBeepOnErrorProperty(ResultBuffer buffer) { }
		public static void SetBigFontFileProperty(ResultBuffer buffer) { }
		public static void SetBitFlagsProperty(ResultBuffer buffer) { }
		public static void SetBlockProperty(ResultBuffer buffer) { }
		public static void SetBlockColorProperty(ResultBuffer buffer) { }
		public static void SetBlockConnectionTypeProperty(ResultBuffer buffer) { }
		public static void SetBlockRotationProperty(ResultBuffer buffer) { }
		public static void SetBlocksProperty(ResultBuffer buffer) { }
		public static void SetBlockScaleProperty(ResultBuffer buffer) { }
		public static void SetBlockScalingProperty(ResultBuffer buffer) { }
		public static void SetBlueProperty(ResultBuffer buffer) { }
		public static void SetBookNameProperty(ResultBuffer buffer) { }
		public static void SetBottomHeightProperty(ResultBuffer buffer) { }
		public static void SetBreaksEnabledProperty(ResultBuffer buffer) { }
		public static void SetBreakSizeProperty(ResultBuffer buffer) { }
		public static void SetBreakSpacingProperty(ResultBuffer buffer) { }
		public static void SetBrightnessProperty(ResultBuffer buffer) { }
		public static void SetCanonicalMediaNameProperty(ResultBuffer buffer) { }
		public static void SetCaptionProperty(ResultBuffer buffer) { }
		public static void SetCategoryNameProperty(ResultBuffer buffer) { }
		public static void SetCenterProperty(ResultBuffer buffer) { }
		public static void SetCenterMarkSizeProperty(ResultBuffer buffer) { }
		public static void SetCenterPlotProperty(ResultBuffer buffer) { }
		public static void SetCenterPointProperty(ResultBuffer buffer) { }
		public static void SetCenterTypeProperty(ResultBuffer buffer) { }
		public static void SetCentroidProperty(ResultBuffer buffer) { }
		public static void SetCheckProperty(ResultBuffer buffer) { }
		public static void SetChordPointProperty(ResultBuffer buffer) { }
		public static void SetCircumferenceProperty(ResultBuffer buffer) { }
		public static void SetClippedProperty(ResultBuffer buffer) { }
		public static void SetClippingEnabledProperty(ResultBuffer buffer) { }
		public static void SetClosedProperty(ResultBuffer buffer) { }
		public static void SetClosed2Property(ResultBuffer buffer) { }
		public static void SetColorProperty(ResultBuffer buffer) { }
		public static void SetColorBookPathProperty(ResultBuffer buffer) { }
		public static void SetColorIndexProperty(ResultBuffer buffer) { }
		public static void SetColorMethodProperty(ResultBuffer buffer) { }
		public static void SetColorNameProperty(ResultBuffer buffer) { }
		public static void SetColorSchemeProperty(ResultBuffer buffer) { }
		public static void SetColumnsProperty(ResultBuffer buffer) { }
		public static void SetColumnSpacingProperty(ResultBuffer buffer) { }
		public static void SetColumnWidthProperty(ResultBuffer buffer) { }
		public static void SetCommandDisplayNameProperty(ResultBuffer buffer) { }
		public static void SetCommentProperty(ResultBuffer buffer) { }
		public static void SetCommentsProperty(ResultBuffer buffer) { }
		public static void SetConfigFileProperty(ResultBuffer buffer) { }
		public static void SetConfigNameProperty(ResultBuffer buffer) { }
		public static void SetConstantProperty(ResultBuffer buffer) { }
		public static void SetConstantWidthProperty(ResultBuffer buffer) { }
		public static void SetConstrainProperty(ResultBuffer buffer) { }
		public static void SetContentBlockNameProperty(ResultBuffer buffer) { }
		public static void SetContentBlockTypeProperty(ResultBuffer buffer) { }
		public static void SetContentTypeProperty(ResultBuffer buffer) { }
		public static void SetContinuousPlotLogProperty(ResultBuffer buffer) { }
		public static void SetContourLinesPerSurfaceProperty(ResultBuffer buffer) { }
		public static void SetContrastProperty(ResultBuffer buffer) { }
		public static void SetControlPointsProperty(ResultBuffer buffer) { }
		public static void SetCoordinateProperty(ResultBuffer buffer) { }
		public static void SetCoordinatesProperty(ResultBuffer buffer) { }
		public static void SetCountProperty(ResultBuffer buffer) { }
		public static void SetCreaseLevelProperty(ResultBuffer buffer) { }
		public static void SetCreaseTypeProperty(ResultBuffer buffer) { }
		public static void SetCreateBackupProperty(ResultBuffer buffer) { }
		public static void SetCurrentSectionTypeProperty(ResultBuffer buffer) { }
		public static void SetCursorSizeProperty(ResultBuffer buffer) { }
		public static void SetCurveTangencyLinesColorProperty(ResultBuffer buffer) { }
		public static void SetCurveTangencyLinesLayerProperty(ResultBuffer buffer) { }
		public static void SetCurveTangencyLinesLinetypeProperty(ResultBuffer buffer) { }
		public static void SetCurveTangencyLinesLinetypeScaleProperty(ResultBuffer buffer) { }
		public static void SetCurveTangencyLinesLineweightProperty(ResultBuffer buffer) { }
		public static void SetCurveTangencyLinesPlotStyleNameProperty(ResultBuffer buffer) { }
		public static void SetCurveTangencyLinesVisibleProperty(ResultBuffer buffer) { }
		public static void SetCustomDictionaryProperty(ResultBuffer buffer) { }
		public static void SetCustomIconPathProperty(ResultBuffer buffer) { }
		public static void SetCustomScaleProperty(ResultBuffer buffer) { }
		public static void SetCvHullDisplayProperty(ResultBuffer buffer) { }
		public static void SetDatabaseProperty(ResultBuffer buffer) { }
		public static void SetDecimalSeparatorProperty(ResultBuffer buffer) { }
		public static void SetDefaultInternetURLProperty(ResultBuffer buffer) { }
		public static void SetDefaultOutputDeviceProperty(ResultBuffer buffer) { }
		public static void SetDefaultPlotStyleForLayerProperty(ResultBuffer buffer) { }
		public static void SetDefaultPlotStyleForObjectsProperty(ResultBuffer buffer) { }
		public static void SetDefaultPlotStyleTableProperty(ResultBuffer buffer) { }
		public static void SetDefaultPlotToFilePathProperty(ResultBuffer buffer) { }
		public static void SetDegreeProperty(ResultBuffer buffer) { }
		public static void SetDegree2Property(ResultBuffer buffer) { }
		public static void SetDeltaProperty(ResultBuffer buffer) { }
		public static void SetDemandLoadARXAppProperty(ResultBuffer buffer) { }
		public static void SetDescriptionProperty(ResultBuffer buffer) { }
		public static void SetDestinationBlockProperty(ResultBuffer buffer) { }
		public static void SetDestinationFileProperty(ResultBuffer buffer) { }
		public static void SetDiameterProperty(ResultBuffer buffer) { }
		public static void SetDictionariesProperty(ResultBuffer buffer) { }
		public static void SetDimConstrDescProperty(ResultBuffer buffer) { }
		public static void SetDimConstrExpressionProperty(ResultBuffer buffer) { }
		public static void SetDimConstrFormProperty(ResultBuffer buffer) { }
		public static void SetDimConstrNameProperty(ResultBuffer buffer) { }
		public static void SetDimConstrReferenceProperty(ResultBuffer buffer) { }
		public static void SetDimConstrValueProperty(ResultBuffer buffer) { }
		public static void SetDimensionLineColorProperty(ResultBuffer buffer) { }
		public static void SetDimensionLineExtendProperty(ResultBuffer buffer) { }
		public static void SetDimensionLinetypeProperty(ResultBuffer buffer) { }
		public static void SetDimensionLineWeightProperty(ResultBuffer buffer) { }
		public static void SetDimLine1SuppressProperty(ResultBuffer buffer) { }
		public static void SetDimLine2SuppressProperty(ResultBuffer buffer) { }
		public static void SetDimLineInsideProperty(ResultBuffer buffer) { }
		public static void SetDimLineSuppressProperty(ResultBuffer buffer) { }
		public static void SetDimStylesProperty(ResultBuffer buffer) { }
		public static void SetDimTxtDirectionProperty(ResultBuffer buffer) { }
		public static void SetDirectionProperty(ResultBuffer buffer) { }
		public static void SetDirectionVectorProperty(ResultBuffer buffer) { }
		public static void SetDisplayProperty(ResultBuffer buffer) { }
		public static void SetDisplayGripsProperty(ResultBuffer buffer) { }
		public static void SetDisplayGripsWithinBlocksProperty(ResultBuffer buffer) { }
		public static void SetDisplayLayoutTabsProperty(ResultBuffer buffer) { }
		public static void SetDisplayLockedProperty(ResultBuffer buffer) { }
		public static void SetDisplayOLEScaleProperty(ResultBuffer buffer) { }
		public static void SetDisplayScreenMenuProperty(ResultBuffer buffer) { }
		public static void SetDisplayScrollBarsProperty(ResultBuffer buffer) { }
		public static void SetDisplaySilhouetteProperty(ResultBuffer buffer) { }
		public static void SetDockedVisibleLinesProperty(ResultBuffer buffer) { }
		public static void SetDockStatusProperty(ResultBuffer buffer) { }
		public static void SetDocumentProperty(ResultBuffer buffer) { }
		public static void SetDocumentsProperty(ResultBuffer buffer) { }
		public static void SetDogLeggedProperty(ResultBuffer buffer) { }
		public static void SetDoglegLengthProperty(ResultBuffer buffer) { }
		public static void SetDraftingProperty(ResultBuffer buffer) { }
		public static void SetDrawingDirectionProperty(ResultBuffer buffer) { }
		public static void SetDrawLeaderOrderTypeProperty(ResultBuffer buffer) { }
		public static void SetDrawMLeaderOrderTypeProperty(ResultBuffer buffer) { }
		public static void SetDriversPathProperty(ResultBuffer buffer) { }
		public static void SetDWFFormatProperty(ResultBuffer buffer) { }
		public static void SetEdgeExtensionDistancesProperty(ResultBuffer buffer) { }
		public static void SetEffectiveNameProperty(ResultBuffer buffer) { }
		public static void SetElevationProperty(ResultBuffer buffer) { }
		public static void SetElevationModelSpaceProperty(ResultBuffer buffer) { }
		public static void SetElevationPaperSpaceProperty(ResultBuffer buffer) { }
		public static void SetEnableProperty(ResultBuffer buffer) { }
		public static void SetEnableBlockRotationProperty(ResultBuffer buffer) { }
		public static void SetEnableBlockScaleProperty(ResultBuffer buffer) { }
		public static void SetEnableBreakProperty(ResultBuffer buffer) { }
		public static void SetEnableDoglegProperty(ResultBuffer buffer) { }
		public static void SetEnableFrameTextProperty(ResultBuffer buffer) { }
		public static void SetEnableLandingProperty(ResultBuffer buffer) { }
		public static void SetEnableStartupDialogProperty(ResultBuffer buffer) { }
		public static void SetEndAngleProperty(ResultBuffer buffer) { }
		public static void SetEndDraftAngleProperty(ResultBuffer buffer) { }
		public static void SetEndDraftMagnitudeProperty(ResultBuffer buffer) { }
		public static void SetEndParameterProperty(ResultBuffer buffer) { }
		public static void SetEndPointProperty(ResultBuffer buffer) { }
		public static void SetEndSmoothContinuityProperty(ResultBuffer buffer) { }
		public static void SetEndSmoothMagnitudeProperty(ResultBuffer buffer) { }
		public static void SetEndSubMenuLevelProperty(ResultBuffer buffer) { }
		public static void SetEndTangentProperty(ResultBuffer buffer) { }
		public static void SetEnterpriseMenuFileProperty(ResultBuffer buffer) { }
		public static void SetEntityColorProperty(ResultBuffer buffer) { }
		public static void SetEntityTransparencyProperty(ResultBuffer buffer) { }
		public static void SetExplodableProperty(ResultBuffer buffer) { }
		public static void SetExtensionLineColorProperty(ResultBuffer buffer) { }
		public static void SetExtensionLineExtendProperty(ResultBuffer buffer) { }
		public static void SetExtensionLineOffsetProperty(ResultBuffer buffer) { }
		public static void SetExtensionLineWeightProperty(ResultBuffer buffer) { }
		public static void SetExtLine1EndPointProperty(ResultBuffer buffer) { }
		public static void SetExtLine1LinetypeProperty(ResultBuffer buffer) { }
		public static void SetExtLine1PointProperty(ResultBuffer buffer) { }
		public static void SetExtLine1StartPointProperty(ResultBuffer buffer) { }
		public static void SetExtLine1SuppressProperty(ResultBuffer buffer) { }
		public static void SetExtLine2EndPointProperty(ResultBuffer buffer) { }
		public static void SetExtLine2LinetypeProperty(ResultBuffer buffer) { }
		public static void SetExtLine2PointProperty(ResultBuffer buffer) { }
		public static void SetExtLine2StartPointProperty(ResultBuffer buffer) { }
		public static void SetExtLine2SuppressProperty(ResultBuffer buffer) { }
		public static void SetExtLineFixedLenProperty(ResultBuffer buffer) { }
		public static void SetExtLineFixedLenSuppressProperty(ResultBuffer buffer) { }
		public static void SetFaceCountProperty(ResultBuffer buffer) { }
		public static void SetFadeProperty(ResultBuffer buffer) { }
		public static void SetFieldLengthProperty(ResultBuffer buffer) { }
		public static void SetFileProperty(ResultBuffer buffer) { }
		public static void SetFilesProperty(ResultBuffer buffer) { }
		public static void SetFirstSegmentAngleConstraintProperty(ResultBuffer buffer) { }
		public static void SetFitProperty(ResultBuffer buffer) { }
		public static void SetFitPointsProperty(ResultBuffer buffer) { }
		public static void SetFitToleranceProperty(ResultBuffer buffer) { }
		public static void SetFloatingRowsProperty(ResultBuffer buffer) { }
		public static void SetFlowDirectionProperty(ResultBuffer buffer) { }
		public static void SetFlyoutProperty(ResultBuffer buffer) { }
		public static void SetFontFileProperty(ResultBuffer buffer) { }
		public static void SetFontFileMapProperty(ResultBuffer buffer) { }
		public static void SetForceLineInsideProperty(ResultBuffer buffer) { }
		public static void SetForegroundLinesColorProperty(ResultBuffer buffer) { }
		public static void SetForegroundLinesEdgeTransparencyProperty(ResultBuffer buffer) { }
		public static void SetForegroundLinesFaceTransparencyProperty(ResultBuffer buffer) { }
		public static void SetForegroundLinesHiddenLineProperty(ResultBuffer buffer) { }
		public static void SetForegroundLinesLayerProperty(ResultBuffer buffer) { }
		public static void SetForegroundLinesLinetypeProperty(ResultBuffer buffer) { }
		public static void SetForegroundLinesLinetypeScaleProperty(ResultBuffer buffer) { }
		public static void SetForegroundLinesLineweightProperty(ResultBuffer buffer) { }
		public static void SetForegroundLinesPlotStyleNameProperty(ResultBuffer buffer) { }
		public static void SetForegroundLinesVisibleProperty(ResultBuffer buffer) { }
		public static void SetFoundPathProperty(ResultBuffer buffer) { }
		public static void SetFractionFormatProperty(ResultBuffer buffer) { }
		public static void SetFreezeProperty(ResultBuffer buffer) { }
		public static void SetFullCRCValidationProperty(ResultBuffer buffer) { }
		public static void SetFullNameProperty(ResultBuffer buffer) { }
		public static void SetFullScreenTrackingVectorProperty(ResultBuffer buffer) { }
		public static void SetGenerationOptionsProperty(ResultBuffer buffer) { }
		public static void SetGeoImageBrightnessProperty(ResultBuffer buffer) { }
		public static void SetGeoImageContrastProperty(ResultBuffer buffer) { }
		public static void SetGeoImageFadeProperty(ResultBuffer buffer) { }
		public static void SetGeoImageHeightProperty(ResultBuffer buffer) { }
		public static void SetGeoImagePositionProperty(ResultBuffer buffer) { }
		public static void SetGeoImageWidthProperty(ResultBuffer buffer) { }
		public static void SetGeolocateProperty(ResultBuffer buffer) { }
		public static void SetGradientAngleProperty(ResultBuffer buffer) { }
		public static void SetGradientCenteredProperty(ResultBuffer buffer) { }
		public static void SetGradientColor1Property(ResultBuffer buffer) { }
		public static void SetGradientColor2Property(ResultBuffer buffer) { }
		public static void SetGradientNameProperty(ResultBuffer buffer) { }
		public static void SetGraphicsWinLayoutBackgrndColorProperty(ResultBuffer buffer) { }
		public static void SetGraphicsWinModelBackgrndColorProperty(ResultBuffer buffer) { }
		public static void SetGreenProperty(ResultBuffer buffer) { }
		public static void SetGridOnProperty(ResultBuffer buffer) { }
		public static void SetGripColorSelectedProperty(ResultBuffer buffer) { }
		public static void SetGripColorUnselectedProperty(ResultBuffer buffer) { }
		public static void SetGripSizeProperty(ResultBuffer buffer) { }
		public static void SetGroupsProperty(ResultBuffer buffer) { }
		public static void SetHandleProperty(ResultBuffer buffer) { }
		public static void SetHasAttributesProperty(ResultBuffer buffer) { }
		public static void SetHasExtensionDictionaryProperty(ResultBuffer buffer) { }
		public static void SetHasLeaderProperty(ResultBuffer buffer) { }
		public static void SetHasSheetViewProperty(ResultBuffer buffer) { }
		public static void SetHasSubSelectionProperty(ResultBuffer buffer) { }
		public static void SetHasVpAssociationProperty(ResultBuffer buffer) { }
		public static void SetHatchObjectTypeProperty(ResultBuffer buffer) { }
		public static void SetHatchStyleProperty(ResultBuffer buffer) { }
		public static void SetHeaderSuppressedProperty(ResultBuffer buffer) { }
		public static void SetHeightProperty(ResultBuffer buffer) { }
		public static void SetHelpFilePathProperty(ResultBuffer buffer) { }
		public static void SetHelpStringProperty(ResultBuffer buffer) { }
		public static void SetHistoryProperty(ResultBuffer buffer) { }
		public static void SetHistoryLinesProperty(ResultBuffer buffer) { }
		public static void SetHorizontalTextPositionProperty(ResultBuffer buffer) { }
		public static void SetHorzCellMarginProperty(ResultBuffer buffer) { }
		public static void SetHWNDProperty(ResultBuffer buffer) { }
		public static void SetHyperlinkBaseProperty(ResultBuffer buffer) { }
		public static void SetHyperlinkDisplayCursorProperty(ResultBuffer buffer) { }
		public static void SetHyperlinksProperty(ResultBuffer buffer) { }
		public static void SetImageFileProperty(ResultBuffer buffer) { }
		public static void SetImageFrameHighlightProperty(ResultBuffer buffer) { }
		public static void SetImageHeightProperty(ResultBuffer buffer) { }
		public static void SetImageVisibilityProperty(ResultBuffer buffer) { }
		public static void SetImageWidthProperty(ResultBuffer buffer) { }
		public static void SetIncrementalSavePercentProperty(ResultBuffer buffer) { }
		public static void SetIndexProperty(ResultBuffer buffer) { }
		public static void SetIndicatorFillColorProperty(ResultBuffer buffer) { }
		public static void SetIndicatorTransparencyProperty(ResultBuffer buffer) { }
		public static void SetInsertionPointProperty(ResultBuffer buffer) { }
		public static void SetInsUnitsProperty(ResultBuffer buffer) { }
		public static void SetInsUnitsFactorProperty(ResultBuffer buffer) { }
		public static void SetIntensityColorSchemeProperty(ResultBuffer buffer) { }
		public static void SetIntersectionBoundaryColorProperty(ResultBuffer buffer) { }
		public static void SetIntersectionBoundaryDivisionLinesProperty(ResultBuffer buffer) { }
		public static void SetIntersectionBoundaryLayerProperty(ResultBuffer buffer) { }
		public static void SetIntersectionBoundaryLinetypeProperty(ResultBuffer buffer) { }
		public static void SetIntersectionBoundaryLinetypeScaleProperty(ResultBuffer buffer) { }
		public static void SetIntersectionBoundaryLineweightProperty(ResultBuffer buffer) { }
		public static void SetIntersectionBoundaryPlotStyleNameProperty(ResultBuffer buffer) { }
		public static void SetIntersectionBoundaryVisibleProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillColorProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillFaceTransparencyProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillHatchAngleProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillHatchPatternNameProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillHatchPatternTypeProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillHatchScaleProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillHatchSpacingProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillLayerProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillLinetypeProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillLinetypeScaleProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillLineweightProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillPlotStyleNameProperty(ResultBuffer buffer) { }
		public static void SetIntersectionFillVisibleProperty(ResultBuffer buffer) { }
		public static void SetInvisibleProperty(ResultBuffer buffer) { }
		public static void SetIsClonedProperty(ResultBuffer buffer) { }
		public static void SetIsDynamicBlockProperty(ResultBuffer buffer) { }
		public static void SetIsLayoutProperty(ResultBuffer buffer) { }
		public static void SetISOPenWidthProperty(ResultBuffer buffer) { }
		public static void SetIsOwnerXlatedProperty(ResultBuffer buffer) { }
		public static void SetIsPartialProperty(ResultBuffer buffer) { }
		public static void SetIsPeriodicProperty(ResultBuffer buffer) { }
		public static void SetIsPlanarProperty(ResultBuffer buffer) { }
		public static void SetIsPrimaryProperty(ResultBuffer buffer) { }
		public static void SetIsQuiescentProperty(ResultBuffer buffer) { }
		public static void SetIsRationalProperty(ResultBuffer buffer) { }
		public static void SetIssuerProperty(ResultBuffer buffer) { }
		public static void SetIsXRefProperty(ResultBuffer buffer) { }
		public static void SetItemNameProperty(ResultBuffer buffer) { }
		public static void SetJogAngleProperty(ResultBuffer buffer) { }
		public static void SetJogLocationProperty(ResultBuffer buffer) { }
		public static void SetJustificationProperty(ResultBuffer buffer) { }
		public static void SetKeyProperty(ResultBuffer buffer) { }
		public static void SetKeyboardAcceleratorProperty(ResultBuffer buffer) { }
		public static void SetKeyboardPriorityProperty(ResultBuffer buffer) { }
		public static void SetKeywordsProperty(ResultBuffer buffer) { }
		public static void SetKnotParameterizationProperty(ResultBuffer buffer) { }
		public static void SetKnotsProperty(ResultBuffer buffer) { }
		public static void SetLabelProperty(ResultBuffer buffer) { }
		public static void SetLabelBlockIdProperty(ResultBuffer buffer) { }
		public static void SetLandingGapProperty(ResultBuffer buffer) { }
		public static void SetLargeButtonsProperty(ResultBuffer buffer) { }
		public static void SetLastHeightProperty(ResultBuffer buffer) { }
		public static void SetLastSavedByProperty(ResultBuffer buffer) { }
		public static void SetLatitudeProperty(ResultBuffer buffer) { }
		public static void SetLayerProperty(ResultBuffer buffer) { }
		public static void SetLayerOnProperty(ResultBuffer buffer) { }
		public static void SetLayerPropertyOverridesProperty(ResultBuffer buffer) { }
		public static void SetLayersProperty(ResultBuffer buffer) { }
		public static void SetLayerStateProperty(ResultBuffer buffer) { }
		public static void SetLayoutProperty(ResultBuffer buffer) { }
		public static void SetLayoutCreateViewportProperty(ResultBuffer buffer) { }
		public static void SetLayoutCrosshairColorProperty(ResultBuffer buffer) { }
		public static void SetLayoutDisplayMarginsProperty(ResultBuffer buffer) { }
		public static void SetLayoutDisplayPaperProperty(ResultBuffer buffer) { }
		public static void SetLayoutDisplayPaperShadowProperty(ResultBuffer buffer) { }
		public static void SetLayoutIDProperty(ResultBuffer buffer) { }
		public static void SetLayoutsProperty(ResultBuffer buffer) { }
		public static void SetLayoutShowPlotSetupProperty(ResultBuffer buffer) { }
		public static void SetLeader1PointProperty(ResultBuffer buffer) { }
		public static void SetLeader2PointProperty(ResultBuffer buffer) { }
		public static void SetLeaderCountProperty(ResultBuffer buffer) { }
		public static void SetLeaderLengthProperty(ResultBuffer buffer) { }
		public static void SetLeaderLineColorProperty(ResultBuffer buffer) { }
		public static void SetLeaderLinetypeProperty(ResultBuffer buffer) { }
		public static void SetLeaderLineTypeIdProperty(ResultBuffer buffer) { }
		public static void SetLeaderLineWeightProperty(ResultBuffer buffer) { }
		public static void SetLeaderTypeProperty(ResultBuffer buffer) { }
		public static void SetLeftProperty(ResultBuffer buffer) { }
		public static void SetLengthProperty(ResultBuffer buffer) { }
		public static void SetLensLengthProperty(ResultBuffer buffer) { }
		public static void SetLimitsProperty(ResultBuffer buffer) { }
		public static void SetLinearScaleFactorProperty(ResultBuffer buffer) { }
		public static void SetLineSpacingDistanceProperty(ResultBuffer buffer) { }
		public static void SetLineSpacingFactorProperty(ResultBuffer buffer) { }
		public static void SetLineSpacingStyleProperty(ResultBuffer buffer) { }
		public static void SetLinetypeProperty(ResultBuffer buffer) { }
		public static void SetLinetypeGenerationProperty(ResultBuffer buffer) { }
		public static void SetLinetypesProperty(ResultBuffer buffer) { }
		public static void SetLinetypeScaleProperty(ResultBuffer buffer) { }
		public static void SetLineweightProperty(ResultBuffer buffer) { }
		public static void SetLineweightDisplayProperty(ResultBuffer buffer) { }
		public static void SetLiveSectionEnabledProperty(ResultBuffer buffer) { }
		public static void SetLoadAcadLspInAllDocumentsProperty(ResultBuffer buffer) { }
		public static void SetLocaleIDProperty(ResultBuffer buffer) { }
		public static void SetLockProperty(ResultBuffer buffer) { }
		public static void SetLockAspectRatioProperty(ResultBuffer buffer) { }
		public static void SetLockedProperty(ResultBuffer buffer) { }
		public static void SetLockPositionProperty(ResultBuffer buffer) { }
		public static void SetLogFileOnProperty(ResultBuffer buffer) { }
		public static void SetLogFilePathProperty(ResultBuffer buffer) { }
		public static void SetLongitudeProperty(ResultBuffer buffer) { }
		public static void SetLowerLeftCornerProperty(ResultBuffer buffer) { }
		public static void SetMacroProperty(ResultBuffer buffer) { }
		public static void SetMainDictionaryProperty(ResultBuffer buffer) { }
		public static void SetMaintainAssociativityProperty(ResultBuffer buffer) { }
		public static void SetMajorAxisProperty(ResultBuffer buffer) { }
		public static void SetMajorRadiusProperty(ResultBuffer buffer) { }
		public static void SetMaskProperty(ResultBuffer buffer) { }
		public static void SetMaterialProperty(ResultBuffer buffer) { }
		public static void SetMaterialsProperty(ResultBuffer buffer) { }
		public static void SetMaxActiveViewportsProperty(ResultBuffer buffer) { }
		public static void SetMaxAutoCADWindowProperty(ResultBuffer buffer) { }
		public static void SetMaxLeaderSegmentsPointsProperty(ResultBuffer buffer) { }
		public static void SetMCloseProperty(ResultBuffer buffer) { }
		public static void SetMDensityProperty(ResultBuffer buffer) { }
		public static void SetMeasurementProperty(ResultBuffer buffer) { }
		public static void SetMenuBarProperty(ResultBuffer buffer) { }
		public static void SetMenuFileProperty(ResultBuffer buffer) { }
		public static void SetMenuFileNameProperty(ResultBuffer buffer) { }
		public static void SetMenuGroupsProperty(ResultBuffer buffer) { }
		public static void SetMenusProperty(ResultBuffer buffer) { }
		public static void SetMinimumTableHeightProperty(ResultBuffer buffer) { }
		public static void SetMinimumTableWidthProperty(ResultBuffer buffer) { }
		public static void SetMinorAxisProperty(ResultBuffer buffer) { }
		public static void SetMinorRadiusProperty(ResultBuffer buffer) { }
		public static void SetMLineScaleProperty(ResultBuffer buffer) { }
		public static void SetModeProperty(ResultBuffer buffer) { }
		public static void SetModelCrosshairColorProperty(ResultBuffer buffer) { }
		public static void SetModelSpaceProperty(ResultBuffer buffer) { }
		public static void SetModelTypeProperty(ResultBuffer buffer) { }
		public static void SetModelViewProperty(ResultBuffer buffer) { }
		public static void SetMomentOfInertiaProperty(ResultBuffer buffer) { }
		public static void SetMonochromeProperty(ResultBuffer buffer) { }
		public static void SetMRUNumberProperty(ResultBuffer buffer) { }
		public static void SetMSpaceProperty(ResultBuffer buffer) { }
		public static void SetMTextAttributeProperty(ResultBuffer buffer) { }
		public static void SetMTextAttributeContentProperty(ResultBuffer buffer) { }
		public static void SetMTextBoundaryWidthProperty(ResultBuffer buffer) { }
		public static void SetMTextDrawingDirectionProperty(ResultBuffer buffer) { }
		public static void SetMVertexCountProperty(ResultBuffer buffer) { }
		public static void SetNameProperty(ResultBuffer buffer) { }
		public static void SetNameNoMnemonicProperty(ResultBuffer buffer) { }
		public static void SetNCloseProperty(ResultBuffer buffer) { }
		public static void SetNDensityProperty(ResultBuffer buffer) { }
		public static void SetNormalProperty(ResultBuffer buffer) { }
		public static void SetNotesProperty(ResultBuffer buffer) { }
		public static void SetNumberOfControlPointsProperty(ResultBuffer buffer) { }
		public static void SetNumberOfCopiesProperty(ResultBuffer buffer) { }
		public static void SetNumberOfFacesProperty(ResultBuffer buffer) { }
		public static void SetNumberOfFitPointsProperty(ResultBuffer buffer) { }
		public static void SetNumberOfLoopsProperty(ResultBuffer buffer) { }
		public static void SetNumberOfVerticesProperty(ResultBuffer buffer) { }
		public static void SetNumCellStylesProperty(ResultBuffer buffer) { }
		public static void SetNumCrossSectionsProperty(ResultBuffer buffer) { }
		public static void SetNumGuidePathsProperty(ResultBuffer buffer) { }
		public static void SetNumVerticesProperty(ResultBuffer buffer) { }
		public static void SetNVertexCountProperty(ResultBuffer buffer) { }
		public static void SetObjectIDProperty(ResultBuffer buffer) { }
		public static void SetObjectNameProperty(ResultBuffer buffer) { }
		public static void SetObjectSnapModeProperty(ResultBuffer buffer) { }
		public static void SetObjectSortByPlottingProperty(ResultBuffer buffer) { }
		public static void SetObjectSortByPSOutputProperty(ResultBuffer buffer) { }
		public static void SetObjectSortByRedrawsProperty(ResultBuffer buffer) { }
		public static void SetObjectSortByRegensProperty(ResultBuffer buffer) { }
		public static void SetObjectSortBySelectionProperty(ResultBuffer buffer) { }
		public static void SetObjectSortBySnapProperty(ResultBuffer buffer) { }
		public static void SetObliqueAngleProperty(ResultBuffer buffer) { }
		public static void SetOleItemTypeProperty(ResultBuffer buffer) { }
		public static void SetOLELaunchProperty(ResultBuffer buffer) { }
		public static void SetOlePlotQualityProperty(ResultBuffer buffer) { }
		public static void SetOLEQualityProperty(ResultBuffer buffer) { }
		public static void SetOleSourceAppProperty(ResultBuffer buffer) { }
		public static void SetOnMenuBarProperty(ResultBuffer buffer) { }
		public static void SetOpenSaveProperty(ResultBuffer buffer) { }
		public static void SetOriginProperty(ResultBuffer buffer) { }
		public static void SetOrthoOnProperty(ResultBuffer buffer) { }
		public static void SetOutputProperty(ResultBuffer buffer) { }
		public static void SetOverrideCenterProperty(ResultBuffer buffer) { }
		public static void SetOverwritePropChangedProperty(ResultBuffer buffer) { }
		public static void SetOwnerIDProperty(ResultBuffer buffer) { }
		public static void SetPageSetupOverridesTemplateFileProperty(ResultBuffer buffer) { }
		public static void SetPaperSpaceProperty(ResultBuffer buffer) { }
		public static void SetPaperUnitsProperty(ResultBuffer buffer) { }
		public static void SetParentProperty(ResultBuffer buffer) { }
		public static void SetPathProperty(ResultBuffer buffer) { }
		public static void SetPatternAngleProperty(ResultBuffer buffer) { }
		public static void SetPatternDoubleProperty(ResultBuffer buffer) { }
		public static void SetPatternNameProperty(ResultBuffer buffer) { }
		public static void SetPatternScaleProperty(ResultBuffer buffer) { }
		public static void SetPatternSpaceProperty(ResultBuffer buffer) { }
		public static void SetPatternTypeProperty(ResultBuffer buffer) { }
		public static void SetPerimeterProperty(ResultBuffer buffer) { }
		public static void SetPeriodicProperty(ResultBuffer buffer) { }
		public static void SetPickAddProperty(ResultBuffer buffer) { }
		public static void SetPickAutoProperty(ResultBuffer buffer) { }
		public static void SetPickBoxSizeProperty(ResultBuffer buffer) { }
		public static void SetPickDragProperty(ResultBuffer buffer) { }
		public static void SetPickFirstProperty(ResultBuffer buffer) { }
		public static void SetPickfirstSelectionSetProperty(ResultBuffer buffer) { }
		public static void SetPickGroupProperty(ResultBuffer buffer) { }
		public static void SetPlotProperty(ResultBuffer buffer) { }
		public static void SetPlotConfigurationsProperty(ResultBuffer buffer) { }
		public static void SetPlotHiddenProperty(ResultBuffer buffer) { }
		public static void SetPlotLegacyProperty(ResultBuffer buffer) { }
		public static void SetPlotLogFilePathProperty(ResultBuffer buffer) { }
		public static void SetPlotOriginProperty(ResultBuffer buffer) { }
		public static void SetPlotPolicyProperty(ResultBuffer buffer) { }
		public static void SetPlotRotationProperty(ResultBuffer buffer) { }
		public static void SetPlotStyleNameProperty(ResultBuffer buffer) { }
		public static void SetPlottableProperty(ResultBuffer buffer) { }
		public static void SetPlotTypeProperty(ResultBuffer buffer) { }
		public static void SetPlotViewportBordersProperty(ResultBuffer buffer) { }
		public static void SetPlotViewportsFirstProperty(ResultBuffer buffer) { }
		public static void SetPlotWithLineweightsProperty(ResultBuffer buffer) { }
		public static void SetPlotWithPlotStylesProperty(ResultBuffer buffer) { }
		public static void SetPolarTrackingVectorProperty(ResultBuffer buffer) { }
		public static void SetPositionProperty(ResultBuffer buffer) { }
		public static void SetPostScriptPrologFileProperty(ResultBuffer buffer) { }
		public static void SetPreferencesProperty(ResultBuffer buffer) { }
		public static void SetPresetProperty(ResultBuffer buffer) { }
		public static void SetPrimaryUnitsPrecisionProperty(ResultBuffer buffer) { }
		public static void SetPrincipalDirectionsProperty(ResultBuffer buffer) { }
		public static void SetPrincipalMomentsProperty(ResultBuffer buffer) { }
		public static void SetPrinterConfigPathProperty(ResultBuffer buffer) { }
		public static void SetPrinterDescPathProperty(ResultBuffer buffer) { }
		public static void SetPrinterPaperSizeAlertProperty(ResultBuffer buffer) { }
		public static void SetPrinterSpoolAlertProperty(ResultBuffer buffer) { }
		public static void SetPrinterStyleSheetPathProperty(ResultBuffer buffer) { }
		public static void SetPrintFileProperty(ResultBuffer buffer) { }
		public static void SetPrintSpoolerPathProperty(ResultBuffer buffer) { }
		public static void SetPrintSpoolExecutableProperty(ResultBuffer buffer) { }
		public static void SetProductOfInertiaProperty(ResultBuffer buffer) { }
		public static void SetProfileRotationProperty(ResultBuffer buffer) { }
		public static void SetProfilesProperty(ResultBuffer buffer) { }
		public static void SetPromptStringProperty(ResultBuffer buffer) { }
		public static void SetPropertyNameProperty(ResultBuffer buffer) { }
		public static void SetProxyImageProperty(ResultBuffer buffer) { }
		public static void SetQNewTemplateFileProperty(ResultBuffer buffer) { }
		public static void SetQuietErrorModeProperty(ResultBuffer buffer) { }
		public static void SetRadiiOfGyrationProperty(ResultBuffer buffer) { }
		public static void SetRadiusProperty(ResultBuffer buffer) { }
		public static void SetRadiusRatioProperty(ResultBuffer buffer) { }
		public static void SetReadOnlyProperty(ResultBuffer buffer) { }
		public static void SetRedProperty(ResultBuffer buffer) { }
		public static void SetRegenerateTableSuppressedProperty(ResultBuffer buffer) { }
		public static void SetRegisteredApplicationsProperty(ResultBuffer buffer) { }
		public static void SetRemoveHiddenLinesProperty(ResultBuffer buffer) { }
		public static void SetRenderSmoothnessProperty(ResultBuffer buffer) { }
		public static void SetRepeatBottomLabelsProperty(ResultBuffer buffer) { }
		public static void SetRepeatTopLabelsProperty(ResultBuffer buffer) { }
		public static void SetRevisionNumberProperty(ResultBuffer buffer) { }
		public static void SetRevolutionAngleProperty(ResultBuffer buffer) { }
		public static void SetRotationProperty(ResultBuffer buffer) { }
		public static void SetRoundDistanceProperty(ResultBuffer buffer) { }
		public static void SetRowHeightProperty(ResultBuffer buffer) { }
		public static void SetRowsProperty(ResultBuffer buffer) { }
		public static void SetRowSpacingProperty(ResultBuffer buffer) { }
		public static void SetSaveAsTypeProperty(ResultBuffer buffer) { }
		public static void SetSavedProperty(ResultBuffer buffer) { }
		public static void SetSavePreviewThumbnailProperty(ResultBuffer buffer) { }
		public static void SetScaleProperty(ResultBuffer buffer) { }
		public static void SetScaleFactorProperty(ResultBuffer buffer) { }
		public static void SetScaleHeightProperty(ResultBuffer buffer) { }
		public static void SetScaleLineweightsProperty(ResultBuffer buffer) { }
		public static void SetScaleWidthProperty(ResultBuffer buffer) { }
		public static void SetSCMCommandModeProperty(ResultBuffer buffer) { }
		public static void SetSCMDefaultModeProperty(ResultBuffer buffer) { }
		public static void SetSCMEditModeProperty(ResultBuffer buffer) { }
		public static void SetSCMTimeModeProperty(ResultBuffer buffer) { }
		public static void SetSCMTimeValueProperty(ResultBuffer buffer) { }
		public static void SetSecondPointProperty(ResultBuffer buffer) { }
		public static void SetSecondSegmentAngleConstraintProperty(ResultBuffer buffer) { }
		public static void SetSectionManagerProperty(ResultBuffer buffer) { }
		public static void SetSectionPlaneOffsetProperty(ResultBuffer buffer) { }
		public static void SetSegmentationProperty(ResultBuffer buffer) { }
		public static void SetSegmentPerPolylineProperty(ResultBuffer buffer) { }
		public static void SetSelectionProperty(ResultBuffer buffer) { }
		public static void SetSelectionSetsProperty(ResultBuffer buffer) { }
		public static void SetSerialNumberProperty(ResultBuffer buffer) { }
		public static void SetSettingsProperty(ResultBuffer buffer) { }
		public static void SetShadePlotProperty(ResultBuffer buffer) { }
		public static void SetSheetViewProperty(ResultBuffer buffer) { }
		public static void SetShortcutMenuProperty(ResultBuffer buffer) { }
		public static void SetShortCutMenuDisplayProperty(ResultBuffer buffer) { }
		public static void SetShowProperty(ResultBuffer buffer) { }
		public static void SetShowAssociativityProperty(ResultBuffer buffer) { }
		public static void SetShowClippedProperty(ResultBuffer buffer) { }
		public static void SetShowCroppedProperty(ResultBuffer buffer) { }
		public static void SetShowHistoryProperty(ResultBuffer buffer) { }
		public static void SetShowIntensityProperty(ResultBuffer buffer) { }
		public static void SetShowPlotStylesProperty(ResultBuffer buffer) { }
		public static void SetShowProxyDialogBoxProperty(ResultBuffer buffer) { }
		public static void SetShowRasterImageProperty(ResultBuffer buffer) { }
		public static void SetShowRotationProperty(ResultBuffer buffer) { }
		public static void SetShowWarningMessagesProperty(ResultBuffer buffer) { }
		public static void SetSingleDocumentModeProperty(ResultBuffer buffer) { }
		public static void SetSliceDepthProperty(ResultBuffer buffer) { }
		public static void SetSmoothnessProperty(ResultBuffer buffer) { }
		public static void SetSnapBasePointProperty(ResultBuffer buffer) { }
		public static void SetSnapOnProperty(ResultBuffer buffer) { }
		public static void SetSnapRotationAngleProperty(ResultBuffer buffer) { }
		public static void SetSolidFillProperty(ResultBuffer buffer) { }
		public static void SetSolidTypeProperty(ResultBuffer buffer) { }
		public static void SetSourceObjectsProperty(ResultBuffer buffer) { }
		public static void SetSplineFrameProperty(ResultBuffer buffer) { }
		public static void SetSplineMethodProperty(ResultBuffer buffer) { }
		public static void SetStandardScaleProperty(ResultBuffer buffer) { }
		public static void SetStandardScale2Property(ResultBuffer buffer) { }
		public static void SetStartAngleProperty(ResultBuffer buffer) { }
		public static void SetStartDraftAngleProperty(ResultBuffer buffer) { }
		public static void SetStartDraftMagnitudeProperty(ResultBuffer buffer) { }
		public static void SetStartParameterProperty(ResultBuffer buffer) { }
		public static void SetStartPointProperty(ResultBuffer buffer) { }
		public static void SetStartSmoothContinuityProperty(ResultBuffer buffer) { }
		public static void SetStartSmoothMagnitudeProperty(ResultBuffer buffer) { }
		public static void SetStartTangentProperty(ResultBuffer buffer) { }
		public static void SetStateProperty(ResultBuffer buffer) { }
		public static void SetState2Property(ResultBuffer buffer) { }
		public static void SetStatusIDProperty(ResultBuffer buffer) { }
		public static void SetStoreSQLIndexProperty(ResultBuffer buffer) { }
		public static void SetStyleNameProperty(ResultBuffer buffer) { }
		public static void SetStyleSheetProperty(ResultBuffer buffer) { }
		public static void SetStylizationProperty(ResultBuffer buffer) { }
		public static void SetSubjectProperty(ResultBuffer buffer) { }
		public static void SetSubMenuProperty(ResultBuffer buffer) { }
		public static void SetSubUnitsFactorProperty(ResultBuffer buffer) { }
		public static void SetSubUnitsSuffixProperty(ResultBuffer buffer) { }
		public static void SetSummaryInfoProperty(ResultBuffer buffer) { }
		public static void SetSupportPathProperty(ResultBuffer buffer) { }
		public static void SetSuppressLeadingZerosProperty(ResultBuffer buffer) { }
		public static void SetSuppressTrailingZerosProperty(ResultBuffer buffer) { }
		public static void SetSuppressZeroFeetProperty(ResultBuffer buffer) { }
		public static void SetSuppressZeroInchesProperty(ResultBuffer buffer) { }
		public static void SetSurfaceNormalsProperty(ResultBuffer buffer) { }
		public static void SetSurfaceTypeProperty(ResultBuffer buffer) { }
		public static void SetSurfTrimAssociativityProperty(ResultBuffer buffer) { }
		public static void SetSymbolPositionProperty(ResultBuffer buffer) { }
		public static void SetSystemProperty(ResultBuffer buffer) { }
		public static void SetTableBreakFlowDirectionProperty(ResultBuffer buffer) { }
		public static void SetTableBreakHeightProperty(ResultBuffer buffer) { }
		public static void SetTablesReadOnlyProperty(ResultBuffer buffer) { }
		public static void SetTableStyleOverridesProperty(ResultBuffer buffer) { }
		public static void SetTabOrderProperty(ResultBuffer buffer) { }
		public static void SetTagStringProperty(ResultBuffer buffer) { }
		public static void SetTaperAngleProperty(ResultBuffer buffer) { }
		public static void SetTargetProperty(ResultBuffer buffer) { }
		public static void SetTempFileExtensionProperty(ResultBuffer buffer) { }
		public static void SetTempFilePathProperty(ResultBuffer buffer) { }
		public static void SetTemplateDWGPathProperty(ResultBuffer buffer) { }
		public static void SetTemplateIdProperty(ResultBuffer buffer) { }
		public static void SetTempXRefPathProperty(ResultBuffer buffer) { }
		public static void SetTextAlignmentPointProperty(ResultBuffer buffer) { }
		public static void SetTextAlignmentTypeProperty(ResultBuffer buffer) { }
		public static void SetTextAngleTypeProperty(ResultBuffer buffer) { }
		public static void SetTextAttachmentDirectionProperty(ResultBuffer buffer) { }
		public static void SetTextAttachmentTypeProperty(ResultBuffer buffer) { }
		public static void SetTextBackgroundFillProperty(ResultBuffer buffer) { }
		public static void SetTextBottomAttachmentTypeProperty(ResultBuffer buffer) { }
		public static void SetTextColorProperty(ResultBuffer buffer) { }
		public static void SetTextDirectionProperty(ResultBuffer buffer) { }
		public static void SetTextEditorProperty(ResultBuffer buffer) { }
		public static void SetTextFillProperty(ResultBuffer buffer) { }
		public static void SetTextFillColorProperty(ResultBuffer buffer) { }
		public static void SetTextFontProperty(ResultBuffer buffer) { }
		public static void SetTextFontSizeProperty(ResultBuffer buffer) { }
		public static void SetTextFontStyleProperty(ResultBuffer buffer) { }
		public static void SetTextFrameDisplayProperty(ResultBuffer buffer) { }
		public static void SetTextGapProperty(ResultBuffer buffer) { }
		public static void SetTextGenerationFlagProperty(ResultBuffer buffer) { }
		public static void SetTextHeightProperty(ResultBuffer buffer) { }
		public static void SetTextInsideProperty(ResultBuffer buffer) { }
		public static void SetTextInsideAlignProperty(ResultBuffer buffer) { }
		public static void SetTextJustifyProperty(ResultBuffer buffer) { }
		public static void SetTextLeftAttachmentTypeProperty(ResultBuffer buffer) { }
		public static void SetTextLineSpacingDistanceProperty(ResultBuffer buffer) { }
		public static void SetTextLineSpacingFactorProperty(ResultBuffer buffer) { }
		public static void SetTextLineSpacingStyleProperty(ResultBuffer buffer) { }
		public static void SetTextMovementProperty(ResultBuffer buffer) { }
		public static void SetTextOutsideAlignProperty(ResultBuffer buffer) { }
		public static void SetTextOverrideProperty(ResultBuffer buffer) { }
		public static void SetTextPositionProperty(ResultBuffer buffer) { }
		public static void SetTextPrecisionProperty(ResultBuffer buffer) { }
		public static void SetTextPrefixProperty(ResultBuffer buffer) { }
		public static void SetTextRightAttachmentTypeProperty(ResultBuffer buffer) { }
		public static void SetTextRotationProperty(ResultBuffer buffer) { }
		public static void SetTextStringProperty(ResultBuffer buffer) { }
		public static void SetTextStyleProperty(ResultBuffer buffer) { }
		public static void SetTextStyleNameProperty(ResultBuffer buffer) { }
		public static void SetTextStylesProperty(ResultBuffer buffer) { }
		public static void SetTextSuffixProperty(ResultBuffer buffer) { }
		public static void SetTextTopAttachmentTypeProperty(ResultBuffer buffer) { }
		public static void SetTextureMapPathProperty(ResultBuffer buffer) { }
		public static void SetTextWidthProperty(ResultBuffer buffer) { }
		public static void SetTextWinBackgrndColorProperty(ResultBuffer buffer) { }
		public static void SetTextWinTextColorProperty(ResultBuffer buffer) { }
		public static void SetThicknessProperty(ResultBuffer buffer) { }
		public static void SetTimeServerProperty(ResultBuffer buffer) { }
		public static void SetTitleProperty(ResultBuffer buffer) { }
		public static void SetTitleSuppressedProperty(ResultBuffer buffer) { }
		public static void SetToleranceDisplayProperty(ResultBuffer buffer) { }
		public static void SetToleranceHeightScaleProperty(ResultBuffer buffer) { }
		public static void SetToleranceJustificationProperty(ResultBuffer buffer) { }
		public static void SetToleranceLowerLimitProperty(ResultBuffer buffer) { }
		public static void SetTolerancePrecisionProperty(ResultBuffer buffer) { }
		public static void SetToleranceSuppressLeadingZerosProperty(ResultBuffer buffer) { }
		public static void SetToleranceSuppressTrailingZerosProperty(ResultBuffer buffer) { }
		public static void SetToleranceSuppressZeroFeetProperty(ResultBuffer buffer) { }
		public static void SetToleranceSuppressZeroInchesProperty(ResultBuffer buffer) { }
		public static void SetToleranceUpperLimitProperty(ResultBuffer buffer) { }
		public static void SetToolbarsProperty(ResultBuffer buffer) { }
		public static void SetToolPalettePathProperty(ResultBuffer buffer) { }
		public static void SetTopProperty(ResultBuffer buffer) { }
		public static void SetTopHeightProperty(ResultBuffer buffer) { }
		public static void SetTopRadiusProperty(ResultBuffer buffer) { }
		public static void SetTotalAngleProperty(ResultBuffer buffer) { }
		public static void SetTotalLengthProperty(ResultBuffer buffer) { }
		public static void SetTranslateIDsProperty(ResultBuffer buffer) { }
		public static void SetTransparencyProperty(ResultBuffer buffer) { }
		public static void SetTrueColorProperty(ResultBuffer buffer) { }
		public static void SetTrueColorImagesProperty(ResultBuffer buffer) { }
		public static void SetTurnHeightProperty(ResultBuffer buffer) { }
		public static void SetTurnsProperty(ResultBuffer buffer) { }
		public static void SetTurnSlopeProperty(ResultBuffer buffer) { }
		public static void SetTwistProperty(ResultBuffer buffer) { }
		public static void SetTwistAngleProperty(ResultBuffer buffer) { }
		public static void SetTypeProperty(ResultBuffer buffer) { }
		public static void SetUCSIconAtOriginProperty(ResultBuffer buffer) { }
		public static void SetUCSIconOnProperty(ResultBuffer buffer) { }
		public static void SetUCSPerViewportProperty(ResultBuffer buffer) { }
		public static void SetUIsolineDensityProperty(ResultBuffer buffer) { }
		public static void SetUnderlayLayerOverrideAppliedProperty(ResultBuffer buffer) { }
		public static void SetUnderlayNameProperty(ResultBuffer buffer) { }
		public static void SetUnderlayVisibilityProperty(ResultBuffer buffer) { }
		public static void SetUnitProperty(ResultBuffer buffer) { }
		public static void SetUnitFactorProperty(ResultBuffer buffer) { }
		public static void SetUnitsProperty(ResultBuffer buffer) { }
		public static void SetUnitsFormatProperty(ResultBuffer buffer) { }
		public static void SetUnitsTypeProperty(ResultBuffer buffer) { }
		public static void SetUpperRightCornerProperty(ResultBuffer buffer) { }
		public static void SetUpsideDownProperty(ResultBuffer buffer) { }
		public static void SetURLProperty(ResultBuffer buffer) { }
		public static void SetURLDescriptionProperty(ResultBuffer buffer) { }
		public static void SetURLNamedLocationProperty(ResultBuffer buffer) { }
		public static void SetUsedProperty(ResultBuffer buffer) { }
		public static void SetUseEntityColorProperty(ResultBuffer buffer) { }
		public static void SetUseLastPlotSettingsProperty(ResultBuffer buffer) { }
		public static void SetUserProperty(ResultBuffer buffer) { }
		public static void SetUserCoordinateSystemsProperty(ResultBuffer buffer) { }
		public static void SetUseStandardScaleProperty(ResultBuffer buffer) { }
		public static void SetUtilityProperty(ResultBuffer buffer) { }
		public static void SetValueProperty(ResultBuffer buffer) { }
		public static void SetVBEProperty(ResultBuffer buffer) { }
		public static void SetVerifyProperty(ResultBuffer buffer) { }
		public static void SetVersionProperty(ResultBuffer buffer) { }
		public static void SetVertCellMarginProperty(ResultBuffer buffer) { }
		public static void SetVertexCountProperty(ResultBuffer buffer) { }
		public static void SetVerticalDirectionProperty(ResultBuffer buffer) { }
		public static void SetVerticalTextPositionProperty(ResultBuffer buffer) { }
		public static void SetVerticesProperty(ResultBuffer buffer) { }
		public static void SetViewingDirectionProperty(ResultBuffer buffer) { }
		public static void SetViewportDefaultProperty(ResultBuffer buffer) { }
		public static void SetViewportOnProperty(ResultBuffer buffer) { }
		public static void SetViewportsProperty(ResultBuffer buffer) { }
		public static void SetViewsProperty(ResultBuffer buffer) { }
		public static void SetViewToPlotProperty(ResultBuffer buffer) { }
		public static void SetVisibilityEdge1Property(ResultBuffer buffer) { }
		public static void SetVisibilityEdge2Property(ResultBuffer buffer) { }
		public static void SetVisibilityEdge3Property(ResultBuffer buffer) { }
		public static void SetVisibilityEdge4Property(ResultBuffer buffer) { }
		public static void SetVisibleProperty(ResultBuffer buffer) { }
		public static void SetVIsolineDensityProperty(ResultBuffer buffer) { }
		public static void SetVisualStyleProperty(ResultBuffer buffer) { }
		public static void SetVolumeProperty(ResultBuffer buffer) { }
		public static void SetWeightsProperty(ResultBuffer buffer) { }
		public static void SetWidthProperty(ResultBuffer buffer) { }
		public static void SetWindowLeftProperty(ResultBuffer buffer) { }
		public static void SetWindowStateProperty(ResultBuffer buffer) { }
		public static void SetWindowTitleProperty(ResultBuffer buffer) { }
		public static void SetWindowTopProperty(ResultBuffer buffer) { }
		public static void SetWireframeTypeProperty(ResultBuffer buffer) { }
		public static void SetWorkspacePathProperty(ResultBuffer buffer) { }
		public static void SetXEffectiveScaleFactorProperty(ResultBuffer buffer) { }
		public static void SetXRefDatabaseProperty(ResultBuffer buffer) { }
		public static void SetXRefDemandLoadProperty(ResultBuffer buffer) { }
		public static void SetXRefEditProperty(ResultBuffer buffer) { }
		public static void SetXRefFadeIntensityProperty(ResultBuffer buffer) { }
		public static void SetXRefLayerVisibilityProperty(ResultBuffer buffer) { }
		public static void SetXScaleFactorProperty(ResultBuffer buffer) { }
		public static void SetXVectorProperty(ResultBuffer buffer) { }
		public static void SetYEffectiveScaleFactorProperty(ResultBuffer buffer) { }
		public static void SetYScaleFactorProperty(ResultBuffer buffer) { }
		public static void SetYVectorProperty(ResultBuffer buffer) { }
		public static void SetZEffectiveScaleFactorProperty(ResultBuffer buffer) { }
		public static void SetZScaleFactorProperty(ResultBuffer buffer) { }

		#endregion
	}
}
