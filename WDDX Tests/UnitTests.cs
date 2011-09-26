using System;
using System.Collections;
using System.Data;
using System.Globalization;

using Mueller.Wddx;

using NUnit.Framework;

namespace Mueller.Wddx.Tests
{
	/// <summary>
	///		This is an NUnit test suite.
	///		Get NUnit here: http://nunit.sourceforge.net
	/// </summary>
	[TestFixture]
	public class WddxTest
	{
		private string basicPacket = "<wddxPacket version=\"1.0\"><header /><data>{0}</data></wddxPacket>";

		public WddxTest() {}

        //added by bsoylu 9/20/2011
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

            //get offset from our timezone to UTC
            localOffset = localZone.GetUtcOffset(localDateTime);

            //format first part of the time
            IsoDateString = localDateTime.ToString(@"yyyy-MM-dd\THH:mm:ss", DateTimeFormatInfo.InvariantInfo);
            //check on hours postfix
            postfix = localOffset.TotalHours.ToString();
            //if there is a period replace with colon otherwise add :0
            if (postfix.Contains("."))
            {
                postfix = postfix.Replace(".", ":");
            }
            else if (Math.Abs(localOffset.TotalHours) > 0) // if we have only zero difference than leave of offset
            {
                postfix += ":0";
            }


            return IsoDateString + postfix;
        }


		// *** ACTUAL TESTS BEGIN HERE ***

		/// <summary>
		///		Tests serialization of the basic WDDX data types.
		/// </summary>
		[Test]
		public void TestBasicTypeSerialization()
		{
			string testString = "This is a test.\nDo not panic.";
			bool testBool = false;
			int testInt = 10;
			short testShort = 34;
			long testLong = 345678923456789123;
			float testFloat = -1.23f;
			double testDouble = 34.672d;
			decimal testDecimal = 32.4562203m;
			DateTime testDate = new DateTime(2001, 6, 17, 12, 00, 30,System.DateTimeKind.Local); //we need to be specific so our parsing to and from UTC is correct

			string expectedString = String.Format(basicPacket, "<string>This is a test.<char code=\"0a\" />Do not panic.</string>");
			string expectedBool = String.Format(basicPacket, "<boolean value=\"false\" />");
			string expectedInt = String.Format(basicPacket, "<number>10</number>");
			string expectedShort = String.Format(basicPacket, "<number>34</number>");
			string expectedLong = String.Format(basicPacket, "<number>345678923456789123</number>");
			string expectedFloat = String.Format(basicPacket, "<number>-1.23</number>");
			string expectedDouble = String.Format(basicPacket, "<number>34.672</number>");
			string expectedDecimal = String.Format(basicPacket, "<number>32.4562203</number>");
            string expectedDate = String.Format(basicPacket, "<dateTime>" + ISO8601DateFormatter(testDate)+ "</dateTime>");
			string expectedNull = String.Format(basicPacket, "<null />");

			WddxSerializer serializer = new WddxSerializer();

			string stringPacket = serializer.Serialize(testString);
            
			Assert.AreEqual(expectedString, stringPacket);

			string boolPacket = serializer.Serialize(testBool);
			Assert.AreEqual(expectedBool, boolPacket);

			string intPacket = serializer.Serialize(testInt);
			Assert.AreEqual(expectedInt, intPacket);

			string shortPacket = serializer.Serialize(testShort);
			Assert.AreEqual(expectedShort, shortPacket);

			string longPacket = serializer.Serialize(testLong);
			Assert.AreEqual(expectedLong, longPacket);

			string floatPacket = serializer.Serialize(testFloat);
			Assert.AreEqual(expectedFloat, floatPacket);

			string doublePacket = serializer.Serialize(testDouble);
			Assert.AreEqual(expectedDouble, doublePacket);

			string decimalPacket = serializer.Serialize(testDecimal);
			Assert.AreEqual(expectedDecimal, decimalPacket);

			string datePacket = serializer.Serialize(testDate);
			Assert.AreEqual(expectedDate, datePacket);

			string nullPacket = serializer.Serialize(null);
			Assert.AreEqual(expectedNull, nullPacket);
		}

