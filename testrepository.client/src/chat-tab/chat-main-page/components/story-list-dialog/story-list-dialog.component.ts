import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { CurrentProjectStoriesDTO } from '../../../user.models.tab';

@Component({
  selector: 'app-story-list-dialog',
  templateUrl: './story-list-dialog.component.html',
  styleUrls: ['./story-list-dialog.component.scss'],
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule]
})
export class StoryListDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<StoryListDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { stories: CurrentProjectStoriesDTO[] }
  ) {}

  selectStory(story: CurrentProjectStoriesDTO): void {
    this.dialogRef.close(story);
  }

  closeDialog(): void {
    this.dialogRef.close();
  }
}
