using System;
using System.Xml;
using System.Collections;

namespace Mueller.Wddx
{
    /// <summary>
    ///		This class will serialize any object that implements the <see cref="ICollection"/>
    ///		interface into a WDDX <c>array</c>.
    /// </summary>
    ///	<seealso cref="IWddxObjectSerializer"/>
    internal class ArraySerializer : IWddxObjectSerializer
    {
		private static ArraySerializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static ArraySerializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(ArraySerializer))
					{
						if (instance == null)
							instance = new ArraySerializer();
					}
				}

				return instance;
			}
		}

		private ArraySerializer() {}

		/// <summary>
		///		Writes the serialized object to the XML stream.
		/// </summary>
		/// <param name="output">A pre-initialized <see cref="XmlTextWriter"/> object.</param>
		/// <param name="obj">Object to serialize.</param>
		public void WriteObject(XmlWriter output, object obj)
		{
			WddxObjectSerializerFactory factory = new WddxObjectSerializerFactory();

			ICollection thisCollection = obj as ICollection;

			if (thisCollection == null)
				throw new WddxException("Attemped to serialize incompatible object. Expected: " + 
					typeof(ICollection).FullName + " but got: " +
					((obj == null) ? "null" : obj.GetType().FullName));

			output.WriteStartElement("array");
			output.WriteAttributeString("length", thisCollection.Count.ToString());

			foreach (object thisObject in thisCollection)
			{
				factory.GetSerializer(thisObject).WriteObject(output, thisObject);
			}

			output.WriteEndElement();
		}
    }
}
