import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
// import moment from 'moment';
import { HttpClient } from '@angular/common/http';
import { ChatService } from './chat.service.tab';
import { StoryDTO, ProjectDTO, CurrentProjectStoriesDTO } from '../user.models.tab';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogData } from 'app/modules/common-ui-components/confirm-dialog/confirm-dialog-data.type';
import { ConfirmDialog } from 'app/modules/common-ui-components/confirm-dialog/confirm-dialog.component';
import { StoryDataService } from 'app/storyview/story-data.service';
import { BackgroundInfoDataService } from 'app/shared-components/background-info/services/background-info-data.service';
import {ContentType} from '../../../types/content-types';
import {ITabBarTab} from '../../../modules/common-ui-components/tab-bar/tab-bar.types';
import { StoryListDialogComponent } from './components/story-list-dialog/story-list-dialog.component';

interface Message {
  sender: string;
  text?: string;
  timestamp: Date;
  originalIndex?: number;
}

interface Conversation {
  id: number;
  title: string;
  messages: Message[];
}

@Component({
  selector: 'chat-main-page',
  templateUrl: './chat-main-page.component.html',
  styleUrls: ['./chat-main-page.component.scss']
})
export class ChatMainPageComponent implements OnInit {
  conversations: Conversation[] = [];
  selectedConversation: Conversation | null = null;
  newMessage: string = '';

  // We'll store the user's "sub" here once we fetch it
  userID: string | null = null;
  sessionId: number | null = null;
  // Default project
  projectList: ProjectDTO[] | null = null;
  defaultProject: ProjectDTO;
  selectedProject: ProjectDTO | null = null;
  defaultProjectSessions: Conversation[] = [];
  projectsLoaded: boolean = false;

  selectionMode: boolean = false;  // Track if we are in selection mode
  selectedConversations: Conversation[] = [];  // Store selected conversations
  droppedStories: StoryDTO[] = [];  // Store dropped stories
  currentProjectStories: CurrentProjectStoriesDTO[] = [];

  // Variable for searchbar
  isSearchBarVisible: boolean = false;
  isSearchBarActive: boolean = false;
  filteredConversations: Conversation[] = [];
  currentSearchTerm: string = '';

  // Variables for scrollbutton
  @ViewChild('chatContainer') chatContainer: ElementRef;
  showScrollButton: boolean = false;
  isLoading: boolean = false;
  backgroundInfoDataServiceBgTabItemsSubscription: any;

  // Variable for sidebar visability
  isSidebarCollapsed: boolean = false;

  constructor(
    private http: HttpClient,
    private chatService: ChatService,  // Inject ChatService
    private dialog: MatDialog, //inject confirm dialog service
    private storyDataService: StoryDataService, // Inject StoryDataService
    private BackgroundDataService: BackgroundInfoDataService
  ) {}

  ngOnInit(): void {
    // Restore session (if any) and then get the user's identity
    this.selectedTab = '1';
    this.restoreSession();
    this.getUserID();
    this.filteredConversations = [...this.conversations];
  }

  setDefaultProject(): void {
    this.defaultProject = this.projectList.find((project) => project.id === this.projectList[0].id);
  }

  ngAfterViewInit(): void {
    this.scrollToBottom();
    this.scrollListener();
  }

