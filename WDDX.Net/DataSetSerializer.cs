using System;
using System.Data;
using System.Collections;
using System.Xml;

namespace Mueller.Wddx
{
    /// <summary>
    ///		This class serializes a <see cref="DataSet"/> into a WDDX <c>recordset</c>.
    /// </summary>
    /// <remarks>
    ///		<para>If the DataSet contains more than one <see cref="DataTable"/>, the DataSet
    ///		will be serialized as a <c>struct</c> containing <c>recordset</c>s.  Otherwise, it
    ///		will be serialized as a single <c>recordset</c>.</para>
    ///		<para>Note that a <c>struct</c> containing <c>recordset</c>s will be deserialized
    ///		as a <see cref="Hashtable"/> containing <see cref="DataSet"/>s, so beware of round-trip
    ///		issues when working with a <see cref="DataSet"/> containing multiple <see cref="DataTable"/>s.</para>
    /// </remarks>
    ///	<seealso cref="IWddxObjectSerializer"/>
    internal class DataSetSerializer : IWddxObjectSerializer
    {
		private static DataSetSerializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static DataSetSerializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(DataSetSerializer))
					{
						if (instance == null)
							instance = new DataSetSerializer();
					}
				}

				return instance;
			}
		}

		private DataSetSerializer() {}

		/// <summary>
		///		Writes the serialized object to the XML stream.
		/// </summary>
		/// <param name="output">A pre-initialized <see cref="XmlTextWriter"/> object.</param>
		/// <param name="obj">Object to serialize.</param>
		public void WriteObject(XmlWriter output, object obj)
		{
			DataSet thisDataSet = obj as DataSet;

			if (thisDataSet == null)
				throw new WddxException("Attemped to serialize incompatible object. Expected: " + 
					typeof(DataSet).FullName + " but got: " +
					((obj == null) ? "null" : obj.GetType().FullName));

			if (thisDataSet.Tables.Count > 1)
			{
				// Output a dataset containing multiple tables as a struct
				// containing recordsets.
				output.WriteStartElement("", "struct", "");

				foreach (DataTable table in thisDataSet.Tables)
				{
					output.WriteStartElement("var");
					output.WriteAttributeString("name", table.TableName);
					WriteTable(output, table);
					output.WriteEndElement();
				}

				output.WriteEndElement();
			}
			else if (thisDataSet.Tables.Count == 1)
			{
				WriteTable(output, thisDataSet.Tables[0]);
			}
			else
				throw new WddxException("DataSet must contain at least one DataTable.");
		}

		/// <summary>
		///		Writes out a single DataTable to the XML Stream.
		/// </summary>
		/// <param name="output">A pre-initialized <see cref="XmlTextWriter"/> object.</param>
		/// <param name="table">The DataTable to be written.</param>
		private void WriteTable(XmlWriter output, DataTable table)
		{
			DataColumnCollection columns = table.Columns;
			DataRowCollection rows = table.Rows;
			WddxObjectSerializerFactory factory = new WddxObjectSerializerFactory();

			string[] ColumnNames = new string[columns.Count];

			for (int i=0; i < ColumnNames.Length; i++)
			{
				ColumnNames[i] = columns[i].ColumnName;
			}

			output.WriteStartElement("recordset");
			output.WriteAttributeString("rowCount", rows.Count.ToString());
			output.WriteAttributeString("fieldNames", String.Join(",", ColumnNames));

			object thisCell;
			foreach (DataColumn column in columns)
			{
				output.WriteStartElement("field");
				output.WriteAttributeString("name", column.ColumnName);
				
				foreach (DataRow row in rows)
				{
					thisCell = row[column];
					factory.GetSerializer(thisCell).WriteObject(output, thisCell);
				}

				output.WriteEndElement();
			}

			output.WriteEndElement();
		}
    }
}
