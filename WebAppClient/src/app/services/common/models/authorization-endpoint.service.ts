import { Injectable } from '@angular/core';
import { HttpClientService } from '../http-client.service';
import { catchError, firstValueFrom, map, Observable, of } from 'rxjs';
import { AssignRoleEndpoint } from '../../../entities/assign-role-endpoint';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AuthorizationEndpointService {

  constructor(private httpClientService: HttpClientService) { }

  // GET
  async getRolesEndpoints(successCallBack?: (response: any) => void, errorCallBack?: (errorMessage: string) => void) {
    const data = await firstValueFrom(
      this.httpClientService.get<AssignRoleEndpoint[]>({
        controller: "endpoints",
        action: "get-roles-endpoints"
      }).pipe(
        map(response => {
          successCallBack && successCallBack(response);
          return response;
        }),
        catchError((errorResponse: HttpErrorResponse) => {
          if (errorCallBack) {
            errorCallBack(errorResponse.message);
          }
          return [];
        })
      )
    );
    return data;
  }

  // GET permissions for the currently authenticated user's roles
  async getCurrentUserRoleEndpoints(): Promise<AssignRoleEndpoint[]> {
    const response = await firstValueFrom(
      this.httpClientService.get<{rolesEndpoints: AssignRoleEndpoint[]}>({
        controller: "endpoints",
        action: "get-current-user-role-endpoints"
      })
    );

    return response.rolesEndpoints;
  }

  // POST
  async assignRoleEndpoints(rolesEndpoints: AssignRoleEndpoint[], successCallBack?: () => void, errorCallBack?: (errorMessage: string) => void) {
    const observable: Observable<any> = this.httpClientService.post({
      controller: "endpoints",
      action: "assign-role-endpoints"
    }, {
      rolesEndpoints: rolesEndpoints
    });

    const promiseData = observable.subscribe({
      next: successCallBack,
      error: errorCallBack
    });

    await promiseData;
  }

  // GET Authoriziton for Pages
  hasAccessToMenu(menuName: string): Promise<boolean> {
    return firstValueFrom(
      this.httpClientService.get<{ hasAccess: boolean }>({
        controller: "endpoints",
        action: "has-access",
        queryString: `menuName=${encodeURIComponent(menuName)}`
      }).pipe(
        map(response => response.hasAccess),
        catchError(error => {
          console.error("Access check failed", error);
          return of(false);
        })
      )
    );
  }

  // GET Authorized Sidebar Menu Items
  async fetchAccessibleMenus(): Promise<string[]> {
    return firstValueFrom(
      this.httpClientService.get<string[]>({
        controller: 'endpoints',
        action: 'accessible-menus'
      })
    );
  }
}
