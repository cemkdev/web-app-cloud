import { Component, OnInit } from '@angular/core';
import { BaseComponent, SpinnerType } from '../../../base/base.component';
import { NgxSpinnerService } from 'ngx-spinner';
import { BasketService } from '../../../services/common/models/basket.service';
import { List_Basket_Item, List_Basket_Item_VM } from '../../../contracts/basket/list_basket_item';
import { Update_Basket_Item } from '../../../contracts/basket/update_basket_item';
import { animate, group, state, style, transition, trigger } from '@angular/animations';
import { OrderService } from '../../../services/common/models/order.service';
import { Create_Order } from '../../../contracts/order/create_order';
import { CustomToastrService, ToastrMessageType, ToastrPosition } from '../../../services/ui/custom-toastr.service';
import { Router } from '@angular/router';

declare var $: any;

@Component({
  selector: 'app-baskets',
  standalone: false,
  templateUrl: './baskets.component.html',
  styleUrl: './baskets.component.scss'
})
export class BasketsComponent extends BaseComponent implements OnInit {

  basketItems: List_Basket_Item[];
  basketItemsVM: List_Basket_Item_VM[] = [];
  totalPriceIntegerPart: string;
  totalPriceFractionPart: string;

  constructor(
    spinner: NgxSpinnerService,
    private basketService: BasketService,
    private orderService: OrderService,
    private toastrService: CustomToastrService,
    private router: Router
  ) {
    super(spinner);
  }

  async ngOnInit() {
    this.showSpinner(SpinnerType.BallAtom);
    await this.getBasketItems();
    this.manipulateBasketItemsData(this.basketItems);
    this.getTotalPrice();
    this.hideSpinner(SpinnerType.BallAtom);
  }

  async getBasketItems() {
    this.basketItems = await this.basketService.get();
  }

  manipulateBasketItemsData(sourceData: List_Basket_Item[]): void {
    this.basketItemsVM = [];

    for (let i = 0; i < sourceData.length; i++) {

      let [integerPart, fractionPart] = sourceData[i].price.toString().split('.');
      if (fractionPart == undefined)
        fractionPart = "00";

      let manipulatedData = {
        basketItemId: sourceData[i].basketItemId,
        productId: sourceData[i].productId,
        name: sourceData[i].name,
        description: sourceData[i].description,
        stock: sourceData[i].stock,
        priceIntegerPart: integerPart,
        priceFractionPart: fractionPart,
        quantity: sourceData[i].quantity,
        productImageFile: sourceData[i].productImageFile
      };
      this.basketItemsVM.push(manipulatedData);
    }
  }

  getTotalPrice() {
    let totalPrice: number = 0

    this.basketItems.forEach(item => {

      totalPrice += item.price * item.quantity;
      totalPrice = parseFloat(totalPrice.toFixed(2));

      let [integerPart, fractionPart] = totalPrice.toString().split('.');
      if (fractionPart == undefined)
        fractionPart = "00";

      this.totalPriceIntegerPart = integerPart;
      this.totalPriceFractionPart = fractionPart;
    });
  }

  async updateQuantity(basketItemId: string, quantity: number) {
    this.showSpinner(SpinnerType.BallAtom);

    const basketItem: Update_Basket_Item = new Update_Basket_Item();
    basketItem.basketItemId = basketItemId;
    basketItem.quantity = quantity;

    await this.basketService.updateQuantity(basketItem);
    await this.getBasketItems();
    this.manipulateBasketItemsData(this.basketItems);
    this.getTotalPrice();
    
    this.hideSpinner(SpinnerType.BallAtom);
  }

  async removeBasketItem(basketItemId: string) {
    this.showSpinner(SpinnerType.BallAtom);
    await this.basketService.remove(basketItemId);
    await this.getBasketItems();
    this.manipulateBasketItemsData(this.basketItems);
    this.getTotalPrice();

    $("." + basketItemId).animate({
      right: '100%'
    }, 500, () => {
      $("." + basketItemId).fadeOut(200, this.hideSpinner(SpinnerType.BallAtom));
    });
  }

  async proceedToCheckout() {
    this.showSpinner(SpinnerType.BallAtom);
    const order: Create_Order = new Create_Order();
    order.address = "Karşıyaka";
    order.description = "Mavişehir, Mavibahçe yanı.";

    await this.orderService.create(order);
    $("#basketModal").modal("hide");
    this.hideSpinner(SpinnerType.BallAtom);

    this.toastrService.message("Your order has been created and is now being processed.", "Order Placed Successfully", {
      messageType: ToastrMessageType.Info,
      position: ToastrPosition.TopRight
    });

    this.router.navigate(["/"]);
  }
}
