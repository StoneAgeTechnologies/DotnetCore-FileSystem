﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StoneAge.FileStore.Domain;

namespace StoneAge.FileStore.Tests
{
    [TestFixture]
    public class FileSystemTests
    {
        [TestFixture]
        class Write
        {
            [Test]
            public async Task WhenFileAndPathValid_ExpectFileWritten()
            {
                //---------------Arrange-------------------
                var path = Path.GetTempPath();
                var fileName = Guid.NewGuid()+".csv";
                var document = Create_CsvFile(fileName);

                var sut = new FileSystem();
                //---------------Act----------------------
                var result = await sut.Write(path, document);
                //---------------Assert-----------------------
                var fileWritten = File.Exists(Path.Combine(path,fileName));
                result.HadError.Should().BeFalse();
                fileWritten.Should().BeTrue();
            }

            [TestCase(" ")]
            [TestCase("")]
            [TestCase(null)]
            public async Task WhenPathContainsNullOrWhiteSpace_ExpectErrorMessage(string path)
            {
                //---------------Arrange-------------------
                var document = Create_CsvFile("test.csv");

                var sut = new FileSystem();
                //---------------Act----------------------
                var result = await sut.Write(path, document);
                //---------------Assert-----------------------
                result.ErrorMessages.Should().BeEquivalentTo(new List<string>
                {
                    "Invalid directory provided"
                });
            }

            [TestCase("abc")]
            [TestCase("~f0")]
            public async Task WhenPathInvalid_ExpectErrorMessage(string path)
            {
                //---------------Arrange-------------------
                var document = Create_CsvFile("test.csv");

                var sut = new FileSystem();
                //---------------Act----------------------
                var result = await sut.Write(path, document);
                //---------------Assert-----------------------
                result.ErrorMessages.Should().BeEquivalentTo(new List<string>
                {
                    "Invalid directory provided"
                });
            }
        }

        [TestFixture]
        class List
        {
            [Test]
            public void WhenDirectoryExist_ExpectContents()
            {
                //---------------Arrange-------------------
                var path = Path.GetTempPath();

                var sut = new FileSystem();
                //---------------Act----------------------
                var result = sut.List(path);
                //---------------Assert-----------------------
                result.Count().Should().BeGreaterOrEqualTo(1);
            }

            [Test]
            public void WhenFilePassedIn_ExpectEmptyList()
            {
                //---------------Arrange-------------------
                var path = Path.GetTempFileName();

                var sut = new FileSystem();
                //---------------Act----------------------
                var result = sut.List(path);
                //---------------Assert-----------------------
                result.Should().BeEmpty();
            }

            [TestCase(" ")]
            [TestCase("")]
            [TestCase(null)]
            public void WhenNullOrWhiteSpaceDirectory_ExpectEmptyList(string path)
            {
                //---------------Arrange-------------------
                var sut = new FileSystem();
                //---------------Act----------------------
                var result = sut.List(path);
                //---------------Assert-----------------------
                result.Should().BeEmpty();
            }
        }

        [TestFixture]
        class Exists
        {
            [Test]
            public void WhenFileDoesNotExist_ExpectFalse()
            {
                //---------------Arrange-------------------
                var path = Create_Missing_File();

                var sut = new FileSystem();
                //---------------Act----------------------
                var result = sut.Exists(path);
                //---------------Assert-----------------------
                result.Should().BeFalse();
            }

            [Test]
            public void WhenFileExist_ExpectTrue()
            {
                //---------------Arrange-------------------
                var path = Create_File();

                var sut = new FileSystem();
                //---------------Act----------------------
                var result = sut.Exists(path);
                //---------------Assert-----------------------
                result.Should().BeTrue();
            }

            [TestCase(" ")]
            [TestCase("")]
            [TestCase(null)]
            public void WhenFileNullOrWhiteSpace_ExpectTrue(string path)
            {
                //---------------Arrange-------------------
                var sut = new FileSystem();
                //---------------Act----------------------
                var result = sut.Exists(path);
                //---------------Assert-----------------------
                result.Should().BeFalse();
            }
            
