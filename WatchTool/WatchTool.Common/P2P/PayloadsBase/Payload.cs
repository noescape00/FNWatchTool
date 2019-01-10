using System.Reflection;

namespace WatchTool.Common.P2P.PayloadsBase
{
    public class Payload
    {
        public virtual string Command
        {
            get
            {
                return this.GetType().GetCustomAttribute<PayloadAttribute>().Name;
            }
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }
    }
}
