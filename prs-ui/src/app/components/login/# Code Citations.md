# Code Citations

## License: unknown
https://github.com/ksukacha/survey/blob/92bba49c8484d76770e9add5d21833ebc542eb48/src/app/auth-intercepter.service.ts

```
## JWT Token Usage Analysis

### 1. Where JWT tokens are created
- **Backend only**: ASP.NET Core `AuthService.GenerateJwtToken()` method
- Returns token in HTTP response as `response.token` in `AuthResponseDto`

### 2. Where JWT tokens are stored
- **localStorage** only (no sessionStorage usage)
- Stored immediately after successful login in [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27)

### 3. All localStorage/sessionStorage keys used
- `'jwt_token'` – Standard key used in most locations
  - [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27) writes here
  - [PersonnelService.getHeaders()](prs-ui/src/app/services/personnel.service.ts#L17) reads here
  - [MasterDataService.getHeaders()](prs-ui/src/app/services/master-data.service.ts#L18) reads here
  - [Dashboard.onLogout()](prs-ui/src/app/components/dashboard/dashboard.ts#L56) deletes here

- `'token'` – Non-standard key used **only** in MftManagementComponent
  - [MftManagementComponent.fetchIngestionHistory()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L64) reads here
  - [MftManagementComponent.viewStagingDashboard()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L85) reads here
  - [MftManagementComponent.triggerReprocessing()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L106) reads here

### 4. All locations reading tokens
| Location | Storage Key | Method |
|----------|-------------|--------|
| PersonnelService | `'jwt_token'` | In `getHeaders()` method, used by search/profile methods |
| MasterDataService | `'jwt_token'` | In `getHeaders()` method, used by master data lookups |
| MftManagementComponent | `'token'` | Direct `localStorage.getItem()` in 3 methods |

### 5. All locations writing tokens
| Location | Storage Key | Trigger |
|----------|-------------|---------|
| [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27) | `'jwt_token'` | After successful authentication |

### 6. All locations deleting tokens
| Location | Storage Key | Trigger |
|----------|-------------|---------|
| [Dashboard.onLogout()](prs-ui/src/app/components/dashboard/dashboard.ts#L56) | `'jwt_token'` | User clicks logout |

---

## Critical Inconsistencies Identified

### 🔴 **CRITICAL BUG: Storage key mismatch**
- Login stores token as `'jwt_token'`
- MftManagementComponent reads from `'token'` (which doesn't exist)
- Falls back to empty string: `localStorage.getItem('token') || ''`
- **Result**: MFT API calls fail with 401 Unauthorized because no valid token is sent

### 🟡 **Code Duplication & Scattered Token Management**
- Token retrieval logic duplicated in multiple places:
  - `PersonnelService.getHeaders()` 
  - `MasterDataService.getHeaders()`
  - `MftManagementComponent.fetchIngestionHistory()`, `viewStagingDashboard()`, `triggerReprocessing()`
- Each location manually constructs Authorization headers
- No single source of truth for token handling
- Violates DRY principle

### 🟡 **No Centralized Token Service**
- No service responsible for token lifecycle (create, read, validate, delete)
- Logic scattered across components and services
- Difficult to audit and maintain

### 🟡 **No Token Validation**
- No check if token is expired before using
- Token lifetime is 60 minutes, but no validation of expiry
- Potential for silent failures with expired tokens

### 🟡 **No Error Handling for Missing Token**
- MFT component uses fallback empty string; others don't check for null
- Silent failures instead of explicit error messaging

---

## Recommended Single Standard Approach

### 1. **Create a dedicated `TokenService`**
Centralize all token operations:

```ts
@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly TOKEN_KEY = 'jwt_token'; // Single constant
  
  // Write token to storage
  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }
  
  // Read token from storage
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }
  
  // Delete token from storage
  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }
  
  // Check if token exists
  hasToken(): boolean {
    return !!this.getToken();
  }
  
  // Decode and check expiry (future)
  isTokenExpired(): boolean {
    // Parse JWT and check exp claim
  }
}
```

### 2. **Create an `AuthHttpInterceptor`**
Automatically attach token to all HTTP requests:

```ts
@Injectable()
export class AuthHttpInterceptor implements HttpInterceptor {
  constructor(private tokenService: TokenService) {}
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.tokenService.getToken();
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }
    return next.handle(req);
  }
}
```

### 3. **Standardize storage key**
-
```


