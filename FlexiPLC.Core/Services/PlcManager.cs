using FlexiPLC.Core.Models;
using FlexiPLC.Core.Services.Plc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FlexiPLC.Core.Services
{
    public class PlcManager
    {
        // PLC 통신 인터페이스를 통한 의존성 주입
        private IPlcCommunicationService _plcService;
        private PlcConfig _config;
        private readonly PlcData _plcData = new PlcData();
        private Timer _readDataTimer;

        public PlcData PlcData => _plcData;

        public PlcManager(string configFilePath)
        {
            // 1. ConfigManager를 사용하여 설정 파일 읽기
            _config = ConfigManager.LoadConfig(configFilePath);

            // 2. PLC 타입에 따라 적절한 통신 서비스 인스턴스 동적 생성 (리플렉션)
            _plcService = CreatePlcServiceInstance(_config.PlcServiceTypeName, _config.ConnectionAddress, _config.ConnectionPort);

            if (_plcService == null)
            {
                throw new InvalidOperationException("지원되지 않는 PLC 타입입니다. config.json의 'PlcServiceTypeName'을 확인하세요.");
            }
        }

        public bool Connect()
        {
            return _plcService.Connect();
        }

        public void StartMonitoring()
        {
            if (_plcService.Connect())
            {
                // 3. 주기적으로 데이터를 읽어오는 타이머 시작
                // 타이머는 주기적으로 람다식(async (state) => await ReadDataFromPlc())을 호출합니다.
                _readDataTimer = new Timer(async (state) => await ReadDataFromPlc(), null, 0, 1000);
            }
        }

        public void StopMonitoring()
        {
            _readDataTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _plcService.Disconnect();
        }

        // 비동기 메서드로 변경하여 UI 스레드 프리징을 방지
        private async Task ReadDataFromPlc()
        {
            try
            {
                // Task.Run을 사용하여 PLC 통신을 별도의 스레드에서 실행
                var newValues = await Task.Run(() => _plcService.ReadData(_config.Items));

                // 데이터 업데이트
                foreach (var kvp in newValues)
                {
                    _plcData[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                // 예외 처리 로직 (로깅 등)
                Console.WriteLine($"데이터 읽기 중 오류 발생: {ex.Message}");
            }
        }

        private IPlcCommunicationService CreatePlcServiceInstance(string plcServiceTypeName, string ipAddress, int port)
        {
            try
            {
                // 1. 문자열로 주어진 클래스 전체 이름을 사용하여 Type 객체 가져오기
                Type serviceType = Type.GetType(plcServiceTypeName);

                if (serviceType == null)
                {
                    throw new InvalidOperationException($"'{plcServiceTypeName}' 타입이 존재하지 않습니다. 어셈블리 이름 또는 전체 이름을 확인하세요.");
                }

                // 2. 해당 타입이 IPlcCommunicationService 인터페이스를 구현하는지 확인
                if (!typeof(IPlcCommunicationService).IsAssignableFrom(serviceType))
                {
                    throw new InvalidOperationException($"'{plcServiceTypeName}' 타입은 IPlcCommunicationService를 구현하지 않았습니다.");
                }

                // 3. 타입에 맞는 생성자를 찾고 인스턴스 생성
                object[] constructorArgs = new object[] { ipAddress, port };
                var instance = Activator.CreateInstance(serviceType, constructorArgs);

                return (IPlcCommunicationService)instance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PLC 서비스 인스턴스 생성 오류: {ex.Message}");
                return null;
            }
        }
    }
}
