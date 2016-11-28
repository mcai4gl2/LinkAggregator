using System;
using System.Collections.Generic;
using System.IO;
using LinkAggregator.Git;
using NUnit.Framework;

namespace LinkAggregator.Tests.Git
{
    [TestFixture]
    public class GitDocumentProcessorTests
    {
        private GitDocumentProcessor _processor;

        [SetUp]
        public void SetUp()
        {
            _processor = new GitDocumentProcessor(Path.GetTempPath(), "git@github.com:mcai4gl2/wiki.git");
        }

        [Test]
        public void can_clone()
        {
            _processor.Clone();
        }

        [Test]
        public void can_write()
        {
            var doc = new Document
            {
                TimeStamp = DateTime.Now,
                Subject = "Test",
                User = "mcai4gl2",
                Body = "Test Body"
            };
            var doc2 = new Document
            {
                TimeStamp = DateTime.Now,
                Subject = "Test2",
                User = "mcai4gl2",
                Body = "Test Body"
            };
            _processor.Process(new List<Document>{doc, doc2});
        }
    }
}
