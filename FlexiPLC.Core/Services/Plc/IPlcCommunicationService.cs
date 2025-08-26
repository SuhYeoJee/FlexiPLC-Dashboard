using System.Collections.Generic;

namespace FlexiPLC.Core.Services
{
    public interface IPlcCommunicationService
    {
        bool Connect();
        void Disconnect();

        Dictionary<string, object> ReadData(List<Models.PlcItem> items);
    }
}