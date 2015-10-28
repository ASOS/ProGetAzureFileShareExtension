using System;
using System.IO;
using System.Linq;
using Inedo.Diagnostics;
using Inedo.NuGet;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;

namespace ProGetAzureFileShareExtension.Tests
{
    [TestFixture]
    public class AzureFileShareNuGetPackageStoreTests
    {
        [Test]
        public void Should_have_paramterless_constructor()
        {
            var type = typeof(AzureFileShareNuGetPackageStore);
            var ctor = type.GetConstructor(new Type[] { });
            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Can_instantiate_using_paramterless_constructor()
        {
            Assert.DoesNotThrow(() => new AzureFileShareNuGetPackageStore());
        }

        [Test]
        [TestCase(null, typeof(ArgumentNullException))]
        [TestCase("", typeof(ArgumentNullException))]
        [TestCase("P", typeof(ArgumentOutOfRangeException))]
        [TestCase("1", typeof(ArgumentOutOfRangeException))]
        [TestCase("not-a-drive-letter", typeof(ArgumentOutOfRangeException))]
        public void Validates_drive_letter(string driveLetter, Type expectedException)
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations)
            {
                RootPath = "RootPath",
                FileShareName = "FileShareName",
                UserName = "UserName",
                AccessKey = "AccessKey",
                DriveLetter = driveLetter
            };
            Assert.Throws(expectedException, () => sut.CreatePackage("packageId", SemanticVersion.Parse("1.2.3")));
        }

