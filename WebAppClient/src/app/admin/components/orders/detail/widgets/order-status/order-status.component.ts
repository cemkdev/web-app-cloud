import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { OrderService } from '../../../../../../services/common/models/order.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { AlertifyService, MessageType, Position } from '../../../../../../services/admin/alertify.service';
import { DialogService } from '../../../../../../services/common/dialog.service';
import { SpinnerType } from '../../../../../../base/base.component';
import { OrderStatusEnum } from '../../../../../../enums/order_status_enum';
import { OrderEventService } from '../../../../../../services/admin/order/order-event.service';
import { OrderStatusHistory, StatusChangeEntry } from '../../../../../../contracts/order/order_status_history';
import { CancelOrderDialogComponent, CancelOrderState } from '../../../../../../dialogs/order-detail-dialog/cancel-order-dialog/cancel-order-dialog.component';
import { DatePipe } from '@angular/common';
import { CurrentOrderStatusDates, DateFormat } from '../../../../../../dto_s/admin/order/order-detail/order_date_format';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-order-status',
  standalone: false,
  templateUrl: './order-status.component.html'
})
export class OrderStatusComponent implements OnInit, OnDestroy {
  @Input() orderId!: string;
  private _subscription: Subscription;

  orderStatusHistory: OrderStatusHistory;
  orderStatusEnum = OrderStatusEnum;
  statusIds: number[] = [];
  currentOrderStatusDates: CurrentOrderStatusDates = {
    cancelled: { weekDay: null, fullDate: null, time: null },
    pending: { weekDay: null, fullDate: null, time: null },
    approved: { weekDay: null, fullDate: null, time: null },
    shipping: { weekDay: null, fullDate: null, time: null },
    delivered: { weekDay: null, fullDate: null, time: null }
  };

  constructor(
    private orderService: OrderService,
    private spinner: NgxSpinnerService,
    private alertify: AlertifyService,
    private dialogService: DialogService,
    private orderEventService: OrderEventService,
    private datePipe: DatePipe
  ) { }

  async ngOnInit() {
    await this.initializeOrderStatus();

    this._subscription = this.orderEventService.refreshOrderItems$
      .subscribe(() => {
        this.initializeOrderStatus(); // This runs when the event is triggered.
      });
  }

  ngOnDestroy(): void {
    this._subscription?.unsubscribe();
  }

  private async initializeOrderStatus() {
    await this.getOrderStatusHistoryById(this.orderId);
    this.setAvailableStatuses(this.orderStatusHistory);
    this.getStatusDates(this.orderStatusHistory);
  }

  triggerOrder() {
    // Notify the order-items-table component after an operation.
    this.orderEventService.triggerRefreshOrderItems();
  }

  updateOrderStatusToCancel() {
    this.dialogService.openDialog({
      componentType: CancelOrderDialogComponent,
      data: CancelOrderState.Yes,
      options: {
        height: '300px',
        width: '550px'
      },
      afterClosed: async () => {
        this.spinner.show(SpinnerType.BallAtom);

        await this.orderService.updateOrderStatus(this.orderId, OrderStatusEnum.Cancelled,
          async () => {
            this.spinner.hide(SpinnerType.BallAtom);
            await this.initializeOrderStatus();
            await this.triggerOrder();
            this.alertify.message("Order Cancelled - The customer has been informed.", {
              dismissOthers: true,
              messageType: MessageType.Message,
              position: Position.TopRight
            });
          },
          (errorMessage) => {
            this.spinner.hide(SpinnerType.BallAtom);
            this.alertify.message(errorMessage, {
              dismissOthers: true,
              messageType: MessageType.Error,
              position: Position.TopRight
            });
          }
        );
      }
    })
  }

  async getOrderStatusHistoryById(orderId: string) {
    this.spinner.show(SpinnerType.BallAtom);

    this.orderStatusHistory = await this.orderService.getOrderStatusHistoryById(orderId,
      () => this.spinner.hide(SpinnerType.BallAtom),
      (errorMessage) => {
        this.spinner.hide(SpinnerType.BallAtom);
        this.alertify.message(errorMessage, {
          dismissOthers: true,
          messageType: MessageType.Error,
          position: Position.TopRight
        });
      }
    );
  }

  setAvailableStatuses(orderStatusHistory: OrderStatusHistory) {
    this.statusIds = orderStatusHistory.history.map(h => h.newStatusId);
  }

  isStatusAvailable(status: OrderStatusEnum): boolean {
    return this.statusIds.includes(status);
  }

  getStatusDates(orderStatusHistory: OrderStatusHistory) {
    for (let i = 0; i < orderStatusHistory.history.length; i++) {
      const element = orderStatusHistory.history[i];

      if (element.newStatusId === this.orderStatusEnum.Cancelled) {
        this.currentOrderStatusDates.cancelled = element.changedDate != null ? this.formatDateParts(element.changedDate) : null;
      }
      if (element.newStatusId === this.orderStatusEnum.Pending) {
        this.currentOrderStatusDates.pending = element.changedDate != null ? this.formatDateParts(element.changedDate) : null;
      }
      if (element.newStatusId === this.orderStatusEnum.Approved) {
        this.currentOrderStatusDates.approved = element.changedDate != null ? this.formatDateParts(element.changedDate) : null;
      }
      if (element.newStatusId === this.orderStatusEnum.Shipping) {
        this.currentOrderStatusDates.shipping = element.changedDate != null ? this.formatDateParts(element.changedDate) : null;
      }
      if (element.newStatusId === this.orderStatusEnum.Delivered) {
        this.currentOrderStatusDates.delivered = element.changedDate != null ? this.formatDateParts(element.changedDate) : null;
      }
    }
  }

  formatDateParts(dateInput: Date | string): DateFormat {
    const date = new Date(dateInput);

    const day = date.getDate().toString().padStart(2, '0');
    const month = date.toLocaleString('en-US', { month: 'short' }); // Apr
    const year = date.getFullYear();

    let hours = date.getHours();
    const minutes = date.getMinutes().toString().padStart(2, '0');
    const ampm = hours >= 12 ? 'PM' : 'AM';

    hours = hours % 12;
    hours = hours ? hours : 12; // 0 => 12

    const weekDay = this.datePipe.transform(date, 'EEE');
    const fullDate = `${day} ${month} ${year}`;
    const time = `${hours}:${minutes} ${ampm}`;

    return { weekDay, fullDate, time };
  }
}