		/// <summary>
		///		Tests deserialization of basic data types.
		/// </summary>
		[Test]
		public void TestBasicTypeDeserialization()
		{
			string expectedString = "This is a test.\nDo not panic.";
			bool expectedBool = false;
			int expectedInt = 10;
			long expectedLong = 3147483647;
			float expectedFloat = -1.23f;
			DateTime expectedDate = new DateTime(2001, 6, 17, 12, 00, 30,System.DateTimeKind.Local);
            DateTime expectedCFDate = new DateTime(2002, 6, 26, 5, 0, 0, System.DateTimeKind.Local);
            //remove one hour from CF time to check on calculations -- bsoylu
            TimeZone localZone = TimeZone.CurrentTimeZone;
            TimeSpan localOffset =localZone.GetUtcOffset(expectedDate);
            TimeSpan minusOneHr = new System.TimeSpan(-1, 0, 0);
            expectedCFDate = expectedCFDate.Add(minusOneHr);

			string stringPacket = String.Format(basicPacket, "<string>This is a test.<char code=\"0a\" />Do not panic.</string>");
			string booleanPacket = String.Format(basicPacket, "<boolean value=\"false\" />");
			string intPacket = String.Format(basicPacket, "<number>10</number>");
			string longPacket = String.Format(basicPacket, "<number>3147483647</number>");
			string floatPacket = String.Format(basicPacket, "<number>-1.23</number>");
			string datePacket = String.Format(basicPacket, "<dateTime>" + ISO8601DateFormatter(expectedDate)  + "</dateTime>");
			string datePacketNoTZ = String.Format(basicPacket, "<dateTime>2001-06-17T12:00:30</dateTime>"); //no time zone is assumed to be zulu in ColFusion, will be converted to local time
			string bogusCFDate = String.Format(basicPacket, "<dateTime>2002-6-26T4:0:0" + localOffset.TotalHours + ":0</dateTime>");
			string nullPacket = String.Format(basicPacket, "<null />");

			WddxDeserializer deserializer = new WddxDeserializer();

			string resultString = (string)deserializer.Deserialize(stringPacket);
			Assert.AreEqual(expectedString, resultString);

			bool resultBool = (bool)deserializer.Deserialize(booleanPacket);
			Assert.AreEqual(expectedBool, resultBool);

			int resultInt = (int)deserializer.Deserialize(intPacket);
			Assert.AreEqual(expectedInt, resultInt);

			long resultLong = (long)deserializer.Deserialize(longPacket);
			Assert.AreEqual(expectedLong, resultLong);

			float resultFloat = (float)deserializer.Deserialize(floatPacket);
			Assert.AreEqual(expectedFloat, resultFloat);

			DateTime resultDate = (DateTime)deserializer.Deserialize(datePacket);
			Assert.AreEqual(expectedDate, resultDate);

			DateTime resultDateNoTZ = (DateTime)deserializer.Deserialize(datePacketNoTZ);
			Assert.AreEqual(expectedDate, resultDateNoTZ);

			DateTime resultCFDate = (DateTime)deserializer.Deserialize(bogusCFDate);
			Assert.AreEqual(expectedCFDate, resultCFDate);

			object resultNull = deserializer.Deserialize(nullPacket);
			Assert.IsNull(resultNull);
		}

		/// <summary>
		///		Tests serialization of basic one-dimensional arrays.
		/// </summary>
		[Test]
		public void TestBasicArraySerialization()
		{
			int[] intArray = {1,2,3,4,5,6,7,8,9,10};
			string[] stringArray = 
			{
				// This is the official list of 
				// metasyntactic variable names, 
				// in order starting with "foo".
				"foo",
				"bar",
				"baz",
				"qux",
				"quux",
				"corge",
				"grault",
				"garply",
				"waldo",
				"fred",
				"plugh",
				"xyzzy",
				"thud"
			};

			string expectedInt = String.Format(basicPacket, "<array length=\"10\"><number>1</number><number>2</number><number>3</number><number>4</number><number>5</number><number>6</number><number>7</number><number>8</number><number>9</number><number>10</number></array>");
			string expectedString = String.Format(basicPacket, "<array length=\"13\"><string>foo</string><string>bar</string><string>baz</string><string>qux</string><string>quux</string><string>corge</string><string>grault</string><string>garply</string><string>waldo</string><string>fred</string><string>plugh</string><string>xyzzy</string><string>thud</string></array>");
			
			WddxSerializer serializer = new WddxSerializer();

			string intPacket = serializer.Serialize(intArray);
			Assert.AreEqual(expectedInt, intPacket);

			string stringPacket = serializer.Serialize(stringArray);
			Assert.AreEqual(expectedString, stringPacket);
		}

