export interface HandshakeData {
  sub: string;
  currentProject: ProjectDTO | null;
  defaultProject: ProjectDTO | null;
  allProjects: ProjectDTO[];
}
export interface SessionDTO {
  id: number;
  title: string;
  created_at: Date;
  user_id?: number;
  messages: ChatMessageDTO[];
  stories?: StoryDTO[];
}

export interface ChatMessageDTO {
  sender: string;
  text: string;
  timestamp: Date;
  session_id: number;
}

export interface StoryDTO {
  id: string;
  title: string;
  description: string;
  background: string[];
  includeDescription?: boolean;
  includeBackground?: boolean;
}

export interface DataRoot {
  userId: string | undefined;
  currentSessionId: number | undefined;
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

export interface ProjectDTO {
  id: number;
  sub: string;
  title: string;
  currentSessionId: number | null;
  sessions: SessionDTO[];
  stories: StoryDTO[];
}

export interface CurrentProjectStoriesDTO {
  sub: string;
  title: string;
}
