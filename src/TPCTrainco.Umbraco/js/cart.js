"use strict";function performSearch(t){var e=$(".empty-message"),a=$(".class-loader");e.fadeOut(100,function(){a.fadeIn(150)});var i=$.parseJSON(t);if("undefined"==i||!i)return!1;if(!i.hasOwnProperty("classTopics")&&!i.hasOwnProperty("classId"))return!1;if(i.hasOwnProperty("classTopics")){if(i.classTopics.length>=4)var o=["all"];else var o=i.classTopics.filter(function(t,e){return i.classTopics.indexOf(t)==e});var n=o.length;if(2==n){o.splice(n-1,0,"and");var l=o.join(" "),c=l.replace("and,","and"),o=c}else if(n>2){o.splice(n-1,0,"and");var l=o.join(", "),c=l.replace("and,","and"),o=c}}a.fadeIn(90),$("#search-results").length&&$("html, body").animate({scrollTop:$("#search-results").offset().top-140},300),app.globalCollection.fetch({data:t,type:"POST",contentType:"application/json",success:function(t){$(".results").empty(),e.fadeOut(100,function(){0===t.length?a.fadeOut(150,function(){e.fadeIn(150).text("We were unable to find classes that fit your preferences. Please change your search terms and try again.")}):(a.fadeOut(150,function(){$(".search-page").length&&e.fadeIn(150).text("Displaying the closest matching "+o+" seminars to your selected location of "+i.location+".")}),app.classView=new app.ClassView({collection:app.globalCollection,el:".results"}),$(".detail-page-app").length&&$(".view-opts").trigger("click"))})}})}window.app=window.app||{},app.CartItemModel=Backbone.Model.extend({}),app.cartItemModel=new app.CartItemModel,window.app=window.app||{},app.ClassModel=Backbone.Model.extend({}),window.app=window.app||{},app.LocationModel=Backbone.Model.extend({}),window.app=window.app||{},app.ScheduleModel=Backbone.Model.extend({}),window.app=window.app||{},app.CartCollection=Backbone.Collection.extend({localStorage:new Backbone.LocalStorage("CartCollection"),url:"/"}),app.cartCollection=new app.CartCollection,window.app=window.app||{},app.ClassCollection=Backbone.Collection.extend({model:app.ClassModel,url:ApiDomain+"/api/seminars/search"}),app.globalCollection=new app.ClassCollection,$(document).ready(function(){}),$("#search-btn").on("click",function(){app.globalCollection.reset(),app.locationCollection.reset(),app.scheduleCollection.reset();var t;t=app.mainSearchSelect.getSearchParams(),performSearch(t)}),$("#search-btn-home").on("click",function(){app.mainSearchSelect.getSearchParams(),$("#main-search").select2("val").toString();window.location.href="/search-seminars/?homeref=1"+window.location.hash}),window.app=window.app||{},app.LocationCollection=Backbone.Collection.extend({model:app.LocationModel,url:ApiDomain+"/api/locations/searchbyseminar"}),app.locationCollection=new app.LocationCollection,window.app=window.app||{},app.ScheduleCollection=Backbone.Collection.extend({model:app.ScheduleModel,url:ApiDomain+"/api/schedules/searchbylocation"}),app.scheduleCollection=new app.ScheduleCollection,window.app=window.app||{},app.CartItemView=Backbone.View.extend({events:{"click .remove":"removeItemFromCart","blur .class-qty":function(t){this.updateItemTotal(t),this.updateCartTotalPrice(t),this.updateCartTotalQuantity()}},initialize:function(t){this.options=t||{},this.template=_.template($("#cartItemTemplate").html()),Backbone.on("calculateSubtotal",this.calculateSubtotal,this),Backbone.on("updateCartTotalPrice",this.updateCartTotalPrice,this),Backbone.on("clearCart",this.removeItemFromCart,this)},render:function(){var t=$(this.template(this.model.toJSON()));this.setElement(t),$("#cart-item-list").append(t);var e=this.model.get("quantity");return this.$el.find(".class-qty").val(e),this},renderFromLocalStore:function(){var t=$(this.template(this.model.toJSON()));return this.setElement(t),$("#cart-item-list").append(t),this.model.set({fromLS:!0,inCart:!0}),this.showDataFromLocalStore(),this.updateCartTotalQuantity(),this.updateCartTotalPrice(),this},showDataFromLocalStore:function(){var t=this.model.get("quantity");this.model.set("quantity",t);var e=this.model.get("price")*t;this.$el.find(".sub-total").text("$"+e),$(".read-only-cart").length?this.$el.find(".class-qty-num").text(t):this.$el.find(".class-qty").val(t)},calculateSubtotal:function(){var t=this.model.get("quantity");this.currentSubTotal=this.model.get("price")*t,this.$el.find(".sub-total").text("$ "+this.currentSubTotal),this.$(".class-qty").val(t),this.updateCartTotalPrice(),this.updateCartTotalQuantity()},updateCartTotalPrice:function(){var t=[];if($(".cart-item").find(".sub-total").each(function(){var e=parseInt($(this).text().replace("$",""));t.push(e)}),0==t.length)var t=[0];Backbone.trigger("updateTotalPrice",t,this.model)},removeItemFromCart:function(t,e){var a=this,i=$(t.currentTarget),o=i.data("theid"),n=function(){a.$el.slideUp(150,function(){a.remove(),a.model.destroy(),a.updateCartTotalQuantity(),a.updateCartTotalPrice(),setTimeout(function(){app.cartCollection.length||$(".cart-empty-msg").fadeIn()},10)})};if(e)n();else{var l=app.cartCollection.findWhere({theId:o}),c=l.get("fromLS");if(c)return n(),!1;var s=app.scheduleCollection.findWhere({id:o});s.set("inCart",!1),n()}},updateCartTotalQuantity:function(){var t=[];if(app.cartCollection.each(function(e){var a=e.get("quantity");t.push(a)}),0==t.length)var t=[0];Backbone.trigger("updateCartTotalQty",t)},updateItemTotal:function(t){t.preventDefault();var e=parseInt(this.$(".class-qty").val());0===e&&this.removeItemFromCart(t),this.listenTo(this.model,"change:quantity",this.calculateSubtotal),this.model.set("quantity",e),this.model.save("quantity",e)}}),window.app=window.app||{},app.CartNotifyView=Backbone.View.extend({events:{"click #check-out":"checkout"},initialize:function(){this.template=_.template($("#cartNotifyTemplate").html()),this.render(),this.totalCost=this.$(".total-cost"),Backbone.on("updateTotalPrice",this.updateTotalPrice,this),Backbone.on("displayTotalPrice",this.displayTotalPrice,this),Backbone.on("updateCartTotalQty",this.updateCartTotalQty,this),localStorage.length>1?(this.$(".cart-empty-msg").hide(),app.cartCollection.fetch({success:function(t,e){t.each(function(t){app.cartItemView=new app.CartItemView({model:t}).renderFromLocalStore()},this)}})):this.$(".cart-empty-msg").show()},render:function(){return this.$el.html(this.template),this},updateTotalPrice:function(t){var e=t,a=e.reduce(function(t,e){return t+e});this.totalCost.text(a)},displayTotalPrice:function(t,e){this.currentPrice=parseInt(this.totalCost.text());var a=t*parseInt(e);this.totalCost.text(this.currentPrice+a)},updateCartTotalQty:function(t){var e=t.reduce(function(t,e){return t+e});this.$(".items-total").text(e+" Items")},checkout:function(){var t=this,e=app.cartCollection.toJSON(),a=[];e.forEach(function(t,e,i){var o=t.theId,n=t.quantity;a.push({Id:o,quantity:n})}),this.$(".checkout-loader").show(),$.ajax({url:ApiDomain+"/api/carts/save",data:JSON.stringify(a),type:"POST",contentType:"application/json"}).done(function(e){var a=e.cartGuid;t.$(".checkout-loader").hide(),window.location.href="/register/?cart="+a}).fail(function(e){t.$(".checkout-loader").hide(),t.$(".btn-wrapper").prepend('<p class="checkout-err-msg">An error occurred. Please try again later.</p>')})},clearCart:function(){var t=!0;Backbone.trigger("clearCart",this,t),this.$(".checkout-loader").hide()}}),app.cartNotifyView=new app.CartNotifyView({el:"#cart"}),window.app=window.app||{},app.ClassView=Backbone.View.extend({initialize:function(){this.render()},render:function(){this.collection.each(function(t){this.renderSeminars(t)},this)},renderSeminars:function(t){this.$el.append(new app.SingleSeminarView({model:t}).render().el).hide().slideDown(500).fadeIn(600)}}),window.app=window.app||{},app.LocationView=Backbone.View.extend({tagName:"div",className:"one-location",events:{"click .location-icon":"showClassLocationMsg"},initialize:function(){this.template=_.template($("#locationTemplate").html());this.render(),this.fetchCounter=this.collection.length},render:function(){var t=this;this.locationIdArr=[],this.collection.each(function(e){var a=e.get("locationId");this.locationIdArr.push(a),t.$el.append(t.template(e.toJSON())).hide().slideDown(200).fadeIn(200),setTimeout(function(){t.renderSchedules(e)},1)},this)},renderSchedules:function(t){var e=this,a=t.get("courseId"),i=t.get("cityId"),o=t.get("searchId"),n=t.get("locationId"),l=this.$(".schedule-items-wrap"),c=this.$el.prev().find(".location-loader");c.css("display","block"),app.scheduleCollection.fetch({remove:!1,data:JSON.stringify({courseId:a,cityId:i,searchId:o,locationId:n}),type:"POST",contentType:"application/json",success:function(t){e.fetchCounter-=1,0===e.fetchCounter&&(c.css("display","none"),app.scheduleView=new app.ScheduleView({collection:app.scheduleCollection,el:l,locationLocId:e.locationIdArr}))},error:function(t){console.log(t)}})},showClassLocationMsg:function(t){t.preventDefault();var e=$(t.currentTarget);e.parent().parent().find(".location-msg").toggleClass("showing")}}),window.app=window.app||{},app.ScheduleView=Backbone.View.extend({tagName:"li",className:"seminar",events:{"click .add-to-cart":function(t){this.addToCart(t)}},initialize:function(t){this.template=_.template($("#scheduleTemplate").html()),this.options=t||{},this.locLocIdArr=t.locationLocId;this.render()},render:function(){var t=this;$.each(t.collection.toJSON(),function(e,a){$.each(t.locLocIdArr,function(e,i){a.locationId===i&&$(t.$el[e]).append(t.template(a)).hide().fadeIn(300)})})},addToCart:function(t){t.preventDefault();var e=$(t.currentTarget),a=this;this.$classQty=e.parent().parent().find(".class-qty");var i=function(){var t=l.get("quant"),e=l.get("id"),i=app.cartCollection.where({theId:e}),n=i[0].get("theId"),c=t+i[0].get("quantity");i[0].set("quantity",c),i[0].save(),o(),e==n&&($("#cart-item-list").find("[data-theid="+n+"]").find(".class-qty").val(t),Backbone.trigger("calculateSubtotal",t)),a.$classQty.val("")},o=function(){setTimeout(function(){$("html, body").animate({scrollTop:$("#cart").offset().top-80},200),$(".cart-visible").hasClass("down")||$(".cart-tab").trigger("click")},300)};if(""==this.$classQty.val()||0==this.$classQty.val())return e.parent().prev().find(".attendee-msg").addClass("showing"),this.$classQty.css("border-color","red"),this.$el.find(".btn-blue-hollow:focus").blur(),setTimeout(function(){e.parent().prev().find(".attendee-msg").removeClass("showing"),a.$classQty.css("border-color","#d7d7d7")},3e3),!1;$(".cart-empty-msg").hide(),e.blur().text("Added!").addClass("added"),o(),setTimeout(function(){e.text("Add to cart").removeClass("added")},2e3);var n=e.data("id"),l=this.collection.get(n),c=l.get("courseId"),s=(l.get("price"),l.get("date")),r=parseInt(this.$classQty.val()),p=l.get("inCart"),d=l.get("id");l.set("quant",r),l.set("theId",d);l.get("quant");if(p)i();else{l.set("inCart",!0);var h=app.globalCollection.findWhere({courseId:c}),u=app.locationCollection.findWhere({courseId:c}),m=h.get("title"),f=u.get("cityState"),w=[],g=!1,C=(this.$(".qty").val(),d);app.cartCollection.each(function(t){w.push(t.get("theId"))});for(var y=0;y<w.length;y++)C==w[y]&&(g=!0);g?i():(app.cartItemModel=new app.CartItemModel({title:m,city:f,date:s,quantity:r,theId:d,price:l.get("price")}),this.listenTo(app.cartCollection,"add",this.renderCartItem),app.cartCollection.create(app.cartItemModel.toJSON()),this.$classQty.val(""),this.stopListening())}},renderCartItem:function(t){var e=t.get("quantity"),a=t.get("price");app.cartItemView=new app.CartItemView({model:t,quantity:e,price:a}).render(),this.addQtyToCart(e,t),Backbone.trigger("calculateSubtotal",e)},addQtyToCart:function(t,e){var a=$(".items-total").text(),i=parseInt(a)+parseInt(t);$(".items-total").text(i+" Items")}}),window.app=window.app||{},app.SingleSeminarView=Backbone.View.extend({tagName:"li",className:"seminar-item col-xs-12",events:{"click .view-opts":"showClassOptions"},initialize:function(){this.template=_.template($("#classTemplate").html()),this.firstClick=!1},render:function(){return this.$el.append(this.template(this.model.toJSON())),this},showClassOptions:function(t){t.preventDefault();var e=this,a=this.model.get("open"),i=this.$(".schedule-item-wrap"),o=$(t.target);a?i.slideUp(400,function(){o.removeClass("red").html('<span class="plus">+</span>View Upcoming Seminars'),e.model.set("open",!1)}):i.slideDown(400,function(){o.addClass("red").html('<span class="plus turn">+</span>View Less'),e.model.set("open",!0)}),i.css({"border-top":"1px solid #D7D7D7"});var n=this.model.get("courseId"),l=this.model.get("searchId"),c=$($(t.currentTarget).closest(".result-description").next(".schedule-item-wrap"));this.firstClick||app.locationCollection.fetch({data:JSON.stringify({courseId:n,searchId:l}),type:"POST",contentType:"application/json",success:function(t){app.locationView=new app.LocationView({collection:app.locationCollection,el:c}),e.model.set("open",!0),e.firstClick=!0}})},updateOriginalModelQuantity:function(t){this.model.set("quantity",t),this.$(".qty").val(t),Backbone.off("updateOriginalModelQuantity")},showTotalPrice:function(t){t.preventDefault();var e=this.model.get("price");Backbone.trigger("displayTotalPrice",this.quantity,e)}}),app.singleSeminarView=new app.SingleSeminarView;