# QA Verification Checklist - HTTP Interceptor Implementation

## Overview
This checklist validates the HTTP interceptor strategy implementation covering:
- JWT token lifecycle management
- Request/response handling
- Error scenarios
- Route protection
- Authorization header injection
- User experience flows

**Test Environment**: 
- Backend: http://localhost:5076
- Frontend: http://localhost:4200
- Chrome DevTools: Open F12 for Network tab inspection

---

## Phase 1: Authentication Flow

### 1.1 User Login - Valid Credentials
**Steps**:
1. Open http://localhost:4200
2. Enter valid email (e.g., test@example.com)
3. Enter correct password
4. Click "Login" button

**Expected Results**:
- ✅ Login API call sent to `/api/auth/login`
- ✅ Backend returns 200 OK with JWT token in response body
- ✅ Token stored in localStorage with key `jwt_token`
- ✅ Success notification shown: "Login successful!"
- ✅ Redirected to `/dashboard` within 2 seconds
- ✅ Network tab shows: Authorization header NOT in login request (public endpoint)

**How to Verify**:
- Open DevTools → Application → Local Storage
- Look for key `jwt_token` with value starting with "eyJ" (JWT header)
- In Network tab, click `/api/auth/login` request
- Verify `Authorization: Bearer` header NOT present (public endpoint, not needed)

---

### 1.2 User Login - Invalid Credentials
**Steps**:
1. Open http://localhost:4200
2. Enter invalid email or wrong password
3. Click "Login" button

**Expected Results**:
- ✅ Login API call sent to `/api/auth/login`
- ✅ Backend returns 401 Unauthorized or 400 Bad Request
- ✅ Error notification shown with backend error message
- ✅ User remains on login page (no redirect)
- ✅ Form inputs preserved for correction

**How to Verify**:
- In Network tab, see 401/400 response
- Console shows: `[ERROR Log] Login.onLogin() Invalid credentials` (or similar)
- localStorage does NOT contain `jwt_token` key

---

### 1.3 User Registration - New Account
**Steps**:
1. Click "Register" link on login page
2. Enter email, password, confirm password
3. Click "Register" button

**Expected Results**:
- ✅ Register API call sent to `/api/auth/register`
- ✅ Backend returns 200 OK
- ✅ Success message shown
- ✅ Redirected to login page (or auto-login if backend returns token)
- ✅ Authorization header NOT in request (public endpoint)

**How to Verify**:
- Network tab shows `/api/auth/register` request
- Response contains new user data or confirmation message
- localStorage remains empty or gets token if auto-login implemented

---

## Phase 2: Token Persistence & Storage

### 2.1 Token Stored in localStorage
**Steps**:
1. Complete successful login (Phase 1.1)
2. Open DevTools → Application → Storage → Local Storage

