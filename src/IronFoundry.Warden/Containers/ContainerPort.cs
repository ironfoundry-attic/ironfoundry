namespace IronFoundry.Warden.Containers
{
    using System;
    using Utilities;

    public class ContainerPort : IEquatable<ContainerPort>
    {
        private readonly ushort port;

        public ContainerPort(ushort suggestedPort, ContainerUser user)
        {
            var localTcpPortManager = new LocalTcpPortManager();
            this.port = localTcpPortManager.ReserveLocalPort(suggestedPort, user);
        }

        public void Delete()
        {
            var localTcpPortManager = new LocalTcpPortManager();
            localTcpPortManager.ReleaseLocalPort(port);
        }

        public static void CleanUp(string handle)
        {
            // TODO
        }

        public static implicit operator string(ContainerPort containerPort)
        {
            return containerPort.port.ToString();
        }

        public static implicit operator ushort(ContainerPort containerPort)
        {
            return containerPort.port;
        }

        public static bool operator ==(ContainerPort x, ContainerPort y)
        {
            if (Object.ReferenceEquals(x, null))
            {
                return Object.ReferenceEquals(y, null);
            }
            return x.Equals(y);
        }

        public static bool operator !=(ContainerPort x, ContainerPort y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return port.ToString();
        }

        public override int GetHashCode()
        {
            return port.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ContainerPort);
        }

        public bool Equals(ContainerPort other)
        {
            if (Object.ReferenceEquals(null, other))
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            return this.GetHashCode() == other.GetHashCode();
        }
    }
}
