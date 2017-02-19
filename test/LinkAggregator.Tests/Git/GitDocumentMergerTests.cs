using System;
using System.IO;
using LinkAggregator.Git;
using NUnit.Framework;

namespace LinkAggregator.Tests.Git
{
    [TestFixture]
    public class GitDocumentMergerTests
    {
        private GitDocumentMerger _merger;

        [SetUp]
        public void SetUp()
        {
            _merger = new GitDocumentMerger(Path.GetTempPath(), "git@github.com:mcai4gl2/wiki.git");
        }

        [Test]
        [Ignore("This is an interactive test")]
        public void can_merge()
        {
            _merger.Process();
        }
    }
}
