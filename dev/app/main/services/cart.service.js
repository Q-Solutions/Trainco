(function() {
  'use strict';

  angular
    .module('train')
    .factory('cartService', cartService);

  /** @ngInject */
  function cartService($log) {

    var service = {
      getCartItems: getCartItems,
      addItem: addItem,
      updateCart: updateCart,
      removeItem: removeItem,
      clearCart: clearCart
    };

    return service;

    function getCartItems() {
      var itemList = window.localStorage.getItem('cartItemList'); // eslint-disable-line
      return itemList && JSON.parse(itemList); // eslint-disable-line
    }

    function addItem(item, qty) {
      var itemStr = window.localStorage.getItem('cartItemList'); // eslint-disable-line
      var itemList = itemStr ? JSON.parse(itemStr) : []; // eslint-disable-line
      var itemInCart = itemList.find(function(cartItem) {
        return cartItem.id === item.id;
      });
      $log.debug(item)
      if (itemInCart) {
        itemInCart.quantity = item.quantity;
      } else {
        itemList.push({
          id: item.id,
          title: item.daysDescription,
          city: item.city,
          state: item.state,
          price: item.price,
          date: item.date,
          quantity: qty
        });
      }

      window.localStorage.setItem('cartItemList', JSON.stringify(itemList)); // eslint-disable-line
    }

    function updateCart(itemId, item, qty) {
      var itemStr = localStorage.getItem('cartItemList'); // eslint-disable-line
      var itemList = itemStr ? JSON.parse(itemStr) : []; // eslint-disable-line
      var itemInCart = itemList.findIndex(function(item) {
        return item.id === itemId;
      });
      itemInCart.quantity = qty;
      itemList.push(item);
    }

    function removeItem(itemId) {
      var itemStr = localStorage.getItem('cartItemList'); // eslint-disable-line
      var itemList = itemStr ? JSON.parse(itemStr) : []; // eslint-disable-line
      var index = itemList.findIndex(function(item) {
        return item.id === itemId;
      });
      if (index === -1) {
        return;
      }
      if (itemList.quantity > 1) {
        itemList[index].quantity -= 1;
      } else {
        itemList.splice(index, 1);
      }

      localStorage.setItem('cartItemList', JSON.stringify(itemList)); // eslint-disable-line
    }

    function clearCart() {
      localStorage.removeItem('cartItemList'); // eslint-disable-line
    }
  }
})();
