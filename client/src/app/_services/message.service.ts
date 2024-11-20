import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { PaginatedResult } from '../_models/pagination';
import { Message } from '../_models/message';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private http = inject(HttpClient);

  baseUrl = environment.apiUrl;

  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = setPaginationHeaders(pageNumber, pageSize);

    params = params.append('Container', container);

    return this.http.get<Message[]>(this.baseUrl + 'messages/get-messages-for-user', {observe: 'response', params}).subscribe({
      next: response => setPaginatedResponse(response, this.paginatedResult),
      error: error => console.log(error)
    })
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }

  sendMessage(username: string, content: string) {
    return this.http.post<Message>(this.baseUrl + 'messages/create-message', {recipientUsername: username, content})
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/delete-message/' + id);
  }
}
