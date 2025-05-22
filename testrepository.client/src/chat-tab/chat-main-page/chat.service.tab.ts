import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, catchError, Observable, tap, throwError } from 'rxjs';
import {HandshakeData, ChatMessage, DataRoot, StoryDTO, ProjectDTO, CurrentProjectStoriesDTO} from '../user.models.tab';
// import { ChatMessageDTO,  Session } from '../user.models.tab';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private apiUrl = 'http://localhost:5186/api/openai-chat';
  private dataRootBehaviourSubject: BehaviorSubject<DataRoot>;
  public dataRoot$: Observable<DataRoot>;
  private storyBehaviourSubject = new BehaviorSubject<StoryDTO | null>(null);
  public story$: Observable<StoryDTO | null> = this.storyBehaviourSubject.asObservable();
  private currentProjectStoriesSubject = new BehaviorSubject<CurrentProjectStoriesDTO[]>([]);
  public currentProjectStories$ = this.currentProjectStoriesSubject.asObservable();


  public defaultProject: ProjectDTO | null = null;
  public currentProject: ProjectDTO | null = null;
  public allProjects: ProjectDTO[] = [];
  public currentProjectStories: CurrentProjectStoriesDTO[] = [];

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
    this.http.post<any>(`http://localhost:5186/sessions/create`, {}).subscribe();
  }

  sendMessageToGpt(prompt: string): void {
    // Check if dataRoot and sessions exist
    if (!this.dataRoot || !this.dataRoot.sessions) {
      console.error('Error: dataRoot or sessions is not initialized.');
      return;
    }

    const userMessage: ChatMessage = {
      text: prompt,
      type: 'user'
    };

    // Update the current session's messages by creating a new session object
    const updatedSessions = this.dataRoot.sessions.map((session) => {
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
      userId: this.dataRoot.userId,
      prompt: prompt
    }).subscribe((response) => {
      if (!response || !response.message) {
        console.error('Error: Invalid response from API.');
        return;
      }

      const botMessage: ChatMessage = {
        text: response.message,
        type: 'bot'
      };

      // Create a new sessions array with the bot's message added
      const updatedSessionsWithBotMessage = this.dataRoot.sessions.map((session) => {
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
      console.error('Error occurred while sending message to API:', error);
    });
  }

  async handShake(UID: string): Promise<void> {
    return new Promise<void>((resolve, reject) => {
      this.http.post<HandshakeData>(
        `http://localhost:5186/api/User/handshake`,
        JSON.stringify(UID),
        { headers: { 'Content-Type': 'application/json' } }
      ).subscribe({
        next: (response) => {

          const userId = response.sub;
          const currentSessionId = response.currentProject?.currentSessionId ?? 0;
          const defaultSessions = response.defaultProject?.sessions ?? [];

          // Handle default project sessions
          this.currentProject = response.currentProject;
          this.defaultProject = response.defaultProject;
          this.allProjects = response.allProjects;

          this.dataRoot = {
            userId,
            currentSessionId,
            sessions: defaultSessions.map((session) => ({
              id: session.id,
              title: session.title ?? 'New Session',
              messages: session.id === currentSessionId
                ? session.messages.map((m) => ({
                    text: m.text,
                    type: m.sender
                  }))
                : []
            }))
          };

          resolve(); // Done
        },
        error: (err) => reject(err)
      });
    });
  }





  handShakeStory(storyId: string, title: string, description: string, background: string[]): void {
    const requestBody = {
      id: storyId,
      title,
      description,
      background: background ?? [],
      sub: this.dataRoot.userId
    };

    console.log('📤 Calling handshake API with:', requestBody);

    this.http.post<StoryDTO>(
      'http://localhost:5186/Story/storyHandshake',
      requestBody,
      { headers: { 'Content-Type': 'application/json' } }
    ).subscribe(
      (story) => {
        console.log('✅ Story successfully linked/created via handshake:', story);
        this.storyBehaviourSubject.next(story);
      },
      (error) => {
        console.error('❌ Error during story handshake:', error);
      }
    );
  }



  clearStory(): void {
    this.storyBehaviourSubject.next(null);
  }


  defaultChat(sessionId: number, userId: string, prompt: string) {
    const body = {
      sessionId,
      userId,
      prompt
    };

    return this.http.post<{ response: string }>(
      'http://localhost:5186/api/openai-chat/chat/default',
      body,
      { headers: { 'Content-Type': 'application/json' } }
    );
  }

  fetchCurrentProjectStories(projectId: number): Observable<CurrentProjectStoriesDTO[]> {
    return this.http.get<CurrentProjectStoriesDTO[]>(
      `http://localhost:5186/Story/currentProject/${projectId}`
    );
  }



}
