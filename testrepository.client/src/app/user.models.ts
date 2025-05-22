export interface HandshakeData {
  id: string;
  currentSession: SessionDTO;
  sessionIds: number[];
}
export interface SessionDTO {
  id: number;
  created_at: Date;
  user_id?: number;
  messages: ChatMessageDTO[];
}
export interface ChatMessageDTO {
  sender: string;
  text: string;
  timestamp: Date;
  session_id: number;
}

export interface DataRoot {
  userId: string | undefined;
  currentSessionId: Number | undefined;
  sessions: Session[];
}

export interface Session {
  id: number;
  messages: ChatMessage[];
}

export interface ChatMessage {
  text: string;
  type: string;
}
