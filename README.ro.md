<div align="center">

# BibliotecaCEITI

[English](README.md) · **Română** · [Русский](README.ru.md)

O aplicație desktop care înlocuiește registrele pe hârtie ale bibliotecii CEITI cu o singură
aplicație Windows: cărți, elevi, împrumuturi, rezervări, alerte de întârziere și rapoarte.

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512bd4?style=for-the-badge&logo=.net)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp)
![WPF](https://img.shields.io/badge/WPF-0078d4?style=for-the-badge&logo=windows)
![MySQL](https://img.shields.io/badge/MySQL-4479a1?style=for-the-badge&logo=mysql)
![Gemini AI](https://img.shields.io/badge/Gemini_AI-4285f4?style=for-the-badge&logo=google-gemini)

[![.NET Core Desktop](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/dotnet-desktop.yml)
[![CI](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/ci.yml/badge.svg)](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/ci.yml)
[![CodeQL](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/codeql.yml/badge.svg)](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/actions/workflows/codeql.yml)

<img width="1536" alt="Demo aplicație" src="https://github.com/user-attachments/assets/959fd07b-1430-4950-94ad-d8e45f6a8714" />

</div>

---

## De ce există acest proiect

Biblioteca școlii funcționează încă pe hârtie: un caiet pentru împrumuturi, altul pentru
manualele împărțite la început de an și niciun mod de a afla cine a întârziat cu o carte,
în afară de a răsfoi paginile una câte una.

BibliotecaCEITI adună totul într-un singur loc. Bibliotecara deschide o singură fereastră și
vede tot inventarul, înregistrează un împrumut din câteva clicuri, trimite o notificare
tuturor elevilor care au întârziat și exportă un raport la finalul semestrului. Bibliotecara
școlii a testat deja aplicația, iar aceasta este gata să fie folosită la nivel de școală.

## Funcționalități

**Inventar**
- Catalog de cărți cu titlu, autor, editură, categorie, limbă, an, imagine de copertă și prețuri
- Mai multe exemplare pentru același titlu, fiecare cu propriul cod de inventar
- Arhivare în loc de ștergere, plus o procedură de casare cu motiv (deteriorare, pierdere, transfer, uzură morală)

**Elevi**
- Fișe de elev cu IDNP, grupă, telefon, e-mail, data nașterii și note
- Cod QR generat pentru fiecare elev, ca legitimația să poată fi scanată în loc de tastată
- Vizualizare per elev a cărților împrumutate și rezervate în acel moment

**Împrumuturi**
- Asistent pas cu pas: cauți elevul, cauți cartea, alegi termenul (14 / 21 / 30 de zile), confirmi
- Mod în masă pentru manuale, ca să dai un set întreg unei grupe dintr-o singură operație
- Restituiri cu confirmare, filtrare după grupă și după stare

**Rezervări**
- Listă de așteptare pentru un titlu care este împrumutat în acel moment
- Elevul este anunțat automat când exemplarul se întoarce

**Alerte**
- Listă de întârzieri cu numărul de zile, filtrată după gravitate (1–4 zile, peste 5 zile)
- Notificări pe e-mail către elevii selectați, cu istoricul mesajelor deja trimise

**Panou de bord și rapoarte**
- Totaluri pentru cărți, exemplare disponibile, împrumutate și rezervate
- Grafic al activității de împrumut pe ultimele luni și top al celor mai citite cărți
- Rapoarte pentru activitatea de împrumut, cele mai citite cărți, cititorii cu întârzieri și cărțile nou adăugate, cu export PDF

**Extra**
- Asistent de carte bazat pe Google Gemini, pentru întrebări despre un titlu
- Autentificare cu utilizator și parolă (parole criptate cu BCrypt) sau cu un cont Google
- Interfață în engleză, română și rusă, schimbată direct din aplicație
- Patru teme: Light, Dark, Emerald și Red-Blue

## Tehnologii folosite

| Nivel | Folosit |
| --- | --- |
| Limbaj / runtime | C#, .NET 8.0 (`net8.0-windows`) |
| Interfață | WPF, teme și stiluri proprii, iconițe FontAwesome5 |
| Bază de date | MySQL (`MySql.Data`), livrată împreună cu aplicația |
| Grafice | LiveChartsCore.SkiaSharpView.WPF |
| AI | DotnetGeminiSDK (`gemini-3.1-flash-lite`) |
| Autentificare | BCrypt.Net-Next, Google.Apis.Auth, System.IdentityModel.Tokens.Jwt |
| E-mail | MailKit / MimeKit prin SMTP cu StartTls |
| Altele | QRCoder |

## Instalare (pentru bibliotecă)

Scopul este un singur clic: fără unelte de dezvoltare, fără server de baze de date, fără nimic
altceva instalat în prealabil.

1. Descarcă `BibliotecaCEITI-Setup.exe` din
   [pagina de versiuni](https://github.com/Nicoleta-git/BibliotecaCEITI-WPF/releases/latest).
2. Rulează-l și urmează pașii din instalator.

Doar Windows pe 64 de biți. Serverul MySQL este inclus în instalator: aplicația îl pornește
la deschidere pe portul 3308 și îl oprește la închiderea ferestrei, deci nu trebuie configurat
nimic manual.

Pagina de descărcare pentru bibliotecară se află în `docs/index.html` și este gândită să fie
publicată prin GitHub Pages. Primul instalator nu a fost încă publicat ca versiune — până
atunci, îl poți construi singur, așa cum este descris la
[Construirea instalatorului](#construirea-instalatorului).

## Rulare din cod sursă (pentru dezvoltatori)

```bash
git clone https://github.com/Nicoleta-git/BibliotecaCEITI-WPF.git
```

```bash
dotnet build BibliotecaCEITI/BibliotecaCEITI.csproj -c Release
```

Ai nevoie de [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) și de Windows.
Merge și cu Visual Studio 2022: deschizi `BibliotecaCEITI.sln` și apeși F5.

Aplicația se așteaptă la un server MySQL pe `127.0.0.1:3308`, cu baza de date
`biblioteca_ceiti_go` (vezi `Services/DataBase_Config.cs`). Fie pui o distribuție MySQL într-un
folder `mysql` lângă executabil, așa cum face instalatorul, fie pornești propriul server pe acel
port și imporți dump-ul SQL din depozit.

## Configurare

| Setare | Unde se află | Necesară pentru |
| --- | --- | --- |
| `GEMINI_API_KEY` | variabilă de mediu de sistem | asistentul AI pentru cărți |
| server SMTP, port, utilizator, parolă | tabelul `setari`, editat din **Setări de sistem** în aplicație | notificările pe e-mail pentru întârzieri |
| client id și secret Google OAuth | tabelul `configurare_oauth` | butonul „Continue with Google” |

Tot ce este în afară de cheia Gemini se configurează din interiorul aplicației, deci bibliotecara
nu trebuie să atingă niciodată baza de date.

## Construirea instalatorului

Instalatorul se construiește cu [Inno Setup](https://jrsoftware.org/isinfo.php), pe baza
fișierului `installer.iss`. Acesta se așteaptă la două foldere lângă script:

- `dist/app` — aplicația publicată (`dotnet publish -c Release -r win-x64`)
- `dist/mysql` — distribuția MySQL care va fi inclusă

Compilezi `installer.iss`, iar fișierul de instalare apare în
`dist-installer/BibliotecaCEITI-Setup.exe`.

## Structura proiectului

```
BibliotecaCEITI/
├── Views/        ferestre și pagini WPF (XAML + code-behind)
├── ViewModels/   view model-uri pentru dashboard și alerte
├── Models/       elev, sesiune bibliotecar, elemente de combo box
├── Services/     bază de date, e-mail, autentificare Google, limbă, funcții utile
├── Themes/       Light, Dark, Emerald, RedBlue și stilurile comune
├── Languages/    Strings.en.xaml, Strings.ro.xaml, Strings.ru.xaml
└── Assets/       logo și imagini de încărcare
docs/             pagina de descărcare, publicată prin GitHub Pages
.github/          workflow-uri de CI, CodeQL, Dependabot și build desktop
installer.iss     scriptul Inno Setup
```

## Integrare continuă

Fiecare push și pull request pe `master` construiește proiectul pe `windows-latest`, adună
avertismentele analizoarelor Roslyn într-un artefact descărcabil și rulează o verificare
`dotnet format` cu rol informativ. CodeQL scanează codul pentru probleme de securitate, iar
Dependabot ține pachetele NuGet la zi. Un build programat care eșuează deschide automat un issue.

## Ce urmează

- Teste automate pentru servicii și pentru fluxul de împrumut
- Instalator semnat, ca Windows SmartScreen să nu mai avertizeze la prima rulare

## Autor

Realizat de [Nicoleta-git](https://github.com/Nicoleta-git) pentru
Centrul de Excelență în Informatică și Tehnologii Informaționale (CEITI), Chișinău.
