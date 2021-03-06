﻿using System.Collections.Generic;

namespace JMMClient.AutoUpdates
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Updates
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("update", IsNullable = false)]
        public List<Update> server { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("update", IsNullable = false)]
        public List<Update> desktop { get; set; }
    }
}
