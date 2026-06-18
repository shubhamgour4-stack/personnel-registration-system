import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { Dashboard } from './components/dashboard/dashboard';
import { Profile } from './components/profile/profile';
import { MftManagementComponent } from './components/mft-management/mft-management.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
    // Public Routes (no authentication required)
    { path: 'login', component: Login },
    { path: 'register', component: Register },
    
    // Protected Routes (require valid JWT token)
    // authGuard checks:
    // 1. Token exists in localStorage
    // 2. Token has not expired
    // If checks fail, user is redirected to /login with error message
    { path: 'dashboard', component: Dashboard, canActivate: [authGuard] },
    { path: 'mft-management', component: MftManagementComponent, canActivate: [authGuard] },
    { path: 'profile/:guid', component: Profile, canActivate: [authGuard] },
    
    // Default Routes
    // If the user goes to the base URL, send them to login
    { path: '', redirectTo: '/login', pathMatch: 'full' },
    
    // If they type a random URL that doesn't exist, send them to login
    { path: '**', redirectTo: '/login' }
];