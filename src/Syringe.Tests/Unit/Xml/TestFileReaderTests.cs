﻿using System.IO;
using System.Linq;
using System.Net;
using NUnit.Framework;
using Syringe.Core.Exceptions;
using Syringe.Core.Extensions;
using Syringe.Core.Tests;
using Syringe.Core.Xml.Reader;

namespace Syringe.Tests.Unit.Xml
{
	public class TestFileReaderTests
	{
		public virtual string XmlExamplesFolder => "Syringe.Tests.Unit.Xml.XmlExamples.Reader.";
		public virtual string FalseString => "false";

		protected virtual ITestFileReader GetTestCaseReader()
        {
			return new TestFileReader();
        }

        protected string GetSingleCaseExample()
		{
			return TestHelpers.ReadEmbeddedFile("single-case.xml", XmlExamplesFolder); 
		}

        protected string GetFullExample()
		{
			return TestHelpers.ReadEmbeddedFile("full.xml", XmlExamplesFolder); 
		}

		[Test]
		public void Read_should_throw_exception_when_testcases_element_is_missing()
		{
			// Arrange
			string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?><something></something>";
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act + Assert
			Assert.Throws<TestCaseException>(() => testCaseReader.Read(stringReader));
		}

		[Test]
		public void Read_should_parse_repeat_attribute()
		{
			// Arrange
			string xml = GetFullExample();
			var stringReader = new StringReader(xml);;
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Assert.That(testFile.Repeat, Is.EqualTo(10));
		}

		[Test]
		public void Read_should_parse_test_vars()
		{
			// Arrange
			string xml = GetFullExample();
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Assert.That(testFile.Variables.Count, Is.EqualTo(4));

			var loginUrlVariable = testFile.Variables.ByName("LOGIN_URL");
			var loginVariable = testFile.Variables.ByName("LOGIN1");
			var passwdVariable = testFile.Variables.ByName("PASSWD1");
			var testTextVariable = testFile.Variables.ByName("SUCCESSFULL_TEST_TEXT");

			Assert.That(loginUrlVariable.Value, Is.EqualTo("http://myserver/login.php"));
			Assert.That(loginUrlVariable.Environment.Name, Is.EqualTo("DevTeam1"));

			Assert.That(loginVariable.Value, Is.EqualTo("bob"));
			Assert.That(loginVariable.Environment.Name, Is.EqualTo("DevTeam2"));

			Assert.That(passwdVariable.Value, Is.EqualTo("sponge"));
			Assert.That(passwdVariable.Environment.Name, Is.EqualTo("DevTeam1"));

			Assert.That(testTextVariable.Value, Is.EqualTo("Welcome Bob"));
			Assert.That(testTextVariable.Environment.Name, Is.EqualTo("DevTeam2"));
		}

		[Test]
		public void Read_should_parse_description_attributes()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test test = testFile.Tests.First();

			Assert.That(test.ShortDescription, Is.EqualTo("short description"));
			Assert.That(test.LongDescription, Is.EqualTo("long description"));
		}

		[Test]
		public void Read_should_parse_method_attribute()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test test = testFile.Tests.First();

