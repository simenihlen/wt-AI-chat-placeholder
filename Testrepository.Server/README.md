# Testrepository Server (Chatbot API with OpenAI & ASP.NET Core)

🚀 This is the **backend API** for the Testrepository chatbot application.  
It is built using **ASP.NET Core** and connects to **PostgreSQL** for chat session management.  
The backend integrates with **OpenAI (ChatGPT)** to generate chatbot responses.

---

## 📌 **Project Overview**
This backend provides:
- **ChatGPT integration** using OpenAI API
- **Session-based chat storage** with PostgreSQL
- **REST API for message handling**
- **Frontend communication** with Angular

---

## 🛠️ **Technology Stack**
| Technology  | Purpose |
|-------------|---------|
| **ASP.NET Core** | Web API backend |
| **Entity Framework Core** | ORM for PostgreSQL |
| **PostgreSQL** | Database for storing chat messages |
| **OpenAI API (ChatGPT)** | AI chatbot responses |
| **Angular** | Frontend client |

---

## 🚀 **Setup & Installation**
### **1️⃣ Prerequisites**
- [.NET SDK](https://dotnet.microsoft.com/en-us/download)
- [PostgreSQL](https://www.postgresql.org/download/)

---

### **2️⃣ Clone the Repository**
```sh
git clone https://github.com/h180786/Testrepository
cd Testrepository.Server

### **3️⃣ Setup Test / Dev Database**
(linux host)
	# Pull the pgvector image
	docker pull ankane/pgvector:latest

	# Run the PostgreSQL container for BachelorDevDatabase
	docker run -d \
	--name pgvector-dev \
	-e POSTGRES_USER=myuser \
	-e POSTGRES_PASSWORD=mypassword \
	-e POSTGRES_DB=postgres \
	-p 15000:5432 \
	ankane/pgvector:latest

# Run the PostgreSQL container for BachelorTestDatabase
docker run -d \
	--name pgvector-test \
	-e POSTGRES_USER=myuser \
	-e POSTGRES_PASSWORD=mypassword \
	-e POSTGRES_DB=postgres \
	-p 15005:5432 \
	ankane/pgvector:latest

	# Wait for PostgreSQL containers to start
	sleep 5

	# Enable pgvector extension on both databases
	docker exec -it pgvector-dev psql -U myuser -d postgres -c "CREATE EXTENSION vector;"
	docker exec -it pgvector-test psql -U myuser -d postgres -c "CREATE EXTENSION vector;"