using System;
using System.Xml;
using System.Data;
using System.Collections;

namespace Mueller.Wddx
{
	/// <summary>
	///		Deserializes a WDDX recordset into a <see cref="DataSet"/> object.
	/// </summary>
    /// <remarks>
    ///		This class is a Singleton class - only one instance of it will ever exist
    ///		in a given AppDomain.
    ///	</remarks>
    ///	<seealso cref="IWddxElementDeserializer"/>
    internal class DataSetDeserializer : IWddxElementDeserializer
    {
		public static DataSetDeserializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static DataSetDeserializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(DataSetDeserializer))
					{
						if (instance == null)
							instance = new DataSetDeserializer();
					}
				}

				return instance;
			}
		}

		private DataSetDeserializer() {}

		/// <summary>
		///		Parses the WDDX element and returns the deserialized
		///		content as a <see cref="DataSet"/> object, advancing the reader to the next
		///		element.
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
		public object ParseElement(XmlReader input)
		{
			if (input.IsEmptyElement)
			{
				input.Skip();
				return new DataSet();
			}

			int Rowcount = Int32.Parse(input.GetAttribute("rowCount"));
			DataTable myDataTable = new DataTable();
			
			Hashtable tableData = new Hashtable();
			ArrayList columnData;
			object elementValue;
			bool firstRow = true;
			IWddxElementDeserializer deserializer;
			DataColumn myDataColumn;
			string ColumnName;

			while (input.Read() && (!(input.Name == "recordset" && input.NodeType == XmlNodeType.EndElement)))
			{
				if (input.Name == "field" && input.NodeType != XmlNodeType.EndElement)
				{
					// create the column information
					ColumnName = input.GetAttribute("name");
					columnData = new ArrayList(Rowcount);
					myDataColumn = new DataColumn();
					myDataColumn.ColumnName = ColumnName;
					// add the column to the table
					myDataTable.Columns.Add(myDataColumn);
					// add the arraylist to the hashtable of data
					tableData.Add(ColumnName, columnData);
					input.Read();

					while (!(input.Name == "field" && input.NodeType == XmlNodeType.EndElement))
					{
						// get all the values
						deserializer = WddxElementDeserializerFactory.GetDeserializer(input.Name);
						elementValue = deserializer.ParseElement(input);
						if (firstRow)
						{
							// set the data type of the column
							myDataColumn.DataType = elementValue.GetType();
							firstRow = false;
						}
						columnData.Add(elementValue);
					}
					firstRow = true;
				}
			}
			input.ReadEndElement();

			// finished parsing data - now build the rest of the dataset
			DataSet myDataSet = new DataSet();
			myDataSet.Tables.Add(myDataTable);

			myDataTable.BeginLoadData();

			// add the data rows
			DataRow myDataRow;
			for (int i=0; i < Rowcount; i++)
			{
				myDataRow = myDataTable.NewRow();
				foreach (string key in tableData.Keys)
				{
					myDataRow[key] = ((ArrayList)tableData[key])[i];
				}
				myDataTable.Rows.Add(myDataRow);
			}

			myDataTable.EndLoadData();

			return myDataSet;
		}
    }
}
