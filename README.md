# Tubes2_toBeAnnounced

## Tech Stack
- **Backend:** C# (.NET)
- **Frontend:** TypeScript (Bun + Vite)

## Requirements

### Backend (C#)
1. Install **.NET 8.0 SDK**
   - Windows: Download from [dot.net](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
   - macOS (Homebrew): `brew install --cask dotnet-sdk`
2. Verify: `dotnet --version`

### Frontend (TypeScript)
1. Install **Bun** runtime
   - Windows (PowerShell): `powershell -c "irm bun.sh/install.ps1 | iex"`
   - macOS/Linux: `curl -fsSL https://bun.sh/install | bash`
2. Verify: `bun --version`

## Getting Started

### 1. Run Backend
```bash
cd backend
dotnet run
```

### 2. Run Frontend
```bash
cd frontend
bun install
bun run dev
```