  handleDrop(event: JQuery.DropEvent, ui: { draggable: HTMLElement }) {
    console.log('🟢 Drop event triggered!', event, ui);

    const jqDraggable = $(ui.draggable);
    const storyId = parseInt(jqDraggable.attr('data-storyoid') || '', 10);

    if (!storyId) {
      console.warn('⚠️ No valid storyId found in dropped element.');
      return;
    }

    // ✅ Initialize variables
    let uniqueId: string = '';
    let title: string = '';
    let strippedDescription: string = '';
    let backgroundTexts: string[] = [];
    let backgroundLoaded = false;
    let storyLoaded = false;

    // ✅ Fetch story details
    this.storyDataService.getGeneral(storyId).subscribe((storyData) => {
      uniqueId = storyData.uniqueId;
      title = storyData.title;
      strippedDescription = this.stripHtmlTags(storyData.description);
      storyLoaded = true;

      console.log('📄 Story details fetched:', { uniqueId, title, strippedDescription });

      checkAndProceed(); // ✅ Check if we can proceed
    });

    // ✅ Fetch background headers
    this.BackgroundDataService.getBgTabHeaders(ContentType.Story, storyId).subscribe((bgTabItems) => {
      const oid = bgTabItems?.flatMap((tabs) => tabs)?.[0]?.oid;

      if (!oid) {
        console.warn('⚠️ No valid OID found. Proceeding without background info.');
        backgroundLoaded = true;
        checkAndProceed();
        return;
      }

      // ✅ Fetch background info using oid
      this.BackgroundDataService.getBgTabItems({
        contentType: ContentType.Story,
        contentId: storyId,
        bgTabId: oid,
        contentFilter: { skip: 0, take: 10 }
      });
      this.backgroundInfoDataServiceBgTabItemsSubscription = this.BackgroundDataService
        .getBgTabItemsSubject(oid)
        .subscribe((bgTabItems) => {
          console.log('Extracted BackgroundInfo:', bgTabItems);
          backgroundTexts = bgTabItems
            .filter((item) => item.text)
            .map((item) => this.stripHtmlTags(item.text));
          backgroundLoaded = true;
          console.log('✅ Cleaned Background Info:', backgroundTexts);

          checkAndProceed(); // ✅ Check if we can proceed
        });
    });

    // ✅ Function to check if everything is loaded before proceeding
    const checkAndProceed = () => {
      if (storyLoaded && backgroundLoaded) {
        console.log('✅ All data loaded. Now processing story.');

        // ✅ Prevent duplicates before adding new story
        if (!this.droppedStories.some(s => s.id === uniqueId)) {
          const newStory: StoryDTO = {
            id: uniqueId,
            title,
            description: strippedDescription,
            background: backgroundTexts,
            includeDescription: false,
            includeBackground: false
          };

          this.droppedStories.push(newStory);
          console.log('✅ Added new story:', newStory);

          // ✅ Show confirmation dialog
          this.openConfirmDialogForDroppedStory(newStory);
        } else {
          console.warn('⚠️ Story already exists, skipping confirmation.');
        }

        // ✅ Send handshake to ensure story is verified/created
        this.chatService.handShakeStory(uniqueId, title, strippedDescription, backgroundTexts);
      }
    };
  }

  /**
   * Retrieve the user's sub from your identity provider.
   * After that, perform a handshake to configure user data.
   */
  getUserID(): void {
    this.http.get('https://news3.wolftech.no/idsrv4/connect/userinfo')
      .subscribe(
        (response: any) => {
          const sub = response.sub;
          if (sub) {
            this.userID = sub;
            // Call handshake once userID is available.
            this.handshake(sub);
            this.loadProjects();
          } else {
            this.userID = null;
          }
        },
        (error) => {
          this.userID = null;
        }
      );
  }

  /**
   * Calls the handshake via the ChatService. The handshake endpoint
   * returns the user configuration data (e.g., userId, currentSession, sessionIds).
   */
  async handshake(uid: string): Promise<void> {
    // Call the handshake method in ChatService.
    await this.chatService.handShake(uid);

    if (this.chatService.allProjects && this.chatService.allProjects.length > 0) {
      this.defaultProject = this.chatService.allProjects[0];
      this.projectList = this.chatService.allProjects.slice(1);
    }
    this.selectedProject = this.chatService.currentProject;

    setTimeout(() => {
      //await delay(5000);
      this.loadUserSessions();
    }, 500);

    this.displayDefaultProject();
    this.projectsLoaded = true;
  }

  /**
   * Load sessions for the logged-in user using their sub.
   */
  loadUserSessions(): void {
    if (!this.userID) {
      console.error('No userID found, cannot load sessions.');
      return;
    }

    // Call the sessions endpoint using the userID (sub)
    this.http.get<Conversation[]>(`http://localhost:5186/sessions/user/${this.userID}`)
      .subscribe(
        (data) => {
          this.conversations = data.map((session) => this.createConversationFromSession(session));

          if (this.conversations.length > 0) {
            const lastSession = this.conversations.find((c) => c.id === this.sessionId)
              || this.conversations[0];
            this.selectConversation(lastSession);
          }
        },
        (error) => console.error('Error loading sessions:', error)
      );
  }

  restoreSession(): void {
    const storedSessionId = localStorage.getItem('sessionId');
    if (storedSessionId) {
      this.sessionId = parseInt(storedSessionId, 10);
    }
  }

