# JSON ↔ C# Generator

A developer tool for converting JSON to C# models and C# classes to JSON samples.

**Live demo:** https://janlu89.github.io/json-csharp-generator/

---

## What it does

**JSON → C#** — Paste any JSON object or array and get fully typed C# classes with configurable options:

- Nullable reference types
- `class` or `record` output
- System.Text.Json or Newtonsoft.Json attributes
- Simple or precise type inference (detects `DateTime`, `Guid`, `Uri`, `long`)
- PascalCase or camelCase property naming
- Configurable root class name
- Schema merging across array instances for better type inference

**C# → JSON** — Paste a C# class and get a representative JSON sample with sensible placeholder values.

**URL fetch** — Load JSON directly from a public URL without leaving the tool.

**Conversion history** — Last 20 conversions stored in memory, restorable with one click.

---

## Architecture

```
json-csharp-generator/
├── src/
│   ├── JsonToCsharp.Engine/     # Pure class library — zero ASP.NET dependencies
│   ├── JsonToCsharp.API/        # .NET 10 Minimal API
│   └── JsonToCsharp.Tests/      # 66 xUnit tests
└── client/                      # Angular 21 SPA
```

### Backend

- **.NET 10 Minimal API** — no controllers, endpoint mapping via extension methods
- **JsonToCsharp.Engine** — stateless conversion engine, usable independently of the API
- **Newtonsoft.Json** for parsing — handles dirty JSON with trailing commas
- **SSRF protection** on the URL fetch endpoint — blocks private IP ranges
- **Docker** multi-stage build for consistent deployments

### Frontend

- **Angular 21** standalone components with Signals throughout
- **CodeMirror 6** for both input and output panes with syntax highlighting
- **Dark/light theme** — CSS custom properties, single class toggle on `<body>`
- No state management library — Signals handle all component communication

---

## Running locally

### Prerequisites

- .NET 10 SDK
- Node.js 20+
- Docker (optional)

### API

```bash
cd src
dotnet run --project JsonToCsharp.API
# API available at http://localhost:5284
```

### Angular

```bash
cd client
npm install
ng serve
# App available at http://localhost:4200
```

### Docker Compose

```bash
docker compose up
# API at http://localhost:5000
# Client at http://localhost:4201
```

---

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/json-to-csharp` | Convert JSON to C# classes |
| `POST` | `/api/csharp-to-json` | Convert C# class to JSON sample |
| `POST` | `/api/fetch-json` | Fetch JSON from a URL (server-side) |
| `GET` | `/health` | Health check |

### Example request

```http
POST /api/json-to-csharp
Content-Type: application/json

{
  "json": "{ \"name\": \"John\", \"age\": 30 }",
  "options": {
    "rootClassName": "Person",
    "useNullableReferenceTypes": true,
    "namingConvention": "PascalCase",
    "attributeStyle": "SystemTextJson",
    "generateAsRecord": false,
    "usePreciseTypes": false
  }
}
```

---

## Design decisions

**Why Newtonsoft.Json for parsing instead of System.Text.Json?**
Newtonsoft is forgiving with dirty JSON — trailing commas, unquoted keys, comments. Real-world JSON from APIs is often not perfectly formatted. System.Text.Json would reject it.

**Why a separate Engine library?**
`JsonToCsharp.Engine` has zero ASP.NET dependencies. It can be used as a NuGet package, in a CLI tool, or in a Blazor app without pulling in the web stack. The API is just one possible host.

**Why Minimal API instead of controllers?**
This project has 3 endpoints. Controllers would add ceremony with no benefit. Minimal API keeps the entry point readable and is the modern .NET approach for simple APIs.

**Why CodeMirror 6 instead of Monaco?**
Monaco is designed for VS Code's architecture and is harder to integrate cleanly in Angular without wrapper libraries. CodeMirror 6 is built as a set of composable modules and integrates naturally.

---

## Testing

```bash
cd src
dotnet test
# 66 tests across JsonToCsharpConverterTests and CsharpToJsonConverterTests
```

---

## Deployment

- **API** — Render.com (Docker, free tier)
- **Frontend** — GitHub Pages (automated via GitHub Actions on push to `master`)
