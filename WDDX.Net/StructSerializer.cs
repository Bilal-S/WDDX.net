using System;
using System.Xml;
using System.Collections;

namespace Mueller.Wddx
{
    /// <summary>
    ///		This class will serialize any object that implements the <see cref="IDictionary"/>
    ///		interface into a WDDX <c>struct</c>.
    /// </summary>
    /// <remarks>
    ///		This class is a Singleton class - only one instance of it will ever exist
    ///		in a given AppDomain.
    ///	</remarks>
    ///	<seealso cref="IWddxObjectSerializer"/>
    internal class StructSerializer : IWddxObjectSerializer
    {
		private static StructSerializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static StructSerializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(StructSerializer))
					{
						if (instance == null)
							instance = new StructSerializer();
					}
				}

				return instance;
			}
		}

		private StructSerializer() {}

		/// <summary>
		///		Writes the serialized object to the XML stream.
		/// </summary>
		/// <param name="output">A pre-initialized <see cref="XmlTextWriter"/> object.</param>
		/// <param name="obj">Object to serialize.</param>
		public void WriteObject(XmlWriter output, object obj)
		{
			IDictionary thisDict = obj as IDictionary;

			if (thisDict == null)
				throw new WddxException("Attemped to serialize incompatible object. Expected: " + 
					typeof(IDictionary).FullName + " but got: " +
					((obj == null) ? "null" : obj.GetType().FullName));

			WddxObjectSerializerFactory factory = new WddxObjectSerializerFactory();

			output.WriteStartElement("struct");

			object thisObject;

			foreach (object Key in thisDict.Keys)
			{
				output.WriteStartElement("var");
				output.WriteAttributeString("name", Key.ToString());

				thisObject = thisDict[Key];
				
				factory.GetSerializer(thisObject).WriteObject(output, thisObject);

				output.WriteEndElement();
			}

			output.WriteEndElement();
		}
    }
}
