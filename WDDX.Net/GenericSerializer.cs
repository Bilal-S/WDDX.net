using System;
using System.Xml;
using System.Reflection;

namespace Mueller.Wddx
{
    /// <summary>
    ///		Provides serialization services for any object not covered by a pre-defined serializer.
    /// </summary>
    /// <remarks>
    ///		<para>If the object has properties or fields, they will be serialized to a WDDX struct.
    ///		If not, a call to the object's <c>ToString()</c> method will be serialized as a string
    ///		value.</para>
    ///		
    ///		<para>This class is a Singleton class - only one instance of it will ever exist
    ///		in a given AppDomain.</para>
	/// </remarks>
    ///	<seealso cref="IWddxObjectSerializer"/>
    internal class GenericSerializer : IWddxObjectSerializer
    {
        private static GenericSerializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static GenericSerializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(GenericSerializer))
					{
						if (instance == null)
							instance = new GenericSerializer();
					}
				}

				return instance;
			}
		}

		private GenericSerializer() {}

		/// <summary>
		///		Writes the serialized object to the XML stream.
		/// </summary>
		/// <param name="output">A pre-initialized <see cref="XmlTextWriter"/> object.</param>
		/// <param name="obj">Object to serialize.</param>
		public void WriteObject(XmlWriter output, object obj)
		{
			if (obj == null)
				throw new WddxException("Attemped to serialize incompatible object: null.");

			Type objType = obj.GetType();
			PropertyInfo[] objProperties = objType.GetProperties();
			FieldInfo[] objFields = objType.GetFields();

			if (objProperties.Length == 0 && objFields.Length == 0)
			{
				output.WriteElementString("string", obj.ToString());
			}
			else
			{
				if (objProperties.Length > 0)
				{
					WddxObjectSerializerFactory factory = new WddxObjectSerializerFactory();

					output.WriteStartElement("", "struct", "");
					output.WriteAttributeString("type", objType.FullName);

					object propertyValue;

					foreach (PropertyInfo property in objProperties)
					{
						if (property.CanRead)
						{
							output.WriteStartElement("var");
							output.WriteAttributeString("name", property.Name);
							
							propertyValue = property.GetValue(obj, null);
                            factory.GetSerializer(propertyValue).WriteObject(output, propertyValue);

							output.WriteEndElement();
						}
					}

					output.WriteEndElement();				
				}
				
				if (objFields.Length > 0)
				{
					WddxObjectSerializerFactory factory = new WddxObjectSerializerFactory();

					output.WriteStartElement("struct");

					object fieldValue;

					foreach (FieldInfo field in objFields)
					{
						if (field.IsPublic)
						{
							output.WriteStartElement("var");
							output.WriteAttributeString("name", field.Name);
						
							fieldValue = field.GetValue(obj);
							factory.GetSerializer(fieldValue).WriteObject(output, fieldValue);

							output.WriteEndElement();
						}
					}

					output.WriteEndElement();
				}				
			}
		}
    }
}
