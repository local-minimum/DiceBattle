using UnityEngine;
using System.Reflection;

namespace DeCrawl.Utils
{
    public class WithActionAttribute : PropertyAttribute
    {        
        public string callback { get; protected set; }
        public string name { get; protected set; }
        public string tooltip { get; protected set; }
        public float split { get; protected set; }

        public BindingFlags bindingFlags { get; protected set; }

        /// <summary>
        /// Adds a button that invokes the callback when clicked
        /// </summary>
        /// <param name="callback">Name of the method on the class</param>
        /// <param name="name">Text in the button</param>
        /// <param name="split">How large part of the width the button should take</param>
        /// <param name="bindingFlags">Where to inspect for the method</param>
        public WithActionAttribute(
            string callback, 
            string name = "Set", 
            float split = 0.85f,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        )
        {
            this.callback = callback;
            this.name = name;
            this.split = split;
            this.bindingFlags = bindingFlags;
        }

        /// <summary>
        /// Adds a button that invokes the callback when clicked
        /// </summary>
        /// <param name="callback">Name of the method on the class</param>
        /// <param name="name">Text in the button</param>
        /// <param name="tooltip">Tooltip for the button</param>
        /// <param name="split">How large part of the width the button should take</param>
        /// <param name="bindingFlags">Where to inspect for the method</param>
        public WithActionAttribute(
            string callback,
            string name,
            string tooltip,
            float split = 0.85f,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        )
        {
            this.callback = callback;
            this.name = name;
            this.split = split;
            this.bindingFlags = bindingFlags;
            this.tooltip = tooltip;
        }
    }
}