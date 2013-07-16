namespace IronFoundry.Warden.ProcessIsolation.Service
{
    using System.Collections.Generic;
    using System.ServiceModel;

    [ServiceContract(CallbackContract = typeof(IJobObjectServiceCallback), SessionMode = SessionMode.Required)]
    public interface IJobObjectService
    {
        [OperationContract(IsOneWay = true, IsInitiating = true)]
        void RegisterJobClient(int processID);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void UnregisterJobClient(int processID);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        void SetJobLimits(JobObjectLimits limits);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        void StartProcess(string fileName, string workingDirectory, string args);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        void StopProcess(int processID);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        List<ProcessInfo> ListProcesses();
    }
}
