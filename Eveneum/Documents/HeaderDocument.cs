﻿using Newtonsoft.Json.Linq;

namespace Eveneum.Documents
{
    class HeaderDocument : EveneumDocument
    {
        public override string Id => GenerateId(this.StreamId);
        public override DocumentType DocumentType => DocumentType.Header;

        public string MetadataType { get; set; }
        public JToken Metadata { get; set; }

        public static string GenerateId(string streamId) => streamId;
    }
}
