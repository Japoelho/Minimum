using System;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;

namespace Minimum.Cross
{
    public class Correios
    {
        #region [ Private ]
        private WsCorreios.AtendeClienteClient _service;
        #endregion

        #region [ Properties ]
        public Exception LastException { get; private set; }
        public ServiceStatus Status { get; private set; } 
        #endregion

        public Correios()
        {
            try
            {
                Binding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                EndpointAddress address = new EndpointAddress("https://apps.correios.com.br/SigepMasterJPA/AtendeClienteService/AtendeCliente");

                _service = new WsCorreios.AtendeClienteClient(binding, address);
                Status = ServiceStatus.Loaded;
            }
            catch (Exception ex)
            {
                LastException = ex;
                Status = ServiceStatus.Error;
            }
        }

        public Address GetAddress(string cep)
        {
            if (cep == null) { return null; }

            string format = Regex.Replace(cep, "[^0-9]", "");

            WsCorreios.enderecoERP endereco = null;
            try
            {
                endereco = _service.consultaCEP(format);                
            }
            catch (Exception ex)
            {
                LastException = ex;
                return null;
            }

            Address address = null;
            if (endereco != null)
            {
                address = new Address();
                address.Street = endereco.end;
                address.District = endereco.bairro;
                address.Number = endereco.complemento;
                address.Street2 = endereco.complemento2;
                address.ZipCode = endereco.cep;
                address.City = endereco.cidade;
                address.State = endereco.uf;
            }

            return address;
        }

        public void GetAddressAsync(string cep, Action<Address> callback)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                e.Result = GetAddress(cep);
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                if (callback != null) { callback.Invoke(e.Result as Address); }
            };
            worker.RunWorkerAsync();
        }
    }

    public class Address
    {
        public string Street { get; set; }
        public string Number { get; set; }
        public string Street2 { get; set; }
        public string District { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }

    public enum ServiceStatus
    {
        Error,
        Loaded
    }
}