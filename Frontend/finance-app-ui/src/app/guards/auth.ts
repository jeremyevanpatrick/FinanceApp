import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Storage } from '../services/storage';
import { jwtDecode } from 'jwt-decode';
import { JwtPayload } from '../models/jwt-payload';

export const authGuard: CanActivateFn = () => {
  const storage = inject(Storage);
  const router = inject(Router);

  const jwtTokenString = storage.get('jwt_token') ?? '';

  try {
    const jwtPayload: JwtPayload = jwtDecode<JwtPayload>(jwtTokenString);
    const tokenExpirationDateMs = jwtPayload.exp * 1000;
    if (tokenExpirationDateMs > Date.now()) {
      return true;
    }
  } catch {}

  return router.createUrlTree(['/login']);
};
