﻿using System;
using System.Xml.Serialization;

namespace eZet.EveLib.EveOnlineApi.Model.Character {
    [Serializable]
    [XmlRoot("result", IsNullable = false)]
    public class Locations : XmlElement {
        [XmlElement("rowset")]
        public RowCollection<Location> Items { get; set; }

        [Serializable]
        [XmlRoot("row")]
        public class Location {
            [XmlAttribute("itemID")]
            public long ItemId { get; set; }

            [XmlAttribute("itemName")]
            public string ItemName { get; set; }

            [XmlAttribute("x")]
            public double X { get; set; }

            [XmlAttribute("y")]
            public double Y { get; set; }

            [XmlAttribute("z")]
            public double Z { get; set; }
        }
    }
}