		/// <summary>
		///		Tests deserialization of basic one-dimensional arrays.
		/// </summary>
		[Test]
		public void TestBasicArrayDeserialization()
		{
			string intPacket = String.Format(basicPacket, "<array length=\"10\"><number>1</number><number>2</number><number>3</number><number>4</number><number>5</number><number>6</number><number>7</number><number>8</number><number>9</number><number>10</number></array>");
			string stringPacket = String.Format(basicPacket, "<array length=\"13\"><string>foo</string><string>bar</string><string>baz</string><string>qux</string><string>quux</string><string>corge</string><string>grault</string><string>garply</string><string>waldo</string><string>fred</string><string>plugh</string><string>xyzzy</string><string>thud</string></array>");

			int[] expectedInt = {1,2,3,4,5,6,7,8,9,10};
			string[] expectedString =
			{
				"foo",
				"bar",
				"baz",
				"qux",
				"quux",
				"corge",
				"grault",
				"garply",
				"waldo",
				"fred",
				"plugh",
				"xyzzy",
				"thud"
			};

			WddxDeserializer deserializer = new WddxDeserializer();

			ArrayList intResult = (ArrayList)deserializer.Deserialize(intPacket);
			int[] intArray = (int[])intResult.ToArray(typeof(int));
			for (int i=0; i < expectedInt.Length; i++)
			{
				Assert.AreEqual(expectedInt[i], intArray[i]);
			}

			ArrayList stringResult = (ArrayList)deserializer.Deserialize(stringPacket);
			string[] stringArray = (string[])stringResult.ToArray(typeof(string));
			for (int i=0; i < expectedString.Length; i++)
			{
				Assert.AreEqual(expectedString[i], stringArray[i]);
			}
		}

		/// <summary>
		///		Tests the serialization of binary data (a byte array).
		/// </summary>
		[Test]
		public void TestBinarySerialization()
		{
			byte[] byteArray = {0x4a, 0x6f, 0x65, 0x6c, 0x20, 0x72, 0x6f, 0x63, 0x6b, 0x73, 0x21};

			string expectedBinary = String.Format(basicPacket, "<binary length=\"11\">Sm9lbCByb2NrcyE=</binary>");

			WddxSerializer serializer = new WddxSerializer();

			string binaryPacket = serializer.Serialize(byteArray);
			Assert.AreEqual(expectedBinary, binaryPacket);
		}

		/// <summary>
		///		Tests the deserialization of binary data.
		/// </summary>
		[Test]
		public void TestBinaryDeserialization()
		{
			string binaryPacket = String.Format(basicPacket, "<binary length=\"11\">Sm9lbCByb2NrcyE=</binary>");
			string binaryPacketNoLength = String.Format(basicPacket, "<binary>Sm9lbCByb2NrcyE=</binary>");

			byte[] expectedBytes = {0x4a, 0x6f, 0x65, 0x6c, 0x20, 0x72, 0x6f, 0x63, 0x6b, 0x73, 0x21};

			WddxDeserializer deserializer = new WddxDeserializer();

			byte[] binaryResult = (byte[])deserializer.Deserialize(binaryPacket);
			for (int i=0; i < expectedBytes.Length; i++)
			{
				Assert.AreEqual(expectedBytes[i], binaryResult[i]);
			}

			binaryResult = (byte[])deserializer.Deserialize(binaryPacketNoLength);
			for (int i=0; i < expectedBytes.Length; i++)
			{
				Assert.AreEqual(expectedBytes[i], binaryResult[i]);
			}
		}

		/// <summary>
		///		Tests the serialization of a basic WDDX struct (a <see cref="Hashtable"/> in this case).
		/// </summary>
		[Test]
		public void TestBasicStructSerialization()
		{
			DateTime expectedDate = new DateTime(1998, 6, 12, 4, 32, 12,System.DateTimeKind.Local); //specifically set to local time
            TimeZone localZone = TimeZone.CurrentTimeZone;
            TimeSpan localOffset = localZone.GetUtcOffset(expectedDate);

			Hashtable thisTable = new Hashtable();
			thisTable.Add("aNull", null);
			thisTable.Add("aString", "a string");
			thisTable.Add("aNumber", -12.456);
			thisTable.Add("aBoolean", true);
			thisTable.Add("aDateTime", expectedDate);

            string expectedStruct = String.Format(basicPacket, "<struct><var name=\"aString\"><string>a string</string></var><var name=\"aDateTime\"><dateTime>" + ISO8601DateFormatter(expectedDate) + "</dateTime></var><var name=\"aNumber\"><number>-12.456</number></var><var name=\"aNull\"><null /></var><var name=\"aBoolean\"><boolean value=\"true\" /></var></struct>");

			WddxSerializer serializer = new WddxSerializer();

			string structPacket = serializer.Serialize(thisTable);
			Assert.AreEqual(expectedStruct, structPacket);
		}

