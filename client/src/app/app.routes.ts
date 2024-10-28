import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { LikedListsComponent } from './liked-lists/liked-lists.component';
import { MessagesComponent } from './messages/messages.component';
import { authGuard } from './_guards/auth.guard';

export const routes: Routes = [
    {path: '', component: HomeComponent},
    {path: '', 
        runGuardsAndResolvers: 'always', 
        canActivate: [authGuard],
        children: [
            {path: 'members', component: MemberListComponent,},
            {path: 'members/:id', component: MemberDetailComponent},
            {path: 'liked-members', component: LikedListsComponent},
            {path: 'messages', component: MessagesComponent},
        ]
    },
    {path: '*', component: HomeComponent, pathMatch: 'full'}
];
