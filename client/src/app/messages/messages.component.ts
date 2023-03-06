import { Component, OnInit } from '@angular/core';
import { PageChangedEvent } from 'ngx-bootstrap/pagination';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css'],
})
export class MessagesComponent implements OnInit {
  messages?: Message[];
  pagination?: Pagination;
  container = 'Unread';
  pageNumber = 1;
  pageSzie = 5;
  loading = false;
  constructor(private messageService: MessageService) {}

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.loading = true;
    this.messageService
      .getMessages(this.pageNumber, this.pageSzie, this.container)
      .subscribe({
        next: (res) => {
          console.log(res);
          this.messages = res.result;
          this.pagination = res.pagination;
          this.loading = false;
        },
      });
  }

  pageChanged($event: PageChangedEvent) {
    if ($event.page !== this.pageNumber) {
      this.pageNumber = $event.page;
      this.loadMessages();
    }
  }

  deleteMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe({
      next: () => {
        const index = this.messages?.findIndex((m) => m.id === id);
        if (index !== undefined) this.messages?.splice(index, 1);
      },
    });
  }
  getMemberLink(username: string) {
    return `/members/${username}`;
  }
}