		/// <summary>
		///		Tests the deserialization of a basic WDDX struct.
		/// </summary>
		[Test]
		public void TestBasicStructDeserialization()
		{
			DateTime expectedDate = new DateTime(1998, 6, 12, 4, 32, 12, System.DateTimeKind.Local);

            string structPacket = String.Format(basicPacket, "<struct><var name=\"aNumber\"><number>-12.456</number></var><var name=\"aBoolean\"><boolean value=\"true\" /></var><var name=\"aDateTime\"><dateTime>" + ISO8601DateFormatter(expectedDate) + "</dateTime></var><var name=\"aNull\"><null /></var><var name=\"aString\"><string>a string</string></var></struct>");
			
			WddxDeserializer deserializer = new WddxDeserializer();

			Hashtable thisTable = (Hashtable)deserializer.Deserialize(structPacket);
			Assert.AreEqual(null, thisTable["aNull"]);
			Assert.AreEqual("a string", (string)thisTable["aString"]);
            Assert.AreEqual(-12.456f, (float)thisTable["aNumber"]);
            /*
            if (thisTable["aNumber"].GetType().Name == "Single")
            {                
            }
            if (thisTable["aNumber"].GetType().Name == "Decimal")
            {
                Assert.AreEqual(-12.456m, (decimal)thisTable["aNumber"]);
            }
			*/
			Assert.AreEqual(true, (bool)thisTable["aBoolean"]);
			Assert.AreEqual(expectedDate, (DateTime)thisTable["aDateTime"]);
		}

		/// <summary>
		///		Tests the serialization of nested and two-dimensional arrays.
		/// </summary>
		[Test]
		public void TestComplexArraySerialization()
		{
			int[][] jaggedArray =  
			{
				new int[] {1,2,3,4,5,6},
				new int[] {1,2,3},
				new int[] {1,2,3,4,5,6,7,8}
			};

			int[,] matrix = 
			{
				{1,1},
				{2,2},
				{3,5},
				{4,5}
			};

			string expectedJagged = String.Format(basicPacket, "<array length=\"3\"><array length=\"6\"><number>1</number><number>2</number><number>3</number><number>4</number><number>5</number><number>6</number></array><array length=\"3\"><number>1</number><number>2</number><number>3</number></array><array length=\"8\"><number>1</number><number>2</number><number>3</number><number>4</number><number>5</number><number>6</number><number>7</number><number>8</number></array></array>");
			// WDDX doesn't support multi-dimensional arrays, so they are flattened
			// to a one-dimensional array (but no data is lost).
			string expectedMatrix = String.Format(basicPacket, "<array length=\"8\"><number>1</number><number>1</number><number>2</number><number>2</number><number>3</number><number>5</number><number>4</number><number>5</number></array>");

			WddxSerializer serializer = new WddxSerializer();

			string jaggedPacket = serializer.Serialize(jaggedArray);
			Assert.AreEqual(expectedJagged, jaggedPacket);

			string matrixPacket = serializer.Serialize(matrix);
			Assert.AreEqual(expectedMatrix, matrixPacket);
		}

		/// <summary>
		///		Tests the deserialization of nested arrays.
		/// </summary>
		[Test]
		public void TestComplexArrayDeserialization()
		{
			string jaggedPacket = String.Format(basicPacket, "<array length=\"3\"><array length=\"6\"><number>1</number><number>2</number><number>3</number><number>4</number><number>5</number><number>6</number></array><array length=\"3\"><number>1</number><number>2</number><number>3</number></array><array length=\"8\"><number>1</number><number>2</number><number>3</number><number>4</number><number>5</number><number>6</number><number>7</number><number>8</number></array></array>");

			int[][] jaggedArray =  
			{
				new int[] {1,2,3,4,5,6},
				new int[] {1,2,3},
				new int[] {1,2,3,4,5,6,7,8}
			};

			WddxDeserializer deserializer = new WddxDeserializer();

			ArrayList resultList = (ArrayList)deserializer.Deserialize(jaggedPacket);
			// jagged arrays come out as an ArrayList containing ArrayLists,
			// and must be accessed as-is, or converted from the inside out.

			for (int i=0; i < jaggedArray.Length; i++)
			{
				for (int j=0; j < jaggedArray[i].Length; j++)
				{
					Assert.AreEqual(jaggedArray[i][j], ((ArrayList)resultList[i])[j]);
				}
			}
		}