## License: MIT
https://github.com/LuanMattos/circle/blob/6e63199ee0e2eac8f087ba1d0c6c5c8f00ee813e/src/app/core/auth/request.interceptor.ts

```
## JWT Token Usage Analysis

### 1. Where JWT tokens are created
- **Backend only**: ASP.NET Core `AuthService.GenerateJwtToken()` method
- Returns token in HTTP response as `response.token` in `AuthResponseDto`

### 2. Where JWT tokens are stored
- **localStorage** only (no sessionStorage usage)
- Stored immediately after successful login in [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27)

### 3. All localStorage/sessionStorage keys used
- `'jwt_token'` – Standard key used in most locations
  - [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27) writes here
  - [PersonnelService.getHeaders()](prs-ui/src/app/services/personnel.service.ts#L17) reads here
  - [MasterDataService.getHeaders()](prs-ui/src/app/services/master-data.service.ts#L18) reads here
  - [Dashboard.onLogout()](prs-ui/src/app/components/dashboard/dashboard.ts#L56) deletes here

- `'token'` – Non-standard key used **only** in MftManagementComponent
  - [MftManagementComponent.fetchIngestionHistory()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L64) reads here
  - [MftManagementComponent.viewStagingDashboard()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L85) reads here
  - [MftManagementComponent.triggerReprocessing()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L106) reads here

### 4. All locations reading tokens
| Location | Storage Key | Method |
|----------|-------------|--------|
| PersonnelService | `'jwt_token'` | In `getHeaders()` method, used by search/profile methods |
| MasterDataService | `'jwt_token'` | In `getHeaders()` method, used by master data lookups |
| MftManagementComponent | `'token'` | Direct `localStorage.getItem()` in 3 methods |

### 5. All locations writing tokens
| Location | Storage Key | Trigger |
|----------|-------------|---------|
| [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27) | `'jwt_token'` | After successful authentication |

### 6. All locations deleting tokens
| Location | Storage Key | Trigger |
|----------|-------------|---------|
| [Dashboard.onLogout()](prs-ui/src/app/components/dashboard/dashboard.ts#L56) | `'jwt_token'` | User clicks logout |

---

## Critical Inconsistencies Identified

### 🔴 **CRITICAL BUG: Storage key mismatch**
- Login stores token as `'jwt_token'`
- MftManagementComponent reads from `'token'` (which doesn't exist)
- Falls back to empty string: `localStorage.getItem('token') || ''`
- **Result**: MFT API calls fail with 401 Unauthorized because no valid token is sent

### 🟡 **Code Duplication & Scattered Token Management**
- Token retrieval logic duplicated in multiple places:
  - `PersonnelService.getHeaders()` 
  - `MasterDataService.getHeaders()`
  - `MftManagementComponent.fetchIngestionHistory()`, `viewStagingDashboard()`, `triggerReprocessing()`
- Each location manually constructs Authorization headers
- No single source of truth for token handling
- Violates DRY principle

### 🟡 **No Centralized Token Service**
- No service responsible for token lifecycle (create, read, validate, delete)
- Logic scattered across components and services
- Difficult to audit and maintain

### 🟡 **No Token Validation**
- No check if token is expired before using
- Token lifetime is 60 minutes, but no validation of expiry
- Potential for silent failures with expired tokens

### 🟡 **No Error Handling for Missing Token**
- MFT component uses fallback empty string; others don't check for null
- Silent failures instead of explicit error messaging

---

## Recommended Single Standard Approach

### 1. **Create a dedicated `TokenService`**
Centralize all token operations:

```ts
@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly TOKEN_KEY = 'jwt_token'; // Single constant
  
  // Write token to storage
  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }
  
  // Read token from storage
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }
  
  // Delete token from storage
  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }
  
  // Check if token exists
  hasToken(): boolean {
    return !!this.getToken();
  }
  
  // Decode and check expiry (future)
  isTokenExpired(): boolean {
    // Parse JWT and check exp claim
  }
}
```

### 2. **Create an `AuthHttpInterceptor`**
Automatically attach token to all HTTP requests:

```ts
@Injectable()
export class AuthHttpInterceptor implements HttpInterceptor {
  constructor(private tokenService: TokenService) {}
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.tokenService.getToken();
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }
    return next.handle(req);
  }
}
```

### 3. **Standardize storage key**
-
```


