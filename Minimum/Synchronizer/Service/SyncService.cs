using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Minimum.Synchronizer.Service
{
    [ServiceContract]
    public interface ISyncService
    {
        [OperationContract]
        string Sync(string data);
    }

    internal class SyncService : ClientBase<ISyncService>, ISyncService
    {
        public SyncService() { }

        public SyncService(string endpointConfigurationName) : base(endpointConfigurationName) { }

        public SyncService(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

        public SyncService(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

        public SyncService(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }

        public string Sync(string data)
        {
            return base.Channel.Sync(data);
        }
    }
}