  private createConversationFromSession(session: any): Conversation {
    const messages = session.messages.map((msg, index) => ({
      sender: msg.sender,
      text: msg.text,
      timestamp: new Date(msg.timestamp),
      originalIndex: index
    })) || [];

    return {
      id: session.id,
      title: session.title || 'New Session',
      messages: this.sortMessages(messages)
    };
  }

  //Methods for sending and displaying messages
  sendMessage(): void {
    if (!this.userID) {
      console.error('Error: No userID found, cannot send message.');
      return;
    }

    if (this.newMessage.trim() && this.sessionId && this.selectedConversation) {

      if (this.selectedTab === '1') {
        this.sendDefaultProjectMessage();
        return;
      } else {
        const originalSessionId = this.sessionId; // Store original ID where we are sending the message to

        const message: Message = {
          sender: 'Me',
          text: this.newMessage,
          timestamp: new Date()
        };

        this.selectedConversation.messages.push(message);
        this.selectedConversation.messages = this.sortMessages(this.selectedConversation.messages);
        this.ensureScrollToBottom();

        const stories = this.droppedStories.map(({  id, title, description, background, includeDescription, includeBackground}) => ({
          id: id,  // Rename uniqueId to id
          title: title,
          description: includeDescription ? description : '',
          background: includeBackground ? background : [],
          sub: this.userID
        }));

        // ✅ Debug log before sending request
        console.log('Sending API request with payload:', {
          sessionId: this.sessionId,
          userId: this.userID,
          prompt: this.newMessage,
          attachedFiles: stories,
          includeOnlySelected: stories.length > 0
        });

        this.isLoading = true;

        this.http.post<{ response: string }>(
          `http://localhost:5186/api/openai-chat/chat`,
          {
            sessionId: this.sessionId,
            userId: this.userID,
            prompt: this.newMessage,
            stories: stories,
            includeOnlySelected: stories.length > 0
          }
        ).subscribe(
          (response) => {
            const targetConversation = this.conversations.find((convo) => convo.id === originalSessionId);
            if (targetConversation ) {
              const botMessage: Message = {
                sender: 'bot',
                text: response.response,
                timestamp: new Date()
              };
              this.selectedConversation.messages.push(botMessage);
              this.selectedConversation.messages = this.sortMessages(this.selectedConversation.messages);

              if (this.selectedConversation && this.selectedConversation.id === originalSessionId) {
                this.ensureScrollToBottom();
              }

              this.updateSessionTitle(this.sessionId);
            }
            this.isLoading = false;
          },
          (error) => {
            console.error('Error sending message to OpenAI:', error);
            this.isLoading = false;
          }
        );
      }

      this.newMessage = '';
      this.selectedConversations = [];
      this.droppedStories = [];
      this.clearStory();
    }
  }

  sendDefaultProjectMessage(): void {
    const message: Message = {
      sender: 'Me',
      text: this.newMessage,
      timestamp: new Date()
    };

    this.selectedConversation.messages.push(message);
    this.selectedConversation.messages = this.sortMessages(this.selectedConversation.messages);
    this.ensureScrollToBottom();

    this.isLoading = true;

    this.chatService.defaultChat(this.sessionId!, this.userID!, this.newMessage).subscribe({
      next: (res) => {
        const botMsg: Message = {
          sender: 'bot',
          text: res.response,
          timestamp: new Date()
        };

        this.selectedConversation.messages.push(botMsg);
        this.selectedConversation.messages = this.sortMessages(this.selectedConversation.messages);
        this.updateSessionTitle(this.sessionId);
        this.ensureScrollToBottom();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error in default chat:', err);
        this.isLoading = false;
      }

    });

    this.newMessage = '';
    this.selectedConversations = [];
    this.droppedStories = [];
    this.clearStory();
  }

  sortMessages(messages: Message[]): Message[] {
    if (!messages || messages.length === 0) {
      return [];
    }
    return messages.sort((a, b) => {
      const timeA = a.timestamp instanceof Date ? a.timestamp : new Date(a.timestamp);
      const timeB = b.timestamp instanceof Date ? b.timestamp : new Date(b.timestamp);

      try {
        const timeCompare = timeA.getTime() - timeB.getTime();
        if (timeCompare !== 0) { // Sort by timestamp first
          return timeCompare;
        } else if (a.sender !== b.sender) { // Else check sender
          return a.sender === 'bot' ? 1 : -1; // Handle ordering for bot messages by changing position
        }
        return (a.originalIndex || 0) - (b.originalIndex || 0);
      } catch (error) {
        console.error('Error sorting messages:', error);
        return 0;
      }
    });
  }