## License: unknown
https://github.com/ksukacha/survey/blob/92bba49c8484d76770e9add5d21833ebc542eb48/src/app/auth-intercepter.service.ts

```
## JWT Token Usage Analysis

### 1. Where JWT tokens are created
- **Backend only**: ASP.NET Core `AuthService.GenerateJwtToken()` method
- Returns token in HTTP response as `response.token` in `AuthResponseDto`

### 2. Where JWT tokens are stored
- **localStorage** only (no sessionStorage usage)
- Stored immediately after successful login in [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27)

### 3. All localStorage/sessionStorage keys used
- `'jwt_token'` – Standard key used in most locations
  - [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27) writes here
  - [PersonnelService.getHeaders()](prs-ui/src/app/services/personnel.service.ts#L17) reads here
  - [MasterDataService.getHeaders()](prs-ui/src/app/services/master-data.service.ts#L18) reads here
  - [Dashboard.onLogout()](prs-ui/src/app/components/dashboard/dashboard.ts#L56) deletes here

- `'token'` – Non-standard key used **only** in MftManagementComponent
  - [MftManagementComponent.fetchIngestionHistory()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L64) reads here
  - [MftManagementComponent.viewStagingDashboard()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L85) reads here
  - [MftManagementComponent.triggerReprocessing()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L106) reads here

### 4. All locations reading tokens
| Location | Storage Key | Method |
|----------|-------------|--------|
| PersonnelService | `'jwt_token'` | In `getHeaders()` method, used by search/profile methods |
| MasterDataService | `'jwt_token'` | In `getHeaders()` method, used by master data lookups |
| MftManagementComponent | `'token'` | Direct `localStorage.getItem()` in 3 methods |

### 5. All locations writing tokens
| Location | Storage Key | Trigger |
|----------|-------------|---------|
| [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27) | `'jwt_token'` | After successful authentication |

### 6. All locations deleting tokens
| Location | Storage Key | Trigger |
|----------|-------------|---------|
| [Dashboard.onLogout()](prs-ui/src/app/components/dashboard/dashboard.ts#L56) | `'jwt_token'` | User clicks logout |

---

## Critical Inconsistencies Identified

### 🔴 **CRITICAL BUG: Storage key mismatch**
- Login stores token as `'jwt_token'`
- MftManagementComponent reads from `'token'` (which doesn't exist)
- Falls back to empty string: `localStorage.getItem('token') || ''`
- **Result**: MFT API calls fail with 401 Unauthorized because no valid token is sent

### 🟡 **Code Duplication & Scattered Token Management**
- Token retrieval logic duplicated in multiple places:
  - `PersonnelService.getHeaders()` 
  - `MasterDataService.getHeaders()`
  - `MftManagementComponent.fetchIngestionHistory()`, `viewStagingDashboard()`, `triggerReprocessing()`
- Each location manually constructs Authorization headers
- No single source of truth for token handling
- Violates DRY principle

### 🟡 **No Centralized Token Service**
- No service responsible for token lifecycle (create, read, validate, delete)
- Logic scattered across components and services
- Difficult to audit and maintain

### 🟡 **No Token Validation**
- No check if token is expired before using
- Token lifetime is 60 minutes, but no validation of expiry
- Potential for silent failures with expired tokens

### 🟡 **No Error Handling for Missing Token**
- MFT component uses fallback empty string; others don't check for null
- Silent failures instead of explicit error messaging

---

## Recommended Single Standard Approach

### 1. **Create a dedicated `TokenService`**
Centralize all token operations:

```ts
@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly TOKEN_KEY = 'jwt_token'; // Single constant
  
  // Write token to storage
  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }
  
  // Read token from storage
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }
  
  // Delete token from storage
  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }
  
  // Check if token exists
  hasToken(): boolean {
    return !!this.getToken();
  }
  
  // Decode and check expiry (future)
  isTokenExpired(): boolean {
    // Parse JWT and check exp claim
  }
}
```

### 2. **Create an `AuthHttpInterceptor`**
Automatically attach token to all HTTP requests:

```ts
@Injectable()
export class AuthHttpInterceptor implements HttpInterceptor {
  constructor(private tokenService: TokenService) {}
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.tokenService.getToken();
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }
    return next.handle(req);
  }
}
```

### 3. **Standardize storage key**
-
```


