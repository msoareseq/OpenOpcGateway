using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using OpcUaApi.Models;

namespace OpcUaApi.Drivers
{
    public class OpcUaClient : IOpcUaClient, IDisposable
    {

        ApplicationInstance application;

        const int ReconnectPeriod = 10;
        Session? session;
        SessionReconnectHandler? reconnectHandler;
        string endpointURL;
        int clientRunTime = Timeout.Infinite;
        static bool autoAccept = false;


        public OpcUaClient(string EndpointURL, bool AutoAccept, int StopTimeout)
        {
            endpointURL = EndpointURL;
            autoAccept = AutoAccept;
            clientRunTime = StopTimeout <= 0 ? Timeout.Infinite : StopTimeout * 1000;

            application = new ApplicationInstance
            {
                ApplicationName = "OPC UA API",
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = "OpcUaClient"
            };
        }

        public async void StartClient()
        {
            // Loading Configuration
            ApplicationConfiguration config = await application.LoadApplicationConfiguration(Path.Combine(AppContext.BaseDirectory, "OpcUaClient.Config.xml"), false);

            // Managing certificates
            bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);
            if (!haveAppCertificate)
            {
                throw new Exception("Application instance certificate invalid!");
            }

            if (haveAppCertificate)
            {
                config.ApplicationUri = X509Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate.Certificate);
                if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    autoAccept = true;
                }
                config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
            }
            else
            {
                Console.WriteLine("    WARN: missing application certificate, using unsecure connection.");
            }

            var selectedEndpoint = CoreClientUtils.SelectEndpoint(endpointURL, haveAppCertificate, 15000);
            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
            session = await Session.Create(config, endpoint, false, "OPC UA Console Client", 60000, new UserIdentity(new AnonymousIdentityToken()), null);
            session.KeepAlive += Client_KeepAlive;

            Console.WriteLine("OPC UA Session created!");
        }

        private static void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == Opc.Ua.StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = autoAccept;
                if (autoAccept)
                {
                    Console.WriteLine("Accepted Certificate: {0}", e.Certificate.Subject);
                }
                else
                {
                    Console.WriteLine("Rejected Certificate: {0}", e.Certificate.Subject);
                }
            }
        }

        private void Client_KeepAlive(Opc.Ua.Client.ISession session, KeepAliveEventArgs e)
        {
            if (e.Status != null && ServiceResult.IsNotGood(e.Status))
            {
                if (reconnectHandler == null)
                {
                    reconnectHandler = new SessionReconnectHandler();
                    reconnectHandler.BeginReconnect(session, ReconnectPeriod * 1000, Client_ReconnectComplete!);
                }
            }
        }

        private void Client_ReconnectComplete(object sender, EventArgs e)
        {
            // ignore callbacks from discarded objects.
            if (!ReferenceEquals(sender, reconnectHandler))
            {
                return;
            }

            session = (Session?)reconnectHandler.Session;
            reconnectHandler.Dispose();
            reconnectHandler = null;
        }

        public void StopClient()
        {
            Dispose();
        }

        public void Dispose()
        {
            session?.Close();
            session?.Dispose();
            session = null;
        }

        public BrowseResults Browse()
        {
            throw new NotImplementedException();
        }

        public ReadResults? GetNodesValues(string[] nodesIds)
        {
            try
            {
                if (session == null) return null;

                ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
                foreach (string n in nodesIds)
                {
                    nodesToRead.Add(new ReadValueId() { NodeId = n, AttributeId = Attributes.Value });
                }

                session.Read(null,
                             0,
                             TimestampsToReturn.Both,
                             nodesToRead,
                             out DataValueCollection resultValues,
                             out DiagnosticInfoCollection diagnosticValues);

                ReadResults readResults = new ReadResults();
                for (int i = 0; i < nodesIds.Length; i++)
                {
                    ReadResult readResult = new ReadResult();
                    readResult.Id = nodesIds[i];
                    readResult.Value = resultValues[i].Value;
                    readResult.Timestamp = new DateTimeOffset(resultValues[i].SourceTimestamp).ToUnixTimeSeconds();
                    readResult.Succeed = StatusCode.IsGood(resultValues[i].StatusCode);
                    readResults.readResults.Add(readResult);
                }

                return readResults;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public ReadResults? GetNodeValue(string nodeId)
        {
            string[] nodesIds = new string[] { nodeId };
            var results = GetNodesValues(nodesIds);
            return results;
        }

        public WriteResults? SetNodesValues(string[] nodeIds, object[] values)
        {
            try
            {
                if (session == null || nodeIds.Length != values.Length) return null;

                WriteValueCollection writeValues = new WriteValueCollection();

                for (int i = 0; i < nodeIds.Length; i++)
                {
                    NodeId writeNode = new NodeId(nodeIds[i]);

                    var actualValue = session.ReadValue(writeNode);
                    Type writeType = actualValue.Value.GetType();
                                        
                    bool isNumber = double.TryParse(values[i].ToString(), out double numberValue);
                                                            
                    DataValue writeDataValue = new DataValue();
                    
                    writeDataValue.Value = Convert.ChangeType(isNumber ? numberValue : values[i].ToString(), writeType);
                                        
                    writeValues.Add(new WriteValue()
                    {
                        NodeId = writeNode,
                        AttributeId = Attributes.Value,
                        Value = writeDataValue
                    });
                }

                session.Write(null, writeValues, out StatusCodeCollection writeResults, out DiagnosticInfoCollection diagnosticInfos);

                WriteResults results = new WriteResults();
                for (int i = 0; i < nodeIds.Length; i++)
                {
                    results.writeResults.Add(new WriteResult()
                    {
                        Id = nodeIds[i],
                        Succeed = StatusCode.IsGood(writeResults[0]),
                        Reason = ""
                    });
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
