# FlexiPLC-Dashboard

1. 새 프로그램 작성, 모니터링 항목 추가: config.json 수정
2. 새 PLC 추가: FlexiPLC.Core/Services/Plc에 구현체를 추가, config.json[PlcServiceTypeName] 변경.
3. Form에 표시: Form1.PlcData_PropertyChanged 스위치 케이스문 수정