  stripHtmlTags(str: string): string {
    return str.replace(/<\/?[^>]+(>|$)/g, '');
  }
  // End of methods for sending and displaying messages

  selectedTab: string = '';

  setActiveTab(tabId: string): void {
    this.selectedTab = tabId;
    if (this.projectsLoaded) {
      if (tabId === '1' && this.defaultProject) {
        this.selectedProject = this.defaultProject;
        this.displayDefaultProject();
      }
    }
  }

  filterTabs: ITabBarTab[] = [{
    key: '1',
    name: 'Sessions',
    icon: 'icon-menu_story'
  }, {
    key: '2',
    name: 'Projects',
    icon: 'icon-timeless'
  }];

  //Methods for loading projects
  loadProjects(): void {
    this.http.get<any[]>(`http://localhost:5186/project/GetProjectsByUserId/${this.userID}`)
      .subscribe(
        (projects) => {
          if (projects && projects.length > 0) {
            this.defaultProject = projects[0];
            this.projectList = projects.slice(1);
            this.projectsLoaded = true;

            if (this.selectedTab === '1') {
              this.displayDefaultProject();
            }
          } else {
            this.defaultProject = null;
            this.projectList = [];
            this.defaultProjectSessions = [];
          }
        },
        (error) => console.error('Error fetching projects:', error)
      );
  }

  displayDefaultProject(): void {
    if (this.defaultProject && this.defaultProject.sessions) {
      const conversationsMap = new Map<number, Conversation>();
      this.conversations.forEach((convo) => {
        conversationsMap.set(convo.id, convo);
      });
      this.defaultProjectSessions = this.defaultProject.sessions.map((session) => {
        // Check if we already have this conversation with updated messages
        if (conversationsMap.has(session.id)) {
          return conversationsMap.get(session.id);
        } else {
          return this.createConversationFromSession(session);
        }
      });

      this.filteredConversations = [...this.conversations];

      if (this.conversations.length > 0) {
        const savedSessionId = localStorage.getItem('sessionId');
        if (savedSessionId) {
          const sessionToSelect = this.conversations.find((c) => c.id === parseInt(savedSessionId)) || this.conversations[0];
          this.selectConversation(sessionToSelect);
        } else {
          this.selectConversation(this.conversations[0]);
        }
      }
    } else {
      console.error('No default project or sessions found.');
    }
  }
  //End of methods for loading projects

  //Methods for creating, selecting and deleting projects
  createProject(title: string) {
    if (!this.userID) {
      console.error('❌ Error: User ID is missing. Ensure handshake is completed.');
      return;
    }

    const requestBody = {
      sub: this.userID, // Send user ID in the body
      title: title      // Send title in the body
    };

    this.http.post(`http://localhost:5186/project/CreateProject`, requestBody)
      .subscribe({
        next: async () => {
          await this.chatService.handShake(this.userID!);

          const currentTab = this.selectedTab;

          if (this.chatService.allProjects && this.chatService.allProjects.length > 0) {
            this.defaultProject = this.chatService.allProjects[0];
            this.projectList = this.chatService.allProjects.slice(1);
          }
          if (currentTab === '1') {
            this.displayDefaultProject();
          }
        },
        error: (error) => console.error('❌ Error creating project:', error)
      });
  }

  openAddProjectDialog() {
    const dialogData: ConfirmDialogData = {
      title: 'Confirm',
      message: 'Are you sure you want to add a new project?',
      placeholder: 'Name project',
      showTextInput: true,
      confirmButtonLabel: 'Yes!'
    };

    this.dialog.open(ConfirmDialog, { data: dialogData })
      .afterClosed().subscribe((result) => {
      if (result) {
        this.createProject(result.text);
      }
    });
  }

  selectProject(project: ProjectDTO): void {
    this.selectedProject = project;
    console.log('Selected project:', project.title);

    if (!this.userID || !this.selectedProject) {
      console.error("Missing userID or project to update.");
      return;
    }


    const url = `http://localhost:5186/api/User/set-current-project?sub=${this.userID}&currentProjectId=${this.selectedProject.id}`;

    this.http.post(url, {}).subscribe({
      next: () => {
        console.log(`✅ Updated current project to: ${this.selectedProject.title}`);
      },
      error: (err) => {
        console.error('❌ Failed to update current project:', err);
      }
    });
  }

