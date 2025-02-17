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
        /// Wether any logs are created (logger or chat)
        /// </summary>
        public bool Log => LogToConsole || LogToChat;

        /// <summary>
        /// Wether actions should be logged to the logger
        /// (unless you are debugging you shouldn't need this)
        /// </summary>
        [DefaultValue(false)]
        public bool LogToConsole { get; set; } = false;


        /// <summary>
        /// Wether actions should be logged to chat
        /// </summary>
        [DefaultValue(true)]
        public bool LogToChat { get; set; } = true;

        /// <summary>
        /// Wether to play a sound when auto-looting
        /// </summary>
        [DefaultValue(true)]
        public bool Sound {get; set; } = true;

        /// <summary>
        /// How long to wait before checking if the window was somehow opened a second time after the entity was autolooted (in ms)
        /// </summary>
        [DefaultValue(250)]
        public int CheckDubbleWindowOpenIntervalInMs { get; set; } = 250;
    }
}
