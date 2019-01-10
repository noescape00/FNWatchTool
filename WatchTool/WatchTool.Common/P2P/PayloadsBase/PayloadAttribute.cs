using System;

namespace WatchTool.Common.P2P.PayloadsBase
{
    /// <summary>
    /// An attribute that enables mapping between command names and P2P netowrk types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PayloadAttribute : Attribute
    {
        /// <summary>
        /// The command name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initialize a new instance of the object.
        /// </summary>
        /// <param name="commandName"></param>
        public PayloadAttribute(string commandName)
        {
            if (commandName.Length > 12)
                throw new ArgumentException("Protocol violation: command name is limited to 12 characters.");

            this.Name = commandName;
        }
    }
}
