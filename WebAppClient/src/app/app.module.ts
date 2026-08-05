import { environment } from '../environments/environment';
import { APP_INITIALIZER, CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AdminModule } from './admin/admin.module';
import { UiModule } from './ui/ui.module';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';

import { RouterModule } from '@angular/router';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { NgxSpinnerModule } from "ngx-spinner";
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { JwtModule } from '@auth0/angular-jwt';
import { LoginComponent } from './ui/components/login/login.component';

import { SocialLoginModule, SocialAuthServiceConfig, GoogleSigninButtonModule } from '@abacritt/angularx-social-login';
import {
  GoogleLoginProvider,
  FacebookLoginProvider
} from '@abacritt/angularx-social-login';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpErrorHandlerInterceptorService } from './services/common/http-error-handler-interceptor.service';
import { DynamicLoadComponentDirective } from './directives/common/dynamic-load-component.directive';
import { provideNgxMask } from 'ngx-mask';
import { DatePipe } from '@angular/common';
import { UserAuthService } from './services/common/models/user-auth.service';
import { ElementAccessControlService } from './services/common/element-access-control.service';

// GET Authorized Sidebar Menu Items
export function preloadUserMenusFactory(userAuthService: UserAuthService): () => Promise<void> {
  return () =>
    new Promise<void>((resolve) => {
      userAuthService.identityCheck(async result => {
        if (result?.isAuthenticated && result?.isAdmin) {
          await userAuthService.preloadAccessibleMenus();
        }
        
        resolve();
      });
    });
}

// GET Authorized Html Items
export function preloadPermissionsFactory(userAuthService: UserAuthService, elementAccessControlServiceService: ElementAccessControlService): () => Promise<void> {
  return () =>
    new Promise<void>((resolve) => {
      userAuthService.identityCheck(async result => {
        if (result?.isAuthenticated) {
          await elementAccessControlServiceService.preloadPermissions(result?.userId);
        }
        
        resolve();
      });
    });
}

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    DynamicLoadComponentDirective
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    AdminModule, UiModule,
    MatIconModule, MatButtonModule, MatToolbarModule, MatMenuModule,
    RouterModule,
    BrowserAnimationsModule,
    ToastrModule.forRoot(),
    NgxSpinnerModule,
    JwtModule.forRoot({
      config: {
        tokenGetter: () => localStorage.getItem("accessToken"),
        allowedDomains: environment.allowedDomains
      }
    }),
    SocialLoginModule,
    ReactiveFormsModule,
    GoogleSigninButtonModule
  ],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: preloadUserMenusFactory,
      deps: [UserAuthService],
      multi: true
    },
    {
      provide: APP_INITIALIZER,
      useFactory: preloadPermissionsFactory,
      deps: [UserAuthService, ElementAccessControlService],
      multi: true
    },
    provideAnimationsAsync(),
    { provide: "baseUrl", useValue: environment.baseUrl, multi: true },
    { provide: "baseSignalRUrl", useValue: environment.baseSignalRUrl, multi: true },
    provideHttpClient(withInterceptorsFromDi()),
    {
      provide: 'SocialAuthServiceConfig',
      useValue: {
        autoLogin: false,
        lang: 'en',
        providers: [
          {
            id: GoogleLoginProvider.PROVIDER_ID,
            provider: new GoogleLoginProvider(
              environment.googleClientId, {
              scopes: 'openid profile email',
            }
            )
          },
          {
            id: FacebookLoginProvider.PROVIDER_ID,
            provider: new FacebookLoginProvider(environment.facebookClientId)
          }
        ],
        onError: (err) => console.error(err)
      } as SocialAuthServiceConfig,
    },
    { provide: HTTP_INTERCEPTORS, useClass: HttpErrorHandlerInterceptorService, multi: true },
    provideNgxMask(),
    DatePipe
  ],
  bootstrap: [AppComponent],
  schemas: [
    CUSTOM_ELEMENTS_SCHEMA]
})
export class AppModule { }
