import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { TitleCasePipe } from '@angular/common';

@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [FormsModule, BsDropdownModule, RouterLink, RouterLinkActive, TitleCasePipe],
  templateUrl: './navigation.component.html',
  styleUrl: './navigation.component.css'
})
export class NavigationComponent {
  private router = inject(Router);
  private toastr = inject(ToastrService);
  accountservice = inject(AccountService);

  model: any = {};

  login(){
    this.accountservice.login(this.model).subscribe({
      next: response => {this.router.navigateByUrl('/members');},
      error: error => {this.toastr.error(error.error); console.log(error)}
    });
  }

  logout(){
    this.accountservice.logout();
    this.router.navigateByUrl('/');
  }
}