		/// <summary>
		///		Tests the serialization of both simple and complex <see cref="DataSet"/>s.
		/// </summary>
		[Test]
		public void TestDataSetSerialization()
		{
			DataSetGenerator gen = new DataSetGenerator();
			DataSet simpleDataSet = gen.MakeSimpleDataSet();

			string expectedSimple = String.Format(basicPacket, "<recordset rowCount=\"3\" fieldNames=\"id,ParentItem\"><field name=\"id\"><number>0</number><number>1</number><number>2</number></field><field name=\"ParentItem\"><string>ParentItem 0</string><string>ParentItem 1</string><string>ParentItem 2</string></field></recordset>");
			string expectedComplex = String.Format(basicPacket, "<struct><var name=\"ParentTable\"><recordset rowCount=\"3\" fieldNames=\"id,ParentItem\"><field name=\"id\"><number>0</number><number>1</number><number>2</number></field><field name=\"ParentItem\"><string>ParentItem 0</string><string>ParentItem 1</string><string>ParentItem 2</string></field></recordset></var><var name=\"ChildTable\"><recordset rowCount=\"15\" fieldNames=\"ChildID,ChildItem,ParentID\"><field name=\"ChildID\"><number>0</number><number>1</number><number>2</number><number>3</number><number>4</number><number>5</number><number>6</number><number>7</number><number>8</number><number>9</number><number>10</number><number>11</number><number>12</number><number>13</number><number>14</number></field><field name=\"ChildItem\"><string>Item 0</string><string>Item 1</string><string>Item 2</string><string>Item 3</string><string>Item 4</string><string>Item 0</string><string>Item 1</string><string>Item 2</string><null /><string>Item 4</string><string>Item 0</string><string>Item 1</string><string>Item 2</string><string>Item 3</string><string>Item 4</string></field><field name=\"ParentID\"><number>0</number><number>0</number><number>0</number><number>0</number><number>0</number><number>1</number><number>1</number><number>1</number><number>1</number><number>1</number><number>2</number><number>2</number><number>2</number><number>2</number><number>2</number></field></recordset></var></struct>");

			WddxSerializer serializer = new WddxSerializer();

			string simplePacket = serializer.Serialize(simpleDataSet);
			Assert.AreEqual(expectedSimple, simplePacket);

			DataSet complexDataSet = gen.MakeComplexDataSet();

			string complexPacket = serializer.Serialize(complexDataSet);
			Assert.AreEqual(expectedComplex, complexPacket);
		}

		/// <summary>
		///		Tests the deserialization of a simple WDDX recordset.
		/// </summary>
		[Test]
		public void TestDataSetDeserialization()
		{
			string datasetPacket = String.Format(basicPacket, "<recordset rowCount=\"3\" fieldNames=\"id,ParentItem\"><field name=\"id\"><number>0</number><number>1</number><number>2</number></field><field name=\"ParentItem\"><string>ParentItem 0</string><string>ParentItem 1</string><string>ParentItem 2</string></field></recordset>");
			
			DataSetGenerator gen = new DataSetGenerator();
			DataSet expectedDataSet = gen.MakeSimpleDataSet();

			WddxDeserializer deserializer = new WddxDeserializer();

			DataSet resultDataSet = (DataSet)deserializer.Deserialize(datasetPacket);
			for (int i=0; i < expectedDataSet.Tables[0].Rows.Count; i++)
			{
				for (int j=0; j < expectedDataSet.Tables[0].Columns.Count; j++)
				{
					Assert.AreEqual(expectedDataSet.Tables[0].Rows[i][j], resultDataSet.Tables[0].Rows[i][j]);
				}
			}
		}

