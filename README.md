Convertor Base_10Strike to StaffCop
===========================================

��������� ���� 10-������: �������������� ����������� (inventoryconfig*.zip � inventorybase*.zip) � ���� XML ������� ��� StaffCop

���������:
----------
���������� C# (.cs) ����� �������� �� ����� �� � ����������������� .NET Framework 3.5 ��� ����
```
SET FWPATH=%windir%\Microsoft.Net\Framework\v3.5
%FWPATH%\csc.exe /out:Convert_Base_10Strike_to_StaffCop.exe /t:exe /recurse:"Convert Base 10Strike to StaffCop\*.cs"
```

���������� �� �������������:
----------------------

- �������� ����� ���� 10-������: �������������� �����������
  � ����� (��������: <strong>Base_10Strike</strong>)
  - ������ � ���������:
    - ������� ����� ������ ������������
    - ������� ����� ���� ����
- � ������� ��������� ��������� � ��������� �������� � ��������� �����:
��������:
```
Convert_Base_10Strike_to_StaffCop.exe .\Base_10Strike\ .\Base_StaffCop\
```
