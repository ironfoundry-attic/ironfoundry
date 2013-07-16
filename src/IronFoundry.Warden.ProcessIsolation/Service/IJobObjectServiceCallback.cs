namespace IronFoundry.Warden.ProcessIsolation.Service
{
    using System.ServiceModel;

    public interface IJobObjectServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void ProcessErrorReceived(string error);

        [OperationContract(IsOneWay = true)]
        void ProcessOutputReceived(string output);

        [OperationContract(IsOneWay = true)]
        void ProcessExit(int exitCode);

        [OperationContract(IsOneWay = true)]
        void ServiceMessageReceived(string message);
    }
}
