using System;
using System.Xml;

namespace Mueller.Wddx
{
    /// <summary>
    ///		Interface that defines the behavior for all serializers.
    /// </summary>
    internal interface IWddxObjectSerializer
    {
		/// <summary>
		///		Writes the serialized object to the XML stream.
		/// </summary>
		/// <param name="output">A pre-initialized <see cref="XmlTextWriter"/> object.</param>
		/// <param name="obj">Object to serialize.</param>
        void WriteObject(XmlWriter output, object obj);
    }
}