## License: MIT
https://github.com/LuanMattos/circle/blob/6e63199ee0e2eac8f087ba1d0c6c5c8f00ee813e/src/app/core/auth/request.interceptor.ts

```
## JWT Token Usage Analysis

### 1. Where JWT tokens are created
- **Backend only**: ASP.NET Core `AuthService.GenerateJwtToken()` method
- Returns token in HTTP response as `response.token` in `AuthResponseDto`

### 2. Where JWT tokens are stored
- **localStorage** only (no sessionStorage usage)
- Stored immediately after successful login in [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27)

### 3. All localStorage/sessionStorage keys used
- `'jwt_token'` – Standard key used in most locations
  - [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27) writes here
  - [PersonnelService.getHeaders()](prs-ui/src/app/services/personnel.service.ts#L17) reads here
  - [MasterDataService.getHeaders()](prs-ui/src/app/services/master-data.service.ts#L18) reads here
  - [Dashboard.onLogout()](prs-ui/src/app/components/dashboard/dashboard.ts#L56) deletes here

- `'token'` – Non-standard key used **only** in MftManagementComponent
  - [MftManagementComponent.fetchIngestionHistory()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L64) reads here
  - [MftManagementComponent.viewStagingDashboard()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L85) reads here
  - [MftManagementComponent.triggerReprocessing()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L106) reads here

### 4. All locations reading tokens
| Location | Storage Key | Method |
|----------|-------------|--------|
| PersonnelService | `'jwt_token'` | In `getHeaders()` method, used by search/profile methods |
| MasterDataService | `'jwt_token'` | In `getHeaders()` method, used by master data lookups |
| MftManagementComponent | `'token'` | Direct `localStorage.getItem()` in 3 methods |

### 5. All locations writing tokens
| Location | Storage Key | Trigger |
|----------|-------------|---------|
| [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27) | `'jwt_token'` | After successful authentication |

### 6. All locations deleting tokens
| Location | Storage Key | Trigger |
|----------|-------------|---------|
| [Dashboard.onLogout()](prs-ui/src/app/components/dashboard/dashboard.ts#L56) | `'jwt_token'` | User clicks logout |

---

## Critical Inconsistencies Identified

### 🔴 **CRITICAL BUG: Storage key mismatch**
- Login stores token as `'jwt_token'`
- MftManagementComponent reads from `'token'` (which doesn't exist)
- Falls back to empty string: `localStorage.getItem('token') || ''`
- **Result**: MFT API calls fail with 401 Unauthorized because no valid token is sent

### 🟡 **Code Duplication & Scattered Token Management**
- Token retrieval logic duplicated in multiple places:
  - `PersonnelService.getHeaders()` 
  - `MasterDataService.getHeaders()`
  - `MftManagementComponent.fetchIngestionHistory()`, `viewStagingDashboard()`, `triggerReprocessing()`
- Each location manually constructs Authorization headers
- No single source of truth for token handling
- Violates DRY principle

### 🟡 **No Centralized Token Service**
- No service responsible for token lifecycle (create, read, validate, delete)
- Logic scattered across components and services
- Difficult to audit and maintain

### 🟡 **No Token Validation**
- No check if token is expired before using
- Token lifetime is 60 minutes, but no validation of expiry
- Potential for silent failures with expired tokens

### 🟡 **No Error Handling for Missing Token**
- MFT component uses fallback empty string; others don't check for null
- Silent failures instead of explicit error messaging

---

## Recommended Single Standard Approach

### 1. **Create a dedicated `TokenService`**
Centralize all token operations:

```ts
@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly TOKEN_KEY = 'jwt_token'; // Single constant
  
  // Write token to storage
  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }
  
  // Read token from storage
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }
  
  // Delete token from storage
  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }
  
  // Check if token exists
  hasToken(): boolean {
    return !!this.getToken();
  }
  
  // Decode and check expiry (future)
  isTokenExpired(): boolean {
    // Parse JWT and check exp claim
  }
}
```

### 2. **Create an `AuthHttpInterceptor`**
Automatically attach token to all HTTP requests:

```ts
@Injectable()
export class AuthHttpInterceptor implements HttpInterceptor {
  constructor(private tokenService: TokenService) {}
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.tokenService.getToken();
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }
    return next.handle(req);
  }
}
```

### 3. **Standardize storage key**
-
```


