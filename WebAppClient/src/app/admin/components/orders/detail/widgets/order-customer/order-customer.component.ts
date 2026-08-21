import { Component, Input } from '@angular/core';
import { OrderCustomer } from '../../../../../../contracts/order/order_customer';
import { OrderService } from '../../../../../../services/common/models/order.service';

@Component({
  selector: 'app-order-customer',
  standalone: false,
  templateUrl: './order-customer.component.html'
})
export class OrderCustomerComponent {
  @Input() orderId!: string;

  customer: OrderCustomer;

  constructor(
    private orderService: OrderService
  ) { }

  async ngOnInit(): Promise<void> {
    this.customer = await this.orderService.getOrderCustomerById(this.orderId);
  }

  formatPhoneNumber(phoneNumber: string | null): string {
    if (!phoneNumber)
      return '-';

    const digits = phoneNumber.replace(/\D/g, '');

    if (digits.startsWith('90') && digits.length === 12) {
      return `+90 ${digits.slice(2, 5)} ${digits.slice(5, 8)} ${digits.slice(8, 10)} ${digits.slice(10, 12)}`;
    }

    return phoneNumber;
  }
}
