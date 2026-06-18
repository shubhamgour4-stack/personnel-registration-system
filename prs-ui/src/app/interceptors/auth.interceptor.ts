import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TokenService } from '../services/token.service';

@Injectable()
export class AuthHttpInterceptor implements HttpInterceptor {
  constructor(private tokenService: TokenService) {}

  /**
   * Intercept all HTTP requests and attach Bearer token if available
   * This eliminates the need to manually construct Authorization headers in services/components
   */
  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const token = this.tokenService.getToken();

    if (!token) {
      console.warn('[AuthHttpInterceptor] No JWT token found in localStorage. Request will be sent without Authorization header.', {
        url: req.url,
        method: req.method
      });
      return next.handle(req);
    }

    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });

    console.debug('[AuthHttpInterceptor] Attached Authorization header', {
      url: req.url,
      method: req.method,
      hasAuthorization: req.headers.has('Authorization')
    });

    return next.handle(req);
  }
}
