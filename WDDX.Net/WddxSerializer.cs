using System;
using System.Xml;
using System.IO;
using System.Text;

namespace Mueller.Wddx
{
    /// <summary>
    ///    This class is the main entry point for serializing any object into WDDX.
    /// </summary>
    public class WddxSerializer
    {
		private const string WDDXVERSION = "1.0";

		private bool xmlDeclaration = false;

		/// <summary>
		///		Determines whether or not the XML processing declaration
		///		(&lt;?xml version="1.0"?&gt;) is added to the WDDX packet.
		///		Defaults to <c>false</c>.
		/// </summary>
		/// <remarks>
		///		As of ColdFusion 5.0, CFWDDX cannot process the XML declaration.
		/// </remarks>
		public bool XmlDeclaration
		{
			get { return xmlDeclaration; }
			set { xmlDeclaration = value; }
		}

		/// <summary>
		///		Serializes an object to a string.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <returns>A string containing the WDDX packet.</returns>
		public string Serialize(object obj)
		{
			StringBuilder sb = new StringBuilder();
			
			using (StringWriter sw = new StringWriter(sb))
			{
				XmlTextWriter writer = new XmlTextWriter(sw);
				writer.Formatting = Formatting.None;
				this.Serialize(writer, obj);
				writer.Close();
			}

			return sb.ToString();
		}

		/// <summary>
		///		Serializes an object to a pre-existing <see cref="XmlWriter"/> object.
		/// </summary>
		/// <remarks>
		///		The <see cref="XmlWriter"/> should be pre-initialized, and can be pointed at many things:
		///		a string, a file, a network stream, the ASP.NET Response stream, etc.
		/// </remarks>
		/// <param name="output">A pre-initialized <see cref="XmlWriter"/> object.</param>
		/// <param name="obj">The object to be serialized.</param>
		public void Serialize(XmlWriter output, object obj)
		{
			WritePacketHeader(output);

			// write packet contents
			WddxObjectSerializerFactory factory = new WddxObjectSerializerFactory();
			IWddxObjectSerializer serializer = factory.GetSerializer(obj);
			serializer.WriteObject(output, obj);

			WritePacketFooter(output);
		}

		private void WritePacketHeader(XmlWriter output)
		{
			// CFWDDX doesn't like the XML declaration tag:
			if (xmlDeclaration)
				output.WriteStartDocument();

			output.WriteStartElement("wddxPacket");
			output.WriteAttributeString("version", WDDXVERSION);
			output.WriteStartElement("header");
			output.WriteEndElement();
			output.WriteStartElement("data");
		}

		private void WritePacketFooter(XmlWriter output)
		{
			output.WriteEndElement(); // data
			output.WriteEndElement(); // wddxPacket

			if (xmlDeclaration)
				output.WriteEndDocument();
		}
    }
}
