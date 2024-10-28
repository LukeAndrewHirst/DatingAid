import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

export const authGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toastr = inject(ToastrService);

  if(accountService.currentUser()) {
    return true;
  } else {
    toastr.error('It seems you’re not logged in. Please log in to access this page.');
    return false;
  }
};