        [Test]
        [TestCase(null, typeof(ArgumentNullException))]
        [TestCase("", typeof(ArgumentNullException))]
        [TestCase("D:\\RandomPath", typeof(ArgumentOutOfRangeException))]
        [TestCase("\\\\server\\share", typeof(ArgumentOutOfRangeException))]
        public void Validates_root_path(string rootPath, Type expectedException)
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations)
            {
                RootPath = rootPath,
                FileShareName = "FileShareName",
                UserName = "UserName",
                AccessKey = "AccessKey",
                DriveLetter = "P:"
            };
            Assert.Throws(expectedException, () => sut.CreatePackage("packageId", SemanticVersion.Parse("1.2.3")));
        }

        [Test]
        [TestCase(null, typeof(ArgumentNullException))]
        [TestCase("", typeof(ArgumentNullException))]
        public void Validates_user_name(string username, Type expectedException)
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations)

            {
                RootPath = "P:\\ProGetPackages",
                FileShareName = "FileShareName",
                UserName = username,
                AccessKey = "AccessKey",
                DriveLetter = "P:"
            };
            Assert.Throws(expectedException, () => sut.CreatePackage("packageId", SemanticVersion.Parse("1.2.3")));
        }

        [Test]
        [TestCase(null, typeof(ArgumentNullException))]
        [TestCase("", typeof(ArgumentNullException))]
        public void Validates_access_key(string accessKey, Type expectedException)
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations)

            {
                RootPath = "P:\\ProGetPackages",
                FileShareName = "FileShareName",
                UserName = "username",
                AccessKey = accessKey,
                DriveLetter = "P:"
            };
            Assert.Throws(expectedException, () => sut.CreatePackage("packageId", SemanticVersion.Parse("1.2.3")));
        }

        [Test]
        [TestCase(null, typeof(ArgumentNullException))]
        [TestCase("", typeof(ArgumentNullException))]
        public void Validates_file_share_name(string fileShareName, Type expectedException)
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations)

            {
                RootPath = "P:\\ProGetPackages",
                FileShareName = fileShareName,
                UserName = "username",
                AccessKey = "accessKey",
                DriveLetter = "P:"
            };
            Assert.Throws(expectedException, () => sut.CreatePackage("packageId", SemanticVersion.Parse("1.2.3")));
        }

        [Test]
        public void Create_package_throws_argument_null_exception_if_package_id_is_null()
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations);
            Assert.Throws<ArgumentNullException>(() => sut.CreatePackage(null, SemanticVersion.Parse("1.2.3")));
        }

        [Test]
        public void Create_package_throws_argument_null_exception_if_package_id_is_empty()
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations);
            Assert.Throws<ArgumentNullException>(() => sut.CreatePackage("", SemanticVersion.Parse("1.2.3")));
        }

        [Test]
        public void Create_package_throws_argument_null_exception_if_package_version_is_null()
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations);
            Assert.Throws<ArgumentNullException>(() => sut.CreatePackage("packageId", null));
        }

        [Test]
        public void Delete_package_throws_argument_null_exception_if_package_id_is_null()
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations);
            Assert.Throws<ArgumentNullException>(() => sut.DeletePackage(null, SemanticVersion.Parse("1.2.3")));
        }

        [Test]
        public void Delete_package_throws_argument_null_exception_if_package_id_is_empty()
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations);
            Assert.Throws<ArgumentNullException>(() => sut.DeletePackage("", SemanticVersion.Parse("1.2.3")));
        }

        [Test]
        public void Delete_package_throws_argument_null_exception_if_package_version_is_null()
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations);
            Assert.Throws<ArgumentNullException>(() => sut.DeletePackage("packageId", null));
        }

        [Test]
        public void Open_package_throws_argument_null_exception_if_package_id_is_null()
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations);
            Assert.Throws<ArgumentNullException>(() => sut.OpenPackage(null, SemanticVersion.Parse("1.2.3")));
        }

        [Test]
        public void Open_package_throws_argument_null_exception_if_package_id_is_empty()
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations);
            Assert.Throws<ArgumentNullException>(() => sut.OpenPackage("", SemanticVersion.Parse("1.2.3")));
        }

        [Test]
        public void Open_package_throws_argument_null_exception_if_package_version_is_null()
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations);
            Assert.Throws<ArgumentNullException>(() => sut.OpenPackage("packageId", null));
        }

        [Test]
        public void Clean_throws_argument_null_exception_if_package_index_is_null()
        {
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var mockFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var sut = new AzureFileShareNuGetPackageStore(mockFileShareMapper, mockFileSystemOperations);
            Assert.Throws<ArgumentNullException>(() => sut.Clean(null));
        }

        [Test]
        public void Open_package_calls_file_share_mapper()
        {
            var fakeFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var sut = new AzureFileShareNuGetPackageStore(fakeFileShareMapper, mockFileSystemOperations)
            {
                RootPath = "P:\\ProGetPackages",
                FileShareName = "filesharename",
                UserName = "username",
                AccessKey = "accesskey",
                DriveLetter = "P:"
            };
            sut.OpenPackage("packageId", SemanticVersion.Parse("1.2.3"));
            fakeFileShareMapper.AssertWasCalled(x => x.Mount(
                Arg<string>.Is.Equal("P:"),
                Arg<string>.Is.Equal(@"\\username.file.core.windows.net\filesharename"),
                Arg<string>.Is.Equal("username"),
                Arg<string>.Is.Equal("accesskey"),
                Arg<ILog>.Is.NotNull));
        }

        [Test]
        public void Create_package_calls_file_share_mapper()
        {
            var fakeFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var sut = new AzureFileShareNuGetPackageStore(fakeFileShareMapper, mockFileSystemOperations)
            {
                RootPath = "P:\\ProGetPackages",
                FileShareName = "filesharename",
                UserName = "username",
                AccessKey = "accesskey",
                DriveLetter = "P:"
            };
            sut.CreatePackage("packageId", SemanticVersion.Parse("1.2.3"));
            fakeFileShareMapper.AssertWasCalled(x => x.Mount(
                Arg<string>.Is.Equal("P:"),
                Arg<string>.Is.Equal(@"\\username.file.core.windows.net\filesharename"),
                Arg<string>.Is.Equal("username"),
                Arg<string>.Is.Equal("accesskey"),
                Arg<ILog>.Is.NotNull));
        }


        [Test]
        public void Delete_package_calls_file_share_mapper()
        {
            var fakeFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var mockFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();
            var sut = new AzureFileShareNuGetPackageStore(fakeFileShareMapper, mockFileSystemOperations)
            {
                RootPath = "P:\\ProGetPackages",
                FileShareName = "filesharename",
                UserName = "username",
                AccessKey = "accesskey",
                DriveLetter = "P:"
            };
            sut.DeletePackage("packageId", SemanticVersion.Parse("1.2.3"));
            fakeFileShareMapper.AssertWasCalled(x => x.Mount(
                Arg<string>.Is.Equal("P:"),
                Arg<string>.Is.Equal(@"\\username.file.core.windows.net\filesharename"),
                Arg<string>.Is.Equal("username"),
                Arg<string>.Is.Equal("accesskey"),
                Arg<ILog>.Is.NotNull));
        }

        [Test]
        public void Open_package_returns_null_when_directory_does_not_exist()
        {
            var fakeFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var fakeFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();

            fakeFileSystemOperations.Expect(x => x.DirectoryExists("P:\\ProGetPackages\\packageId")).Return(false);
            var sut = new AzureFileShareNuGetPackageStore(fakeFileShareMapper, fakeFileSystemOperations)
            {
                RootPath = "P:\\ProGetPackages",
                FileShareName = "filesharename",
                UserName = "username",
                AccessKey = "accesskey",
                DriveLetter = "P:"
            };
            var result = sut.OpenPackage("packageId", SemanticVersion.Parse("1.2.3"));
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Open_package_returns_null_when_package_file_does_not_exist()
        {
            var fakeFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var fakeFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();

            fakeFileSystemOperations.Expect(x => x.DirectoryExists("P:\\ProGetPackages\\packageId")).Return(true);
            fakeFileSystemOperations.Expect(x => x.FileExists("P:\\ProGetPackages\\packageId\\packageId.1.2.3.nupkg")).Return(false);
            var sut = new AzureFileShareNuGetPackageStore(fakeFileShareMapper, fakeFileSystemOperations)
            {
                RootPath = "P:\\ProGetPackages",
                FileShareName = "filesharename",
                UserName = "username",
                AccessKey = "accesskey",
                DriveLetter = "P:"
            };
            var result = sut.OpenPackage("packageId", SemanticVersion.Parse("1.2.3"));
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Open_package_returns_file_stream()
        {
            var fakeFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var fakeFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();

            fakeFileSystemOperations.Expect(x => x.DirectoryExists("P:\\ProGetPackages\\packageId")).Return(true);
            var versionPath = "P:\\ProGetPackages\\packageId\\packageId.1.2.3.nupkg";
            fakeFileSystemOperations.Expect(x => x.FileExists(versionPath)).Return(true);
            using (var returnedStream = new MemoryStream())
            {
                fakeFileSystemOperations.Expect(
                    x => x.GetFileStream(versionPath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
                    .Return(returnedStream);
                var sut = new AzureFileShareNuGetPackageStore(fakeFileShareMapper, fakeFileSystemOperations)
                {
                    RootPath = "P:\\ProGetPackages",
                    FileShareName = "filesharename",
                    UserName = "username",
                    AccessKey = "accesskey",
                    DriveLetter = "P:"
                };
                var result = sut.OpenPackage("packageId", SemanticVersion.Parse("1.2.3"));
                Assert.That(result, Is.EqualTo(returnedStream));
            }
        }

        [Test]
        public void Create_package_returns_file_stream()
        {
            var fakeFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var fakeFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();

            fakeFileSystemOperations.Expect(x => x.DirectoryExists("P:\\ProGetPackages\\packageId")).Return(true);
            var versionPath = "P:\\ProGetPackages\\packageId\\packageId.1.2.3.nupkg";
            fakeFileSystemOperations.Expect(x => x.FileExists(versionPath)).Return(true);
            using (var returnedStream = new MemoryStream())
            {
                fakeFileSystemOperations.Expect(
                    x => x.GetFileStream(versionPath, FileMode.Create, FileAccess.Write, FileShare.Delete))
                    .Return(returnedStream);
                var sut = new AzureFileShareNuGetPackageStore(fakeFileShareMapper, fakeFileSystemOperations)
                {
                    RootPath = "P:\\ProGetPackages",
                    FileShareName = "filesharename",
                    UserName = "username",
                    AccessKey = "accesskey",
                    DriveLetter = "P:"
                };
                var result = sut.CreatePackage("packageId", SemanticVersion.Parse("1.2.3"));
                Assert.That(result, Is.EqualTo(returnedStream));
            }
        }

        [Test]
        public void Delete_package_deletes_file_and_directory()
        {
            var fakeFileShareMapper = MockRepository.GenerateMock<IFileShareMapper>();
            var fakeFileSystemOperations = MockRepository.GenerateMock<IFileSystemOperations>();

            var packagePath = "P:\\ProGetPackages\\packageId";
            fakeFileSystemOperations.Expect(x => x.DirectoryExists(packagePath)).Return(true);
            var versionPath = "P:\\ProGetPackages\\packageId\\packageId.1.2.3.nupkg";
            fakeFileSystemOperations.Expect(x => x.FileExists(versionPath)).Return(true);
            fakeFileSystemOperations.Expect(x => x.EnumerateFileSystemEntries(packagePath))
                .Return(Enumerable.Empty<string>());
            var sut = new AzureFileShareNuGetPackageStore(fakeFileShareMapper, fakeFileSystemOperations)
            {
                RootPath = "P:\\ProGetPackages",
                FileShareName = "filesharename",
                UserName = "username",
                AccessKey = "accesskey",
                DriveLetter = "P:"
            };
            sut.DeletePackage("packageId", SemanticVersion.Parse("1.2.3"));
            fakeFileSystemOperations.AssertWasCalled(x => x.DeleteFile(versionPath));
            fakeFileSystemOperations.AssertWasCalled(x => x.DeleteDirectory(packagePath));
        }

    }
}
