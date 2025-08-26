import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { HasRoleDirective } from '../_directives/has-role.directive';

@Component({
    selector: 'app-navigation',
    imports: [FormsModule, BsDropdownModule, RouterLink, RouterLinkActive, HasRoleDirective],
    templateUrl: './navigation.component.html',
    styleUrl: './navigation.component.css'
})
export class NavigationComponent {
  private router = inject(Router);
  private toastr = inject(ToastrService);
  accountService = inject(AccountService);


  model: any = {};

  login(){
    this.accountService.login(this.model).subscribe({
      next: response => {this.router.navigateByUrl('/members');},
      error: error => {this.toastr.error(error.error); console.log(error)}
    });
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
