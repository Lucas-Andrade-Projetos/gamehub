import { Routes } from '@angular/router';
import { Login } from '../features/account/login/login';
import { Register } from '../features/account/register/register';
import { authGuard } from '../core/guards/auth-guard';
import { LoginBtn } from '../features/login-btn/login-btn';

export const routes: Routes = [
    { path: '', component: LoginBtn},
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [authGuard],
        children: [
            // { path: 'batalhaRural', component: BatalhaRural }
        ]
    },
    { path: 'login', component: Login },
    { path: 'register', component: Register }
];
