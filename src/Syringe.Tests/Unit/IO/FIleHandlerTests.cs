﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Syringe.Core.Configuration;
using Syringe.Core.IO;

namespace Syringe.Tests.Unit.IO
{
    [TestFixture]
    public class FileHandlerTests
    {
        private Mock<IConfiguration> _configurationMock;

		private string _testsFile;
	    private string _testsDirectory;
	    private string _testsFileFullPath;
	    private string _testsWriteFileFullPath;
	    private string _testsFileToDeleteFullPath;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
			_testsFile = "test.xml";
			_testsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "tests");

	        _testsFileFullPath = Path.Combine(_testsDirectory, _testsFile);
			_testsWriteFileFullPath = Path.Combine(_testsDirectory, "testsWrite.xml");
			_testsFileToDeleteFullPath = Path.Combine(_testsDirectory, "fileToDelete.xml");

			if (!Directory.Exists(_testsDirectory))
                Directory.CreateDirectory(_testsDirectory);

            if (!File.Exists(_testsFileFullPath))
            {
				File.WriteAllText(_testsFileFullPath, "Test Data");
            }

            if (!File.Exists(_testsFileToDeleteFullPath))
            {
				File.WriteAllText(_testsFileToDeleteFullPath, "Delete file");
            }
        }

		[TestFixtureTearDown]
		public void TearDownFixture()
		{
			Directory.Delete(_testsDirectory, true);
		}

		[SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(x => x.TestFilesBaseDirectory).Returns(_testsDirectory);
        }

        [Test]
        public void GetFileFullPath_should_throw_FileNotFoundException_if_file_is_missing()
        {
            // given
            FileHandler fileHandler = new FileHandler(_configurationMock.Object);
            string fileName = "filedoesnotexist.xml";

            // when + then
            Assert.Throws<FileNotFoundException>(() => fileHandler.GetFileFullPath(fileName));
        }
        [Test]
        public void GetFileFullPath_should_return_file_path_if_file_exists()
        {
            // given
            FileHandler fileHandler = new FileHandler(_configurationMock.Object);

            // when
            string fileFullPath = fileHandler.GetFileFullPath(_testsFile);

            // then
            Assert.AreEqual(_testsFileFullPath, fileFullPath);
        }

        [Test]
        public void CreateFileFullPath_should_create_correct_path()
        {
            // given
            FileHandler fileHandler = new FileHandler(_configurationMock.Object);

            // when
            string fileFullPath = fileHandler.CreateFileFullPath(_testsFile);

            // then
            Assert.AreEqual(_testsFileFullPath, fileFullPath);
        }

        [Test]
        public void FileExists_should_return_true_if_file_exists()
        {
            // given
            FileHandler fileHandler = new FileHandler(_configurationMock.Object);

            // when
            bool fileExists = fileHandler.FileExists(_testsFileFullPath);

            // then
            Assert.IsTrue(fileExists);
        }

        [Test]
        public void FileExists_should_return_false_if_does_not_exist()
        {
            // given
            FileHandler fileHandler = new FileHandler(_configurationMock.Object);

            // when
            bool fileExists = fileHandler.FileExists("somefakepath/filedoesnotexist.xml");

            // then
            Assert.IsFalse(fileExists);
        }

        [Test]
        public void ReadAllText_should_return_file_contents()
        {
            // given
            FileHandler fileHandler = new FileHandler(_configurationMock.Object);

            // when
            string allText = fileHandler.ReadAllText(_testsFileFullPath);

            // then
            Assert.IsTrue(allText.Contains("Test Data"));
        }

        [Test]
        public void WriteAllText_should_return_true_when_contents_written()
        {
            // given
            FileHandler fileHandler = new FileHandler(_configurationMock.Object);

            // when
            bool allText = fileHandler.WriteAllText(_testsWriteFileFullPath, "test");

            // then
            Assert.IsTrue(allText);
        }

        [Test]
        public void GetFileNames_should_get_filenames_list()
        {
            // given
            FileHandler fileHandler = new FileHandler(_configurationMock.Object);

            // when
            IEnumerable<string> files = fileHandler.GetFileNames();

            // then
            Assert.IsTrue(files.Count() == 1);
            Assert.IsTrue(files.First() == _testsFile);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void CreateFilename_should_throw_argument_null_exception_when_filename_is_empty(string fileName)
        {
            // given + when + then
            Assert.Throws<ArgumentNullException>(() => new FileHandler(_configurationMock.Object).CreateFilename(fileName));
        }

        [TestCase("test")]
        [TestCase("cases")]
        public void CreateFilename_should_add_xml_extension_if_it_is_missing(string fileName)
        {
            // given
            FileHandler fileHandler = new FileHandler(_configurationMock.Object);

            // when
            string createdFileName = fileHandler.CreateFilename(fileName);

            // then
            Assert.AreEqual(string.Concat(fileName, ".xml"), createdFileName);
        }

        [TestCase("test.xml")]
        [TestCase("cases.xml")]
        public void CreateFilename_should__return_correct_name_if_passed_in_correctly(string fileName)
        {
            // given
            FileHandler fileHandler = new FileHandler(_configurationMock.Object);

            // when
            string createdFileName = fileHandler.CreateFilename(fileName);

            // then
            Assert.AreEqual(fileName, createdFileName);
        }

        [Test]
        public void WriteAllText_should_return_falseu_when_text_failed_to_write()
        {
            // given
            FileHandler fileHandler = new FileHandler(_configurationMock.Object);

            // when
            bool allText = fileHandler.WriteAllText("invalidPath%*()", "test");

            // then
            Assert.IsFalse(allText);
        }

        [Test]
        public void DeleteFile_should_return_true_when_file_is_deleted()
        {
            // given
            FileHandler fileHandler = new FileHandler(_configurationMock.Object);

            // when
            bool allText = fileHandler.DeleteFile(_testsFileToDeleteFullPath);

            // then
            Assert.IsTrue(allText);
        }
    }
}
