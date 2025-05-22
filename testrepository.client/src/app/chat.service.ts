import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { ChatMessageDTO, HandshakeData, Session, ChatMessage, DataRoot } from './user.models';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private apiUrl = 'http://localhost:5186/api/openai-chat';
  private dataRootBehaviourSubject: BehaviorSubject<DataRoot>;
  public dataRoot$: Observable<DataRoot>;

  constructor(private http: HttpClient) {
    const initialUserConfig: DataRoot = {
      userId: undefined,
      currentSessionId: undefined,
      sessions: []
    };
    this.dataRootBehaviourSubject = new BehaviorSubject<DataRoot>(initialUserConfig);
    this.dataRoot$ = this.dataRootBehaviourSubject.asObservable();
  }

  get dataRoot(): DataRoot {
    return this.dataRootBehaviourSubject.value;
  }

  set dataRoot(value: DataRoot) {
    this.dataRootBehaviourSubject.next(value);
  }

  startNewSession(): void {
    this.http.post<any>(`${this.apiUrl}/start-session`, {}).subscribe();
  }


  sendMessageToGpt(prompt: string): void {
    // Check if dataRoot and sessions exist
    if (!this.dataRoot || !this.dataRoot.sessions) {
      console.error("Error: dataRoot or sessions is not initialized.");
      return;
    }

    const userMessage: ChatMessage = {
      text: prompt,
      type: 'user',
    };

    // Update the current session's messages by creating a new session object
    const updatedSessions = this.dataRoot.sessions.map(session => {
      // Copy the session and update messages
      if (session.id === this.dataRoot.currentSessionId) {
        return {
          ...session, // Copy existing session
          messages: [...session.messages, userMessage] // Add the user message
        };
      }
      return session; // No changes for other sessions
    });

    // Reassign dataRoot with the updated sessions
    this.dataRoot = { ...this.dataRoot, sessions: updatedSessions };

    // Send the message to the API and handle the response
    this.http.post<any>(`${this.apiUrl}/chat`, {
      sessionId: this.dataRoot.currentSessionId,
      prompt: prompt
    }).subscribe(response => {
      if (!response || !response.message) {
        console.error("Error: Invalid response from API.");
        return;
      }

      const botMessage: ChatMessage = {
        text: response.message,
        type: 'bot'
      };

      // Create a new sessions array with the bot's message added
      const updatedSessionsWithBotMessage = this.dataRoot.sessions.map(session => {
        if (session.id === this.dataRoot.currentSessionId) {
          return {
            ...session, // Copy existing session
            messages: [...session.messages, botMessage] // Add the bot's response
          };
        }
        return session; // No changes for other sessions
      });

      // Reassign dataRoot again with the updated sessions
      this.dataRoot = { ...this.dataRoot, sessions: updatedSessionsWithBotMessage };
    }, (error) => {
      // Handle API call error
      console.error("Error occurred while sending message to API:", error);
    });
  }


  handShake(UID: string): void {
    this.http.post<HandshakeData>(
      `${this.apiUrl}/handshake`,
      JSON.stringify(UID),
      { headers: { 'Content-Type': 'application/json' } }
    ).subscribe(response => {
      const { id: userId, currentSession, sessionIds } = response;
      const currentSessionId = currentSession.id; // Extract `id` separately
      let currentSessionMsgs: ChatMessage[] = currentSession.messages.map(m => ({ text: m.text, type: m.sender }));
      this.dataRoot = {
        userId,
        currentSessionId,
        sessions: sessionIds.map(sessionId => ({
          id: sessionId,
          messages: sessionId === currentSessionId ? currentSessionMsgs : []
        }))
      };

    });
  }
}