## License: unknown
https://github.com/ksukacha/survey/blob/92bba49c8484d76770e9add5d21833ebc542eb48/src/app/auth-intercepter.service.ts

```
## JWT Token Usage Analysis

### 1. Where JWT tokens are created
- **Backend only**: ASP.NET Core `AuthService.GenerateJwtToken()` method
- Returns token in HTTP response as `response.token` in `AuthResponseDto`

### 2. Where JWT tokens are stored
- **localStorage** only (no sessionStorage usage)
- Stored immediately after successful login in [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27)

### 3. All localStorage/sessionStorage keys used
- `'jwt_token'` – Standard key used in most locations
  - [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27) writes here
  - [PersonnelService.getHeaders()](prs-ui/src/app/services/personnel.service.ts#L17) reads here
  - [MasterDataService.getHeaders()](prs-ui/src/app/services/master-data.service.ts#L18) reads here
  - [Dashboard.onLogout()](prs-ui/src/app/components/dashboard/dashboard.ts#L56) deletes here

- `'token'` – Non-standard key used **only** in MftManagementComponent
  - [MftManagementComponent.fetchIngestionHistory()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L64) reads here
  - [MftManagementComponent.viewStagingDashboard()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L85) reads here
  - [MftManagementComponent.triggerReprocessing()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L106) reads here

### 4. All locations reading tokens
| Location | Storage Key | Method |
|----------|-------------|--------|
| PersonnelService | `'jwt_token'` | In `getHeaders()` method, used by search/profile methods |
| MasterDataService | `'jwt_token'` | In `getHeaders()` method, used by master data lookups |
| MftManagementComponent | `'token'` | Direct `localStorage.getItem()` in 3 methods |

### 5. All locations writing tokens
| Location | Storage Key | Trigger |
|----------|-------------|---------|
| [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27) | `'jwt_token'` | After successful authentication |

### 6. All locations deleting tokens
| Location | Storage Key | Trigger |
|----------|-------------|---------|
| [Dashboard.onLogout()](prs-ui/src/app/components/dashboard/dashboard.ts#L56) | `'jwt_token'` | User clicks logout |

---

## Critical Inconsistencies Identified

### 🔴 **CRITICAL BUG: Storage key mismatch**
- Login stores token as `'jwt_token'`
- MftManagementComponent reads from `'token'` (which doesn't exist)
- Falls back to empty string: `localStorage.getItem('token') || ''`
- **Result**: MFT API calls fail with 401 Unauthorized because no valid token is sent

### 🟡 **Code Duplication & Scattered Token Management**
- Token retrieval logic duplicated in multiple places:
  - `PersonnelService.getHeaders()` 
  - `MasterDataService.getHeaders()`
  - `MftManagementComponent.fetchIngestionHistory()`, `viewStagingDashboard()`, `triggerReprocessing()`
- Each location manually constructs Authorization headers
- No single source of truth for token handling
- Violates DRY principle

### 🟡 **No Centralized Token Service**
- No service responsible for token lifecycle (create, read, validate, delete)
- Logic scattered across components and services
- Difficult to audit and maintain

### 🟡 **No Token Validation**
- No check if token is expired before using
- Token lifetime is 60 minutes, but no validation of expiry
- Potential for silent failures with expired tokens

### 🟡 **No Error Handling for Missing Token**
- MFT component uses fallback empty string; others don't check for null
- Silent failures instead of explicit error messaging

---

## Recommended Single Standard Approach

### 1. **Create a dedicated `TokenService`**
Centralize all token operations:

```ts
@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly TOKEN_KEY = 'jwt_token'; // Single constant
  
  // Write token to storage
  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }
  
  // Read token from storage
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }
  
  // Delete token from storage
  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }
  
  // Check if token exists
  hasToken(): boolean {
    return !!this.getToken();
  }
  
  // Decode and check expiry (future)
  isTokenExpired(): boolean {
    // Parse JWT and check exp claim
  }
}
```

### 2. **Create an `AuthHttpInterceptor`**
Automatically attach token to all HTTP requests:

```ts
@Injectable()
export class AuthHttpInterceptor implements HttpInterceptor {
  constructor(private tokenService: TokenService) {}
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.tokenService.getToken();
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }
    return next.handle(req);
  }
}
```

### 3. **Standardize storage key**
-
```


## License: MIT
https://github.com/LuanMattos/circle/blob/6e63199ee0e2eac8f087ba1d0c6c5c8f00ee813e/src/app/core/auth/request.interceptor.ts