  deleteProject(project: ProjectDTO): void {
    if (!this.userID) {
      console.error('No userID found, cannot delete project.');
      return;
    }

    if (this.selectedProject !== this.defaultProject) {
      this.http.delete<any>(
        `http://localhost:5186/project/DeleteProject/${project.id}`
      ).subscribe(
        () => {
          this.loadProjects();
        },
        (error) => console.error('Error deleting project:', error)
      );
    } else if (this.selectedProject === this.defaultProject) {
      console.log('Cannot delete default project.');
    }
  }

  openDeleteProjectDialog(project: ProjectDTO) {
    const dialogData: ConfirmDialogData = {
      title: 'Confirm',
      message: 'Are you sure you want to delete this project?',
      confirmButtonLabel: 'Pretty sure'
    };

    this.dialog.open(ConfirmDialog, { data: dialogData })
      .afterClosed().subscribe((result) => {
      if (result) {
        this.deleteProject(this.selectedProject);
      }
    });
  }
  //End of methods for creating, selecting and deleting projects

  //Methods for creating, selecting and deleting chats
  startNewChat(): void {
    if (!this.userID) {
      console.error('No userID found, cannot start a new chat.');
      return;
    }

    const projectId = this.selectedTab === '1'
      ? this.defaultProject?.id
      : this.selectedProject?.id;

    if (!projectId) {
      return;
    }

    const payload = { projectId: projectId};

    this.isLoading = true;

    this.http.post<any>(
      `http://localhost:5186/sessions/create`,
      payload,
      { headers: { 'Content-Type': 'application/json' } }
    ).subscribe(
      (newSession) => {
        const newSessionId = newSession.session_id;
        this.sessionId = newSession;
        this.loadUserSessions();
        this.loadProjects();
        this.isLoading = false;
      },
      (error) => {
        console.error('Error creating session:', error)
        this.isLoading = true;
      }
    );
  }

  selectConversation(convo: Conversation): void {
    if (this.selectionMode) {
      console.log("🛑 Selection mode is ON, can't switch conversations.");
      return; // Prevent switching while in selection mode
    }

    // If selection mode is OFF, allow switching to the selected conversation
    this.sessionId = convo.id;
    localStorage.setItem('sessionId', this.sessionId.toString());

    console.log('✅ Switched to conversation:', convo.title);

    this.http.get<Conversation>(`http://localhost:5186/sessions/${convo.id}`)
      .subscribe(
        (freshSession) => {
          // Update the selected conversation with the latest from server
          this.selectedConversation = this.createConversationFromSession(freshSession);

          // Update the copy in the conversations array
          const index = this.conversations.findIndex((c) => c.id === convo.id);
          if (index !== -1) {
            this.conversations[index] = this.selectedConversation;
          }

          // Update in default project sessions if present
          const defaultIndex = this.defaultProjectSessions.findIndex((c) => c.id === convo.id);
          if (defaultIndex !== -1) {
            this.defaultProjectSessions[defaultIndex] = this.selectedConversation;
          }

          if (this.isSearchBarActive && this.currentSearchTerm) {
            // Apply highlight
            setTimeout(() => {
              this.applyHighlight(this.currentSearchTerm);
              this.scrollToFirstMatch();
            }, 200);
          } else {
            this.ensureScrollToBottom();
          }
        },
        (error) => console.error('Error fetching session:', error)
      );
  }
  deleteChat(convo: Conversation): void {
    if (!this.userID) {
      console.error('No userID found, cannot delete chat.');
      return;
    }

    const deletedSessionId = convo.id;

    this.http.delete<any>(
      `http://localhost:5186/sessions/delete/${convo.id}/${this.userID}`
    ).subscribe(
      () => {
        const deletedIndex = this.conversations.findIndex((c) => c.id === deletedSessionId);

        this.conversations = this.conversations.filter((c) => c.id !== deletedSessionId);

        this.filteredConversations = this.filteredConversations.filter((c) => c.id !== deletedSessionId);

        this.defaultProjectSessions = this.defaultProjectSessions.filter((c) => c.id !== deletedSessionId);

        if (this.projectList) {
          this.projectList.forEach((project) => {
            if (project.sessions) {
              project.sessions = project.sessions.filter((s) => s.id !== deletedSessionId);
            }
          });
        }

        if (this.selectedConversation && this.selectedConversation.id === deletedSessionId) {
          const nextIndex = deletedIndex < this.conversations.length ? deletedIndex : deletedIndex - 1;

          if (nextIndex >= 0 && this.conversations[nextIndex]) {
            this.selectConversation(this.conversations[nextIndex]);
          } else {
            this.selectedConversation = null;
            this.sessionId = null;
            localStorage.removeItem('sessionId');
          }
        }
        console.log('✅ Session deleted successfully:', deletedSessionId);
      },
      (error) => console.error('Error deleting session:', error)
    );
  }

