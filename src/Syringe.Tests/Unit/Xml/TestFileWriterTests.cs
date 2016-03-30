﻿using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using Org.XmlUnit.Builder;
using Org.XmlUnit.Constraints;
using Syringe.Core.Tests;
using Syringe.Core.Xml.Writer;

namespace Syringe.Tests.Unit.Xml
{
	public class TestFileWriterTests
	{
		public static string XmlExamplesFolder = "Syringe.Tests.Unit.Xml.XmlExamples.Writer.";

		[Test]
		public void write_should_add_variables()
		{
			// Arrange
			string expectedXml = TestHelpers.ReadEmbeddedFile("variables.xml", XmlExamplesFolder);

			TestFile testFile = CreateTestFile();
			testFile.Variables.Add(new Variable("name1", "value1", "env1"));
			testFile.Variables.Add(new Variable("name2", "value2", "env2"));
			TestFileWriter xmlWriter = CreateTestCaseWriter();

			// Act
			string actualXml = xmlWriter.Write(testFile);

			// Assert
			Assert.That(Input.FromString(actualXml), CompareConstraint.IsIdenticalTo(Input.FromString(expectedXml)));
		}

		[Test]
		public void write_should_add_repeat_attribute()
		{
			// Arrange
			string expectedXml = TestHelpers.ReadEmbeddedFile("repeat.xml", XmlExamplesFolder);

			TestFile testFile = CreateTestFile();
			testFile.Repeat = 10;
			TestFileWriter xmlWriter = CreateTestCaseWriter();

			// Act
			string actualXml = xmlWriter.Write(testFile);

			// Assert
			Assert.That(Input.FromString(actualXml), CompareConstraint.IsIdenticalTo(Input.FromString(expectedXml)));
		}

		[Test]
		public void write_should_add_all_case_attributes()
		{
			// Arrange
			string expectedXml = TestHelpers.ReadEmbeddedFile("all-attributes.xml", XmlExamplesFolder);

			var testCase = new Test()
			{
				Position = 1,
				ShortDescription = "short description",
				LongDescription = "long description",
				Url = "http://myserver",
				Method = "post",
				PostType = "text/xml",
				VerifyResponseCode = HttpStatusCode.Accepted,
				ErrorMessage = "my error message",
			};
			TestFile testFile = CreateTestFile(testCase);
			TestFileWriter xmlWriter = CreateTestCaseWriter();

			// Act
			string actualXml = xmlWriter.Write(testFile);

			// Assert
			Assert.That(Input.FromString(actualXml), CompareConstraint.IsIdenticalTo(Input.FromString(expectedXml)));			
		}

		[Test]
		public void write_should_add_header_key_and_values_with_cdata_encoding_when_value_has_html_entities()
		{
			// Arrange
			string expectedXml = TestHelpers.ReadEmbeddedFile("headers.xml", XmlExamplesFolder);

			var testCase = new Test() { Position = 0 };
			testCase.AddHeader("key1", "value1");
			testCase.AddHeader("key2", "some <marvellous> HTML &&&&.");

			TestFile testFile = CreateTestFile(testCase);
			TestFileWriter xmlWriter = CreateTestCaseWriter();

			// Act
			string actualXml = xmlWriter.Write(testFile);

			// Assert
			Assert.That(Input.FromString(actualXml), CompareConstraint.IsIdenticalTo(Input.FromString(expectedXml)));
		}

		[Test]
		public void write_should_add_postbody_with_cdata()
		{
			// Arrange
			string expectedXml = TestHelpers.ReadEmbeddedFile("postbody.xml", XmlExamplesFolder);

			var testCase = new Test() { Position = 0};
			testCase.PostBody = "username=corey&password=welcome&myhtml=<body></body>";
			TestFile testFile = CreateTestFile(testCase);
			TestFileWriter xmlWriter = CreateTestCaseWriter();

			// Act
			string actualXml = xmlWriter.Write(testFile);

			// Assert
			Assert.That(Input.FromString(actualXml), CompareConstraint.IsIdenticalTo(Input.FromString(expectedXml)));
		}

		[Test]
		public void write_should_add_parseresponses()
		{
			// Arrange
			string expectedXml = TestHelpers.ReadEmbeddedFile("capturedvariables.xml", XmlExamplesFolder);

			var testCase = new Test() { Position = 0 };
			testCase.CapturedVariables.Add(new CapturedVariable("1", "here is (.*?) regex"));
			testCase.CapturedVariables.Add(new CapturedVariable("2", "plain text"));
			testCase.CapturedVariables.Add(new CapturedVariable("3", "This is encoded <test> &."));

			TestFile testFile = CreateTestFile(testCase);
			TestFileWriter xmlWriter = CreateTestCaseWriter();

			// Act
			string actualXml = xmlWriter.Write(testFile);

			// Assert
			Assert.That(Input.FromString(actualXml), CompareConstraint.IsIdenticalTo(Input.FromString(expectedXml)));
		}

		[Test]
		public void write_should_add_verifications()
		{
			// Arrange
			string expectedXml = TestHelpers.ReadEmbeddedFile("assertions.xml", XmlExamplesFolder);

			var testCase = new Test() { Position = 0 };
			testCase.VerifyPositives.Add(new Assertion("p90-1", "regex1", AssertionType.Positive));
			testCase.VerifyPositives.Add(new Assertion("p90-2", "regex2", AssertionType.Positive));
			testCase.VerifyPositives.Add(new Assertion("p90-3", "this 3rd positive needs CDATA as it has <html> & stuff in it (.*)", AssertionType.Positive));

			testCase.VerifyNegatives.Add(new Assertion("negev1", "regex1", AssertionType.Negative));
			testCase.VerifyNegatives.Add(new Assertion("negev2", "regex2", AssertionType.Negative));
			testCase.VerifyNegatives.Add(new Assertion("negev3", "this 3rd negative needs CDATA as it has <html> & stuff in it (.*)", AssertionType.Negative));

			TestFile testFile = CreateTestFile(testCase);
			TestFileWriter xmlWriter = CreateTestCaseWriter();

			// Act
			string actualXml = xmlWriter.Write(testFile);

			// Assert
			Assert.That(Input.FromString(actualXml), CompareConstraint.IsIdenticalTo(Input.FromString(expectedXml)));
		}

		[Test]
		public void write_should_write_large_files()
		{
			// Arrange
			string expectedXml = TestHelpers.ReadEmbeddedFile("large-file.xml", XmlExamplesFolder);

			var caseCollection = new TestFile();
			var list = new List<Test>();		

			for (int i = 0; i < 100; i++)
			{
				var testCase = new Test()
				{
                    Position = 0,
					ShortDescription = "short description" +i,
					LongDescription = "long description",
					Url = "http://myserver",
					Method = "post",
					PostType = "text/xml",
					VerifyResponseCode = HttpStatusCode.Accepted,
					ErrorMessage = "my error message",
				};

				list.Add(testCase);
			}

			caseCollection.Tests = list;
			TestFileWriter xmlWriter = CreateTestCaseWriter();

			// Act
			string actualXml = xmlWriter.Write(caseCollection);

			// Assert
			Assert.That(Input.FromString(actualXml), CompareConstraint.IsIdenticalTo(Input.FromString(expectedXml)));
		}

		private TestFileWriter CreateTestCaseWriter()
		{
			return new TestFileWriter();
		}

		private TestFile CreateTestFile()
		{
			return new TestFile();
		}

		private TestFile CreateTestFile(Test testTest)
		{
			var testFile = new TestFile();
			var list = new List<Test>();
			list.Add(testTest);
			testFile.Tests = list;

			return testFile;
		}
	}
}