import { calculateTotalPrice } from '../../utils';
export function CartDirective() {
  'ngInject';

  let directive = {
    restrict: 'E',
    templateUrl: 'app/components/cart/cart.html',
    scope: {
    },
    controller: CartController,
    controllerAs: 'vm',
    bindToController: true
  };

  return directive;
}

class CartController {

  constructor(cartService, $log, $scope) {
    'ngInject';
    this.$log = $log;
    this.cartService = cartService;
    this.$scope = $scope;

    this.cartItemList = cartService.getCartItems() || [];
    this.cartTotalPrice = calculateTotalPrice(this.cartItemList);

    $scope.$on('cartUpdated', (event, data) => {
      this.cartItemList = cartService.getCartItems() || [];
      this.cartTotalPrice = calculateTotalPrice(this.cartItemList);
    });

    this.removeItemFromCart = (itemId) => {
      cartService.removeItem(itemId);
      this.cartItemList = this.cartService.getCartItems() || [];
      this.cartTotalPrice = calculateTotalPrice(this.cartItemList);
    };

    /**
     * Handle key input
     * @param  {object} e the event
     * ng-keydown="searchInput.handleInput($event)"
     */
    this.handleLocInput = (e, cartItem, qty) => {
      if (e.keyCode === 13) {
        cartService.addItem(cartItem);
        this.cartItemList = cartService.getCartItems() || [];
        this.cartTotalPrice = calculateTotalPrice(this.cartItemList);
      }
    }
  }

}
