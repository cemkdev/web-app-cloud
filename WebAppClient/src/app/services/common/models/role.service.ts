import { Injectable } from '@angular/core';
import { HttpClientService } from '../http-client.service';
import { catchError, firstValueFrom, map, Observable } from 'rxjs';
import { List_Roles } from '../../../contracts/role/list_roles';
import { Update_Role } from '../../../contracts/role/update_role';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class RoleService {

  constructor(private httpClientService: HttpClientService) { }

  // LIST / READ
  async getRoles() {
    const observable: Observable<List_Roles[]> = this.httpClientService.get({
      controller: "roles",
      action: "get-roles"
    });

    return await firstValueFrom(observable);;
  }

  // READ BY ID
  async readById(id: string, successCallBack?: () => void, errorCallBack?: (errorMessage: string) => void): Promise<Update_Role> {

    const data = await firstValueFrom(
      this.httpClientService.get<Update_Role>({
        controller: "roles",
        action: "get-role-by-id"
      }, id).pipe(
        map(response => {
          successCallBack && successCallBack();
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

  // CREATE
  async createRole(role: { name: string, isAdmin: boolean }, successCallBack?: () => void, errorCallBack?: (error) => void) {
    const observable: Observable<any> = this.httpClientService.post({
      controller: "roles",
      action: "create-role"
    }, role);

    const promiseData = firstValueFrom(observable);
    promiseData.then(successCallBack)
      .catch(errorCallBack);

    return await promiseData as { succeeded: boolean };
  }

  // PUT / UPDATE
  update(role: Update_Role, successCallBack?: any, errorCallBack?: (errorMessage: string) => void) {
    this.httpClientService.patch({
      controller: "roles",
      action: "update-role"
    }, role)
      .subscribe({
        next: result => {
          successCallBack();
        },
        error: (errorResponse: HttpErrorResponse) => {
          const errors: Array<{ key: string, value: Array<string> }> = errorResponse.error;

          let message = "";

          errors.forEach(error => {
            error.value.forEach(value => {
              message += `${value}<br>`;
            });
          });
          
          errorCallBack(message);
        }
      });
  }

  // DELETE
  async delete(id: string) {
    const deleteObservable: Observable<any> = this.httpClientService.delete<any>({
      controller: "roles"
    }, id);

    await firstValueFrom(deleteObservable);
  }

  // DELETE RANGE - POST
  async deleteRange(roleIds: string[]) {
    const deleteObservable: Observable<any> = this.httpClientService.deleteRange<any>({
      controller: "roles",
      action: "delete-range-role"
    }, {
      roleIds
    });

    await firstValueFrom(deleteObservable);
  }
}