			Assert.That(test.Method, Is.EqualTo("post"));
		}

		[Test]
		public void Read_should_default_method_property_to_get_when_it_doesnt_exist()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			xml = xml.Replace(@"method=""post""", "");

			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test test = testFile.Tests.First();

			Assert.That(test.Method, Is.EqualTo("get"));
		}

		[Test]
		public void Read_should_parse_url_attribute()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test testcase = testFile.Tests.First();
			Assert.That(testcase.Url, Is.EqualTo("http://myserver"));
		}

		[Test]
		public void Read_should_throw_exception_when_url_attribute_doesnt_exist()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			xml = xml.Replace(@"url=""http://myserver""", "");

			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act + Assert
			Assert.Throws<TestCaseException>(() => testCaseReader.Read(stringReader));
		}

		[Test]
		public void Read_should_parse_postbody_attribute()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test testcase = testFile.Tests.First();
			Assert.That(testcase.PostBody, Is.EqualTo("username=corey&password=welcome"));
		}

		[Test]
		public void Read_should_parse_errormessage_attribute()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test testcase = testFile.Tests.First();
			Assert.That(testcase.ErrorMessage, Is.EqualTo("my error message"));
		}

		[Test]
		public void Read_should_parse_posttype_attribute()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test testcase = testFile.Tests.First();
			Assert.That(testcase.PostType, Is.EqualTo("text/xml"));
		}

		[Test]
		public void Read_should_use_default_posttype_value_when_attribute_is_empty()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			xml = xml.Replace("posttype=\"text/xml\"", "");

			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test testcase = testFile.Tests.First();
			Assert.That(testcase.PostType, Is.EqualTo("application/x-www-form-urlencoded"));
		}

		[Test]
		public void Read_should_parse_responsecode_attribute()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();
			var expectedCode = HttpStatusCode.NotFound;

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test testcase = testFile.Tests.First();
			Assert.That(testcase.VerifyResponseCode, Is.EqualTo(expectedCode));
		}

		[Test]
		public void Read_should_use_default_responsecode_value_when_attribute_is_empty()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			xml = xml.Replace("verifyresponsecode=\"404\"", "");

			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();
			var expectedCode = HttpStatusCode.OK;

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test testcase = testFile.Tests.First();
			Assert.That(testcase.VerifyResponseCode, Is.EqualTo(expectedCode));
		}

		[Test]
		public void Read_should_use_default_responsecode_value_when_attribute_is_invalid_code()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			xml = xml.Replace("verifyresponsecode=\"404\"", "verifyresponsecode=\"20000000\"");

			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();
			var expectedCode = HttpStatusCode.OK;

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test testcase = testFile.Tests.First();
			Assert.That(testcase.VerifyResponseCode, Is.EqualTo(expectedCode));
		}

		[Test]
		public void Read_should_parse_addheader_attribute()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test test = testFile.Tests.First();
			Assert.That(test.Headers[0].Key, Is.EqualTo("mykey"));
			Assert.That(test.Headers[0].Value, Is.EqualTo("12345"));

			Assert.That(test.Headers[1].Key, Is.EqualTo("bar"));
			Assert.That(test.Headers[1].Value, Is.EqualTo("foo"));

			Assert.That(test.Headers[2].Key, Is.EqualTo("emptyvalue"));
			Assert.That(test.Headers[2].Value, Is.EqualTo(""));

			Assert.That(test.Headers[3].Key, Is.EqualTo("Cookie"));
			Assert.That(test.Headers[3].Value, Is.EqualTo("referer=harrispilton.com"));
		}

		[Test]
		public void Read_should_populate_parseresponse()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test testcase = testFile.Tests.First();
            Assert.That(testcase.CapturedVariables.Count, Is.EqualTo(3));
			Assert.That(testcase.CapturedVariables[0].Regex, Is.EqualTo("parse 1"));
			Assert.That(testcase.CapturedVariables[1].Regex, Is.EqualTo("parse 11"));
			Assert.That(testcase.CapturedVariables[2].Regex, Is.EqualTo("parse 99"));
		}

		[Test]
		public void Read_should_populate_verifypositive()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test testcase = testFile.Tests.First();
            Assert.That(testcase.VerifyPositives.Count, Is.EqualTo(3));
			Assert.That(testcase.VerifyPositives[0].Regex, Is.EqualTo("positive 1"));
			Assert.That(testcase.VerifyPositives[1].Regex, Is.EqualTo("positive 22"));
			Assert.That(testcase.VerifyPositives[2].Regex, Is.EqualTo("positive 99"));
		}

		[Test]
		public void Read_should_populate_verifynegative()
		{
			// Arrange
			string xml = GetSingleCaseExample();
			var stringReader = new StringReader(xml);
			var testCaseReader = GetTestCaseReader();

			// Act
			TestFile testFile = testCaseReader.Read(stringReader);

			// Assert
			Test testcase = testFile.Tests.First();
            Assert.That(testcase.VerifyNegatives.Count, Is.EqualTo(3));
			Assert.That(testcase.VerifyNegatives[0].Regex, Is.EqualTo("negative 1"));
			Assert.That(testcase.VerifyNegatives[1].Regex, Is.EqualTo("negative 6"));
			Assert.That(testcase.VerifyNegatives[2].Regex, Is.EqualTo("negative 66"));
		}
    }
}