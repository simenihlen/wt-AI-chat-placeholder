import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';
import { ChatMainPageComponent } from './chat-main-page.component';
import { ChatService } from './chat.service.tab';
import { MarkdownPipe } from './markdown-pipe';
import { Pipe, PipeTransform } from '@angular/core';
import { StoryDataService } from 'app/storyview/story-data.service';
import { BackgroundInfoDataService } from 'app/shared-components/background-info/services/background-info-data.service';
import { of } from 'rxjs';

// Mock translate pipe
@Pipe({
  name: 'translate'
})
class MockTranslatePipe implements PipeTransform {
  transform(value: any): any {
    return value;
  }
}

describe('ChatMainPageComponent', () => {
  let component: ChatMainPageComponent;
  let fixture: ComponentFixture<ChatMainPageComponent>;
  let httpMock: HttpTestingController;
  let chatService: jasmine.SpyObj<ChatService>;
  let storyDataService: jasmine.SpyObj<StoryDataService>;
  let backgroundInfoDataService: jasmine.SpyObj<BackgroundInfoDataService>;

  beforeEach(async () => {
    const chatServiceSpy = jasmine.createSpyObj('ChatService',
      ['handShake', 'sendMessageToGpt', 'clearStory', 'handShakeStory', 'fetchCurrentProjectStories', 'defaultChat']);
    const storyDataServiceSpy = jasmine.createSpyObj('StoryDataService',
      ['getGeneral', 'getStory']);
    const backgroundInfoDataServiceSpy = jasmine.createSpyObj('BackgroundInfoDataService',
      ['getBgTabHeaders', 'getBgTabItems', 'getBgTabItemsSubject']);

    backgroundInfoDataServiceSpy.getBgTabItemsSubject.and.returnValue(of([]));
    chatServiceSpy.fetchCurrentProjectStories.and.returnValue(of([]));
    storyDataServiceSpy.getGeneral.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, FormsModule],
      declarations: [
        ChatMainPageComponent,
        MarkdownPipe,
        MockTranslatePipe
      ],
      providers: [
        { provide: ChatService, useValue: chatServiceSpy },
        { provide: StoryDataService, useValue: storyDataServiceSpy },
        { provide: BackgroundInfoDataService, useValue: backgroundInfoDataServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ChatMainPageComponent);
    component = fixture.componentInstance;

    component.filteredConversations = [];
    component.conversations = [];

    httpMock = TestBed.inject(HttpTestingController);
    chatService = TestBed.inject(ChatService) as jasmine.SpyObj<ChatService>;
    storyDataService = TestBed.inject(StoryDataService) as jasmine.SpyObj<StoryDataService>;
    backgroundInfoDataService = TestBed.inject(BackgroundInfoDataService) as jasmine.SpyObj<BackgroundInfoDataService>;
  });

  afterEach(() => {
    if (httpMock) {
      httpMock.verify();
    }
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should get user ID on init', fakeAsync(() => {
    const mockUser = { sub: 'test-user-id' };
    spyOn(component, 'loadUserSessions');
    fixture.detectChanges();
    const req = httpMock.expectOne('https://news3.wolftech.no/idsrv4/connect/userinfo');
    expect(req.request.method).toBe('GET');
    req.flush(mockUser);
    const projectsReq = httpMock.expectOne('http://localhost:5186/project/GetProjectsByUserId/test-user-id');
    expect(projectsReq.request.method).toBe('GET');
    projectsReq.flush([{ id: 1, name: 'Test Project' }]);
    expect(component.userID).toBe('test-user-id');
    tick(500);
  }));

  it('should load user sessions', fakeAsync(() => {
    component.userID = 'test-user-id';
    const mockSessions = [{
      id: 1,
      title: 'Test Session 1',
      messages: [
        { sender: 'Me', text: 'Hello', timestamp: '2021-01-01T00:00:00Z' },
        { sender: 'AI', text: 'Hi', timestamp: '2021-01-01T00:01:00Z' }
      ]
    }, {
      id: 2,
      title: 'Test Session 2',
      messages: [
        { sender: 'Me', text: 'How are you', timestamp: '2021-01-01T00:00:00Z' },
        { sender: 'AI', text: 'I am a bot', timestamp: '2021-01-01T00:01:00Z' }
      ]
    }];
    component.loadUserSessions();
    const req = httpMock.expectOne('http://localhost:5186/sessions/user/test-user-id');
    expect(req.request.method).toBe('GET');
    req.flush(mockSessions);
    tick();
    expect(component.conversations.length).toBe(2);
    expect(component.conversations[0].title).toBe('Test Session 1');
    expect(component.conversations[1].title).toBe('Test Session 2');
    expect(component.conversations[0].messages.length).toBe(2);
    expect(component.conversations[1].messages.length).toBe(2);
    expect(component.conversations[0].messages[0].sender).toBe('Me');
    expect(component.conversations[0].messages[1].sender).toBe('AI');
    expect(component.conversations[0].messages[0].text).toBe('Hello');
    expect(component.conversations[0].messages[0].timestamp instanceof Date).toBe(true);
  }));

  it('should delete a chat', fakeAsync(() => {
    component.userID = 'test-user-id';
    component.conversations = [
      { id: 1, title: 'Test Chat', messages: [] },
      { id: 2, title: 'Another Chat', messages: [] }
    ];
    component.selectedConversation = component.conversations[0];
    component.sessionId = 1;
    spyOn(component, 'deleteChat').and.callFake((conversation) => {
      component.conversations = component.conversations.filter((c) => c.id !== conversation.id);
      component.loadUserSessions();
    });
    spyOn(component, 'loadUserSessions');

    component.deleteChat(component.conversations[0]);
    expect(component.conversations.length).toBe(1);
    expect(component.conversations[0].title).toBe('Another Chat');
    expect(component.loadUserSessions).toHaveBeenCalled();
  }));

  it('should filter conversations when searching', () => {
    component.conversations = [
      { id: 1, title: 'Apple chat', messages: [] },
      { id: 2, title: 'Banana talk', messages: [] }
    ];
    const mockEvent = {
      target: { value: 'apple' }
    } as unknown as KeyboardEvent;
    component.searchConversations(mockEvent);
    expect(component.conversations.length).toBe(1);
    expect(component.conversations[0].title).toBe('Apple chat');
  });

  it('should send a message', fakeAsync(() => {
    //TODO
  }));

  it('messages in session is sorted', () => {
    const messages = [
      { sender: 'Me', text: 'Hello', timestamp: new Date('2021-01-01T00:01:00Z') },
      { sender: 'AI', text: 'Hi', timestamp: new Date('2021-01-01T00:00:00Z') }
    ];
    const sortedMessages = component.sortMessages(messages);
    expect(sortedMessages[0].sender).toBe('AI');
    expect(sortedMessages[1].sender).toBe('Me');
  });
});
