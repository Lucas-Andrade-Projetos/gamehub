import { Routes } from '@angular/router';
import { authGuard } from '../core/guards/auth-guard';
import { BatalhaRural } from '../features/games/batalha-rural/batalha-rural';
import { Home } from '../features/home/home';

export const routes: Routes = [
    { path: '', redirectTo: 'home', pathMatch:'full'},

    { path: 'home', component: Home },
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [authGuard],
        children: [
            { path: 'batalhaRural', component: BatalhaRural }
        ]
    },
    {path: '**', redirectTo: 'home'}
];
