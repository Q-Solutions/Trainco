(function() {
  'use strict';

  angular
    .module('train')
    .directive('shoppingCart', shoppingCart);

  /** @ngInject */
  function shoppingCart() {
    var directive = {
      restrict: 'E',
      templateUrl: 'app/components/cart/cart.html',
      scope: {},
      controller: CartController,
      controllerAs: 'vm',
      bindToController: true
    };

    return directive;

    /** @ngInject */
    function CartController(cartService, $log, $scope, $http, $window, $localStorage) {
      var vm = this;
      var purchaseAPI = 'http://trainco.axial-client.com/api/carts/save';
      vm.cartItem = {};


      vm.$storage = $localStorage;
      function calculateTotalPrice(itemList) {
        var totalPrice = itemList ? itemList.reduce(function(acc, item) {
          return acc + item.quantity * parseFloat(item.price);
        }, 0) : 0;
        return parseFloat(totalPrice.toFixed(2));
      }

      vm.cartItemList = cartService.getCartItems() || [];
      vm.cartTotalPrice = calculateTotalPrice(vm.cartItemList);

      $scope.$on('cartUpdated', function(event, data) {
       vm.cartItemList = cartService.getCartItems() || [];
      vm.cartTotalPrice = calculateTotalPrice(vm.cartItemList);
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
    vm.handleLocInput = function (e, cartItem, qty) {
      if (e.keyCode === 13) {
        cartService.addItem(cartItem);
        this.cartItemList = cartService.getCartItems() || [];
        this.cartTotalPrice = calculateTotalPrice(this.cartItemList);
      }
    }



      vm.cartImages = {
        initial: '/assets/images/icon-cart-tab.png',
        final: '/assets/images/icon-cart-close-arrow.png',
        current: '/assets/images/icon-cart-tab.png'
      };
      vm.swapHere = function() {
        if (vm.cartImages.current === vm.cartImages.final) {
          vm.cartImages.current = vm.cartImages.initial
        } else if (vm.cartImages.current === vm.cartImages.initial) {
          vm.cartImages.current = vm.cartImages.final
        }
        ;
      };

      vm.doPurchase = function() {
        vm.cartItemList = vm.cartService.getCartItems() || [];
        $log.debug(vm.cartItemList)
        var cartDataArr = [];
        vm.cartItemList.forEach(function(item, index, array) {
          var id = item.id;
          var quantity = item.quantity;
          cartDataArr.push({
            Id: id,
            quantity: quantity
          })
          $log.debug(cartDataArr)
          window.localStorage.setItem('cartDataArr', JSON.stringify(cartDataArr)); // eslint-disable-line

        });

        $http({
          method: 'POST',
          url: purchaseAPI,
          data: JSON.stringify(cartDataArr),
          contentType: 'application/json'
        }).then(_success()).catch(_error());

        function _success(response) {
          var redirectGuid = response.cartGuid;
          window.location.href = '/register/?cart=' + response.cartGuid;
        }

        function _error(err) {
          $log.debug(err)
        }

      }

    }
  }

})();
