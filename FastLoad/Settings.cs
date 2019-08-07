using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace FastLoad
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool enableSkipMenu { get; set; } = false;
        internal bool enableFastLoad { get; set; } = true;


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}
