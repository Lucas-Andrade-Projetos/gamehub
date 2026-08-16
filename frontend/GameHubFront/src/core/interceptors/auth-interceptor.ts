import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AccountService } from '../services/account-service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AccountService).currentUser()?.token;

  if (!token) return next(req);

  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
