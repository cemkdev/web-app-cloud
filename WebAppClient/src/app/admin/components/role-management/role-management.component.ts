import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatTableDataSource } from '@angular/material/table';
import { BaseComponent, SpinnerType } from '../../../base/base.component';
import { AlertifyService, MessageType, Position } from '../../../services/admin/alertify.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { RoleService } from '../../../services/common/models/role.service';
import { List_Roles } from '../../../contracts/role/list_roles';
import { DialogService } from '../../../services/common/dialog.service';
import { UpdateRoleDialogComponent } from '../../../dialogs/role-dialog/update-role-dialog/update-role-dialog.component';
import { DeleteDialogComponent, DeleteState } from '../../../dialogs/delete-dialog/delete-dialog.component';

declare var $: any;

@Component({
  selector: 'app-role-management',
  standalone: false,
  templateUrl: './role-management.component.html'
})
export class RoleManagementComponent extends BaseComponent implements OnInit {
  displayedColumns: string[] = ['select', 'index', 'name', 'isAdmin', 'edit', 'delete'];
  dataSource: MatTableDataSource<List_Roles> = null;
  selection = new SelectionModel<List_Roles>(true, []);
  roleForm!: FormGroup;

  roles: List_Roles[] = [];
  totalItemCount: number = 0;

  constructor(
    spinner: NgxSpinnerService,
    private alertifyService: AlertifyService,
    private dialogService: DialogService,
    private fb: FormBuilder,
    private roleService: RoleService
  ) {
    super(spinner)
  }

  async ngOnInit() {
    await this.initializeComponent();
  }

  async initializeComponent() {
    this.initializeForm();
    await this.getRoles();
  }

  initializeForm() {
    this.roleForm = this.fb.group({
      name: ['', Validators.required],
      isAdmin: [false]
    });
  }

  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.filter(role => role.name !== 'SystemAdministrator').length;
    return numSelected === numRows;
  }

  async toggleAllRows() {
    if (await this.isAllSelected()) {
      this.selection.clear();
      return;
    }
    this.selection.select(...this.dataSource.data.filter(role => role.name !== 'SystemAdministrator'));
  }

  checkboxLabel(row?: any): string {
    if (!row) {
      const allSelected = this.selection.hasValue() && this.dataSource?.data.length === this.selection.selected.length;
      return `${allSelected ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${row.id + 1}`;
  }

  async getRoles() {
    this.showSpinner(SpinnerType.BallAtom);
    this.roles = await this.roleService.getRoles();
    this.dataSource = new MatTableDataSource<List_Roles>(this.roles);
    this.totalItemCount = this.roles.length;
    this.hideSpinner(SpinnerType.BallAtom);
  }

  async createRole() {
    if (this.roleForm.valid) {
      this.showSpinner(SpinnerType.BallAtom);

      const formValue = this.roleForm.value;
      await this.roleService.createRole(formValue, async () => {
        this.hideSpinner(SpinnerType.BallAtom);
        this.alertifyService.message("The role has been successfully added.", {
          dismissOthers: true,
          messageType: MessageType.Success,
          position: Position.TopRight
        });
        await this.getRoles();
      }, errorMessage => {
        this.hideSpinner(SpinnerType.BallAtom);
        this.alertifyService.message(errorMessage, {
          dismissOthers: true,
          messageType: MessageType.Error,
          position: Position.TopRight
        });
      });
    }
  }

  deleteSelectedRoles() {
    const selectedIds = this.selection.selected.map(product => product.id);

    if (selectedIds.length === 0) {
      this.alertifyService.message("No item selected.", {
        messageType: MessageType.Warning,
        position: Position.TopCenter
      });
      return;
    }

    this.dialogService.openDialog({
      componentType: DeleteDialogComponent,
      options: {
        width: '400px',
        height: '230px'
      },
      data: DeleteState.Yes,
      afterClosed: async () => {
        this.showSpinner(SpinnerType.BallAtom);

        try {
          await this.roleService.deleteRange(selectedIds);
          for (let id of selectedIds) {
            const btn: HTMLElement = document.querySelector(`tr[data-id="${id}"]`) as HTMLElement;
            if (btn) {
              $(btn).animate({
                left: '100%'
              }, 500, () => {
                $(btn).fadeOut(100);
              });
            }
          }
          this.selection.clear();

          await this.initializeComponent();

          this.alertifyService.message("Items successfully deleted.", {
            messageType: MessageType.Success,
            position: Position.TopRight
          });
        } catch (error) {
          this.alertifyService.message("An error occurred while deleting items.", {
            messageType: MessageType.Error,
            position: Position.TopRight
          });
        } finally {
          this.hideSpinner(SpinnerType.BallAtom);
        }
      }
    });
  }

  updateRole(id: string) {
    const dialogRef = this.dialogService.openDialog({
      componentType: UpdateRoleDialogComponent,
      data: id,
      options: {
        width: '500px'
      }
    });

    dialogRef.afterClosed().subscribe(async result => {
      if (result == 'updated') {
        await this.initializeComponent();
      }
      else if (result != null && result !== '' && result != 'updated') {
        this.alertifyService.message(result, {
          dismissOthers: true,
          messageType: MessageType.Error,
          position: Position.TopRight
        });
      }
    });
  }
}
