using FlexiPLC.Core.Services;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;

// Form1 클래스가 속한 프로젝트의 네임스페이스
namespace FlexiPLC.Dashboard.WinForms
{
    public partial class Form1 : Form
    {
        private PlcManager _plcManager;

        // config.json 파일이 실행 파일과 같은 폴더에 있어야 합니다.
        private readonly string _configFilePath = "config.json";

        public Form1()
        {
            InitializeComponent();

            // PlcManager 인스턴스 초기화
            try
            {
                _plcManager = new PlcManager(_configFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"초기화 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 오류 발생 시 애플리케이션 종료
                Application.Exit();
                return;
            }

            // PlcData의 PropertyChanged 이벤트를 구독하여 UI 업데이트
            _plcManager.PlcData.PropertyChanged += PlcData_PropertyChanged;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_plcManager.Connect())
            {
                _plcManager.StartMonitoring();
                lblConnectionStatus.Text = "연결됨";
                lblConnectionStatus.BackColor = Color.Green;
                MessageBox.Show("PLC 연결 및 모니터링 시작!");
            }
            else
            {
                lblConnectionStatus.Text = "연결 실패";
                lblConnectionStatus.BackColor = Color.Red;
                MessageBox.Show("PLC 연결 실패!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _plcManager.StopMonitoring();
            lblConnectionStatus.Text = "연결 해제";
            lblConnectionStatus.BackColor = Color.Gray;
            MessageBox.Show("PLC 연결 해제 및 모니터링 중지!");
        }

        private void PlcData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // UI 스레드에서 UI 업데이트
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<object, PropertyChangedEventArgs>(PlcData_PropertyChanged), sender, e);
                return;
            }

            // 바인딩된 속성에 따라 UI 컨트롤 업데이트
            switch (e.PropertyName)
            {
                case "MotorStatus":
                    // 예시: MotorStatus를 표시하는 라벨 업데이트
                    if (_plcManager.PlcData["MotorStatus"] is bool motorStatus)
                    {
                        lblMotorStatus.Text = motorStatus ? "ON" : "OFF";
                        lblMotorStatus.BackColor = motorStatus ? Color.Green : Color.Red;
                    }
                    break;
                case "CurrentTemperature":
                    // 예시: CurrentTemperature를 표시하는 라벨 업데이트
                    if (_plcManager.PlcData["CurrentTemperature"] is int temperature)
                    {
                        lblTemperature.Text = temperature.ToString();
                    }
                    break;
                    // config.json에 정의된 다른 항목들도 여기에 추가
            }
        }
    }
}
