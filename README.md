# 📦 Agora

**Agora** là dự án được xây dựng dựa trên kiến trúc **N-Layers / Clean Architecture**, tập trung vào sự tách biệt giữa các mối quan tâm (Separation of Concerns) và khả năng mở rộng.

## 🏗 Kiến trúc Dự án

Solution được chia thành các tầng chức năng rõ ràng:

* **Agora.Domain**: Chứa các Entities, Interfaces và Core Logic.
* **Agora.Application**: Chứa Business Logic, DTOs, Services.
* **Agora.Infrastructure**: Xử lý Database Context, Repositories, External Services.
* **Agora.Auth**: Module xử lý xác thực và phân quyền.
* **Agora.Payment**: Module tích hợp thanh toán.
* **Agora.API**: Entry point của ứng dụng (Web API).

---

## 🚀 Getting Started

Làm theo các bước dưới đây để cài đặt và khởi chạy dự án trên môi trường local.

### Yêu cầu phần mền
* ** .NET 10**
* ** Visual code**
* ** SQL Server 16**

### 1. Clone Project

Mở terminal và chạy lệnh sau để tải mã nguồn về máy:

```bash
git clone [https://github.com/HSUxTHP/Agora](https://github.com/HSUxTHP/Agora)
cd Agora
```
### 2. Restore Dependencies
Tải và khôi phục toàn bộ các thư viện NuGet cần thiết cho solution:

```bash
dotnet restore
```

### 3. Khởi tạo Database SQL Server
Dự án sử dụng SQL Server
Yêu cầu tải SQL Server để có thể sử dụng

- mở File Agora.sql và chạy từng khối

5. Chạy ứng dụng (Run API)
Khởi động Web API:

```bash
cd Agora.API
dotnet build
dotnet run
```
#### Sau khi khởi động thành công, API sẽ lắng nghe tại: 👉 https://localhost:5000 (hoặc port được cấu hình trong launchSettings.json).

## 🧩 Công cụ phát triển
Bạn có thể mở dự án bằng các IDE phổ biến:

Visual Studio: Mở file Agora.sln.

Visual Studio Code: Mở thư mục root và gõ code .

[text](https://localhost:5000/Swagger/index.html)

## 📂 Cấu trúc thư mục
```bash
Agora/
├── Agora.sln               # Solution file
├── Agora.Domain/           # Core Entities & Domain Logic
├── Agora.Application/      # Business Services & Use Cases
├── Agora.Infrastructure/   # Data Access & External Libs
├── Agora.Auth/             # Authentication Module
├── Agora.Payment/          # Payment Processing Module
├── Agora.API/              # REST API Layer
├── docs/                    # Docs
└── .gitignore              # Git ignore rules
```

## Connection Layer
<p align="center">
  <img src="docs/LayerConnect.png" width="400">
</p>