**Expected Results**:
- ✅ Key `jwt_token` is present
- ✅ Value is a valid JWT (format: `xxxxx.yyyyy.zzzzz`)
- ✅ Token is readable in JWT decoder (https://jwt.io)
- ✅ Decoded payload contains: `sub` (email), `jti` (GUID), `UserId`, `Name`
- ⚠️ Note: Does NOT contain `role` claim (architectural gap - backend fix needed)

**How to Verify**:
```javascript
// Run in DevTools Console:
const token = localStorage.getItem('jwt_token');
console.log(token);
// Copy token to https://jwt.io for decoding
```

---

### 2.2 Token Expiry Information
**Steps**:
1. After successful login, open DevTools Console
2. Run the following code to check token expiry

**Expected Results**:
- ✅ Token `exp` claim is approximately 60 minutes from login
- ✅ TokenService.isTokenExpired() returns false (within valid period)
- ✅ TokenService.getTokenExpirySeconds() returns seconds remaining

**How to Verify**:
```javascript
// Run in DevTools Console:
const token = localStorage.getItem('jwt_token');
const parts = token.split('.');
const payload = JSON.parse(atob(parts[1]));
console.log('Token expires at:', new Date(payload.exp * 1000));
console.log('Current time:', new Date());
// Expiry should be ~60 minutes from login
```

---

## Phase 3: Page Refresh & Token Persistence

### 3.1 Refresh Dashboard Page While Logged In
**Steps**:
1. Successfully login (Phase 1.1)
2. You are on `/dashboard`
3. Press F5 or click Refresh button
4. Wait for page to load

**Expected Results**:
- ✅ Page reloads successfully
- ✅ Dashboard loads with personnel search available
- ✅ localStorage still contains `jwt_token` key
- ✅ User remains logged in (no redirect to login)
- ✅ Token NOT sent in initial login request (only in subsequent API calls)

**How to Verify**:
- User is still on `/dashboard` after refresh
- localStorage still has `jwt_token`
- No console errors about missing token

---

### 3.2 Refresh MFT Management Page While Logged In
**Steps**:
1. Successfully login
2. Navigate to `/mft-management`
3. Wait for ingestion history to load
4. Press F5 to refresh
5. Wait for page to reload

**Expected Results**:
- ✅ Page reloads without redirect to login
- ✅ Ingestion history reloads automatically
- ✅ localStorage contains `jwt_token`
- ✅ MFT API calls include Authorization header

**How to Verify**:
- Page loads successfully
- Table shows ingestion history data
- Network tab shows `/api/mft/history` request with `Authorization: Bearer <token>`

---

### 3.3 Refresh Profile Page While Logged In
**Steps**:
1. Successfully login
2. Search for a personnel on dashboard
3. Click "View Profile" for a personnel
4. You are on `/profile/{guid}`
5. Press F5 to refresh

**Expected Results**:
- ✅ Profile page loads successfully
- ✅ User remains logged in
- ✅ Profile data reloads from backend
- ✅ localStorage contains `jwt_token`

**How to Verify**:
- Profile details are displayed after refresh
- No 401 error in Network tab

---

## Phase 4: Route Protection & Auth Guard

### 4.1 Access Protected Route Without Token
**Steps**:
1. Open DevTools → Application → Storage → Local Storage
2. Delete the `jwt_token` key
3. Manually navigate to http://localhost:4200/dashboard
4. Wait for page to load

**Expected Results**:
- ✅ AuthGuard detects missing token
- ✅ Error notification shown: "Please log in to access this page."
- ✅ User redirected to `/login` automatically
- ✅ Console shows: `[AuthGuard] Access denied - No token found`

**How to Verify**:
- URL changes to `/login`
- Error notification visible
- Console shows auth guard warning

---

### 4.2 Access Protected Route With Expired Token
**Steps**:
1. Manually edit `jwt_token` in localStorage
2. Change the `exp` claim to a past timestamp (e.g., subtract 10000 from current exp)
3. Save and navigate to http://localhost:4200/dashboard
4. Wait for page to load

**Expected Results**:
- ✅ AuthGuard detects expired token
- ✅ Error notification shown: "Your session has expired. Please log in again."
- ✅ Token removed from localStorage
- ✅ User redirected to `/login`
- ✅ Console shows: `[AuthGuard] Access denied - Token expired`

**How to Verify**:
- URL changes to `/login`
- localStorage no longer has `jwt_token`
- Error notification visible

---

### 4.3 Access Public Route (Login) Without Token
**Steps**:
1. Delete `jwt_token` from localStorage
2. Navigate to http://localhost:4200/login

**Expected Results**:
- ✅ Login page loads successfully (no AuthGuard blocking)
- ✅ User can enter credentials
- ✅ No error notification shown

**How to Verify**:
- Login page is fully functional
- Can type in email/password fields

---

## Phase 5: Authorization Header Validation

### 5.1 Authorization Header on Protected API Call
**Steps**:
1. Successfully login (Phase 1.1)
2. On dashboard, perform a personnel search
3. Open DevTools → Network tab
4. Search for `/api/personnel/search` request

**Expected Results**:
- ✅ Request includes header: `Authorization: Bearer eyJ...`
- ✅ Bearer token matches `jwt_token` from localStorage
- ✅ Header automatically attached by AuthHttpInterceptor
- ✅ Component code does NOT manually set Authorization header

**How to Verify**:
- Click `/api/personnel/search` in Network tab
- Go to "Headers" section
- Look for: `Authorization: Bearer <token>`
- Verify token matches localStorage value

---

### 5.2 Authorization Header Absent on Public API Call
**Steps**:
1. Open http://localhost:4200/login
2. Open DevTools → Network tab
3. Enter credentials and click Login
4. In Network tab, find `/api/auth/login` request
5. Click it and check headers

**Expected Results**:
- ✅ Request does NOT include Authorization header
- ✅ This is correct (login endpoint is public)
- ✅ Backend does not require [Authorize] for login

**How to Verify**:
- Authorization header is NOT present in login request
- Other headers present: Content-Type, Accept, etc.

---

### 5.3 Token Attached to MFT API Calls
**Steps**:
1. Successfully login
2. Navigate to `/mft-management`
3. Wait for ingestion history to load
4. Open DevTools → Network tab
5. Search for `/api/mft/history` request
6. Click it and check headers

**Expected Results**:
- ✅ Request includes: `Authorization: Bearer <token>`
- ✅ Token is same as localStorage `jwt_token`
- ✅ Header automatically added by AuthHttpInterceptor

**How to Verify**:
- Click `/api/mft/history` request
- Check Headers section for Authorization header
- Token value matches localStorage

---

## Phase 6: Error Handling & 401 Unauthorized

### 6.1 Token Expiry During Session (401 Response)
**Steps**:
1. Successfully login (Phase 1.1)
2. Edit `jwt_token` in localStorage to have expired `exp` claim
3. On dashboard, perform a personnel search
4. API call will return 401 Unauthorized

**Expected Results**:
- ✅ Backend returns 401 Unauthorized
- ✅ ErrorInterceptor catches 401 response
- ✅ ErrorInterceptor calls TokenService.removeToken()
- ✅ localStorage `jwt_token` is deleted
- ✅ Error notification shown: "Your session has expired. Please log in again."
- ✅ User redirected to `/login` automatically
- ✅ Console shows: `[401 Unauthorized] <error details>`

**How to Verify**:
- URL changes to `/login`
- localStorage no longer contains `jwt_token`
- Error notification visible
- Network tab shows 401 response from backend

---

### 6.2 Unauthorized (401) on Protected Endpoint
**Steps**:
1. Delete `jwt_token` from localStorage manually
2. Open DevTools Console
3. Run this code to make an unauthorized API call:
```javascript
fetch('http://localhost:5076/api/personnel/search', {
  method: 'GET',
  headers: {
    'Authorization': 'Bearer invalid_token'
  }
})
.then(r => r.json())
.then(d => console.log(d))
```

**Expected Results**:
- ✅ Backend returns 401 Unauthorized
- ✅ ErrorInterceptor (or console) catches response
- ✅ Error message: "Your session has expired..."
- ✅ Redirect to login triggered

**How to Verify**:
- Console shows 401 response
- No valid data returned

---

### 6.3 Forbidden (403) on Admin-Only Endpoint
**Steps**:
1. Login as regular (non-admin) user
2. Navigate to `/mft-management`
3. Click "Trigger Reprocessing" button on a file
4. API call will return 403 Forbidden

**Expected Results**:
- ✅ Backend returns 403 Forbidden
- ✅ ErrorInterceptor catches 403 response
- ✅ Token NOT removed (user stays logged in)
- ✅ Error notification shown: "You do not have permission to access this resource."
- ✅ User remains on `/mft-management` page
- ✅ Console shows: `[403 Forbidden] <error details>`

**Note**: This requires backend to enforce `[Authorize(Roles="Admin")]`. Currently, backend JWT doesn't include role claims (architectural gap).

**How to Verify**:
- Error notification visible
- User still on mft-management page
- Network tab shows 403 response

---

## Phase 7: Logout Flow

### 7.1 Click Logout Button
**Steps**:
1. Successfully login (Phase 1.1)
2. You are on `/dashboard`
3. Click "Logout" button (top right)
4. Wait for navigation

**Expected Results**:
- ✅ TokenService.removeToken() called
- ✅ localStorage `jwt_token` key is deleted
- ✅ Success notification shown: "Logged out successfully."
- ✅ User redirected to `/login` page
- ✅ Dashboard is no longer accessible
- ✅ Next attempt to access `/dashboard` is blocked by AuthGuard

**How to Verify**:
- URL is `/login`
- localStorage has no `jwt_token`
- Can see login form again

---

### 7.2 Verify Token Cleared After Logout
**Steps**:
1. Successfully logout (Phase 7.1)
2. Open DevTools → Application → Local Storage
3. Search for `jwt_token`

**Expected Results**:
- ✅ `jwt_token` key is NOT present
- ✅ localStorage is completely cleared of auth data
- ✅ Attempting to navigate to protected routes will show: "Please log in to access this page."

**How to Verify**:
- `jwt_token` not found in localStorage
- Error notification appears when trying to access `/dashboard`

---

## Phase 8: Protected Screen Functionality

### 8.1 Personnel Search on Dashboard
**Steps**:
1. Successfully login
2. You are on `/dashboard`
3. Enter search criteria (first name, last name, email, status)
4. Click "Search" button
5. Wait for results

**Expected Results**:
- ✅ Search API call sent to `/api/personnel/search`
- ✅ Authorization header included: `Authorization: Bearer <token>`
- ✅ Backend returns 200 OK with personnel list
- ✅ Success notification shown: "Found X profile(s)"
- ✅ Results displayed in table
- ✅ Each result has "View Profile" link

**How to Verify**:
- Network tab shows `/api/personnel/search` with 200 response
- Results table populated with data
- Success notification visible

---

### 8.2 View Personnel Profile
**Steps**:
1. From personnel search results (Phase 8.1)
2. Click "View Profile" link on any result
3. You are navigated to `/profile/{guid}`

**Expected Results**:
- ✅ AuthGuard allows navigation (token valid)
- ✅ API call to `/api/personnel/{guid}` sent
- ✅ Authorization header included
- ✅ Backend returns 200 OK with profile details
- ✅ Profile page displays all fields:
  - Email
  - First Name
  - Last Name
  - Employment Status
  - Grade
  - Line of Service
  - Office Location
  - Portfolio Required flag
  - Master Data fields populated

**How to Verify**:
- Page loads successfully
- All profile fields displayed
- Network tab shows successful API call with Authorization header

---

### 8.3 Update Profile (If Available)
**Steps**:
1. On profile page (Phase 8.2)
2. If profile is incomplete, click "Complete Profile" or "Update"
3. Edit fields and submit

**Expected Results**:
- ✅ PUT/POST request sent to `/api/personnel/complete-profile`
- ✅ Authorization header included
- ✅ Backend validates user permissions
- ✅ Profile updated successfully
- ✅ Success notification shown
- ✅ Form cleared or redirected

**How to Verify**:
- Network tab shows request with Authorization header
- Backend processes request successfully (200 OK)
- Data persists after page refresh

---

## Phase 9: MFT Management Screen

### 9.1 View Ingestion History
**Steps**:
1. Successfully login
2. Navigate to `/mft-management`
3. Wait for ingestion history to load

**Expected Results**:
- ✅ AuthGuard allows navigation (token valid)
- ✅ API call sent to `/api/mft/history`
- ✅ Authorization header: `Authorization: Bearer <token>`
- ✅ Backend returns 200 OK with file list
- ✅ Table displays:
  - File ID
  - File Name
  - Country Code
  - Records Received/Processed/Failed
  - Processing Status
  - Start/End Times
- ✅ Each row has action buttons: View Staging, Reprocess

**How to Verify**:
- Page loads without auth redirect
- Table populated with MFT files
- Network tab shows `/api/mft/history` with Authorization header

---

### 9.2 View Staging Dashboard
**Steps**:
1. On MFT management page (Phase 9.1)
2. Click "View Staging" button on any file
3. Wait for staging records to load

**Expected Results**:
- ✅ API call sent to `/api/mft/staging/{fileId}`
- ✅ Authorization header included
- ✅ Backend returns 200 OK with staging records
- ✅ Staging table displays:
  - Row Number
  - GUID
  - Employment Status
  - Work Office
  - Line of Service
  - Grade
  - Portfolio Required
  - Validation Status
  - Validation Message
- ✅ Filters available: Status filter, GUID search
- ✅ Success notification: "Loaded X staging record(s)"

**How to Verify**:
- Staging table appears below history
- Data populates correctly
- Filter works
- Network tab shows API call with Authorization header

---

### 9.3 Trigger File Reprocessing (Admin Only)
**Steps**:
1. On MFT management page (Phase 9.1)
2. Click "Reprocess" button on a file
3. Confirm action if prompted

**Expected Results**:
- ✅ API call sent to `/api/mft/reprocess/{fileId}`
- ✅ Authorization header: `Authorization: Bearer <token>`
- ✅ If user is Admin:
  - Backend returns 200 OK
  - Success notification: "File reprocessing initiated successfully."
  - History and staging views refresh automatically
- ✅ If user is NOT Admin:
  - Backend returns 403 Forbidden
  - ErrorInterceptor catches 403
  - Error notification: "You do not have permission to access this resource."
  - User remains on page (not logged out)

**Note**: Current backend JWT doesn't include role claims, so 403 handling can't be fully tested yet.

**How to Verify**:
- Network tab shows `/api/mft/reprocess/{fileId}` request
- Authorization header present
- Appropriate success or error notification shown

---

### 9.4 MFT Error Details (If Available)
**Steps**:
1. On MFT management page
2. Click "View Errors" button on a file (if errors exist)

**Expected Results**:
- ✅ API call sent to `/api/mft/errors/{fileId}`
- ✅ Authorization header included
- ✅ Backend returns 200 OK with error details
- ✅ Error details displayed in table/modal

**How to Verify**:
- Error details appear
- Network tab shows API call with Authorization header

---

## Phase 10: Master Data Screens

### 10.1 Load Master Data (Employment Status)
**Steps**:
1. On profile page or registration form
2. Open "Employment Status" dropdown
3. Verify options are loaded

**Expected Results**:
- ✅ API call sent to `/api/masterdata/employmentstatus`
- ✅ Authorization header included
- ✅ Backend returns 200 OK with status list
- ✅ Dropdown populated with options:
  - Active
  - Inactive
  - On Leave
  - Retired
  - (or actual values from backend)

**How to Verify**:
- Dropdown shows options
- Network tab shows API call with Authorization header
- No 401/403 errors

---

### 10.2 Load Master Data (Grades)
**Steps**:
1. On profile page or form
2. Open "Grade" dropdown
3. Verify options are loaded

**Expected Results**:
- ✅ API call sent to `/api/masterdata/grades`
- ✅ Authorization header included
- ✅ Dropdown populated with grade options

**How to Verify**:
- Dropdown shows grade options
- Network tab shows successful API call

---

### 10.3 Load Master Data (Line of Service)
**Steps**:
1. On profile page or form
2. Open "Line of Service" dropdown
3. Verify options are loaded

**Expected Results**:
- ✅ API call sent to `/api/masterdata/lineofservice`
- ✅ Authorization header included
- ✅ Dropdown populated with service options

**How to Verify**:
- Dropdown shows options
- Network tab shows successful API call

---

### 10.4 Load Master Data (Office Locations)
**Steps**:
1. On profile page, select a country code first
2. Open "Office Location" dropdown
3. Verify location options for that country are loaded

**Expected Results**:
- ✅ API call sent to `/api/masterdata/officelocations/{countryCode}`
- ✅ Authorization header included
- ✅ Dropdown populated with office locations for selected country

**How to Verify**:
- Locations appear based on country selection
- Network tab shows parametrized API call with country code

---

## Phase 11: Error Scenarios & Edge Cases

### 11.1 Network Connection Lost
**Steps**:
1. Successfully login
2. Go to DevTools → Network tab
3. Check "Offline" checkbox to simulate network failure
4. Try to perform search or navigate to MFT

**Expected Results**:
- ✅ HTTP request fails (status 0)
- ✅ ErrorInterceptor catches network error
- ✅ Error notification shown: "Network error. Please check your internet connection and try again."
- ✅ User remains on current page
- ✅ User can uncheck "Offline" and retry
- ✅ Console shows: `[Network Error]`

**How to Verify**:
- Network error notification visible
- Network tab shows failed requests
- Can recover by going back online and retrying

---

### 11.2 Backend Server Error (500)
**Steps**:
1. Successfully login
2. Have backend return 500 error (manual or via test endpoint if available)
3. Try to perform search or API call

**Expected Results**:
- ✅ Backend returns 500 Internal Server Error
- ✅ ErrorInterceptor catches 500 error
- ✅ Error notification shown: "Server error: 500. Please try again later."
- ✅ User remains logged in (token not cleared)
- ✅ User can retry the operation
- ✅ Console shows: `[5xx Server Error]`

**How to Verify**:
- Error notification visible
- User still logged in (can navigate to other pages)
- Network tab shows 500 response

---

### 11.3 Invalid JSON Response (Malformed Data)
**Steps**:
1. Backend returns malformed JSON (if testable)
2. Try API call

**Expected Results**:
- ✅ Request fails gracefully
- ✅ Error notification shown to user
- ✅ Console shows parsing error details
- ✅ Application remains functional

**How to Verify**:
- No white screen of death
- Error notification visible
- Can continue using app

---

### 11.4 Rapid Navigation Between Protected Routes
**Steps**:
1. Successfully login
2. Quickly click between Dashboard, MFT, Profile pages
3. Complete fast navigation

**Expected Results**:
- ✅ AuthGuard validates token for each route
- ✅ All routes load successfully
- ✅ API calls include Authorization headers
- ✅ No race conditions or stale data
- ✅ Token remains valid throughout

**How to Verify**:
- All pages load without auth errors
- No "session expired" messages
- Data loads correctly on each page

---

## Phase 12: Security Validation

### 12.1 XSS Risk Assessment (localStorage)
**Current State**: ⚠️ **RISK IDENTIFIED**
- JWT stored in localStorage (vulnerable to XSS attacks)
- If attacker injects JavaScript, can steal token
- Recommendations: Consider httpOnly cookies (requires backend changes)

**Test**: 
- Try to access token from browser console:
```javascript
localStorage.getItem('jwt_token')  // Should return token (XSS risk!)
```

---

### 12.2 Token Not in Cookies/Headers Visible in App
**Steps**:
1. Right-click on page → "Inspect" → Console
2. Run: `document.cookie`

**Expected Results**:
- ✅ No cookie contains JWT token
- ✅ Token only in localStorage (single storage mechanism)

**How to Verify**:
- `document.cookie` does not contain `jwt_token`
- Token only accessible via `localStorage.getItem('jwt_token')`

---

### 12.3 Token Not Logged in Console
**Steps**:
1. Open DevTools → Console
2. Perform login and search
3. Look for token values in logs

**Expected Results**:
- ✅ Full JWT token NOT printed in console
- ✅ Error messages don't include token values
- ✅ Sensitive data not logged

**How to Verify**:
- Search console for "eyJ" (JWT header) - should not find it in logs
- Only context information logged (e.g., "API call failed", not the token)

---

## Phase 13: Interceptor Chain Execution Order

### 13.1 Verify Interceptor Execution Order
**Steps**:
1. Successfully login
2. Open DevTools → Console
3. Perform a personnel search
4. Check console logs for interceptor messages

**Expected Results**:
- ✅ AuthHttpInterceptor executes first (adds token)
- ✅ Request sent to backend with Authorization header
- ✅ Response received
- ✅ ErrorInterceptor processes response
- ✅ If success (2xx): Passes to component
- ✅ If error (4xx/5xx): Handles error
- ✅ Component receives result/error

**Execution Flow**:
```
Component makes HTTP call
  ↓
AuthHttpInterceptor.intercept() 
  ├─ Get token
  ├─ Clone request
  ├─ Add Authorization header
  └─ Pass to next handler
  ↓
ErrorInterceptor.intercept()
  ├─ Send request
  ├─ Check response status
  ├─ If error: Handle (401/403/5xx/network)
  └─ Pass through or throw error
  ↓
Component's subscribe() handler
  ├─ Receive data (if success)
  └─ Handle error (if ErrorInterceptor threw)
```

**How to Verify**:
- Open console and filter for "[AuthInterceptor]" or "[ErrorInterceptor]" logs
- Verify order of execution

---

## Phase 14: Regression Testing

### 14.1 Ensure Existing Features Still Work
**Steps**:
1. All previous functionality should still work
2. Nothing should break due to interceptor changes

**Expected Results**:
- ✅ Login still works
- ✅ Search still works
- ✅ MFT management still works
- ✅ Profile viewing still works
- ✅ Form submissions still work
- ✅ Master data dropdowns still work
- ✅ No new JavaScript errors introduced

**How to Verify**:
- DevTools Console has no new errors
- All features functional as before
- No breaking changes in UI/UX

---

### 14.2 No Double Authorization Headers
**Steps**:
1. Perform any API call after login
2. Check Network tab

**Expected Results**:
- ✅ Authorization header appears exactly once
- ✅ No duplicate headers
- ✅ No "Authorization: Bearer Bearer <token>" (double Bearer)

**How to Verify**:
- Network tab request headers show one Authorization header
- Header value is: `Authorization: Bearer eyJ...` (not doubled)

---

### 14.3 No Header Duplication in Services
**Steps**:
1. Open PersonnelService, MasterDataService
2. Verify no `getHeaders()` method exists
3. Verify HTTP calls don't manually set Authorization

**Expected Results**:
- ✅ No manual header construction
- ✅ Services simplified (no `private getHeaders()`)
- ✅ All services rely on AuthHttpInterceptor
- ✅ DRY principle: Single source of truth

**How to Verify**:
- Open [src/app/services/personnel.service.ts](src/app/services/personnel.service.ts)
- Search for "getHeaders" - should not find it
- Search for "Authorization" - should not find it in service code
- Only in interceptor

---

## Phase 15: Performance Validation

### 15.1 No Performance Degradation
**Steps**:
1. Open DevTools → Performance tab
2. Perform login and search
3. Monitor performance metrics

**Expected Results**:
- ✅ Page load time similar to before
- ✅ No additional memory leaks
- ✅ No increased request latency
- ✅ Interceptors execute quickly (< 1ms)

**How to Verify**:
- Performance tab shows acceptable metrics
- No console warnings about memory

---

### 15.2 Network Waterfall Chart
**Steps**:
1. Open DevTools → Network tab
2. Disable cache
3. Refresh page
4. Perform login
5. View Network waterfall

**Expected Results**:
- ✅ All API requests include Authorization header (small overhead)
- ✅ Request latency not significantly increased
- ✅ No requests blocked or delayed

**How to Verify**:
- Network tab shows requests in expected order
- Response times reasonable

---

## Summary: Test Case Matrix

| Phase | Test Case | Status | Notes |
|-------|-----------|--------|-------|
| 1.1 | Valid Login | ⏳ | Try first |
| 1.2 | Invalid Login | ⏳ | Test error handling |
| 1.3 | Registration | ⏳ | Test public endpoint |
| 2.1 | Token Stored | ⏳ | Verify localStorage |
| 2.2 | Token Expiry | ⏳ | Check exp claim |
| 3.1 | Refresh Dashboard | ⏳ | Test persistence |
| 3.2 | Refresh MFT | ⏳ | Test persistence |
| 3.3 | Refresh Profile | ⏳ | Test persistence |
| 4.1 | Access Without Token | ⏳ | Test AuthGuard |
| 4.2 | Access With Expired Token | ⏳ | Test AuthGuard |
| 4.3 | Access Public Route | ⏳ | Test no guard needed |
| 5.1 | Auth Header Present | ⏳ | Verify interceptor |
| 5.2 | Auth Header Absent (Public) | ⏳ | Verify correct behavior |
| 5.3 | MFT Auth Header | ⏳ | Verify interceptor |
| 6.1 | 401 Token Expired | ⏳ | Test error handling |
| 6.2 | 401 Invalid Token | ⏳ | Test error handling |
| 6.3 | 403 Forbidden | ⏳ | Test RBAC |
| 7.1 | Logout Button | ⏳ | Test token removal |
| 7.2 | Token Cleared | ⏳ | Verify localStorage |
| 8.1 | Personnel Search | ⏳ | Test protected API |
| 8.2 | View Profile | ⏳ | Test protected route |
| 8.3 | Update Profile | ⏳ | Test form submission |
| 9.1 | Ingestion History | ⏳ | Test MFT screen |
| 9.2 | Staging Dashboard | ⏳ | Test MFT screen |
| 9.3 | Reprocess File | ⏳ | Test admin endpoint |
| 9.4 | View Errors | ⏳ | Test MFT details |
| 10.1-10.4 | Master Data | ⏳ | Test dropdowns |
| 11.1 | Network Error | ⏳ | Test error handling |
| 11.2 | Server Error (500) | ⏳ | Test error handling |
| 11.3 | Malformed Data | ⏳ | Test error handling |
| 11.4 | Rapid Navigation | ⏳ | Test concurrency |
| 12.1 | XSS Risk | ⚠️ | localStorage risk |
| 12.2 | No Cookies | ✅ | Verified |
| 12.3 | Token Not Logged | ✅ | Verified |
| 13.1 | Interceptor Order | ⏳ | Test execution |
| 14.1 | Regression | ⏳ | Test all features |
| 14.2 | No Double Headers | ⏳ | Test interceptor |
| 14.3 | No Header Duplication | ✅ | Code verified |
| 15.1 | Performance | ⏳ | Monitor metrics |
| 15.2 | Network Waterfall | ⏳ | Monitor requests |

---

## Notes & Known Issues

### Architectural Gaps (Not Fixed Yet)
- ❌ **Role Claims Missing in JWT**: Backend `GenerateJwtToken()` doesn't include role claims, so `[Authorize(Roles="Admin")]` enforcement can't be properly tested
- ⚠️ **XSS Risk**: JWT stored in localStorage (should use httpOnly cookies)
- ⚠️ **Signing Key in Source**: JWT signing key hardcoded in appsettings.Development.json

### Future Enhancements
- 🔄 Replace console notifications with toast/snackbar UI component
- 🔄 Implement token refresh mechanism
- 🔄 Add request timeout handling
- 🔄 Implement request retry logic
- 🔄 Add detailed request logging/debugging

---

**Test Environment Setup**:
```bash
# Terminal 1: Start backend
cd C:\Shubham\Personnel-Registration-System\src\Presentation\PRS.API
dotnet run

# Terminal 2: Start frontend
cd C:\Shubham\Personnel-Registration-System\prs-ui
npm start
# or: ng serve

# Access UI at: http://localhost:4200
# Access API at: http://localhost:5076
```

---

**Last Updated**: June 10, 2026
**Status**: Ready for QA Testing