		/// <summary>
		///		Tests the serialization of a complex Hashtable, containing a number of other data types and objects.
		/// </summary>
		[Test]
		public void TestComplexStructSerialization()
		{
			byte[] byteArray = {0x4a, 0x6f, 0x65, 0x6c, 0x20, 0x72, 0x6f, 0x63, 0x6b, 0x73, 0x21};
			ArrayList mixedArray = new ArrayList();
			mixedArray.Add(10);
			mixedArray.Add("second element");

			DataSetGenerator gen = new DataSetGenerator();
			DataSet simpleDataSet = gen.MakeSimpleDataSet();

			DateTime expectedDate = new DateTime(1998, 6, 12, 4, 32, 12,System.DateTimeKind.Local);

			Hashtable thisTable = new Hashtable();
			thisTable.Add("aNull", null);
			thisTable.Add("aString", "a string");
			thisTable.Add("aNumber", -12.456);
			thisTable.Add("aBoolean", true);
			thisTable.Add("aDateTime", expectedDate);
			thisTable.Add("aBinary", byteArray);
			thisTable.Add("anArray", mixedArray);
			thisTable.Add("aRecordset", simpleDataSet);

			string expectedStruct = GetComplexPacket(expectedDate);

			WddxSerializer serializer = new WddxSerializer();

			string structPacket = serializer.Serialize(thisTable);
			Assert.AreEqual(expectedStruct, structPacket);
		}

		/// <summary>
		///		Tests the deserialization of a complex struct.
		/// </summary>
		[Test]
		public void TestComplexStructDeserialization()
		{
			DateTime expectedDate = new DateTime(1998, 6, 12, 4, 32, 12);

			string structPacket = GetComplexPacket(expectedDate);
			
			WddxDeserializer deserializer = new WddxDeserializer();

			Hashtable resultTable = (Hashtable)deserializer.Deserialize(structPacket);

		}

		/// <summary>
		///		Tests the serialization of a generic object that does not fit any of the pre-defined types.
		///		In this case, the properties of the object will be serialized as a WDDX struct.
		/// </summary>
		[Test]
		public void TestGenericSerialization()
		{
			TestObject obj = new TestObject();
			obj.foo = "a string";
			obj.bar = 42;
			obj.baz = new DateTime(1975, 6, 17,0,0,0,System.DateTimeKind.Local);

			string expectedGeneric = String.Format(basicPacket, "<struct><var name=\"foo\"><string>a string</string></var><var name=\"bar\"><number>42</number></var><var name=\"baz\"><dateTime>" + ISO8601DateFormatter(obj.baz) + "</dateTime></var></struct>");

			WddxSerializer serializer = new WddxSerializer();

			string genericPacket = serializer.Serialize(obj);
			Assert.AreEqual(expectedGeneric, genericPacket);
		}

		/// <summary>
		///		Tests the validity of a packet.
		/// </summary>
		[Test]
		public void TestIsValid()
		{
			DateTime expectedDate = new DateTime(1998, 6, 12, 4, 32, 12,System.DateTimeKind.Local);
			WddxDeserializer deserializer = new WddxDeserializer();
            //string packet = String.Format(basicPacket, "<struct><var name=\"valor\"><string /></var></struct>");
            string packet = GetComplexPacket(expectedDate);
            Hashtable resultTable = (Hashtable)deserializer.Deserialize(packet);

            Assert.IsTrue(deserializer.IsValid(packet), "Packet is not valid when it should be");
            Assert.IsTrue(!deserializer.IsValid(GetBrokenPacket()), "Packet is valid when it should not be.");
            			
		}

		/// <summary>
		///		Tests performing validation while deserializing.
		/// </summary>
		[Test]
		[ExpectedException(typeof(WddxValidationException))]
		public void TestValidDeserialization()
		{
			WddxDeserializer deserializer = new WddxDeserializer();

			// test with an invalid packet
			Hashtable resultTable1 = (Hashtable)deserializer.Deserialize(GetBrokenPacket(), true);
		}

		/// <summary>
		///		Test to reproduce a reported deserialization bug.
		///		In versions prior to 1.0.3, this packet would cause
		///		an infinite loop.
		/// </summary>
		[Test]
		public void TestStringDeserializationBug()
		{
			string packet = String.Format(basicPacket, "<struct><var name=\"valor\"><string /></var></struct>");

			WddxDeserializer deserializer = new WddxDeserializer();

			Hashtable result = (Hashtable)deserializer.Deserialize(packet);

			string content = (string)result["valor"];

			//Assert.AreEqual( "Empty string not deserialized correctly.", String.Empty, content);
            //nunit changed order -- bsoylu
            Assert.AreEqual(String.Empty, content, "Empty string not deserialized correctly.");
        }

