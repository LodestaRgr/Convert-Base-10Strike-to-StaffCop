Convertor Base_10Strike to StaffCop
===========================================

Конвертор базы 10-Страйк: Инвентаризация Компьютеров (inventoryconfig*.zip и inventorybase*.zip) в базу XML формата для StaffCop

Установка:
----------
Компиляция C# (.cs) файла возможна на любом ПК с предустановленной .NET Framework 3.5 или выше
```
SET FWPATH=%windir%\Microsoft.Net\Framework\v3.5
%FWPATH%\csc.exe /out:Convert_Base_10Strike_to_StaffCop.exe /t:exe /recurse:"Convert Base 10Strike to StaffCop\*.cs"
```

Инструкция по использованию:
----------------------

- Сделайте бэкап базы 10-Страйк: Инвентаризация Компьютеров
  в папку (например: <strong>Base_10Strike</strong>)
  - Сервис и настройка:
    - Создать архив файлов конфигурации
    - Создать архив всей базы
- В консоли запустить программу с указанием входящий и исходящий папок:
Например:
```
Convert_Base_10Strike_to_StaffCop.exe .\Base_10Strike\ .\Base_StaffCop\
```