```
## JWT Token Usage Analysis

### 1. Where JWT tokens are created
- **Backend only**: ASP.NET Core `AuthService.GenerateJwtToken()` method
- Returns token in HTTP response as `response.token` in `AuthResponseDto`

### 2. Where JWT tokens are stored
- **localStorage** only (no sessionStorage usage)
- Stored immediately after successful login in [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27)

### 3. All localStorage/sessionStorage keys used
- `'jwt_token'` – Standard key used in most locations
  - [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27) writes here
  - [PersonnelService.getHeaders()](prs-ui/src/app/services/personnel.service.ts#L17) reads here
  - [MasterDataService.getHeaders()](prs-ui/src/app/services/master-data.service.ts#L18) reads here
  - [Dashboard.onLogout()](prs-ui/src/app/components/dashboard/dashboard.ts#L56) deletes here

- `'token'` – Non-standard key used **only** in MftManagementComponent
  - [MftManagementComponent.fetchIngestionHistory()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L64) reads here
  - [MftManagementComponent.viewStagingDashboard()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L85) reads here
  - [MftManagementComponent.triggerReprocessing()](prs-ui/src/app/components/mft-management/mft-management.component.ts#L106) reads here

### 4. All locations reading tokens
| Location | Storage Key | Method |
|----------|-------------|--------|
| PersonnelService | `'jwt_token'` | In `getHeaders()` method, used by search/profile methods |
| MasterDataService | `'jwt_token'` | In `getHeaders()` method, used by master data lookups |
| MftManagementComponent | `'token'` | Direct `localStorage.getItem()` in 3 methods |

### 5. All locations writing tokens
| Location | Storage Key | Trigger |
|----------|-------------|---------|
| [Login.onLogin()](prs-ui/src/app/components/login/login.ts#L27) | `'jwt_token'` | After successful authentication |

### 6. All locations deleting tokens
| Location | Storage Key | Trigger |
|----------|-------------|---------|
| [Dashboard.onLogout()](prs-ui/src/app/components/dashboard/dashboard.ts#L56) | `'jwt_token'` | User clicks logout |

---

## Critical Inconsistencies Identified

### 🔴 **CRITICAL BUG: Storage key mismatch**
- Login stores token as `'jwt_token'`
- MftManagementComponent reads from `'token'` (which doesn't exist)
- Falls back to empty string: `localStorage.getItem('token') || ''`
- **Result**: MFT API calls fail with 401 Unauthorized because no valid token is sent

### 🟡 **Code Duplication & Scattered Token Management**
- Token retrieval logic duplicated in multiple places:
  - `PersonnelService.getHeaders()` 
  - `MasterDataService.getHeaders()`
  - `MftManagementComponent.fetchIngestionHistory()`, `viewStagingDashboard()`, `triggerReprocessing()`
- Each location manually constructs Authorization headers
- No single source of truth for token handling
- Violates DRY principle

### 🟡 **No Centralized Token Service**
- No service responsible for token lifecycle (create, read, validate, delete)
- Logic scattered across components and services
- Difficult to audit and maintain

### 🟡 **No Token Validation**
- No check if token is expired before using
- Token lifetime is 60 minutes, but no validation of expiry
- Potential for silent failures with expired tokens

### 🟡 **No Error Handling for Missing Token**
- MFT component uses fallback empty string; others don't check for null
- Silent failures instead of explicit error messaging

---

## Recommended Single Standard Approach

### 1. **Create a dedicated `TokenService`**
Centralize all token operations:

```ts
@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly TOKEN_KEY = 'jwt_token'; // Single constant
  
  // Write token to storage
  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }
  
  // Read token from storage
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }
  
  // Delete token from storage
  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }
  
  // Check if token exists
  hasToken(): boolean {
    return !!this.getToken();
  }
  
  // Decode and check expiry (future)
  isTokenExpired(): boolean {
    // Parse JWT and check exp claim
  }
}
```

### 2. **Create an `AuthHttpInterceptor`**
Automatically attach token to all HTTP requests:

```ts
@Injectable()
export class AuthHttpInterceptor implements HttpInterceptor {
  constructor(private tokenService: TokenService) {}
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.tokenService.getToken();
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }
    return next.handle(req);
  }
}
```

### 3. **Standardize storage key**
-
```

