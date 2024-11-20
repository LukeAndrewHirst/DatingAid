import { Component, inject, input, OnInit, output, ViewChild } from '@angular/core';
import { Message } from '../../_models/message';
import { MessageService } from '../../_services/message.service';
import { TimeagoModule } from 'ngx-timeago';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [TimeagoModule, FormsModule],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css'
})
export class MemberMessagesComponent {
  @ViewChild('messageForm') messageForm?: NgForm;
  private messageService = inject(MessageService);

  username = input.required<string>();
  messages = input.required<Message[]>();
  updateMessages = output<Message>();

  messageContent = '';
  defaultDate = '0001-01-01T00:00:00';

  DateParse(){
    return new Date(Date.parse(this.defaultDate));
  }

  sendMessage() {
    this.messageService.sendMessage(this.username(), this.messageContent).subscribe({
      next: message => {
        this.updateMessages.emit(message);
        this.messageForm?.reset();
      }
    })
  }
}
