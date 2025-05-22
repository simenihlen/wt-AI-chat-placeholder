import { MaterialModule } from 'app/modules/material.module';
import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommonPipesModule } from 'app/modules/common-pipes/common-pipes.module';
import { MarkdownPipe } from './chat-main-page/markdown-pipe';
import { ChatMainPageComponent } from './chat-main-page/chat-main-page.component';
import { CommonDirectivesModule } from 'app/modules/common-directives/common-directives.module';
import {CommonUIComponentsModule} from '../../modules/common-ui-components/common-ui-components.module';
import { StoryListDialogComponent } from './chat-main-page/components/story-list-dialog/story-list-dialog.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule,
    CommonPipesModule,
    CommonDirectivesModule,
    CommonUIComponentsModule,
    StoryListDialogComponent
  ],
  declarations: [ ChatMainPageComponent, MarkdownPipe ],
  exports: [ ChatMainPageComponent]
})
export class ChatModule {}