        /// <summary>
        ///		Test to reproduce a decimal conversion.
        ///		In versions prior to 1.0.4, this packet would cause wrong conversion
        ///		due to loss of precision
        /// </summary>
        [Test]
        public void TestDecimalDeserializationBug()
        {
            string packet = String.Format(basicPacket, "<struct><var name=\"aDecimal\"><number>154523.85</number></var></struct>");

            WddxDeserializer deserializer = new WddxDeserializer();

            Hashtable result = (Hashtable)deserializer.Deserialize(packet);

            decimal content = (decimal)result["aDecimal"];
                       
            //nunit changed order -- bsoylu
            Assert.AreEqual(154523.85m, content, "Decimal is not deserialized correctly.");
            
        }

		private string GetComplexPacket(DateTime expectedDate)
		{
			//return String.Format(basicPacket, "<struct><var name=\"aNull\"><null /></var><var name=\"anArray\"><array length=\"2\"><number>10</number><string>second element</string></array></var><var name=\"aNumber\"><number>-12.456</number></var><var name=\"aRecordset\"><recordset rowCount=\"3\" fieldNames=\"id,ParentItem\"><field name=\"id\"><number>0</number><number>1</number><number>2</number></field><field name=\"ParentItem\"><string>ParentItem 0</string><string>ParentItem 1</string><string>ParentItem 2</string></field></recordset></var><var name=\"aBoolean\"><boolean value=\"true\" /></var><var name=\"aDateTime\"><dateTime>" + ISO8601DateFormatter(expectedDate) + "</dateTime></var><var name=\"aBinary\"><binary length=\"11\">Sm9lbCByb2NrcyE=</binary></var><var name=\"aString\"><string>a string</string></var></struct>");
            //reordered return as expected -- bsoylu
            return String.Format(basicPacket, "<struct><var name=\"aBoolean\"><boolean value=\"true\" /></var><var name=\"anArray\"><array length=\"2\"><number>10</number><string>second element</string></array></var><var name=\"aBinary\"><binary length=\"11\">Sm9lbCByb2NrcyE=</binary></var><var name=\"aDateTime\"><dateTime>" + ISO8601DateFormatter(expectedDate) + "</dateTime></var><var name=\"aRecordset\"><recordset rowCount=\"3\" fieldNames=\"id,ParentItem\"><field name=\"id\"><number>0</number><number>1</number><number>2</number></field><field name=\"ParentItem\"><string>ParentItem 0</string><string>ParentItem 1</string><string>ParentItem 2</string></field></recordset></var><var name=\"aString\"><string>a string</string></var><var name=\"aNull\"><null /></var><var name=\"aNumber\"><number>-12.456</number></var></struct>");
		}

