import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { Dashboard } from './components/dashboard/dashboard';
import { Profile } from './components/profile/profile';

export const routes: Routes = [
    { path: 'login', component: Login },
    { path: 'register', component: Register },
    { path: 'dashboard', component: Dashboard },
    { path: 'profile/:guid', component: Profile },
    
    // If the user goes to the base URL, send them to login
    { path: '', redirectTo: '/login', pathMatch: 'full' },
    
    // If they type a random URL that doesn't exist, send them to login
    { path: '**', redirectTo: '/login' }
];