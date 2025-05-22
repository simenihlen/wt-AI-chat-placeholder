import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface Message {
  sender: string;
  text?: string;
  content?: string;  // Change from "text" to "content" to match Angular template
  timestamp: Date;
}

interface Conversation {
  id: number;
  title: string;
  messages: Message[];
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  conversations: Conversation[] = [];
  selectedConversation: Conversation | null = null;
  newMessage: string = '';
  userID: number = 2; // Assume logged-in user
  sessionId: number | null = null; // Store session ID for OpenAI chat

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.restoreSession();
    this.loadUserSessions();
  }

  /** 🔹 Restore session after page refresh */
  restoreSession(): void {
    const storedSessionId = localStorage.getItem('sessionId');
    if (storedSessionId) {
      this.sessionId = parseInt(storedSessionId, 10);
    }
  }

  /** 🔹 Fetch user sessions from API */
  loadUserSessions(): void {
    this.http.get<Conversation[]>(`http://localhost:5186/sessions/user/2`).subscribe(
      (data) => {
        this.conversations = data.map(session => ({
          id: session.id,
          title: "New Session", // ✅ Force static title
          messages: session.messages ?
            session.messages.map(msg => ({
              sender: msg.sender,
              content: msg.text || msg.content, // ✅ Ensure correct mapping
              timestamp: new Date(msg.timestamp)
            }))
            : [] // ✅ Ensure messages exist
        }));

        if (this.conversations.length > 0) {
          const lastSession = this.conversations.find(c => c.id === this.sessionId) || this.conversations[0];
          this.selectConversation(lastSession);
        }
      },
      (error) => console.error('Error loading sessions:', error)
    );
  }


  /** 🔹 Select a conversation */
  selectConversation(convo: Conversation): void {
    this.selectedConversation = convo;
    this.sessionId = convo.id;
    localStorage.setItem('sessionId', this.sessionId.toString()); // Save session for reload
  }

  /** 🔹 Start a new OpenAI chat session */
  startNewChat(): void {
    const userId = 2;
    this.http.post<any>(`http://localhost:5186/sessions/create?userId=${userId}`, {})
      .subscribe(
        (newSession) => {
          // 1. Session created on the server
          // 2. Now re-fetch the entire list of sessions
          this.loadUserSessions();
        },
        (error) => console.error('Error creating session:', error)
      );
  }



  /** 🔹 Send a message to OpenAI */
  sendMessage(): void {
    if (this.newMessage.trim() && this.sessionId) {
      const message: Message = {
        sender: 'Me',
        content: this.newMessage,
        timestamp: new Date()
      };

      // Add message to conversation
      this.selectedConversation?.messages.push(message);

      // Call OpenAI API
      this.http.post<{ message: string }>(`http://localhost:5186/api/openai-chat/chat`, {
        sessionId: this.sessionId,
        prompt: this.newMessage
      }).subscribe(
        (response) => {
          const botMessage: Message = {
            sender: 'AI',
            content: response.message,
            timestamp: new Date()
          };
          this.selectedConversation?.messages.push(botMessage);
        },
        (error) => console.error('Error sending message to OpenAI:', error)
      );

      this.newMessage = '';
    }
  }
}
