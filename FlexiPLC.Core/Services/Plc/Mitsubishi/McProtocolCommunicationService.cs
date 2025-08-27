using FlexiPLC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
// MCProtocol의 Mitsubishi 네임스페이스에 대한 별칭 사용
using McMitsubishi = MCProtocol.Mitsubishi;

namespace FlexiPLC.Core.Services.Plc.Mitsubishi
{
    /// <summary>
    /// MCProtocol 미쓰비시 PLC용 IPlcCommunicationService 구현체
    /// </summary>
    public class McProtocolCommunicationService : IPlcCommunicationService
    {
        // PLC 연결의 기본으로 Plc 인터페이스를 사용합니다.
        private McMitsubishi.Plc _plc;
        private readonly string _ipAddress;
        private readonly int _port;

        /// <summary>
        /// IP 주소와 포트 번호를 주입하는 생성자
        /// </summary>
        /// <param name="ipAddress">PLC의 IP 주소</param>
        /// <param name="port">PLC의 포트 번호</param>
        public McProtocolCommunicationService(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
        }

        /// <summary>
        /// PLC에 연결합니다.
        /// </summary>
        /// <returns>연결 성공 시 true, 실패 시 false</returns>
        public bool Connect()
        {
            try
            {
                // McProtocolTcp 인스턴스를 생성하고 동기적으로 연결을 엽니다.
                var mcTcpApp = new McMitsubishi.McProtocolTcp(_ipAddress, _port, McMitsubishi.McFrame.MC3E);
                Task.Run(async () => await mcTcpApp.Open()).Wait();
                _plc = mcTcpApp;

                return _plc.Connected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PLC 연결 실패: {ex.Message}");
                _plc?.Close();
                return false;
            }
        }

        /// <summary>
        /// PLC와의 연결을 끊습니다.
        /// </summary>
        public void Disconnect()
        {
            if (_plc != null && _plc.Connected)
            {
                _plc.Close();
            }
        }

