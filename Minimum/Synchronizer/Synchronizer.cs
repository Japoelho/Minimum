using Minimum.Synchronizer.Model;
using Minimum.Synchronizer.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Minimum.Synchronizer
{
    public class Synchronizer
    {
        public Exception LastException { get; private set; }
        public ISynchronizer SyncWorker { private get; set; }
        
        public string ServiceURL { get; set; }
        public long PacketMaxBufferSize { get; set; }

        private Binding _binding;
        private BackgroundWorker _syncWorker;

        public Synchronizer()
        {
            PacketMaxBufferSize = 1048576;

            _binding = new BasicHttpBinding() { SendTimeout = TimeSpan.FromMinutes(2), MaxReceivedMessageSize = PacketMaxBufferSize, MaxBufferSize = Convert.ToInt32(PacketMaxBufferSize) };
        }

        public bool Synchronize<T>(Action<int, object> reportProgress = null, params object[] parameters) where T : class
        {
            if (SyncWorker == null)
            {
                LastException = new InvalidOperationException("The synchronization interface is not defined.");
                return false;
            }

            LastException = null;

            bool hasMore = false;
            IList<T> records = null;
            do
            {
                if (_syncWorker != null && _syncWorker.CancellationPending) { return false; }

                records = SyncWorker.GetRecords<T>(ref hasMore, parameters);

                if (records != null && records.Count > 0)
                {
                    IList<T> updated = Update<T>(records, parameters);

                    if (updated == null) { return false; }

                    for (int i = 0; i < updated.Count; i++)
                    { if (!SyncWorker.SetRecord<T>(updated[i], true, parameters)) { LastException = new Exception(SyncWorker.ErrorMessage); return false; } }                    
                }
            }
            while (hasMore == true);

            records = new List<T>();
            bool working = true;

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += (s, e) =>
            {
                bool serverHasMore = false;
                IList<T> newRecords = null;
                do
                {
                    if (worker.CancellationPending) { return; }

                    newRecords = Request<T>(ref serverHasMore, parameters);

                    if (newRecords == null) { break; }

                    for (int i = 0; i < newRecords.Count; i++) { records.Add(newRecords[i]); }
                }
                while (serverHasMore == true);
                
                working = false;
            };
            worker.RunWorkerAsync();

            while (true)
            {
                if (_syncWorker != null && _syncWorker.CancellationPending) { worker.CancelAsync(); return false; }

                if (records.Count == 0)
                {
                    if (working == true) { continue; }
                    break;
                }

                if (!SyncWorker.SetRecord<T>(records[0], false, parameters)) { LastException = new Exception(SyncWorker.ErrorMessage); return false; }
                if (reportProgress != null) { reportProgress.Invoke(0, records[0]); }

                records.RemoveAt(0);
            }

            return LastException == null;
        }

        public void SynchronizeAsync<T>(Action<bool> callback = null, Action<int, object> reportProgress = null, params object[] parameters) where T : class
        {
            _syncWorker = new BackgroundWorker();
            _syncWorker.DoWork += (s, e) =>
            {
                e.Result = Synchronize<T>(reportProgress, parameters);
            };
            _syncWorker.ProgressChanged += (s, e) =>
            {
                reportProgress.Invoke(e.ProgressPercentage, e.UserState);
            };
            _syncWorker.RunWorkerCompleted += (s, e) =>
            {
                if (callback != null) { callback.Invoke((bool)e.Result); }
            };
            _syncWorker.WorkerReportsProgress = reportProgress != null;
            _syncWorker.WorkerSupportsCancellation = true;
            _syncWorker.RunWorkerAsync();
        }

        public void CancelAsync()
        {
            if (_syncWorker != null) { _syncWorker.CancelAsync(); }
        }

        public Custom Action(Custom custom)
        {
            Packet packet = new Packet();
            packet.Type = PacketType.Custom;
            packet.Data = Serializer.JSON.Load(custom);

            Packet response = Send(packet);
            if (response == null) { return null; }
            if (response.Type == PacketType.Error) { LastException = new Exception(response.Data); return null; }

            Custom received = Serializer.JSON.Load<Custom>(response.Data);

            return received;
        }

        public void ActionAsync(Custom custom, Action<Custom> callback = null)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                e.Result = Action(custom);
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                if (callback != null) { callback.Invoke(e.Result as Custom); }
            };
            worker.RunWorkerAsync();
        }

        #region [ Online ]
        private IList<T> Request<T>(ref bool hasMore, params object[] parameters) where T : class
        {
            Request request = new Request();
            request.Type = typeof(T).FullName + ", " + typeof(T).Assembly.GetName().Name;
            request.Parameters = parameters;

            Packet packet = new Packet();
            packet.Type = PacketType.Request;
            packet.Data = Serializer.JSON.Load(request);

            hasMore = false;

            packet = Send(packet);
            if (packet == null) { return null; }
            if (packet.Type == PacketType.Error) { LastException = new Exception(packet.Data); return null; }

            Response response = Serializer.JSON.Load<Response>(packet.Data);            
            hasMore = response.HasMore;

            for (int i = 0; i < response.Parameters.Length; i++) { parameters[i] = response.Parameters[i]; }
            IList<T> updated = Serializer.JSON.Load<List<T>>(response.Data);

            return updated;
        }

        private IList<T> Update<T>(IList<T> records, params object[] parameters) where T : class
        {
            Update update = new Update();
            update.Type = typeof(T).FullName + ", " + typeof(T).Assembly.GetName().Name;
            update.Data = Serializer.JSON.Load(records);
            update.Parameters = parameters;

            Packet packet = new Packet();
            packet.Type = PacketType.Update;
            packet.Data = Serializer.JSON.Load(update);

            Packet response = Send(packet);
            if (response == null) { return null; }
            if (response.Type == PacketType.Error) { LastException = new Exception(response.Data); return null; }
            
            IList<T> updated = Serializer.JSON.Load<List<T>>(response.Data);

            return updated;
        }        
        #endregion

        #region [ Private ]
        private Packet Send(Packet packet)
        {
            SyncService serviceProvider = null;
            try
            {
                EndpointAddress address = new EndpointAddress(ServiceURL);

                serviceProvider = new SyncService(_binding, address);
            }
            catch (Exception ex)
            {
                LastException = new InvalidOperationException("Error configuring the service with the information provided. Check the inner exception for details.", ex);
                return null;
            }

            string send = Serializer.JSON.Load(packet);
            string receive = null;
            bool status = true;

            try
            {
                receive = serviceProvider.Execute(send);
            }
            catch (Exception ex)
            {
                LastException = ex;
                status = false;
            }
            finally
            {
                serviceProvider.Close();
            }

            if (!status) { return null; }

            Packet response = null;
            try
            {
                response = Serializer.JSON.Load<Packet>(receive);
            }
            catch (Exception ex)
            {
                LastException = new InvalidCastException("The server returned invalid data.", ex);
            }

            return response;
        }

        private bool SetRecords<T>(IList<T> records, object[] parameters) where T : class
        {
            for (int i = 0; i < records.Count; i++)
            {
                if (!SyncWorker.SetRecord<T>(records[i], false, parameters)) { return false; }
            }

            return true;
        }
        #endregion

        #region [ Server ]
        public string Execute(string data)
        {
            Packet packet = null;
            try
            {
                packet = Serializer.JSON.Load<Packet>(data);
            }
            catch (Exception ex)
            {
                packet = new Packet();                
                packet.Type = PacketType.Error;
                packet.Data = ex.Message;
            }

            switch (packet.Type)
            {
                case PacketType.Request:
                    {
                        if (SyncWorker == null)
                        {
                            packet.Type = PacketType.Error;
                            packet.Data = "The service synchronization interface is not defined.";
                            break;
                        }

                        Request request = Serializer.JSON.Load<Request>(packet.Data);

                        Type dataType = Type.GetType(request.Type);
                        if (dataType == null)
                        {
                            packet.Type = PacketType.Error;
                            packet.Data = "The service was unable to resolve the requested type from the assembly.";
                            break;
                        }

                        MethodInfo genericMethod = SyncWorker.GetType().GetMethod("GetRecords").MakeGenericMethod(dataType);
                        
                        Response response = new Response();
                        try
                        {
                            object[] arguments = new object[] { response.HasMore, request.Parameters };
                            object result = genericMethod.Invoke(SyncWorker, arguments);

                            response.HasMore = (bool)arguments[0];
                            response.Parameters = (object[])arguments[1];

                            if (result != null) { response.Data = Serializer.JSON.Load(result); }
                        }
                        catch (Exception ex)
                        {
                            packet.Type = PacketType.Error;
                            packet.Data = "The service synchronization interface threw an exception: " + ex.Message;
                            break;
                        }

                        if (response.Data == null)
                        {
                            packet.Type = PacketType.Error;
                            packet.Data = SyncWorker.ErrorMessage;
                        }
                        else
                        {
                            packet.Type = PacketType.Success;
                            packet.Data = Serializer.JSON.Load(response);
                        }
                        break;
                    }
                case PacketType.Update:
                    {
                        if (SyncWorker == null)
                        {
                            packet.Type = PacketType.Error;
                            packet.Data = "The service synchronization interface is not defined.";
                            break;
                        }

                        Update update = Serializer.JSON.Load<Update>(packet.Data);

                        Type dataType = Type.GetType(update.Type);
                        if (dataType == null)
                        {
                            packet.Type = PacketType.Error;
                            packet.Data = "The service was unable to create the requested type from the assembly.";
                        }

                        Type listType = typeof(List<>).MakeGenericType(dataType);

                        MethodInfo deserializer = typeof(Serializer.JSON).GetMethod("Load", new Type[] { typeof(String) }).MakeGenericMethod(listType);
                        object list = deserializer.Invoke(this, new object[] { update.Data });

                        MethodInfo genericMethod = this.GetType().GetMethod("SetRecords", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(dataType);

                        bool response = true;
                        try
                        {
                            response = (bool)genericMethod.Invoke(this, new object[] { list, update.Parameters });
                        }
                        catch (Exception ex)
                        {
                            packet.Type = PacketType.Error;
                            packet.Data = "The service synchronization interface threw an exception: " + ex.Message;
                            break;
                        }

                        if (response == true)
                        {
                            packet.Type = PacketType.Success;
                            packet.Data = Serializer.JSON.Load(list);
                        }
                        else
                        {
                            packet.Type = PacketType.Error;
                            packet.Data = SyncWorker.ErrorMessage;                            
                        }
                        break;
                    }
                case PacketType.Custom:                    
                    {
                        Custom custom = Serializer.JSON.Load<Custom>(packet.Data);

                        if (SyncWorker == null)
                        {
                            packet.Type = PacketType.Error;
                            packet.Data = "The service synchronization interface is not defined.";
                            break;
                        }

                        Custom result = null;
                        try
                        {
                            result = SyncWorker.Action(custom);                            
                        }
                        catch (Exception ex)
                        {
                            packet.Type = PacketType.Error;
                            packet.Data = "The service synchronization interface threw an exception: " + ex.Message;
                            break;
                        }

                        if (result == null)
                        {
                            packet.Type = PacketType.Error;
                            packet.Data = SyncWorker.ErrorMessage;
                        }
                        else
                        {
                            packet.Type = PacketType.Success;
                            packet.Data = Serializer.JSON.Load(result);
                        }
                        break;
                    }
            }

            return Serializer.JSON.Load(packet);
        }
        #endregion
    }
}