namespace IronFoundry.Warden.ProcessIsolation.Service
{
    using System.Collections.Generic;
    using System.ServiceModel;

    [ServiceContract(CallbackContract = typeof(IProcessHostClientCallback), SessionMode = SessionMode.Required)]
    public interface IProcessHostService
    {
        [OperationContract(IsOneWay = true, IsInitiating = true)]
        void RegisterClient(int processID);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void UnregisterClient(int processID);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        void SetProcessLimits(int processID, ResourceLimits limits);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        void SetJobLimits(ResourceLimits limits);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        int StartProcess(string fileName, string workingDirectory, string args);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        void StopProcess(int processID);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        List<ProcessInfo> ListProcesses();
    }
}
