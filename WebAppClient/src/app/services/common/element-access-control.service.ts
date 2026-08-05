import { Injectable } from '@angular/core';
import { AssignRoleEndpoint } from '../../entities/assign-role-endpoint';
import { AuthorizationEndpointService } from './models/authorization-endpoint.service';
import { UserService } from './models/user.service';

@Injectable({
  providedIn: 'root'
})
export class ElementAccessControlService {
  private _permissions: AssignRoleEndpoint[] = [];

  get permissions(): AssignRoleEndpoint[] {
    return this._permissions;
  }

  set permissions(perms: AssignRoleEndpoint[]) {
    this._permissions = perms;
  }

  constructor(
    private authorizationService: AuthorizationEndpointService,
    private userService: UserService
  ) { }

  async preloadPermissions(userId: string): Promise<void> {
    
    this.permissions = await this.authorizationService.getCurrentUserRoleEndpoints();

    // const response = await this.authorizationService.getRolesEndpoints();
    // const allRolePermissions = response.rolesEndpoints ?? response;

    // const userRoles = await this.userService.getRolesByUserId(userId);

    // const filteredRolePermissions = allRolePermissions.filter(rp =>
    //   userRoles.some(userRole => userRole.roleId === rp.roleId && userRole.isAssigned)
    // );
    // this.permissions = filteredRolePermissions ?? [];
  }

  clearPermissions(): void {
    this.permissions = [];
  }

  hasPermission(menuName: string, endpointCode: string): boolean {
    return this.permissions.some(role =>
      role.roleEndpoints.some(e =>
        e.menuName === menuName &&
        e.endpointCode === endpointCode &&
        e.isAuthorized
      )
    );
  }
}
