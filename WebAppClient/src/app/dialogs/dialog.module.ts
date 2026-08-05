import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DeleteDialogComponent } from './delete-dialog/delete-dialog.component';

import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { SelectProductImageDialogComponent } from './product-dialogs/select-product-image-dialog/select-product-image-dialog.component';
import { FileUploadModule } from '../services/common/file-upload/file-upload.module';

import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

import { CreateProductDialogComponent } from './product-dialogs/create-product-dialog/create-product-dialog.component';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS, MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { UpdateProductDialogComponent } from './product-dialogs/update-product-dialog/update-product-dialog.component';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { CompleteOrderDialogComponent } from './order-detail-dialog/complete-order-dialog/complete-order-dialog.component';
import { CancelOrderDialogComponent } from './order-detail-dialog/cancel-order-dialog/cancel-order-dialog.component';
import { UpdateRoleDialogComponent } from './role-dialog/update-role-dialog/update-role-dialog.component';
import { AssignRoleDialogComponent } from './user-dialogs/assign-role-dialog/assign-role-dialog.component';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { QrcodeDialogComponent } from './product-dialogs/qrcode-dialog/qrcode-dialog.component';
import { ScanQrcodeDialogComponent } from './product-dialogs/scan-qrcode-dialog/scan-qrcode-dialog.component';

import { NgxScannerQrcodeModule, LOAD_WASM } from 'ngx-scanner-qrcode';

LOAD_WASM('assets/wasm/ngx-scanner-qrcode.wasm').subscribe();

@NgModule({
  declarations: [
    DeleteDialogComponent,
    SelectProductImageDialogComponent,
    CreateProductDialogComponent,
    UpdateProductDialogComponent,
    CompleteOrderDialogComponent,
    CancelOrderDialogComponent,
    UpdateRoleDialogComponent,
    AssignRoleDialogComponent,
    QrcodeDialogComponent,
    ScanQrcodeDialogComponent
  ],
  imports: [
    CommonModule,
    MatButtonModule,
    MatDialogModule, // MatDialog
    FileUploadModule,
    MatCardModule, MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    MatTableModule, MatSortModule,
    MatCheckboxModule,
    MatTooltipModule,
    FormsModule,
    NgxScannerQrcodeModule
  ],
  providers: [
    {
      provide: MAT_FORM_FIELD_DEFAULT_OPTIONS,
      useValue: { appearance: 'outline', subscriptSizing: 'dynamic' },
    }
  ]
})
export class DialogModule { }
