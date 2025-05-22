import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
//import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ChatService } from './chat.service';
import {HttpClientModule} from '@angular/common/http';  // Import ChatService

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    //AppRoutingModule
    FormsModule,
    HttpClientModule // Add this to enable API calls
  ],
  providers: [ChatService],
  bootstrap: [AppComponent]
})
export class AppModule { }
