using System;
using System.Xml;
using System.Globalization;

namespace Mueller.Wddx
{
	/// <summary>
	///		Deserializes a boolean element.
	/// </summary>
    /// <remarks>
    ///		This class is a Singleton class - only one instance of it will ever exist
    ///		in a given AppDomain.
    ///	</remarks>
    ///	<seealso cref="IWddxElementDeserializer"/>
	internal class BoolDeserializer : IWddxElementDeserializer
	{
		private static BoolDeserializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static BoolDeserializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(BoolDeserializer))
					{
						if (instance == null)
							instance = new BoolDeserializer();
					}
				}

				return instance;
			}
		}

		private BoolDeserializer() {}

		/// <summary>
		///		Parses the WDDX element and returns the deserialized
		///		content as a <see cref="System.Boolean"/> object, advancing the reader to the next
		///		element.
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
		public object ParseElement(XmlReader input)
		{
			// read the boolean string
			string boolString = input.GetAttribute("value").ToLower();
			// advance the reader to the next element
			input.Skip();

			return boolString.Equals("true");
		}
	}

	/// <summary>
	///		Deserializes a WDDX null element.
	/// </summary>
    /// <remarks>
    ///		This class is a Singleton class - only one instance of it will ever exist
    ///		in a given AppDomain.
    ///	</remarks>
    ///	<seealso cref="IWddxElementDeserializer"/>
	internal class NullDeserializer : IWddxElementDeserializer
	{
		private static NullDeserializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static NullDeserializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(NullDeserializer))
					{
						if (instance == null)
							instance = new NullDeserializer();
					}
				}

				return instance;
			}
		}

		private NullDeserializer() {}

		/// <summary>
		///		Parses the WDDX element and returns the deserialized
		///		content as a null object, advancing the reader to the next
		///		element.
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
		public object ParseElement(XmlReader input)
		{
			// advance the reader to the next element
			input.Skip();
			return null;
		}
	}

	/// <summary>
	///		Deserializes a dateTime element.
	/// </summary>
    /// <remarks>
    ///		This class is a Singleton class - only one instance of it will ever exist
    ///		in a given AppDomain.
    ///	</remarks>
    ///	<seealso cref="IWddxElementDeserializer"/>
	internal class DateTimeDeserializer : IWddxElementDeserializer
	{
		private static DateTimeDeserializer instance = null;

		private string[] dateFormats;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static DateTimeDeserializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(DateTimeDeserializer))
					{
						if (instance == null)
							instance = new DateTimeDeserializer();
					}
				}

				return instance;
			}
		}

		private DateTimeDeserializer()
		{
			// accepts ISO8601, or ColdFusion's loose version of ISO8601
			dateFormats = new string[] 
							{
								@"yyyy-MM-dd\THH:mm:sszzz", 
								@"yyyy-M-d\TH:m:szzz", 
								@"yyyy-M-d\TH:m:sz", 
								@"yyyy-M-d\TH:m:sz\:\0"
							};
		}

		/// <summary>
		///		Parses the WDDX element and returns the deserialized
		///		content as a <see cref="System.DateTime"/> object, advancing the reader to the next
		///		element.
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
		public object ParseElement(XmlReader input)
		{
			DateTime returnDate;
			string dateString = input.ReadString().Trim();

			if (dateString.Length == 0)
				throw new WddxException("A dateTime element cannot be empty.");

			try
			{
				// parse the date according to ISO8601 rules
				returnDate = DateTime.ParseExact(dateString, dateFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
				returnDate = returnDate.ToLocalTime();
			}
			catch (FormatException)
			{
				// if that fails, parse it with the default date parser.
				try
				{
					returnDate = DateTime.Parse(dateString);
				}
				catch (FormatException fe)
				{
					throw new WddxException("Error parsing date string: " + dateString, fe);
				}
			}

			input.ReadEndElement();

			return returnDate;
		}
	}

	/// <summary>
	///		Deserializes a number element.
	/// </summary>
    /// <remarks>
    ///		<para>If the contents of the "number" element contain a decimal point,
    ///		the value returned will be a <see cref="System.Single"/> if that data type
    ///		can hold the number, otherwise it will be a <see cref="System.Double"/>.</para>
    ///		
    ///		<para>If the contents of the "number" element do not contain a decimal point,
    ///		the value returned will be a <see cref="System.Int32"/> if that data type
    ///		can hold the number, otherwise it will be a <see cref="System.Int64"/>.</para>
    /// 
    ///		<para>This class is a Singleton class - only one instance of it will ever exist
    ///		in a given AppDomain.</para>
    ///	</remarks>
    ///	<seealso cref="IWddxElementDeserializer"/>
	internal class NumberDeserializer : IWddxElementDeserializer
	{
		private static NumberDeserializer instance = null;

		/// <summary>
		///		Provides access to the instance of this class.
		/// </summary>
		public static NumberDeserializer Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(NumberDeserializer))
					{
						if (instance == null)
							instance = new NumberDeserializer();
					}
				}

				return instance;
			}
		}

		private NumberDeserializer() {}

		/// <summary>
		///		Parses the WDDX element and returns the deserialized
		///		content as a numeric object (see class reference), advancing the reader to the next
		///		element.
		/// </summary>
		/// <param name="input">The pre-initialized <see cref="System.Xml.XmlTextReader"/> pointing to the WDDX to be parsed.</param>
		public object ParseElement(XmlReader input)
		{
			string numberString = input.ReadElementString();
            int decPos = numberString.IndexOf('.');

            if (decPos > -1)
			{
				// return the smallest floating-point type that the number will fit in
                double dblNum = Double.Parse(numberString);
				try
				{
                    //check how many digits after decimal point if it is less than 15 this precision is 
                    //supported by wddx. We do this to maintain backward compatibility where possible
                    float fltNum = Single.Parse(numberString);
                    decimal decNum = Decimal.Parse(numberString);                    

                    //check if there is a precision difference between float and decimal. 
                    //If we loose precision we will use decimal type instead 
                    // -- bsoylu
                    if ((decimal)fltNum != decNum)
                        return decNum;
                    else
                        return fltNum;
				}
				catch (OverflowException)
				{
                    //we will convert to decimal if we are loosing precision with double numbers
                    //we do not return allways decimal to maintain backward compatibility
                    // -- bsoylu
                    if (numberString.Length - decPos < 15)
                    {
                        decimal decNum = Decimal.Parse(numberString);
                        if ((decimal)dblNum != decNum)
                            return decNum;
                        else
                            return dblNum;
                    }
                    else
                    {
                        //catch all
                        return dblNum;
                    }
				}
			}
			else
			{
				// return an int if the number is small enough, else return a long
				try
				{
					return Int32.Parse(numberString);
				}
				catch (OverflowException)
				{
					return Int64.Parse(numberString);
				}
			}
		}
	}
}