            [Test]
            public void WhenDirectoryExist_ExpectTrue()
            {
                //---------------Arrange-------------------
                var path = Path.GetTempPath();

                var sut = new FileSystem();
                //---------------Act----------------------
                var result = sut.Exists(path);
                //---------------Assert-----------------------
                result.Should().BeTrue();
            }
        }

        [TestFixture]
        class Delete
        {
            [Test]
            public void WhenFileExist_ExpectItIsRemoved()
            {
                //---------------Arrange-------------------
                var path = Create_File();
                
                var sut = new FileSystem();
                //---------------Act----------------------
                sut.Delete(path);
                //---------------Assert-----------------------
                var fileExists = File.Exists(path);
                fileExists.Should().BeFalse();
            }

            [Test]
            public void WhenFileDoesNotExist_ExpectNothingToHappen()
            {
                //---------------Arrange-------------------
                var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                var sut = new FileSystem();
                //---------------Act----------------------
                sut.Delete(path);
                //---------------Assert-----------------------
                var fileExists = File.Exists(path);
                fileExists.Should().BeFalse();
            }

            [TestCase(" ")]
            [TestCase("")]
            [TestCase(null)]
            public void WhenFileNullOrWhitespace_ExpectNoExceptionsThrown(string path)
            {
                //---------------Arrange-------------------
                var sut = new FileSystem();
                //---------------Act----------------------
                Assert.DoesNotThrow(()=>sut.Delete(path));
                //---------------Assert-----------------------
                var fileExists = File.Exists(path);
                fileExists.Should().BeFalse();
            }

            [Test]
            public void WhenDirectoryExist_ExpectItIsRemoved()
            {
                //---------------Arrange-------------------
                var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(path);

                var sut = new FileSystem();
                //---------------Act----------------------
                sut.Delete(path);
                //---------------Assert-----------------------
                var fileExists = File.Exists(path);
                fileExists.Should().BeFalse();
            }
        }

        [TestFixture]
        class ReadDocument
        {
            [Test]
            public void GivenFileExist_ExpectDocumentWithBytesReturned()
            {
                //---------------Arrange-------------------
                var contents = "hi, this is some text for a file";

                var path = Create_File(contents);
                
                var sut = new FileSystem();
                //---------------Act----------------------
                var actual = sut.GetDocument(path);
                //---------------Assert-----------------------
                var expected = "hi, this is some text for a file";
                actual.ToString().Should().Be(expected);
            }

            [Test]
            public void GivenFileDoesExist_ExpectNullDocument()
            {
                //---------------Arrange-------------------
                var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                var sut = new FileSystem();
                //---------------Act----------------------
                var actual = sut.GetDocument(path);
                //---------------Assert-----------------------
                actual.Should().Be(FileSystem.NullDocument);
            }

            [Test]
            public void GivenNullPath_ExpectNullDocument()
            {
                //---------------Arrange-------------------
                var sut = new FileSystem();
                //---------------Act----------------------
                var actual = sut.GetDocument(null);
                //---------------Assert-----------------------
                actual.Should().Be(FileSystem.NullDocument);
            }
        }

        private static string Create_File(string content)
        {
            var tmp = Path.GetTempPath();
            var path = Path.Combine(tmp, Guid.NewGuid().ToString());

            File.WriteAllText(path, content);

            return path;
        }

        private static string Create_File()
        {
            return Create_File(string.Empty);
        }

        private static string Create_Missing_File()
        {
            var tmp = Path.GetTempPath();
            var path = Path.Combine(tmp, Guid.NewGuid().ToString());

            return path;
        }

        private static IDocument Create_CsvFile(string fileName)
        {
            var csvFile = new DocumentBuilder()
                                .With_Name(fileName)
                                .With_Bytes(new byte[5])
                                .Create_Document();
            return csvFile;
        }

    }
}
