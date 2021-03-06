﻿using Newtonsoft.Json.Linq;

namespace Eveneum.Documents
{
    class EventDocument : EveneumDocument
    {
        public override string Id => GenerateId(this.StreamId, this.Version);
        public override DocumentType DocumentType => DocumentType.Event;

        public string MetadataType { get; set; }
        public JToken Metadata { get; set; }
        public string BodyType { get; set; }
        public JToken Body { get; set; }

        public static string GenerateId(string streamId, ulong version) => $"{streamId}{Separator}{version}";
    }
}
