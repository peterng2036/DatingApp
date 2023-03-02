import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'client!ad!!!!';
  users: any;
  constructor(
    private httpClient: HttpClient,
    private accountService: AccountService
  ) {}

  ngOnInit(): void {
    this.getUsers();
    this.setCurrentUser();
  }

  private getUsers() {
    this.httpClient.get('https://localhost:5001/api/users').subscribe({
      next: (res) => {
        this.users = res;
      },
      error: (error) => {
        console.error(error);
      },
      complete: () => {
        console.log('Request has completed');
      },
    });
  }

  setCurrentUser() {
    const userJson = localStorage.getItem('user');
    if (!userJson) return;
    const user: User = JSON.parse(userJson);
    this.accountService.setCurrentUser(user);
  }
}
