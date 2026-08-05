import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RoleAccessComponent } from './role-access.component';
import { RouterModule } from '@angular/router';

import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';

@NgModule({
  declarations: [
    RoleAccessComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild([
      { path: "", component: RoleAccessComponent }
    ]),
    MatCheckboxModule, MatButtonModule, MatDividerModule, FormsModule, MatIconModule, MatFormFieldModule, MatSelectModule, MatTooltipModule
  ]
})
export class RoleAccessModule { }
