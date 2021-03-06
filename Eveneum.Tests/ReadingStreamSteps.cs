﻿using System.Threading.Tasks;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Eveneum.Tests.Infrastrature;
using Newtonsoft.Json.Linq;
using Eveneum.Documents;
using System.Linq;

namespace Eveneum.Tests
{
    [Binding]
    public class ReadingStreamSteps
    {
        private readonly CosmosDbContext Context;

        ReadingStreamSteps(CosmosDbContext context)
        {
            this.Context = context;
        }

        [When(@"I read stream ([^\s-])")]
        public async Task WhenIReadStream(string streamId)
        {
            ScenarioContext.Current.SetStreamId(streamId);
            ScenarioContext.Current.SetStream(await this.Context.EventStore.ReadStream(streamId));
        }

        [Then(@"the non-existing stream is returned")]
        public void ThenTheNon_ExistingStreamIsReturned()
        {
            var stream = ScenarioContext.Current.GetStream();

            Assert.IsFalse(stream.HasValue);
        }

        [Then(@"the stream ([^\s-]) in version (\d+) is returned")]
        public void ThenTheStreamInVersionIsReturned(string streamId, ulong version)
        {
            var stream = ScenarioContext.Current.GetStream();

            Assert.IsTrue(stream.HasValue);
            Assert.AreEqual(streamId, stream.Value.StreamId);
            Assert.AreEqual(version, stream.Value.Version);
            Assert.IsNull(stream.Value.Metadata);
        }

        [Then(@"the stream ([^\s-]) with metadata in version (\d+) is returned")]
        public void ThenTheStreamWithMetadataInVersionIsReturned(string streamId, ulong version)
        {
            var stream = ScenarioContext.Current.GetStream();

            Assert.IsTrue(stream.HasValue);
            Assert.AreEqual(streamId, stream.Value.StreamId);
            Assert.AreEqual(version, stream.Value.Version);
            Assert.AreEqual(JToken.FromObject(ScenarioContext.Current.GetHeaderMetadata()), JToken.FromObject(stream.Value.Metadata));
        }

        [Then(@"no snapshot is returned")]
        public void ThenNoSnapshotIsReturned()
        {
            var stream = ScenarioContext.Current.GetStream();

            Assert.IsTrue(stream.HasValue);
            Assert.IsFalse(stream.Value.Snapshot.HasValue);
        }

        [Then(@"a snapshot for version (\d+) is returned")]
        public void ThenASnapshotForVersionIsReturned(ulong version)
        {
            var stream = ScenarioContext.Current.GetStream();

            Assert.IsTrue(stream.HasValue);
            Assert.IsTrue(stream.Value.Snapshot.HasValue);
            Assert.AreEqual(version, stream.Value.Snapshot.Value.Version);
            Assert.AreEqual(JToken.FromObject(ScenarioContext.Current.GetSnapshot()), JToken.FromObject(stream.Value.Snapshot.Value.Data));
            Assert.IsNull(stream.Value.Snapshot.Value.Metadata);
        }

        [Then(@"a snapshot with metadata for version (\d+) is returned")]
        public void ThenASnapshotWithMetadataForVersionIsReturned(ulong version)
        {
            var stream = ScenarioContext.Current.GetStream();

            Assert.IsTrue(stream.HasValue);
            Assert.IsTrue(stream.Value.Snapshot.HasValue);
            Assert.AreEqual(version, stream.Value.Snapshot.Value.Version);
            Assert.AreEqual(JToken.FromObject(ScenarioContext.Current.GetSnapshot()), JToken.FromObject(stream.Value.Snapshot.Value.Data));
            Assert.IsNotNull(stream.Value.Snapshot.Value.Metadata);
            Assert.AreEqual(JToken.FromObject(ScenarioContext.Current.GetSnapshotMetadata()), JToken.FromObject(stream.Value.Snapshot.Value.Metadata));
        }

        [Then(@"no events are returned")]
        public void ThenNoEventsAreReturned()
        {
            var stream = ScenarioContext.Current.GetStream();

            Assert.IsTrue(stream.HasValue);
            Assert.IsEmpty(stream.Value.Events);
        }

        [Then(@"events from version (\d+) to (\d+) are returned")]
        public async Task ThenEventsFromVersionToAreReturned(ulong fromVersion, ulong toVersion)
        {
            var stream = ScenarioContext.Current.GetStream();
            var allDocuments = await CosmosSetup.QueryAllDocumentsInStream<EventDocument>(this.Context.Client, this.Context.Database, this.Context.Collection, this.Context.PartitionKey, stream.Value.StreamId);           
            var eventDocuments = allDocuments.ToDictionary(x => x.Version);

            Assert.IsTrue(stream.HasValue);
            Assert.IsNotEmpty(stream.Value.Events);
            Assert.AreEqual(toVersion - fromVersion + 1, stream.Value.Events.Length);

            for(ulong version = fromVersion, index = 0; version <= toVersion; ++version, ++index)
            {
                var @event = stream.Value.Events[index];

                Assert.AreEqual(version, @event.Version);
                Assert.IsTrue(eventDocuments.ContainsKey(version));

                var eventDocument = eventDocuments[version];

                Assert.AreEqual(eventDocument.Metadata, JToken.FromObject(@event.Metadata));
            }
        }
    }
}