  openDeleteConversationDialog() {
    const dialogData: ConfirmDialogData = {
      title: 'Confirm',
      message: 'Are you sure you want to delete this conversation?',
      confirmButtonLabel: 'Pretty sure'
    };

    this.dialog.open(ConfirmDialog, { data: dialogData })
      .afterClosed().subscribe((result) => {
      if (result) {
        this.deleteChat(this.selectedConversation);
      }
    });
  }
  //End of methods for creating, selecting and deleting chats

  //Methods for adding and removing stories
  openConfirmDialogForDroppedStory(story: StoryDTO): void {
    const dialogRef = this.dialog.open(ConfirmDialog, {
      data: {
        title: 'Confirm Story Addition',
        message: `You dropped the story "${story.title}". Press confirm to add it.`,
        confirmButtonLabel: 'Confirm'
      },
      width: '400px'
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        console.log('✅ User confirmed the selection.');
        if (this.selectedConversation) {
          const storyMessage: Message = {
            sender: 'system',
            text: `Story "${story.title}" added.`,
            timestamp: new Date()
          };
          this.selectedConversation.messages.push(storyMessage);
          this.selectedConversation.messages = this.sortMessages(this.selectedConversation.messages);
          this.ensureScrollToBottom();
        }
      } else {
        console.log('❌ User canceled.');
      }
    });
  }

  updateStory(story: StoryDTO): void {
    // Find the story in the droppedStories list and update its properties
    const foundStory = this.droppedStories.find(s => s.id === story.id);
    if (foundStory) {
      foundStory.includeDescription = story.includeDescription;
      foundStory.includeBackground = story.includeBackground;
      console.log('✅ Updated Story:', foundStory);
    }
  }

  removeStory(story: StoryDTO): void {
    const index = this.droppedStories.findIndex(s => s.id === story.id);
    if (index !== -1) {
      this.droppedStories.splice(index, 1);
    }

    if (this.selectedConversation && this.selectedConversation.messages) {
      this.selectedConversation.messages = this.selectedConversation.messages.filter(
        (msg) => (msg.sender !== 'system')
      );
    }
  }

  clearStory(): void {
    this.chatService.clearStory();
  }

  openStoryList(): void {
    const projectId = this.selectedProject.id;
    if (!projectId) return;

    this.chatService.fetchCurrentProjectStories(projectId).subscribe({
      next: (stories) => {
        this.currentProjectStories = stories;
        console.log('📚 Stories for current project:', this.currentProjectStories);
        const dialogRef = this.dialog.open(StoryListDialogComponent, {
          data: { stories: this.currentProjectStories },
          width: '500px'
        });

        dialogRef.afterClosed().subscribe((result) => {
          if (result) {
            // add function if needed
          }
        });
      },
      error: (error) => {
        console.error('❌ Failed to fetch stories:', error);
      }
    });
  }

  searchConversations(event: KeyboardEvent): void {
    const searchInput = (document.getElementById('search-bar') as HTMLInputElement);
    const searchTerm = searchInput?.value?.trim() || '';

    this.isSearchBarActive = searchTerm.length > 0;
    this.currentSearchTerm = searchTerm;

    if (!searchTerm) {
      this.loadUserSessions(); // Reload the original list if search term is empty
      this.loadProjects();
      return;
    }

    this.defaultProjectSessions = this.defaultProjectSessions.filter((convo) => {
      return convo.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        convo.messages.some((msg) => msg.text?.toLowerCase().includes(searchTerm.toLowerCase()));
    });

    if (this.projectList) {
      this.projectList = this.projectList.map((project) => {
        const filteredSessions = project.sessions.filter((session) =>
          session.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
          session.messages.some((msg) => msg.text?.toLowerCase().includes(searchTerm.toLowerCase()))
        );
        return {
          ...project,
          sessions: filteredSessions
        };
      });
      this.projectList = this.projectList.filter((project) => project.sessions.length > 0);
    }
    this.applyHighlight(searchTerm);
  }

  applyHighlight(searchTerm: string): void {
    const messageElements = document.getElementsByClassName('message-text');

    Array.from(messageElements).forEach((element) => {
      element.innerHTML = element.innerHTML.replace(/<mark>(.*?)<\/mark>/g, '$1');
    });

    if (searchTerm) {
      try {
        const escapedSearchTerm = searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');

        Array.from(messageElements).forEach((element) => {
          const tempDiv = document.createElement('div');
          tempDiv.innerHTML = element.innerHTML;

          const walkNodes = (node) => {
            if (node.nodeType === 3) {
              if (node.textContent.toLowerCase().includes(searchTerm.toLowerCase())) {
                const regex = new RegExp(`(${escapedSearchTerm})`, 'gi');
                const newText = node.textContent.replace(regex, '<mark>$1</mark>');

                const span = document.createElement('span');
                span.innerHTML = newText;
                node.parentNode.replaceChild(span, node);
              }
            } else if (node.nodeType === 1) {
              if (node.nodeName !== 'MARK') {
                Array.from(node.childNodes).forEach(walkNodes);
              }
            }
          };
          Array.from(tempDiv.childNodes).forEach(walkNodes);
          element.innerHTML = tempDiv.innerHTML;
        });
      } catch (error) {
        console.error('Error highlighting search term:', error);
      }
    }
  }

  toggleSearchBarVisibility() {
    this.isSearchBarVisible = !this.isSearchBarVisible;

    if (!this.isSearchBarVisible) {
      this.currentSearchTerm = '';
      this.isSearchBarActive = false;
      this.filteredConversations = [...this.conversations];
      this.applyHighlight('');
    }
  }

  scrollToFirstMatch(): void {
    if (!this.currentSearchTerm) return;
    setTimeout(() => {
      const markedElements = document.getElementsByTagName('mark');
      if (markedElements.length > 0) {
        const firstMatch = markedElements[0];

        let messageContainer = firstMatch;
        while (messageContainer && !messageContainer.classList.contains('message')) {
          messageContainer = messageContainer.parentElement;
        }

        if (messageContainer) {
          messageContainer.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
      }
    }, 200);
  }

  scrollToBottom(): void {
    if (this.chatContainer) {
      const chat = this.chatContainer.nativeElement;
      chat.scrollTo({
        top: chat.scrollHeight,
        behavior: 'smooth'
      });

      setTimeout(() => {
        this.showScrollButton = false;
      }, 100);
    }
  }

  scrollListener(): void {
    this.chatContainer.nativeElement.addEventListener('scroll', () => {
      const chat = this.chatContainer.nativeElement;
      const button = document.getElementById('scrollButton');
      const isAtBottom = Math.abs(chat.scrollHeight - chat.scrollTop - chat.clientHeight) < 1;
      this.showScrollButton = !isAtBottom;

      if (button) {
        button.style.display = this.showScrollButton ? 'block' : 'none';
      }
    });
  }

  ensureScrollToBottom(): void {
    if (this.selectedConversation?.messages?.length > 0) {
      requestAnimationFrame(() => {
        this.scrollToBottom();
      });
    }
  }
  //End of methods for searching in conversations and projects

  //Methods in header
  updateSessionTitle(sessionId: number): void {
    this.http.get<Conversation>(`http://localhost:5186/sessions/${sessionId}`)
      .subscribe(
        (updatedSession) => {
          const index = this.conversations.findIndex((convo) => convo.id === sessionId);
          if (index !== -1) {
            this.conversations[index].title = updatedSession.title;
          }
          if (this.selectedConversation?.id === sessionId) {
            this.selectedConversation.title = updatedSession.title;
          }
          if (this.defaultProject?.sessions) {
            const session = this.defaultProject.sessions.find(s => s.id === sessionId);
            if (session) {
              session.title = updatedSession.title;
            }
          }
          if (this.projectList) {
            this.projectList.forEach((project) => {
              if (project.sessions) {
                const session = project.sessions.find(s => s.id === sessionId);
                if (session) {
                  session.title = updatedSession.title;
                }
              }
            });
          }
        },
        (error) => console.error('Error updating session title:', error)
      );
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }
  //End of methods in header

}
