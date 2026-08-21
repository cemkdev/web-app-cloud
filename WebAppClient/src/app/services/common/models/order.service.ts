import { Injectable } from '@angular/core';
import { HttpClientService } from '../http-client.service';
import { Create_Order } from '../../../contracts/order/create_order';
import { catchError, firstValueFrom, map, Observable } from 'rxjs';
import { List_Order } from '../../../contracts/order/list_order';
import { HttpErrorResponse } from '@angular/common/http';
import { Order_Detail } from '../../../contracts/order/order_detail';
import { OrderStatusHistory } from '../../../contracts/order/order_status_history';
import { OrderCustomer } from '../../../contracts/order/order_customer';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  constructor(private httpClientService: HttpClientService) { }

  // GET / LIST / READ
  async getAllOrders(page: number = 0, size: number = 10, successCallBack?: () => void, errorCallBack?: (errorMessage: string) => void): Promise<{ totalOrderCount: number, orders: List_Order[] }> {
    const data = await firstValueFrom(
      this.httpClientService.get<{ totalOrderCount: number, orders: List_Order[] }>({
        controller: "orders",
        action: "get-all-orders",
        queryString: `page=${page}&size=${size}`
      }).pipe(
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

  // GET BY ID
  async getOrderById(id: string, successCallBack?: () => void, errorCallBack?: (errorMessage: string) => void): Promise<Order_Detail> {
    const data = await firstValueFrom(
      this.httpClientService.get<Order_Detail>({
        controller: "orders",
        action: "get-order-by-id"
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

  async getOrderCustomerById(id: string): Promise<OrderCustomer> {
    return await firstValueFrom(
      this.httpClientService.get<OrderCustomer>({
        controller: "orders",
        action: "get-order-customer-by-id"
      }, id)
    );
  }

  // POST / CREATE
  async create(order: Create_Order): Promise<void> {
    const observable: Observable<any> = this.httpClientService.post({
      controller: "orders",
      action: "create-order"
    }, order);

    await firstValueFrom(observable);
  }

  // DELETE
  async delete(id: string) {
    const deleteObservable: Observable<any> = this.httpClientService.delete<any>({
      controller: "orders",
      action: "delete"
    }, id);

    await firstValueFrom(deleteObservable);
  }

  // DELETE RANGE - POST
  async deleteRange(orderIds: string[]) {
    const deleteObservable: Observable<any> = this.httpClientService.deleteRange<any>({
      controller: "orders",
      action: "delete-range"
    }, {
      orderIds
    });

    await firstValueFrom(deleteObservable);
  }

  // UPDATE ORDER STATUS - PUT
  async updateOrderStatus(orderId: string, newStatus: number, successCallBack?: () => void, errorCallBack?: (errorMessage: string) => void) {
    const observable: Observable<any> = this.httpClientService.put({
      controller: "orders",
      action: "update-order-status"
    }, {
      orderId,
      newStatus
    });

    await firstValueFrom(observable.pipe(
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
    ));
  }

  // GET ORDER STATUS HISTORY BY ID
  async getOrderStatusHistoryById(orderId: string, successCallBack?: () => void, errorCallBack?: (errorMessage: string) => void): Promise<OrderStatusHistory> {
    const data = await firstValueFrom(
      this.httpClientService.get<OrderStatusHistory>({
        controller: "orders",
        action: "get-order-status-history-by-id"
      }, orderId).pipe(
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
}
