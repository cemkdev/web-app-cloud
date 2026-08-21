import { Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { OrderService } from '../../../../../../services/common/models/order.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { AlertifyService, MessageType, Position } from '../../../../../../services/admin/alertify.service';
import { SpinnerType } from '../../../../../../base/base.component';
import { MatTableDataSource } from '@angular/material/table';
import { Order_Detail, OrderBasketItem_VM } from '../../../../../../contracts/order/order_detail';
import { MatSort } from '@angular/material/sort';
import { DialogService } from '../../../../../../services/common/dialog.service';
import { CompleteOrderDialogComponent, CompleteOrderState } from '../../../../../../dialogs/order-detail-dialog/complete-order-dialog/complete-order-dialog.component';
import { OrderStatusEnum } from '../../../../../../enums/order_status_enum';
import { OrderEventService } from '../../../../../../services/admin/order/order-event.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-order-items-table',
  standalone: false,
  templateUrl: './order-items-table.component.html'
})
export class OrderItemsTableComponent implements OnInit, OnDestroy {
  @Input() orderId!: string;
  private _subscription: Subscription;

  displayedColumns: string[] = ['index', 'imagePath', 'productName', 'productDescription', 'itemPrice', 'quantity', 'rating', 'totalItemAmount'];
  dataSource: MatTableDataSource<OrderBasketItem_VM> = null;

  order: Order_Detail;
  orderVM: OrderBasketItem_VM[] = [];
  orderStatus_VM: {
    badgeClass: string
    statusText: string
  }
  orderStatusEnum = OrderStatusEnum;

  totalOrderAmount: string;
  sampleDiscount: string = "2.99";
  sampleEstimatedTax: string = "2.99";

  @ViewChild(MatSort) sort: MatSort;

  constructor(
    private orderService: OrderService,
    private spinner: NgxSpinnerService,
    private alertify: AlertifyService,
    private dialogService: DialogService,
    private orderEventService: OrderEventService
  ) { }

  async ngOnInit() {
    await this.initializeOrderDetail();

    this._subscription = this.orderEventService.refreshOrderItems$
      .subscribe(() => {
        this.initializeOrderDetail(); // This runs when the event is triggered.
      });
  }
  ngOnDestroy(): void {
    this._subscription?.unsubscribe();
  }

  private async initializeOrderDetail() {
    await this.getOrderById(this.orderId);
    this.orderStatus_VM = this.getOrderStatusInfo(this.order.statusId);
    this.getTotalPrice();
    this.manipulateBasketItemsData(this.order);
  }

  triggerOrder() {
    // Notify the order-items-table component after an operation.
    this.orderEventService.triggerRefreshOrderItems();
  }

  async getOrderById(id: string) {
    this.spinner.show(SpinnerType.BallAtom);

    this.order = await this.orderService.getOrderById(id,
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

  manipulateBasketItemsData(sourceData: Order_Detail): void {
    this.orderVM = [];

    for (let i = 0; i < sourceData.orderBasketItems.length; i++) {
      this.getStars(sourceData.orderBasketItems[i].rating);

      // Each Item Integer/Fraction Divisions
      let [itemIntegerPart, itemFractionPart] = sourceData.orderBasketItems[i].price.toFixed(2).split('.');
      if (itemFractionPart == undefined)
        itemFractionPart = "00";

      // Total Item Integer/Fraction Divisions
      let [totalItemIntegerPart, totalItemFractionPart] = (sourceData.orderBasketItems[i].price * sourceData.orderBasketItems[i].quantity).toFixed(2).split('.');
      if (totalItemFractionPart == undefined)
        totalItemFractionPart = "00";

      let manipulatedData = {
        imagePath: sourceData.orderBasketItems[i].orderProductImageFile != null ? sourceData.orderBasketItems[i].orderProductImageFile.path : "",
        productName: sourceData.orderBasketItems[i].name,
        productDescription: sourceData.orderBasketItems[i].description,
        itemPrice: `${itemIntegerPart}.${itemFractionPart}`,
        quantity: sourceData.orderBasketItems[i].quantity,
        rating: this.getStars(sourceData.orderBasketItems[i].rating),
        totalItemAmount: `${totalItemIntegerPart}.${totalItemFractionPart}`,
        isProductDeleted: sourceData.orderBasketItems[i].isProductDeleted
      };
      this.orderVM.push(manipulatedData);
      this.dataSource = new MatTableDataSource<OrderBasketItem_VM>(this.orderVM);
      this.dataSource.sort = this.sort;
      this.dataSource.sortingDataAccessor = (item, property) => {
        switch (property) {
          case 'itemPrice':
            return parseFloat(item.itemPrice);
          case 'quantity':
            return item.quantity;
          case 'rating':
            return item.rating.rating;
          case 'totalItemAmount':
            return item.totalItemAmount;
          default:
            return item[property];
        }
      };
    }
  }

  getStars(rating: number): Partial<any> {
    const stars = [];

    for (let i = 1; i <= 5; i++) {
      if (rating >= i) {
        stars.push('star');
      } else if (rating >= i - 0.5) {
        stars.push('star_half');
      } else {
        stars.push('star_outline');
      }
    }
    return {
      stars, rating
    };
  }

  // Total Order Amount Calculation and Integer/Fraction Divisions
  getTotalPrice() {
    let totalPrice: number = 0

    this.order.orderBasketItems.forEach(item => {

      totalPrice += item.price * item.quantity;
      totalPrice = parseFloat(totalPrice.toFixed(2));

      let [integerPart, fractionPart] = totalPrice.toFixed(2).split('.');
      if (fractionPart == undefined)
        fractionPart = "00";

      this.totalOrderAmount = `${integerPart}.${fractionPart}`;
    });
  }

  getOrderStatusInfo(statusId: number) {
    let badgeClass = 'badge-unknown';
    let statusText = 'Unknown';

    const statusEnumKeys = Object.keys(this.orderStatusEnum).filter(key => !isNaN(Number(key)));
    statusEnumKeys.forEach(key => {
      const currentStatusId = Number(key);
      if (currentStatusId === statusId) {
        badgeClass = `badge-${OrderStatusEnum[currentStatusId].toLowerCase()}`;  // 'badge-cancelled', 'badge-pending' gibi
        statusText = OrderStatusEnum[currentStatusId] ?? 'Unknown';
      }
    });

    return { badgeClass, statusText };
  }

  updateOrderStatus() {
    this.dialogService.openDialog({
      componentType: CompleteOrderDialogComponent,
      data: CompleteOrderState.Yes,
      options: {
        height: '300px',
        width: '550px'
      },
      afterClosed: async () => {
        this.spinner.show(SpinnerType.BallAtom);
        await this.orderService.updateOrderStatus(this.orderId, OrderStatusEnum.Approved,
          async () => {
            this.spinner.hide(SpinnerType.BallAtom);
            await this.initializeOrderDetail();
            await this.triggerOrder();
            this.alertify.message("Order Approved - The customer has been informed.", {
              dismissOthers: true,
              messageType: MessageType.Success,
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
}
