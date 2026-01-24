import { Routes } from '@angular/router';
import { Home } from '../features/home/home';
import { Login } from '../features/account/login/login';
import { Register } from '../features/account/register/register';
import { authGuard } from '../core/guards/auth-guard';
import { WelcomingText } from '../layout/welcoming-text/welcoming-text';

export const routes: Routes = [
    { path: '', component: WelcomingText },
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [authGuard],
        children: [
            // { path: 'batalhaRural', component: BatalhaRural }
        ]
    },
    { path: 'welcome', component: WelcomingText },
    { path: 'login', component: Login },
    { path: 'register', component: Register },
    { path: '**', component: Home }
];
