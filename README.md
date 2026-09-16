# Учет кладов для Windows

Небольшое настольное приложение на .NET 8 WinForms для учета плановых и фактически полученных кладов. Интерфейс намеренно выполнен в темно-серой цветовой схеме.

## Возможности

- история всех записей с датой и временем добавления;
- выбор кладмена от 1 до 20;
- ввод веса в граммах;
- разделение записи на **план** и **факт**;
- автоматический расчет суммы по тарифу **350 ₽ за грамм**;
- итоговые плановый и фактический вес и сумма;
- локальное сохранение данных между запусками в `%LocalAppData%\CladTracker\entries.json`.

## Запуск

Требуется Windows и .NET 8 SDK или Runtime с Windows Desktop Runtime:

```powershell
dotnet run
```

## Сборка

Для обычной сборки:

```powershell
dotnet build
```

Для самостоятельного Windows x64-приложения:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Готовый одиночный файл находится в `bin\Release\net8.0-windows\win-x64\publish\CladTracker.exe`.
Он self-contained и не требует установленного .NET Runtime. Для запуска скачайте `CladTracker.exe`
из artifact `CladTracker-windows-x64` на странице workflow GitHub Actions и запустите двойным кликом.
