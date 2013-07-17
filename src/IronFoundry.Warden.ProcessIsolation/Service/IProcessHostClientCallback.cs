namespace IronFoundry.Warden.ProcessIsolation.Service
{
    using System.ServiceModel;

    public interface IProcessHostClientCallback
    {
        [OperationContract(IsOneWay = true)]
        void ProcessErrorReceived(int pid, string error);

        [OperationContract(IsOneWay = true)]
        void ProcessOutputReceived(int pid, string output);

        [OperationContract(IsOneWay = true)]
        void ProcessExit(int pid, int exitCode);

        [OperationContract(IsOneWay = true)]
        void ServiceMessageReceived(string message);
    }
}
