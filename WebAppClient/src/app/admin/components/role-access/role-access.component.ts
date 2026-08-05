import { Component, OnInit, ViewChild } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { ApplicationService } from '../../../services/common/models/application.service';
import { BaseComponent, SpinnerType } from '../../../base/base.component';
import { EndpointMenu } from '../../../contracts/application-configurations/endpoint-menu';
import { AlertifyService, MessageType, Position } from '../../../services/admin/alertify.service';
import { List_Roles } from '../../../contracts/role/list_roles';
import { RoleService } from '../../../services/common/models/role.service';
import { AssignRoleEndpoint } from '../../../entities/assign-role-endpoint';
import { AuthorizationEndpointService } from '../../../services/common/models/authorization-endpoint.service';

@Component({
  selector: 'app-role-access',
  standalone: false,
  templateUrl: './role-access.component.html'
})
export class RoleAccessComponent extends BaseComponent implements OnInit {
  data: EndpointMenu[] = [];

  roles: List_Roles[] = [];
  availableRoles: List_Roles[] = [];
  selectedRoles: List_Roles[] = [];
  isEditing: boolean = false;

  originalPermissions: { [code: string]: { [role: string]: boolean } } = {};
  permissions: { [code: string]: { [role: string]: boolean } } = {};

  constructor(
    spinner: NgxSpinnerService,
    private alertifyService: AlertifyService,
    private applicationService: ApplicationService,
    private roleService: RoleService,
    private authorizationEndpointService: AuthorizationEndpointService
  ) {
    super(spinner);
  }

  async ngOnInit() {
    await this.initializeComponent();
  }

  async initializeComponent() {
    await this.getRoles();
    this.availableRoles = [...this.roles];

    this.selectedRoles = this.roles.slice(0, 3);
    this.availableRoles = this.roles.slice(3);

    await this.getMenus();
    this.constructTableStructure()
    await this.loadRoleEndpointPermissions();
    this.initRoles(); // 'availableRoles' update for selected roles when component initialized.
  }

  toggleEdit() {
    this.isEditing = !this.isEditing;
  }

  async getRoles() {
    this.showSpinner(SpinnerType.BallAtom);
    this.roles = await this.roleService.getRoles();
    this.hideSpinner(SpinnerType.BallAtom);
  }

  constructTableStructure(): void {
    for (const endpointMenu of this.data) {
      for (const endpointDefinition of endpointMenu.endpoints) {
        this.permissions[endpointDefinition.code] = {};

        for (const role of this.roles) {
          this.permissions[endpointDefinition.code][role.id] = false;
        }
      }
    }
  }

  async getMenus(): Promise<void> {
    this.data = await this.applicationService.getAuthorizeDefinitionEndpoints();

    this.data = this.data.map(endpointMenu => {
      endpointMenu.name = this.addSpacesBeforeCapitalLetters(endpointMenu.name);  // We are adding a space to the 'Menu Name'.
      return endpointMenu;
    });
  }

  addSpacesBeforeCapitalLetters(input: string): string {
    let result = input[0];

    for (let i = 1; i < input.length; i++) {
      const char = input[i];
      if (char === char.toUpperCase()) {
        result += ' ';
      }
      result += char;
    }

    return result;
  }

  async loadRoleEndpointPermissions(): Promise<void> {
    this.showSpinner(SpinnerType.BallAtom);
    this.authorizationEndpointService.getRolesEndpoints(
      (response) => {
        const rolesEndpoints = response.rolesEndpoints ?? response;

        for (const actionCode in this.permissions) {
          for (const roleId in this.permissions[actionCode]) {
            this.permissions[actionCode][roleId] = false;
          }
        }

        for (const roleEndpoint of rolesEndpoints) {
          const roleId = roleEndpoint.roleId;
          for (const endpoint of roleEndpoint.roleEndpoints) {
            const code = endpoint.endpointCode;
            const isAuthorized = endpoint.isAuthorized;

            if (this.permissions[code] && this.permissions[code][roleId] !== undefined) {
              this.permissions[code][roleId] = isAuthorized;
            }
          }
        }
        this.originalPermissions = JSON.parse(JSON.stringify(this.permissions));
        this.hideSpinner(SpinnerType.BallAtom);
      },
      (errorMessage) => {
        this.alertifyService.message("Error loading role permissions: " + errorMessage, {
          messageType: MessageType.Error,
          position: Position.TopRight
        });
        this.hideSpinner(SpinnerType.BallAtom);
      }
    );
  }

  async assignRoleToEndpoints() {
    this.showSpinner(SpinnerType.BallAtom);

    const assignRequests: AssignRoleEndpoint[] = [];
    for (const role of this.selectedRoles) {
      const assignRequest = new AssignRoleEndpoint();
      assignRequest.roleId = role.id;
      assignRequest.roleEndpoints = [];

      for (const endpointMenu of this.data) {
        for (const endpointDefinition of endpointMenu.endpoints) {
          const isAuthorized = this.permissions[endpointDefinition.code]?.[role.id] ?? false;

          assignRequest.roleEndpoints.push({
            menuName: endpointMenu.name.replaceAll(' ', ''),
            endpointCode: endpointDefinition.code,
            isAuthorized: isAuthorized
          });
        }
      }
      assignRequests.push(assignRequest);
    }

    await this.authorizationEndpointService.assignRoleEndpoints(assignRequests, async () => {
      this.alertifyService.message("Permissions updated successfully.", {
        dismissOthers: true,
        messageType: MessageType.Success,
        position: Position.TopRight
      });
      await this.initializeComponent();
      this.hideSpinner(SpinnerType.BallAtom);
    }, async error => {
      this.alertifyService.message("Error updating role!", {
        dismissOthers: true,
        messageType: MessageType.Error,
        position: Position.TopRight
      });
      await this.initializeComponent();
      this.hideSpinner(SpinnerType.BallAtom);
    });

    this.isEditing = false;
  }

  hasChanges(): boolean {
    return JSON.stringify(this.permissions) !== JSON.stringify(this.originalPermissions);
  }

  // Remove selected roles when component initialized first time, from availableRoles.
  initRoles() {
    for (let role of this.selectedRoles) {
      this.removeRoleFromAvailable(role);
    }
  }

  addRole(role: List_Roles) {
    // Max 3 roles on the table
    if (this.selectedRoles.length < 4) {
      // Add new role to selected roles
      this.selectedRoles.push(role);
      // Remove added role from availableRoles
      this.removeRoleFromAvailable(role);
    } else {
      alert('You can only select up to 4 roles.');
    }
  }

  @ViewChild('roleSelect') roleSelect!: any;
  removeRole(role: List_Roles) {
    // const index = this.selectedRoles.indexOf(role);
    const index = this.selectedRoles.findIndex(r => r.id === role.id);
    if (index > -1) {
      this.selectedRoles.splice(index, 1);
      this.addRoleToAvailable(role);

      // Force UI update
      this.availableRoles = [...this.availableRoles];

      // Reset selection (clear select)
      if (this.roleSelect) {
        this.roleSelect.writeValue(null);
      }
    }
  }

  removeRoleFromAvailable(role: List_Roles) {
    const index = this.availableRoles.findIndex(r => r.id === role.id);
    if (index > -1) {
      this.availableRoles.splice(index, 1);
    }
  }

  addRoleToAvailable(role: List_Roles) {
    if (this.availableRoles.findIndex(r => r.id === role.id) === -1) {
      this.availableRoles.push(role);
    }
  }
}
