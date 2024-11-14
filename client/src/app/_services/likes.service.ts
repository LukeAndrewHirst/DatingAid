import { inject, Injectable, signal } from '@angular/core';

import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class LikesService {
  private http = inject(HttpClient);

  baseUrl = environment.apiUrl;
  likeIds = signal<number[]>([]);
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);

  toggleLike(targetId: number) {
    return this.http.post(`${this.baseUrl}likes/${targetId}`, {});
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = setPaginationHeaders(pageNumber, pageSize);

    params = params.append('predicate', predicate);

    return this,this.http.get<Member[]>(`${this.baseUrl}likes/get-user-likes`, {observe: 'response', params}).subscribe({
      next: response => setPaginatedResponse(response, this.paginatedResult),
      error: error => console.log(error)
    })
  }

  getLikeIds() {
    return this.http.get<number[]>(`${this.baseUrl}likes/get-like-list`).subscribe({
      next: ids => this.likeIds.set(ids)
    })
  }
}
