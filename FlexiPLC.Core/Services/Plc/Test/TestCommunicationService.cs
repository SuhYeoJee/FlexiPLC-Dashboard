using System;
using System.Collections.Generic;
using FlexiPLC.Core.Models;
using FlexiPLC.Core.Services.Plc;

namespace FlexiPLC.Core.Services.Plc.Test
{
    // 클래스 이름을 변경하여 테스트용임을 명확히 함
    public class TestCommunicationService : IPlcCommunicationService
    {
        private bool _isConnected = false;

        public TestCommunicationService(string ipAddress, int port)
        {
            Console.WriteLine($"테스트용 PLC 클라이언트 초기화: {ipAddress}:{port}");
        }

        public bool Connect()
        {
            // 테스트를 위해 항상 true를 반환
            _isConnected = true;
            Console.WriteLine("테스트용 PLC에 연결되었습니다.");
            return true;
        }

        public void Disconnect()
        {
            _isConnected = false;
            Console.WriteLine("테스트용 PLC 연결이 해제되었습니다.");
        }

        public Dictionary<string, object> ReadData(List<PlcItem> items)
        {
            if (!_isConnected)
            {
                Console.WriteLine("PLC가 연결되지 않았습니다. 데이터를 읽을 수 없습니다.");
                return new Dictionary<string, object>();
            }

            var data = new Dictionary<string, object>();

            foreach (var item in items)
            {
                object value = GetDummyValue(item.DataType);
                data[item.Name] = value;

                Console.WriteLine($"데이터 읽기 - 이름: {item.Name}, 주소: {item.Address}, 값: {value}");
            }
            return data;
        }

        private object GetDummyValue(string dataType)
        {
            // 데이터 타입에 따라 가상의 값을 반환하는 헬퍼 메서드
            switch (dataType.ToLower())
            {
                case "bool":
                    return true;
                case "int":
                    Random rand = new Random();
                    return rand.Next(1, 101);
                default:
                    return null;
            }
        }
    }
}