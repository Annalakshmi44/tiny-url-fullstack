# 🔗 Tiny URL App

A full-stack URL shortener application built using **Angular**, **ASP.NET Core Minimal API**, and **Azure-ready infrastructure**.

---

## 🚀 Features

* Short URL generation
* Public & Private URLs
* Click tracking per URL
* Search and delete URLs
* Copy short URL to clipboard

---

## 🧱 Tech Stack

* **Frontend:** Angular
* **Backend:** ASP.NET Core Minimal API
* **Database:** SQLite / Azure SQL
* **Cloud:** Azure
* **CI/CD:** GitHub Actions
* **Infrastructure:** Terraform

---

## 📁 Project Structure

```
tiny-url/
├── src/
│   ├── TinyUrl.Api/           # Backend API
│   └── TinyUrl.Web/           # Angular Frontend
    └── TinyUrl.Functions/     # Azure Function
├── infra/                     # Terraform IaC
├── .github/
│   └── workflows/             # CI/CD pipelines
├── .gitignore
└── README.md
```

---

## ▶️ Run Locally

### 🔹 Backend (ASP.NET Core API)

```bash
cd src/TinyUrl.Api
dotnet run
```

API will run at:

```
http://localhost:5290
```

Swagger UI:

```
http://localhost:5290/swagger
```

---

### 🔹 Frontend (Angular)

```bash
cd src/TinyUrl.Web
npm install
ng serve
```

Frontend will run at:

```
http://localhost:4200
```

---

## ☁️ Deployment

* Azure Web App (Backend Hosting)
* Azure Static Hosting / App Service (Frontend)
* GitHub Actions for CI/CD
* Terraform for Infrastructure provisioning

---

## 📸 Screenshots

*Add your application screenshots here*

---

## 👩‍💻 Author

**Annalakshmi Arjunan**

---

## ⭐ Notes

This project demonstrates:

* Full-stack development (Angular + .NET)
* REST API design
* Cloud-ready architecture
* CI/CD and Infrastructure as Code

---
