(function() {
  'use strict';

  angular
    .module('train.components')
    .controller('CartController', CartController);
  CartController.$inject = ['cartService', '$log', '$scope', '$http', '$window', '$document', '$timeout', 'CONSTANTS'];
  /** @ngInject */
  function CartController(cartService, $log, $scope, $http, $window, $document, $timeout, CONSTANTS) {
    var vm = this;
    var purchaseAPI = CONSTANTS.CART_API_URL;
    vm.cartItem = {};
    vm.isCartOpened = false;
    var queryString = window.location.search.substring(1);
    if (queryString.indexOf('cart') !== -1)
        vm.isCartOpened = true;
    vm.cartIsOpen = function() {
      vm.isCartOpened =! vm.isCartOpened
    }
    function calculateTotalPrice(itemList) {
      var totalPrice = itemList ? itemList.reduce(function(acc, item) {
        return acc + item.quantity * parseFloat(item.price);
      }, 0) : 0;
      return parseFloat(totalPrice.toFixed(2));
    }

    vm.cartItemList = cartService.getCartItems() || [];
    vm.cartTotalPrice = calculateTotalPrice(vm.cartItemList);

    $scope.$on('cartUpdated', function(event, data) {
      vm.isCartOpened = true;
      vm.cartItemList = cartService.getCartItems() || [];
      vm.cartTotalPrice = calculateTotalPrice(vm.cartItemList);
      $('html, body').stop(true).animate({scrollTop: 285}, 500);

    });

    vm.removeItemFromCart = function(itemId) {
      cartService.removeItem(itemId);
      vm.cartItemList = cartService.getCartItems() || [];
      vm.cartTotalPrice = calculateTotalPrice(vm.cartItemList);
    };

    /**
     * Handle key input
     * @param  {object} e the event
     * ng-keydown="searchInput.handleInput($event)"
     */
    vm.handleQuantInput = function(e, cartItem) {
      cartService.updateCart(cartItem);
      vm.cartItemList = cartService.getCartItems() || [];
      vm.cartTotalPrice = calculateTotalPrice(vm.cartItemList);
    }

    vm.sanitizeQuantInput = function(e, cartItem) {
      if (cartItem.quantity < 1) {
        cartItem.quantity = 1;
      }
      vm.handleQuantInput(e, cartItem);
    }

    vm.cartImages = {
      initial: '/assets/images/icon-cart-tab.png',
      final: '/assets/images/icon-cart-close-arrow.png',
      current: '/assets/images/icon-cart-tab.png'
    };
    vm.isActive = false;

    vm.doPurchase = function() {
      vm.cartItemList = cartService.getCartItems() || [];
      var cartDataArr = [];
      vm.cartItemList.forEach(function(item, index, array) {
        var id = item.id;
        var quantity = item.quantity;
        cartDataArr.push({
          Id: id,
          quantity: quantity
        })
        localStorage.setItem('cartDataArr', JSON.stringify(cartDataArr)); // eslint-disable-line
      });

      $http({
        method: 'POST',
        url: purchaseAPI,
        data: JSON.stringify(cartDataArr),
        contentType: 'application/json'
      }).success(function(data) {
        localStorage.setItem('guid', data.cartGuid);
        vm.redirectGuid = data.cartGuid;
        if (localStorage.getItem('tcJWT')) {
          $window.location.href = '/register/?cart=' + data.cartGuid + '&token=' + localStorage.getItem('tcJWT');
        } else {
        // $window.location.href = '/register/?cart=' + data.cartGuid;
        $window.location.href = '/dashboard/checkout/' + data.cartGuid;
        }
      });
    }
  }

})();
