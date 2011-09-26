using System;
using System.Xml;
using System.Globalization;

namespace Mueller.Wddx
{
	/// <summary>
	///		Serializes a boolean value.
	/// </summary>
    ///	<seealso cref="IWddxObjectSerializer"/>
	internal class BoolSerializer : IWddxObjectSerializer
	{
		private static BoolSerializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static BoolSerializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(BoolSerializer))
					{
						if (instance == null)
							instance = new BoolSerializer();
					}
				}

				return instance;
			}
		}

		private BoolSerializer() {}

		/// <summary>
		///		Writes the serialized object to the XML stream.
		/// </summary>
		/// <param name="output">A pre-initialized <see cref="XmlTextWriter"/> object.</param>
		/// <param name="obj">Object to serialize.</param>
		public void WriteObject(XmlWriter output, object obj)
		{
			bool val;

			try
			{
				val = (bool)obj;
			}
			catch (InvalidCastException e)
			{
				throw new WddxException("Attemped to serialize incompatible object. Expected: " + typeof(bool).FullName + " but got: " + obj.GetType().FullName, e);
			}

			output.WriteStartElement("boolean");
			output.WriteAttributeString("value", (val ? "true" : "false"));
			output.WriteEndElement();
		}
	}

	/// <summary>
	///		Serializes a null value.
	/// </summary>
    ///	<seealso cref="IWddxObjectSerializer"/>
	internal class NullSerializer : IWddxObjectSerializer
	{
		private static NullSerializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static NullSerializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(NullSerializer))
					{
						if (instance == null)
							instance = new NullSerializer();
					}
				}

				return instance;
			}
		}

		private NullSerializer() {}

		/// <summary>
		///		Writes the serialized object to the XML stream.
		/// </summary>
		/// <param name="output">A pre-initialized <see cref="XmlTextWriter"/> object.</param>
		/// <param name="obj">Object to serialize.</param>
		public void WriteObject(XmlWriter output, object obj)
		{
			if (obj == null || obj == DBNull.Value)
			{
				output.WriteStartElement("null");
				output.WriteEndElement();
			}
			else
				throw new ArgumentException("NullSerializer can only serialize nulls!");
		}
	}

	/// <summary>
	///		Serializes a <see cref="DateTime"/> object.
	/// </summary>
    ///	<seealso cref="IWddxObjectSerializer"/>
	internal class DateTimeSerializer : IWddxObjectSerializer
	{
		private static DateTimeSerializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static DateTimeSerializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(DateTimeSerializer))
					{
						if (instance == null)
							instance = new DateTimeSerializer();
					}
				}

				return instance;
			}
		}

		private DateTimeSerializer() {}

		/// <summary>
		///		Writes the serialized object to the XML stream.
		/// </summary>
		/// <param name="output">A pre-initialized <see cref="XmlTextWriter"/> object.</param>
		/// <param name="obj">Object to serialize.</param>
		public void WriteObject(XmlWriter output, object obj)
		{
            DateTime theDate;
                

			try
			{
				theDate = ((DateTime)obj).ToLocalTime(); //we have to assume local time and add offset to end of string for correct implementation of ISO8601 -- bsoylu
                
			}
			catch (InvalidCastException e)
			{
				throw new WddxException("Attemped to serialize incompatible object. Expected: " + typeof(DateTime).FullName + " but got: " + obj.GetType().FullName, e);
			}
            //determine offset and prefix

			output.WriteStartElement("dateTime");
			// Output the date with timezone info in ISO8601 format.
			// http://www.w3.org/TR/NOTE-datetime
            //-- bsoylu 9/20/2011 changes:
            // adjusted for .net 2, coldfusion (orignator of WDDX) does not fully comply with the ISO8601 format 
            // the correct implementation of ISO8601 expresses time in local time with time offsets or Zulu time (Z) we cannot use "Z" instead will use -0:00

            output.WriteString(ISO8601DateFormatter(theDate));
			output.WriteEndElement();
		}

        /// <summary>
        ///		Returns the correctly formatted date in ISO8601 format
        /// </summary>
        /// <param name="localDateTime">a datetime value in local time format.</param>
        
        private String ISO8601DateFormatter(DateTime localDateTime)
        {
            string IsoDateString = "";
            string postfix = "";
            TimeZone localZone = TimeZone.CurrentTimeZone;
            TimeSpan localOffset;
            


            localOffset = localZone.GetUtcOffset(localDateTime);

            //format first part of the time
            IsoDateString = localDateTime.ToString(@"yyyy-MM-dd\THH:mm:ss", DateTimeFormatInfo.InvariantInfo);
            //check on hours postfix
            postfix = localOffset.TotalHours.ToString();
            //if there is a period replace with colon otherwise add :0
            if (postfix.Contains(".")) {
                postfix = postfix.Replace(".", ":");
            } else {
                postfix += ":0";
            }


            return IsoDateString + postfix;
        }
	}




	/// <summary>
	///		Serializes a number.
	/// </summary>
    /// <remarks>
    ///		<para>Any of the following data types will be serialized as a WDDX <c>number</c>
    ///		element.</para>
    ///		<list type="bullet">
    ///			<item><description><see cref="System.Byte"/></description></item>
    ///			<item><description><see cref="System.Int16"/></description></item>
    ///			<item><description><see cref="System.Int32"/></description></item>
    ///			<item><description><see cref="System.Int64"/></description></item>
    ///			<item><description><see cref="System.Single"/></description></item>
    ///			<item><description><see cref="System.Double"/></description></item>
    ///			<item><description><see cref="System.Decimal"/>*</description></item>
    ///		</list>
    ///		
    ///		<para>
    ///			* WDDX does not support the precision of <see cref="System.Decimal"/>, so this type
    ///			will be converted to <see cref="System.Double"/> before serialization.
    ///		</para>
    ///	</remarks>
    ///	<seealso cref="IWddxObjectSerializer"/>
	internal class NumberSerializer : IWddxObjectSerializer
	{
		private static NumberSerializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static NumberSerializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(NumberSerializer))
					{
						if (instance == null)
							instance = new NumberSerializer();
					}
				}

				return instance;
			}
		}

		private NumberSerializer() {}

		/// <summary>
		///		Writes the serialized object to the XML stream.
		/// </summary>
		/// <param name="output">A pre-initialized <see cref="XmlTextWriter"/> object.</param>
		/// <param name="obj">Object to serialize.</param>
		public void WriteObject(XmlWriter output, object obj)
		{
			output.WriteStartElement("number");

			// format numbers for "round-trip" parsing
			switch (obj.GetType().FullName)
			{
				case "System.Byte":
				case "System.Int16":
				case "System.Int32":
				case "System.Int64":
				case "System.Single":
				case "System.Double":
					output.WriteString(obj.ToString());
					break;
				case "System.Decimal":
					// WDDX doesn't support this precision, so convert it to a double first (old comment)
                    // new comment: bsoylu :
                    // not correct. WDDX supports decimal precision up to 15 digits to right of decimal point
                    decimal decNum = (decimal)obj;
                    string decToString = decNum.ToString();
                    if (decToString.Length - decToString.IndexOf(".") <= 15)
                    {
                        output.WriteString(decToString);
                    }
                    else
                    {
                        output.WriteString(Convert.ToDouble((decimal)obj).ToString());
                    }
					break;
				default:
					throw new WddxException("Invalid numeric type: " + obj.GetType().FullName);
			}

			output.WriteEndElement();
		}
	}
}
