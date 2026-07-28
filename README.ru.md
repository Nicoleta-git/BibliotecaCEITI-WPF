<div align="center">

# BibliotecaCEITI

[English](README.md) · [Română](README.ro.md) · **Русский**

Настольное приложение, которое заменяет бумажные журналы школьной библиотеки CEITI одной
программой для Windows: книги, ученики, выдачи, брони, уведомления о просрочках и отчёты.

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512bd4?style=for-the-badge&logo=.net)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp)
![WPF](https://img.shields.io/badge/WPF-0078d4?style=for-the-badge&logo=windows)
![MySQL](https://img.shields.io/badge/MySQL-4479a1?style=for-the-badge&logo=mysql)
![Gemini AI](https://img.shields.io/badge/Gemini_AI-4285f4?style=for-the-badge&logo=google-gemini)

[![.NET Core Desktop](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/dotnet-desktop.yml)
[![CI](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/ci.yml/badge.svg)](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/ci.yml)
[![CodeQL](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/codeql.yml/badge.svg)](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/codeql.yml)

<img width="1536" alt="Демонстрация приложения" src="https://github.com/user-attachments/assets/959fd07b-1430-4950-94ad-d8e45f6a8714" />

</div>

---

## Зачем нужен этот проект

Школьная библиотека до сих пор работает на бумаге: одна тетрадь для выдачи книг, другая —
для учебников, которые раздают в начале года, и никакой возможности узнать, кто задержал
книгу, кроме как перелистать всё вручную.

BibliotecaCEITI собирает всё это в одном месте. Библиотекарь открывает одно окно и видит
весь фонд, оформляет выдачу за несколько кликов, отправляет напоминание всем ученикам,
которые просрочили возврат, и выгружает отчёт в конце семестра. Библиотекарь нашей школы
уже протестировала приложение, и оно готово к использованию в масштабах всей школы.

## Возможности

**Фонд**
- Каталог книг: название, автор, издательство, категория, язык, год, обложка и цены
- Несколько экземпляров одного издания, у каждого свой инвентарный номер
- Архивирование вместо удаления и отдельная процедура списания с указанием причины (повреждение, утеря, передача, устаревание)

**Ученики**
- Карточка ученика: IDNP, группа, телефон, e-mail, дата рождения и заметки
- QR-код для каждого ученика, чтобы билет можно было отсканировать, а не вводить вручную
- По каждому ученику видно, какие книги у него на руках и какие забронированы

**Выдача книг**
- Пошаговый мастер: найти ученика, найти книгу, выбрать срок (14 / 21 / 30 дней), подтвердить
- Массовый режим для учебников — выдать целый комплект всей группе за одну операцию
- Возврат с подтверждением, фильтры по группе и по статусу

**Брони**
- Очередь ожидания на издание, которое сейчас на руках
- Ученик получает уведомление автоматически, как только экземпляр вернули

**Уведомления о просрочках**
- Список просрочек с числом дней, с фильтром по степени (1–4 дня, больше 5 дней)
- Письма выбранным ученикам и история уже отправленных уведомлений

**Панель и отчёты**
- Итоги по книгам: всего, доступно, выдано, забронировано
- График выдач за последние месяцы и топ самых читаемых книг
- Отчёты по активности выдач, самым читаемым книгам, читателям с просрочками и новым поступлениям, с экспортом в PDF

**Дополнительно**
- Помощник по книгам на базе Google Gemini — можно задать вопрос о конкретном издании
- Вход по логину и паролю (пароли хешируются BCrypt) или через аккаунт Google
- Интерфейс на английском, румынском и русском, переключается прямо в приложении
- Четыре темы: Light, Dark, Emerald и Red-Blue

## Технологии

| Уровень | Используется |
| --- | --- |
| Язык / среда | C#, .NET 8.0 (`net8.0-windows`) |
| Интерфейс | WPF, собственные темы и стили, иконки FontAwesome5 |
| База данных | MySQL (`MySql.Data`), поставляется вместе с приложением |
| Графики | LiveChartsCore.SkiaSharpView.WPF |
| AI | DotnetGeminiSDK (`gemini-3.1-flash-lite`) |
| Аутентификация | BCrypt.Net-Next, Google.Apis.Auth, System.IdentityModel.Tokens.Jwt |
| Почта | MailKit / MimeKit по SMTP со StartTls |
| Прочее | QRCoder |

## Установка (для библиотеки)

Задача — установка в один клик: без средств разработки, без сервера баз данных, без
предварительной подготовки.

1. Скачайте `BibliotecaCEITI-Setup.exe` со
   [страницы релизов](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/releases/latest).
2. Запустите файл и пройдите шаги установщика.

Только 64-битная Windows. Сервер MySQL входит в установщик: приложение запускает его при
старте на порту 3308 и останавливает при закрытии окна, так что вручную настраивать нечего.

Страница загрузки для библиотекаря лежит в `docs/index.html` и рассчитана на публикацию
через GitHub Pages. Первый установщик пока не опубликован как релиз — до этого момента его
можно собрать самостоятельно, как описано в разделе
[Сборка установщика](#сборка-установщика).

## Запуск из исходников (для разработчиков)

```bash
git clone https://github.com/Nicoleta-git/BibliotecaCEITI-WPF.git
```

```bash
dotnet build BibliotecaCEITI/BibliotecaCEITI.csproj -c Release
```

Нужны [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) и Windows. Подойдёт и
Visual Studio 2022: откройте `BibliotecaCEITI.sln` и нажмите F5.

Приложение ожидает сервер MySQL на `127.0.0.1:3308` с базой `biblioteca_ceiti_go`
(см. `Services/DataBase_Config.cs`). Либо положите дистрибутив MySQL в папку `mysql` рядом с
исполняемым файлом, как это делает установщик, либо запустите свой сервер на этом порту и
импортируйте SQL-дамп из репозитория.

## Настройка

| Параметр | Где хранится | Для чего нужен |
| --- | --- | --- |
| `GEMINI_API_KEY` | системная переменная окружения | помощник по книгам на базе AI |
| SMTP-сервер, порт, пользователь, пароль | таблица `setari`, редактируется в разделе **Системные настройки** | письма о просрочках |
| Google OAuth client id и secret | таблица `configurare_oauth` | кнопка «Continue with Google» |

Всё, кроме ключа Gemini, настраивается внутри приложения, поэтому библиотекарю не приходится
работать с базой данных напрямую.

## Сборка установщика

Установщик собирается в [Inno Setup](https://jrsoftware.org/isinfo.php) из файла
`installer.iss`. Рядом со скриптом должны лежать две папки:

- `dist/app` — опубликованное приложение (`dotnet publish -c Release -r win-x64`)
- `dist/mysql` — дистрибутив MySQL, который войдёт в установщик

После компиляции `installer.iss` готовый файл появится в
`dist-installer/BibliotecaCEITI-Setup.exe`.

## Структура проекта

```
BibliotecaCEITI/
├── Views/        окна и страницы WPF (XAML + code-behind)
├── ViewModels/   view model для панели и уведомлений
├── Models/       ученик, сессия библиотекаря, элементы combo box
├── Services/     база данных, почта, вход через Google, язык, вспомогательные функции
├── Themes/       Light, Dark, Emerald, RedBlue и общие стили
├── Languages/    Strings.en.xaml, Strings.ro.xaml, Strings.ru.xaml
└── Assets/       логотип и изображения загрузки
docs/             страница загрузки, публикуемая через GitHub Pages
.github/          workflow для CI, CodeQL, Dependabot и сборки desktop-приложения
installer.iss     скрипт Inno Setup
```

## Непрерывная интеграция

Каждый push и pull request в `master` собирает проект на `windows-latest`, складывает
предупреждения анализаторов Roslyn в отдельный артефакт и запускает проверку `dotnet format`
в информационном режиме. CodeQL ищет уязвимости в коде, Dependabot следит за актуальностью
NuGet-пакетов. Если запланированная сборка падает, автоматически создаётся issue.

## Планы

- Автоматические тесты для сервисов и сценария выдачи книг
- Подписанный установщик, чтобы Windows SmartScreen не предупреждал при первом запуске

## Автор

Разработано [Nicoleta-git](https://github.com/Nicoleta-git) для
Центра передового опыта в информатике и информационных технологиях (CEITI), Кишинёв.
