import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;
  members = signal<Member[]>([]);

  getMembers() {
    return this.http.get<Member[]>(this.baseUrl+ 'users/get-all-users').subscribe({
      next: members => this.members.set(members)
    });
  }

  getMember(username: string) {
    const member = this.members().find(u => u.userName === username);
    if(member !== undefined) return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/get-user-by-username/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users/update-user', member).pipe(
      tap(() => {
        this.members.update(members => members.map(m => m.userName === member.userName ? member : m))
      })
    )
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photo: Photo) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.id).pipe(
      tap(() => {
        this.members.update(members => members.map(m => {
          if(m.photos.includes(photo)) {
            m.photos = m.photos.filter(m => m.id !== photo.id)
          }
          return m
        }))
      })
    );
  }
}
