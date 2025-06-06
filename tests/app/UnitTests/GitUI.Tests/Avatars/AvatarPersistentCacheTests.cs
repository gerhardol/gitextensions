﻿using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using GitCommands;
using GitUI.Avatars;
using NSubstitute;

namespace GitUITests.Avatars
{
    [TestFixture]
    public sealed class AvatarPersistentCacheTests : AvatarCacheTestBase
    {
        private string _avatarImageCachePath = AppSettings.AvatarImageCachePath;
        private string _email1AvatarPath;
        private IFileSystem _fileSystem;
        private DirectoryBase _directory;
        private FileBase _file;
        private FileInfoBase _fileInfo;
        private IFileInfoFactory _fileInfoFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _fileSystem = Substitute.For<IFileSystem>();
            _directory = Substitute.For<DirectoryBase>();
            _fileSystem.Directory.Returns(_directory);
            _file = Substitute.For<FileBase>();
            _fileSystem.File.Returns(_file);
            _fileInfo = Substitute.For<FileInfoBase>();
            _fileInfo.Exists.Returns(true);
            _fileInfoFactory = Substitute.For<IFileInfoFactory>();
            _fileInfoFactory.New(Arg.Any<string>()).Returns(_fileInfo);
            _fileSystem.FileInfo.Returns(_fileInfoFactory);

            AppSettings.AvatarProvider = AvatarProvider.Default;

            _cache = new FileSystemAvatarCache(_inner, _fileSystem);
            _email1AvatarPath = Path.Combine(_avatarImageCachePath, $"{_email1}.{_size}px.png");
        }

        [Test]
        public async Task GetAvatarAsync_should_create_if_folder_absent()
        {
            MockFileSystem fileSystem = new();
            fileSystem.Directory.Exists(_avatarImageCachePath).Should().BeFalse();
            _cache = new FileSystemAvatarCache(_inner, fileSystem);

            ClassicAssert.AreSame(_img1, await _cache.GetAvatarAsync(_email1, _name1, _size));

            fileSystem.Directory.Exists(_avatarImageCachePath).Should().BeTrue();
        }

        [Test]
        public async Task GetAvatarAsync_should_create_image_from_stream()
        {
            MockFileSystem fileSystem = new();
            fileSystem.Directory.Exists(_avatarImageCachePath).Should().BeFalse();
            _cache = new FileSystemAvatarCache(_inner, fileSystem);

            ClassicAssert.AreSame(_img1, await _cache.GetAvatarAsync(_email1, _name1, _size));

            fileSystem.Directory.Exists(_avatarImageCachePath).Should().BeTrue();
            fileSystem.File.Exists(_email1AvatarPath).Should().BeTrue();
        }

        [Test]
        public async Task GetAvatarAsync_uses_inner_if_file_expired()
        {
            _fileInfo.Exists.Returns(true);
            _fileInfo.LastWriteTime.Returns(new DateTime(2010, 1, 1));
            _fileSystem.File.OpenWrite(Arg.Any<string>()).Returns(_ => (Stream)new MemoryStream());
            _fileSystem.File.Delete(Arg.Any<string>());

            await MissAsync(_email1, _name1);

            _fileSystem.File.Received(1).Delete(_email1AvatarPath);

            _file.OpenRead(Arg.Any<string>()).Returns(c => GetPngStream());
            _fileInfo.LastWriteTime.Returns(DateTime.Now);
            _fileSystem.ClearReceivedCalls();
            _fileInfo.ClearReceivedCalls();
            _file.ClearReceivedCalls();

            Image image = await _cache.GetAvatarAsync(_email1, _name1, 16);

            image.Should().NotBeNull();
            _ = _fileInfo.Received(1).LastWriteTime;

            _fileSystem.File.Received(1).OpenRead(_email1AvatarPath);
        }

        [Test]
        public async Task ClearCacheAsync_should_return_if_folder_absent()
        {
            _directory.Exists(Arg.Any<string>()).Returns(false);

            await _cacheCleaner.ClearCacheAsync();

            _directory.DidNotReceive().GetFiles(Arg.Any<string>());
        }

        [Test]
        public async Task ClearCacheAsync_should_remove_all()
        {
            MockFileSystem fileSystem = new();
            _cache = new FileSystemAvatarCache(_inner, fileSystem);

            fileSystem.AddFile(Path.Combine(_avatarImageCachePath, "a@a.com.16px.png"), new MockFileData(""));
            fileSystem.AddFile(Path.Combine(_avatarImageCachePath, "b@b.com.16px.png"), new MockFileData(""));
            fileSystem.AllFiles.Should().HaveCount(2);

            await _cacheCleaner.ClearCacheAsync();

            fileSystem.AllFiles.Should().BeEmpty();
        }

        [Test]
        public void ClearCacheAsync_should_ignore_errors()
        {
            _directory.Exists(Arg.Any<string>()).Returns(true);
            _directory.GetFiles(_avatarImageCachePath).Returns(["c:\\file.txt", "boot.sys"]);
            _file.When(x => x.Delete(Arg.Any<string>()))
                .Do(x => throw new DivideByZeroException());

            Func<Task> act = () => _cacheCleaner.ClearCacheAsync();
            act.Should().NotThrowAsync();
        }
    }
}
