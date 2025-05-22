# 📡 Testrepository API Documentation

This document provides details on all available API endpoints in **Testrepository.Server**.

---

## 🔹 **API Overview**
This is a quick summary of all API endpoints available in the Testrepository Server.

| **Category**         | **Method** | **Endpoint**                              | **Description** |
|----------------------|-----------|------------------------------------------|----------------|
| **Azure Chat API**   | `POST`    | `/api/azure-chat/start-session`          | Starts a new chat session for Azure OpenAI. |
| **Azure Chat API**   | `POST`    | `/api/azure-chat/chat`                   | Sends a message to Azure OpenAI and retrieves a response. |
| **OpenAI Chat API**  | `POST`    | `/api/openai-chat/start-session`         | Starts a new chat session for OpenAI (GPT-4). |
| **OpenAI Chat API**  | `POST`    | `/api/openai-chat/chat`                  | Sends a message to OpenAI (GPT-4) and retrieves a response. |
| **Chat Messages API**| `GET`     | `/api/chat/messages`                     | Retrieves all chat messages stored in the database. |
| **Chat Messages API**| `POST`    | `/api/chat/send`                         | Stores a new user message in the database. |
| **Chat Messages API**| `GET`     | `/api/chat/session/{sessionId}`          | Retrieves all messages for a specific session. |
| **Chat Messages API**| `POST`    | `/api/chat/session/{sessionId}/send`     | Sends a new message to a specific session. |
| **Sessions API**     | `POST`    | `/api/sessions/create`                   | Creates a new chat session. |
| **Sessions API**     | `GET`     | `/api/sessions/{sessionId}`              | Retrieves session details by session ID. |
| **Sessions API**     | `GET`     | `/api/sessions/user/{userId}`            | Retrieves all sessions for a specific user. |
| **Sessions API**     | `GET`     | `/api/sessions`                          | Retrieves all available sessions. |
| **Sessions API**     | `PUT`     | `/api/sessions/{sessionId}`              | Updates a session by adding messages. |
| **Test Endpoints**   | `GET`     | `/TestString`                            | Returns a simple test response. |



## 🛠️ **Error Handling**
| Status Code | Meaning                          | Possible Causes |
|------------|----------------------------------|----------------|
| `200 OK`   | ✅ Request was successful.       | Everything worked as expected. |
| `400 Bad Request` | ❌ Invalid request. | Missing required fields, incorrect JSON format. |
| `401 Unauthorized` | 🔒 Authentication failed. | Invalid API key, missing credentials. |
| `404 Not Found` | 🔍 Resource not found. | Requested session or message does not exist. |
| `500 Internal Server Error` | ⚠️ Server error. | Database issues, API key misconfiguration, or unhandled exceptions. |
