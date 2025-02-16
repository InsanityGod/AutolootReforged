using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoLootReforged.Config
{
    public class ModConfig
    {
        /// <summary>
        /// Wether actions should be logged
        /// (unless you are debugging you shouldn't need this)
        /// </summary>
        [DefaultValue(false)]
        public bool Log { get; set; } = false;

        /// <summary>
        /// Wether to play a sound when auto-looting
        /// </summary>
        [DefaultValue(true)]
        public bool Sound {get; set; } = true;
    }
}
