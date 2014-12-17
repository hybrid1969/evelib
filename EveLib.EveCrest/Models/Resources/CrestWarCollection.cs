﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using eZet.EveLib.Modules.Models.Entities;

namespace eZet.EveLib.Modules.Models.Resources {
    /// <summary>
    ///     Represents a CREST collection of wars
    /// </summary>
    [DataContract]
    public sealed class CrestWarCollection : CrestCollectionResource<CrestWar> {
        public CrestWarCollection() {
            Version = "application/vnd.ccp.eve.WarsCollection-v1+json";
        }

        /// <summary>
        ///     A list of wars
        /// </summary>
        [DataMember(Name = "items")]
        public IReadOnlyList<CrestLinkedEntity<CrestWar>> Items { get; set; }
    }
}