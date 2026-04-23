# Tubes2_toBeAnnounced

## Tech Stack
- **Backend:** C# (.NET)
- **Frontend:** TypeScript (Bun + Vite)

## Requirements

### Opsi 1: Menjalankan dengan Docker

1. Install **Docker Desktop**
   - Download dari [docker.com](https://www.docker.com/products/docker-desktop/)
2. Pastikan Docker *engine* sudah berjalan di *background*.

### Opsi 2 : Menjalankan secara lokal

#### Backend (C#)
1. Install **.NET 10.0 SDK**
   - Windows: Download from [dot.net](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
   - macOS (Homebrew): `brew install --cask dotnet-sdk`
2. Verify: `dotnet --version`

#### Frontend (TypeScript)
1. Install **Bun** runtime
   - Windows (PowerShell): `powershell -c "irm bun.sh/install.ps1 | iex"`
   - macOS/Linux: `curl -fsSL https://bun.sh/install | bash`
2. Verify: `bun --version`

## Getting Started

### Docker

Jalankan perintah ini di *root directory*:
```bash
docker-compose up -d --build
```

- Frontend dapat diakses di: `http://localhost:5173`
- Backend dapat diakses di : `http://loalhost:5080`

Untuk mematikan aplikasi:
```bash
docker-compose down
```

### Manual (lokal)

#### 1. Run Backend
```bash
cd backend
dotnet run
```

#### 2. Run Frontend
```bash
cd frontend
bun install
bun run dev
```

## Daftar Fitur & Status Implementasi

| No | Poin | Ya | Tidak |
|:--:|:---|:---:|:---:|
| 1 | Aplikasi berhasil di kompilasi tanpa kesalahan | ✔ |  |
| 2 | Aplikasi berhasil dijalankan | ✔ |  |
| 3 | Aplikasi dapat menerima input URL web, pilihan algoritma, CSS selector, dan jumlah hasil | ✔ |  |
| 4 | Aplikasi dapat melakukan scraping terhadap web pada input | ✔ |  |
| 5 | Aplikasi dapat menampilkan visualisasi pohon DOM | ✔ |  |
| 6 | Aplikasi dapat menelusuri pohon DOM dan menampilkan hasil penelusuran | ✔ |  |
| 7 | Aplikasi dapat menandai jalur tempuh oleh algoritma | ✔ |  |
| 8 | Aplikasi dapat menyimpan jalur yang ditempuh algoritma dalam traversal log | ✔ |  |
| 9 | **[Bonus]** Membuat video |  | ✔ |
| 10 | **[Bonus]** Deploy aplikasi | ✔ |  |
| 11 | **[Bonus]** Implementasi animasi pada penelusuran pohon | ✔ |  |
| 12 | **[Bonus]** Implementasi multithreading | ✔ |  |
| 13 | **[Bonus]** Implementasi LCA Binary Lifting | ✔ |  |

## Contributors
| Nama                           | NIM      | Github                                |
| ------------------------------ | -------- | --------------------------------------|
| Ray Owen Martin.               | 13524033 | [Tensai-033](https://github.com/Tensai-033)
| Emilio Justin                  | 13524043 | [Valz0504](https://github.com/Valz0504)
| Farrell Limjaya                | 13524046 | [Defaro123](https://github.com/Defaro123)