		private string GetBrokenPacket()
		{
			// This packet has two deliberate errors:
			// the date is in an invalid format, and there is
			// an illegal character in the base-64-encoded binary data.
			// Both errors should be caught by the XSD used for validation.
			return String.Format(basicPacket, "<struct><var name=\"aNull\"><null /></var><var name=\"anArray\"><array length=\"2\"><number>10</number><string>second element</string></array></var><var name=\"aNumber\"><number>-12.456</number></var><var name=\"aRecordset\"><recordset rowCount=\"3\" fieldNames=\"id,ParentItem\"><field name=\"id\"><number>0</number><number>1</number><number>2</number></field><field name=\"ParentItem\"><string>ParentItem 0</string><string>ParentItem 1</string><string>ParentItem 2</string></field></recordset></var><var name=\"aBoolean\"><boolean value=\"true\" /></var><var name=\"aDateTime\"><dateTime>6/17/75</dateTime></var><var name=\"aBinary\"><binary length=\"11\">Sm9lb%Byb2NrcyE=</binary></var><var name=\"aString\"><string>a string</string></var></struct>");
		}
	}

	/// <summary>
	///		Generic object for the <see cref="WddxTest.TestGenericSerialization"/> test.
	/// </summary>
	internal struct TestObject
	{
		public string foo;
		public int bar;
		public DateTime baz;
	}

	/// <summary>
	///		Generates DataSets for the DataSet test, without requiring a database.
	/// </summary>
	internal class DataSetGenerator
	{
		private DataSet myDataSet;

		public DataSet MakeSimpleDataSet()
		{
			MakeParentTable();
			return myDataSet;
		}

		public DataSet MakeComplexDataSet()
		{
			MakeChildTable();

			DataRelation myDataRelation;
			DataColumn parentColumn;
			DataColumn childColumn;
			parentColumn = myDataSet.Tables["ParentTable"].Columns["id"];
			childColumn = myDataSet.Tables["ChildTable"].Columns["ParentID"];
			myDataRelation = new DataRelation("parent2Child", parentColumn, childColumn);
			myDataSet.Tables["ChildTable"].ParentRelations.Add(myDataRelation);

			return myDataSet;
		}

		private DataTable MakeParentTable()
		{
			// Create a new DataTable.
			System.Data.DataTable myDataTable = new DataTable("ParentTable");
			// Declare variables for DataColumn and DataRow objects.
			DataColumn myDataColumn;
			DataRow myDataRow;

			// Create new DataColumn, set DataType, ColumnName and add to DataTable.    
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "id";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = true;
			// Add the Column to the ColumnsCollection.
			myDataTable.Columns.Add(myDataColumn);

			// Create second column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType = System.Type.GetType("System.String");
			myDataColumn.ColumnName = "ParentItem";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "ParentItem";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			// Add the column to the table.
			myDataTable.Columns.Add(myDataColumn);

			// Make the ID column the primary key column.
			DataColumn[] PrimaryKeyColumns = new DataColumn[1];
			PrimaryKeyColumns[0] = myDataTable.Columns["id"];
			myDataTable.PrimaryKey = PrimaryKeyColumns;

			//TODO: FIX THIS CRAP
			myDataSet = new DataSet();
			myDataSet.Tables.Add(myDataTable);

			// Create three new DataRow objects and add them to the DataTable
			for (int i = 0; i<= 2; i++)
			{
				myDataRow = myDataTable.NewRow();
				myDataRow["id"] = i;
				myDataRow["ParentItem"] = "ParentItem " + i;
				myDataTable.Rows.Add(myDataRow);
			}

			return myDataTable;
		}

		private DataTable MakeChildTable()
		{
			// Create a new DataTable.
			DataTable myDataTable = new DataTable("ChildTable");
			DataColumn myDataColumn;
			DataRow myDataRow;

			// Create first column and add to the DataTable.
			myDataColumn = new DataColumn();
			myDataColumn.DataType= System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "ChildID";
			myDataColumn.AutoIncrement = true;
			myDataColumn.Caption = "ID";
			myDataColumn.ReadOnly = true;
			myDataColumn.Unique = true;
			// Add the column to the ColumnsCollection.
			myDataTable.Columns.Add(myDataColumn);

			// Create second column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType= System.Type.GetType("System.String");
			myDataColumn.ColumnName = "ChildItem";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "ChildItem";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			myDataTable.Columns.Add(myDataColumn);

			// Create third column.
			myDataColumn = new DataColumn();
			myDataColumn.DataType= System.Type.GetType("System.Int32");
			myDataColumn.ColumnName = "ParentID";
			myDataColumn.AutoIncrement = false;
			myDataColumn.Caption = "ParentID";
			myDataColumn.ReadOnly = false;
			myDataColumn.Unique = false;
			myDataTable.Columns.Add(myDataColumn);

			myDataSet.Tables.Add(myDataTable);

			// Create three sets of DataRow objects, five rows each, and add to DataTable.
			for(int i = 0; i <= 4; i ++)
			{
				myDataRow = myDataTable.NewRow();
				myDataRow["childID"] = i;
				myDataRow["ChildItem"] = "Item " + i;
				myDataRow["ParentID"] = 0 ;
				myDataTable.Rows.Add(myDataRow);
			}
			for(int i = 0; i <= 4; i ++)
			{
				myDataRow = myDataTable.NewRow();
				myDataRow["childID"] = i + 5;
				if (i == 3)
					myDataRow["ChildItem"] = DBNull.Value;
				else
					myDataRow["ChildItem"] = "Item " + i;
				myDataRow["ParentID"] = 1 ;
				myDataTable.Rows.Add(myDataRow);
			}
			for(int i = 0; i <= 4; i ++)
			{
				myDataRow = myDataTable.NewRow();
				myDataRow["childID"] = i + 10;
				myDataRow["ChildItem"] = "Item " + i;
				myDataRow["ParentID"] = 2 ;
				myDataTable.Rows.Add(myDataRow);
			}

			return myDataTable;
		}
	}
}
