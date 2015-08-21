'use strict';

window.app = window.app || {};

// "view the cart" view
app.CartItemView = Backbone.View.extend({
    template: _.template($('#cartItemTemplate').html()),

    events: {
        'click .remove': 'removeItemFromCart',
        'blur .class-qty': function(e) {
            this.updateItemTotal(e);
            this.updateCartTotalPrice(e);
            this.updateCartTotalQuantity();
        }
    },

    initialize: function(options) {
        var _this = this;
        this.options = options || {};
        // this.listenTo(app.cartCollection, 'remove', this.updateCartTotalPrice, this.updateCartTotalQuantity);
        Backbone.on('calculateSubtotal', this.calculateSubtotal, this);
        Backbone.on('updateCartTotalPrice', this.updateCartTotalPrice, this);
    },

    render: function() {
        var $tpl = $(this.template(this.model.toJSON()));
        this.setElement($tpl);
        $('#cart-item-list').append($tpl);
        var quantity = this.model.get('quantity');
        this.$el.find('.class-qty').val(quantity);
        return this;
    },

    // this is only called when pulling from localStorage
    renderFromLocalStore: function() {
        var $tpl = $(this.template(this.model.toJSON()));
        this.setElement($tpl);
        $('#cart-item-list').append($tpl);
        this.model.set({
            fromLS: true,
            inCart: true
        });

        // fill in the cart data
        this.showDataFromLocalStore();

        // fill in the cart's total quantity and price
        this.updateCartTotalQuantity();
        this.updateCartTotalPrice();
        
        return this;
    },

    // fills view data in from local store model data
    showDataFromLocalStore: function() {
        var lsModelQuantity = this.model.get('quantity');
        this.model.set('quantity', lsModelQuantity);
        var lsModelSubTotal = this.model.get('price') * lsModelQuantity;
        this.$el.find('.sub-total').text('$' + lsModelSubTotal);
        this.$el.find('.class-qty').val(lsModelQuantity);
    },


    // quantity of each item in cart, changes on update or blur when item is in cart
    insertQuantity: function(model, quantity) {
        this.model.set('quantity', quantity);
        this.$el.find('.class-qty').last().val(quantity);        
    },

    // calculates subtotal for individual item
    calculateSubtotal: function() {
        // var quant = parseInt(quantity);
        // var isModelFromLocalStore = this.model.get('fromLS');
        // if(isModelFromLocalStore) {
        //     return false;
        // }
        var quantity = this.model.get('quantity');
        // setting quantity on new model
        // this.model.set('quantity', quant);
        this.currentSubTotal = this.model.get('price') * quantity;
        this.$el.find('.sub-total').text('$ ' + this.currentSubTotal);

        // this line updates the quantity in the just-added item
        this.$('.class-qty').val(quantity);

        // Backbone.off('calculateSubtotal');

        // updates total dollar value of cart on click of add item
        this.updateCartTotalPrice();
    },

    // if one clicks update button, sums subtotals
    updateCartTotalPrice: function() {
        var subTotalsArr = [];

        $('.cart-item').find('.sub-total').each(function() {
            var dollarAmount = parseInt($(this).text().replace('$', ''));
            subTotalsArr.push(dollarAmount);
        });

        // if one removes all the items in the cart, set the array val to zero
        if(subTotalsArr.length == 0) {
            var subTotalsArr = [0];
        }
        Backbone.trigger('updateTotalPrice', subTotalsArr, this.model);
    },

    // removes item from cart, re-calculates total price
    removeItemFromCart: function(e) {
        e.preventDefault();
        var _this = this;

        // remove the item from the DOM
        this.$el.slideUp(150, function() {
            _this.remove();
        });

        // remove the item from the collection
        this.model.destroy();

        // decrement cart total number
        this.updateCartTotalQuantity();

        // decrement cart total price
        this.updateCartTotalPrice();

    },

    // updates the cart total on cart item quantity update. purely in DOM. only called on remove.
    updateCartTotalQuantity: function() {
        var quantityArr = [];

        app.cartCollection.each(function(cartItemModel) {
            var itemQuantity = cartItemModel.get('quantity');
            quantityArr.push(itemQuantity);
        });

        if(quantityArr.length == 0) {
            var quantityArr = [0];
        }

        Backbone.trigger('updateCartTotalQty', quantityArr);
    },

    // calculates num of items for each item in the cart
    updateItemTotal: function(e) {
        e.preventDefault();
        var updatedQty = parseInt(this.$('.class-qty').val());

        // if someone changes the quantity to zero, remove item
        if(updatedQty === 0) {
            this.removeItemFromCart(e);
        }

        this.listenTo(this.model, 'change:quantity', this.calculateSubtotal);
        this.model.set('quantity', updatedQty);
                // console.log(this.model)
        // this.calculateSubtotal(updatedQty);
        // Backbone.trigger('updateOriginalModelQuantity', updatedQty);
    },

    // updates quantity for single item
    updateQuantity: function(quantity) {

        this.$el.find('.class-qty').val(quantity)



        // Backbone.trigger('calculateSubtotal', quantity);

        // updates the quantity of the original element if changed from the cart.
        // listener attached here so it only runs once
        // Backbone.on('updateOriginalModelQuantity', this.updateOriginalModelQuantity, this);
    }


});