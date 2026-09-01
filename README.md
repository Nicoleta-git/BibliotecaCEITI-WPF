<div align="center">

# BibliotecaCEITI

**English** · [Română](README.ro.md) · [Русский](README.ru.md)

A desktop application that replaces the paper registers of the CEITI school library
with a single Windows app: books, students, loans, reservations, overdue alerts and reports.

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512bd4?style=for-the-badge&logo=.net)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp)
![WPF](https://img.shields.io/badge/WPF-0078d4?style=for-the-badge&logo=windows)
![MySQL](https://img.shields.io/badge/MySQL-4479a1?style=for-the-badge&logo=mysql)
![Gemini AI](https://img.shields.io/badge/Gemini_AI-4285f4?style=for-the-badge&logo=google-gemini)

[![.NET Core Desktop](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/dotnet-desktop.yml)
[![CI](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/ci.yml/badge.svg)](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/ci.yml)
[![CodeQL](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/codeql.yml/badge.svg)](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/codeql.yml)

<img width="1536" alt="App demo" src="https://github.com/user-attachments/assets/959fd07b-1430-4950-94ad-d8e45f6a8714" />

</div>

---

## Why this project exists

The library of our school still runs on paper: a notebook for loans, another one for
textbooks handed out at the beginning of the year, and no way to tell who is late with
a book without going through the pages one by one.

BibliotecaCEITI puts all of that in one place. The librarian opens a single window and
can see the whole inventory, register a loan in a few clicks, send a reminder to every
student who is late, and export a report at the end of the term. The librarian of our
school has already tested it and the application is ready to be deployed school-wide.

## Features

**Inventory**
- Book catalogue with title, author, publisher, category, language, year, cover image and prices
- Multiple copies per title, each with its own inventory code
- Archiving instead of deletion, plus a formal write-off flow with a reason (damage, loss, transfer, obsolescence)

**Students**
- Student records with IDNP, group, phone, e-mail, date of birth and notes
- QR code generated for every student, so a card can be scanned instead of typed
- Per-student view of currently borrowed and reserved books

**Loans**
- Step-by-step loan wizard: find the student, find the book, choose the term (14 / 21 / 30 days), confirm
- Bulk mode for textbooks, to hand out a whole set to an entire group at once
- Returns with confirmation, filtering by group and by status

**Reservations**
- Waiting queue for a title that is currently borrowed
- The student is notified automatically when the copy comes back

**Alerts**
- Overdue list with days late, filtered by severity (1–4 days, over 5 days)
- E-mail notifications sent to selected students, with a history of what was already sent

**Dashboard and reports**
- Totals for books, available, borrowed and reserved copies
- Loan activity chart over the last months and a top of the most-read books
- Reports on loan activity, most-read books, readers with penalties and newly added books, with PDF export

**Extras**
- Book assistant powered by Google Gemini, for questions about a title
- Login with username and password (hashed with BCrypt) or with a Google account
- Interface in English, Romanian and Russian, switchable at runtime
- Four themes: Light, Dark, Emerald and Red-Blue

## Tech stack

| Layer | Used |
| --- | --- |
| Language / runtime | C#, .NET 8.0 (`net8.0-windows`) |
| UI | WPF, custom themes and styles, FontAwesome5 icons |
| Database | MySQL (`MySql.Data`), bundled with the app |
| Charts | LiveChartsCore.SkiaSharpView.WPF |
| AI | DotnetGeminiSDK (`gemini-3.1-flash-lite`) |
| Auth | BCrypt.Net-Next, Google.Apis.Auth, System.IdentityModel.Tokens.Jwt |
| E-mail | MailKit / MimeKit over SMTP with StartTls |
| Other | QRCoder |

## Installing (for the library)

The goal is one click: no development tools, no database server, nothing else to install first.

1. Download `BibliotecaCEITI-Setup.exe` from the
   [releases page](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/releases/latest).
2. Run it and follow the wizard.

Windows 64-bit only. The MySQL server ships inside the installer: the application starts it
on launch on port 3308 and shuts it down when you close the window, so there is nothing to
configure by hand.

The download page for the librarian is in `docs/index.html` and is meant to be served with
GitHub Pages. The first installer has not been published as a release yet — until then, build
it yourself as described in [Building the installer](#building-the-installer).

## Running from source (for developers)

```bash
git clone https://github.com/Nicoleta-git/BibliotecaCEITI-WPF.git
```

```bash
dotnet build BibliotecaCEITI/BibliotecaCEITI.csproj -c Release
```

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and Windows.
Visual Studio 2022 works too: open `BibliotecaCEITI.sln` and press F5.

The application expects a MySQL server on `127.0.0.1:3308` with the database
`biblioteca_ceiti_go` (see `Services/DataBase_Config.cs`). Either place a MySQL distribution
in a `mysql` folder next to the executable, the way the installer does, or start your own
server on that port and import the SQL dump from the repository.

## Configuration

| Setting | Where it lives | Needed for |
| --- | --- | --- |
| `GEMINI_API_KEY` | system environment variable | the AI book assistant |
| SMTP server, port, user, password | `setari` table, edited from **System settings** in the app | overdue e-mail notifications |
| Google OAuth client id and secret | `configurare_oauth` table | the "Continue with Google" button |

Everything except the Gemini key is configured from inside the application, so the librarian
never has to touch the database.

## Building the installer

The installer is built with [Inno Setup](https://jrsoftware.org/isinfo.php) from `installer.iss`.
It expects two folders next to the script:

- `dist/app` — the published application (`dotnet publish -c Release -r win-x64`)
- `dist/mysql` — the MySQL distribution that gets bundled

Compile `installer.iss` and the setup file appears in `dist-installer/BibliotecaCEITI-Setup.exe`.

## Project structure

```
BibliotecaCEITI/
├── Views/        WPF windows and pages (XAML + code-behind)
├── ViewModels/   dashboard and alerts view models
├── Models/       student, librarian session, combo items
├── Services/     database, e-mail, Google auth, language, helpers
├── Themes/       Light, Dark, Emerald, RedBlue and shared styles
├── Languages/    Strings.en.xaml, Strings.ro.xaml, Strings.ru.xaml
└── Assets/       logo and loading images
docs/             download page published with GitHub Pages
.github/          CI, CodeQL, Dependabot and desktop build workflows
installer.iss     Inno Setup script
```

## Continuous integration

Every push and pull request on `master` builds the project on `windows-latest`, collects the
Roslyn analyzer warnings into a downloadable artifact, and runs a `dotnet format` check as
advisory. CodeQL scans the code for security issues, and Dependabot keeps the NuGet packages
up to date. A failed scheduled build automatically opens an issue.

## Roadmap

- Automated tests for the services and the loan flow
- Signed installer, so Windows SmartScreen stops warning on first run

## Team

- [@Nicoleta-git](https://github.com/Nicoleta-git)
- [@Misha-cybernet](https://github.com/Misha-cybernet)
- [@00NYXth](https://github.com/00NYXth)

Built at the Centre of Excellence in Informatics and Information Technologies (CEITI), Chișinău.
