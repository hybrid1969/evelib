﻿namespace eZet.EveLib.Modules.Models.Resources {
    public class CrestResource : ICrestResource {
        public virtual bool IsDeprecated { get; set; }

        public virtual string Version { get; protected set; }
    }
}