        /// <summary>
        /// PlcItem 목록을 기반으로 PLC에서 데이터를 읽습니다.
        /// </summary>
        /// <param name="items">읽을 데이터 항목 목록</param>
        /// <returns>항목 이름을 키로 하여 읽은 데이터가 포함된 딕셔너리</returns>
        public Dictionary<string, object> ReadData(List<PlcItem> items)
        {
            var data = new Dictionary<string, object>();

            if (_plc == null || !_plc.Connected)
            {
                Console.WriteLine("PLC에 연결되어 있지 않습니다.");
                return data;
            }

            try
            {
                foreach (var item in items)
                {
                    // 문자열에서 장치 유형과 주소를 파싱합니다.
                    var (deviceType, address) = ParsePlcAddress(item.Address);

                    // 비동기적으로 데이터를 읽고 결과를 기다립니다.
                    byte[] readBytes;

                    switch (item.DataType.ToLower())
                    {
                        case "bool":
                            // 단일 비트를 읽습니다. 라이브러리는 단일 비트를 위해 최소 1워드(2바이트)를 읽는 것으로 보
                            // 부울 값은 첫 번째 바이트에 있다고 가정합니다.
                            readBytes = Task.Run(async () => await _plc.ReadDeviceBlock(deviceType, address, 1)).Result;
                            if (readBytes.Length > 0)
                            {
                                data[item.Name] = (readBytes[0] & 0x1) == 1;
                            }
                            break;
                        case "int":
                        case "int16":
                            // 16비트 부호 있는 정수(2바이트)를 읽습니다.
                            readBytes = Task.Run(async () => await _plc.ReadDeviceBlock(deviceType, address, 2)).Result;
                            if (readBytes.Length >= 2)
                            {
                                data[item.Name] = BitConverter.ToInt16(readBytes, 0);
                            }
                            break;
                        case "uint16":
                            // 16비트 부호 없는 정수(2바이트)를 읽습니다.
                            readBytes = Task.Run(async () => await _plc.ReadDeviceBlock(deviceType, address, 2)).Result;
                            if (readBytes.Length >= 2)
                            {
                                data[item.Name] = BitConverter.ToUInt16(readBytes, 0);
                            }
                            break;
                        case "int32":
                            // 32비트 부호 있는 정수(4바이트)를 읽습니다.
                            readBytes = Task.Run(async () => await _plc.ReadDeviceBlock(deviceType, address, 4)).Result;
                            if (readBytes.Length >= 4)
                            {
                                data[item.Name] = BitConverter.ToInt32(readBytes, 0);
                            }
                            break;
                        case "single":
                            // 32비트 부동 소수점 숫자(4바이트)를 읽습니다.
                            readBytes = Task.Run(async () => await _plc.ReadDeviceBlock(deviceType, address, 4)).Result;
                            if (readBytes.Length >= 4)
                            {
                                data[item.Name] = BitConverter.ToSingle(readBytes, 0);
                            }
                            break;
                        case "double":
                            // 64비트 부동 소수점 숫자(8바이트)를 읽습니다.
                            readBytes = Task.Run(async () => await _plc.ReadDeviceBlock(deviceType, address, 8)).Result;
                            if (readBytes.Length >= 8)
                            {
                                data[item.Name] = BitConverter.ToDouble(readBytes, 0);
                            }
                            break;
                        case "string":
                            // 문자열을 읽습니다. 이 코드는 고정 길이를 가정합니다.
                            // PlcItem에 길이 속성이 필요할 수 있습니다. 여기서는 20자(40바이트)를 읽습니다.
                            const int stringLengthBytes = 40; // 20 characters * 2 bytes/char (MCProtocol의 경우)
                            readBytes = Task.Run(async () => await _plc.ReadDeviceBlock(deviceType, address, stringLengthBytes)).Result;
                            if (readBytes.Length >= stringLengthBytes)
                            {
                                data[item.Name] = Encoding.ASCII.GetString(readBytes).TrimEnd('\0'); // 널 종결자 제거
                            }
                            break;
                        default:
                            Console.WriteLine($"지원되지 않는 데이터 형식: {item.DataType}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"데이터 읽기 중 오류 발생: {ex.Message}");
            }

            return data;
        }

        /// <summary>
        /// 간단한 PLC 주소 문자열(예: "D100", "M0")을 장치 유형과 주소로 파싱합니다.
        /// </summary>
        /// <param name="addressString">config.json의 주소 문자열</param>
        /// <returns>PlcDeviceType과 주소 정수(int)를 포함하는 튜플</returns>
        private (McMitsubishi.PlcDeviceType, int) ParsePlcAddress(string addressString)
        {
            if (string.IsNullOrEmpty(addressString) || addressString.Length < 2)
            {
                throw new FormatException("잘못된 주소 형식");
            }

            // 첫 번째 문자를 장치 유형으로 추출합니다.
            char deviceChar = addressString[0];
            // 나머지 문자열을 주소 번호로 추출합니다.
            string addressNumberString = addressString.Substring(1);

            if (!int.TryParse(addressNumberString, out int address))
            {
                throw new FormatException($"주소 번호가 유효한 정수가 아닙니다: {addressNumberString}");
            }

            switch (deviceChar)
            {
                case 'D':
                    return (McMitsubishi.PlcDeviceType.D, address);
                case 'M':
                    return (McMitsubishi.PlcDeviceType.M, address);
                case 'X':
                    return (McMitsubishi.PlcDeviceType.X, address);
                case 'Y':
                    return (McMitsubishi.PlcDeviceType.Y, address);
                case 'W':
                    return (McMitsubishi.PlcDeviceType.W, address);
                case 'B':
                    return (McMitsubishi.PlcDeviceType.B, address);
                case 'L':
                    return (McMitsubishi.PlcDeviceType.L, address);
                case 'F':
                    return (McMitsubishi.PlcDeviceType.F, address);
                case 'V':
                    return (McMitsubishi.PlcDeviceType.V, address);
                case 'Z':
                    return (McMitsubishi.PlcDeviceType.Z, address);
                default:
                    throw new NotSupportedException($"지원되지 않는 PLC 장치 유형: {deviceChar}");
            }
        }
    }
}
