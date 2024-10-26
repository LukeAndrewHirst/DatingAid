import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';

@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [FormsModule, BsDropdownModule],
  templateUrl: './navigation.component.html',
  styleUrl: './navigation.component.css'
})
export class NavigationComponent {
  accountservice = inject(AccountService);

  model: any = {};

  login(){
    this.accountservice.login(this.model).subscribe({
      next: response => {console.log(response);},
      error: error => {console.log(error);}
    });
  }

  logout(){
    this.accountservice.logout()
  }
}
