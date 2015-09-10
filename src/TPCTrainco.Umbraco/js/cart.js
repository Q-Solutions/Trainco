"use strict";function performSearch(searchParams){var $emptyMsg=$(".empty-message"),$classLoader=$(".class-loader");$emptyMsg.fadeOut(100,function(){$classLoader.fadeIn(150)});var dataReFormat=$.parseJSON(searchParams);if("undefined"==dataReFormat||!dataReFormat)return!1;if(!dataReFormat.hasOwnProperty("classTopics")&&!dataReFormat.hasOwnProperty("classId"))return!1;if(dataReFormat.hasOwnProperty("classTopics")){if(dataReFormat.classTopics.length>=4)var topics=["all"];else var topics=dataReFormat.classTopics.filter(function(item,pos){return dataReFormat.classTopics.indexOf(item)==pos});var length=topics.length;if(2==length){topics.splice(length-1,0,"and");var topicsList=topics.join(" "),topicsListTwo=topicsList.replace("and,","and"),topics=topicsListTwo}else if(length>2){topics.splice(length-1,0,"and");var topicsList=topics.join(", "),topicsListTwo=topicsList.replace("and,","and"),topics=topicsListTwo}}$classLoader.fadeIn(90),$("#search-results").length&&$("html, body").animate({scrollTop:$("#search-results").offset().top-140},300),app.globalCollection.fetch({data:searchParams,type:"POST",contentType:"application/json",success:function(data){$(".results").empty(),$emptyMsg.fadeOut(100,function(){0===data.length?$classLoader.fadeOut(150,function(){$emptyMsg.fadeIn(150).text("We were unable to find classes that fit your preferences. Please change your search terms and try again.")}):($classLoader.fadeOut(150,function(){$(".search-page").length&&$emptyMsg.fadeIn(150).text("Displaying the closest matching "+topics+" seminars to your selected location of "+dataReFormat.location+".")}),app.classView=new app.ClassView({collection:app.globalCollection,el:".results"}),$(".detail-page-app").length&&$(".view-opts").trigger("click"))})}})}window.app=window.app||{},app.CartItemModel=Backbone.Model.extend({}),app.cartItemModel=new app.CartItemModel,window.app=window.app||{},app.ClassModel=Backbone.Model.extend({}),window.app=window.app||{},app.LocationModel=Backbone.Model.extend({}),window.app=window.app||{},app.ScheduleModel=Backbone.Model.extend({}),window.app=window.app||{},app.CartCollection=Backbone.Collection.extend({localStorage:new Backbone.LocalStorage("CartCollection"),url:"/"}),app.cartCollection=new app.CartCollection,window.app=window.app||{},app.ClassCollection=Backbone.Collection.extend({model:app.ClassModel,url:ApiDomain+"/api/seminars/search"}),app.globalCollection=new app.ClassCollection;var performSearchCallback=function(){app.globalCollection.reset(),app.locationCollection.reset(),app.scheduleCollection.reset();var searchParams,searchLocationVal=$("#main-search").select2("val");searchLocationVal?(searchParams=app.mainSearchSelect.getSearchParams(),performSearch(searchParams)):($(".empty-location-msg").show(),$(".class-loader").hide())},performHomeSearchCallback=function(){app.mainSearchSelect.getSearchParams(),$("#main-search").select2("val").toString();window.location.href="/search-seminars/?homeref=1"+window.location.hash};$("#search-btn").on("click",performSearchCallback),$("#search-btn-home").on("click",performHomeSearchCallback),$(document).keydown(function(){13==event.which&&($("#search-btn").length&&performSearchCallback(),$("#search-btn-home").length&&performHomeSearchCallback())}),window.app=window.app||{},app.LocationCollection=Backbone.Collection.extend({model:app.LocationModel,url:ApiDomain+"/api/locations/searchbyseminar"}),app.locationCollection=new app.LocationCollection,window.app=window.app||{},app.ScheduleCollection=Backbone.Collection.extend({model:app.ScheduleModel,url:ApiDomain+"/api/schedules/searchbylocation"}),app.scheduleCollection=new app.ScheduleCollection,window.app=window.app||{},app.CartItemView=Backbone.View.extend({events:{"click .remove":"removeItemFromCart","blur .class-qty":function(e){this.updateItemTotal(e),this.updateCartTotalPrice(e),this.updateCartTotalQuantity()}},initialize:function(options){this.options=options||{},this.template=_.template($("#cartItemTemplate").html()),Backbone.on("calculateSubtotal",this.calculateSubtotal,this),Backbone.on("updateCartTotalPrice",this.updateCartTotalPrice,this),Backbone.on("clearCart",this.removeItemFromCart,this)},render:function(){var $tpl=$(this.template(this.model.toJSON()));this.setElement($tpl),$("#cart-item-list").append($tpl);var quantity=this.model.get("quantity");return this.$el.find(".class-qty").val(quantity),this},renderFromLocalStore:function(){var $tpl=$(this.template(this.model.toJSON()));return this.setElement($tpl),$("#cart-item-list").append($tpl),this.model.set({fromLS:!0,inCart:!0}),this.showDataFromLocalStore(),this.updateCartTotalQuantity(),this.updateCartTotalPrice(),this},showDataFromLocalStore:function(){var lsModelQuantity=this.model.get("quantity");this.model.set("quantity",lsModelQuantity);var lsModelSubTotal=this.model.get("price")*lsModelQuantity;this.$el.find(".sub-total").text("$"+lsModelSubTotal),$(".read-only-cart").length?this.$el.find(".class-qty-num").text(lsModelQuantity):this.$el.find(".class-qty").val(lsModelQuantity)},calculateSubtotal:function(){var quantity=this.model.get("quantity");this.currentSubTotal=this.model.get("price")*quantity,this.$el.find(".sub-total").text("$ "+this.currentSubTotal),this.$(".class-qty").val(quantity),this.updateCartTotalPrice(),this.updateCartTotalQuantity()},updateCartTotalPrice:function(){var subTotalsArr=[];if($(".cart-item").find(".sub-total").each(function(){var dollarAmount=parseInt($(this).text().replace("$",""));subTotalsArr.push(dollarAmount)}),0==subTotalsArr.length)var subTotalsArr=[0];Backbone.trigger("updateTotalPrice",subTotalsArr,this.model)},removeItemFromCart:function(e,success){var _this=this,target=$(e.currentTarget),id=target.data("theid"),removeCartItem=function(){_this.$el.slideUp(150,function(){_this.remove(),_this.model.destroy(),_this.updateCartTotalQuantity(),_this.updateCartTotalPrice(),setTimeout(function(){app.cartCollection.length||$(".cart-empty-msg").fadeIn()},10)})};if(success)removeCartItem();else{var cartItemFromLS=app.cartCollection.findWhere({theId:id}),isItemFromLS=cartItemFromLS.get("fromLS");if(isItemFromLS)return removeCartItem(),!1;var originalScheduleModel=app.scheduleCollection.findWhere({id:id});originalScheduleModel.set("inCart",!1),removeCartItem()}},updateCartTotalQuantity:function(){var quantityArr=[];if(app.cartCollection.each(function(cartItemModel){var itemQuantity=cartItemModel.get("quantity");quantityArr.push(itemQuantity)}),0==quantityArr.length)var quantityArr=[0];Backbone.trigger("updateCartTotalQty",quantityArr)},updateItemTotal:function(e){e.preventDefault();var updatedQty=parseInt(this.$(".class-qty").val());0===updatedQty&&this.removeItemFromCart(e),this.listenTo(this.model,"change:quantity",this.calculateSubtotal),this.model.set("quantity",updatedQty),this.model.save("quantity",updatedQty)}}),window.app=window.app||{},app.CartNotifyView=Backbone.View.extend({events:{"click #check-out":"checkout"},initialize:function(){this.template=_.template($("#cartNotifyTemplate").html()),this.render(),this.totalCost=this.$(".total-cost"),Backbone.on("updateTotalPrice",this.updateTotalPrice,this),Backbone.on("displayTotalPrice",this.displayTotalPrice,this),Backbone.on("updateCartTotalQty",this.updateCartTotalQty,this),localStorage.length>1?(this.$(".cart-empty-msg").hide(),app.cartCollection.fetch({success:function(coll,resp){coll.each(function(modelFromStorage){app.cartItemView=new app.CartItemView({model:modelFromStorage}).renderFromLocalStore()},this)}})):this.$(".cart-empty-msg").show()},render:function(){return this.$el.html(this.template),this},updateTotalPrice:function(subTotals){var subTotArray=subTotals,updatedTotalPrice=subTotArray.reduce(function(a,b){return a+b});this.totalCost.text(updatedTotalPrice)},displayTotalPrice:function(quantity,price){this.currentPrice=parseInt(this.totalCost.text());var totalPrice=quantity*parseInt(price);this.totalCost.text(this.currentPrice+totalPrice)},updateCartTotalQty:function(quantityArray){var updatedQty=quantityArray.reduce(function(a,b){return a+b});this.$(".items-total").text(updatedQty+" Items")},checkout:function(){var _this=this,cartData=app.cartCollection.toJSON(),cartDataArray=[];cartData.forEach(function(item,index,array){var id=item.theId,quant=item.quantity;cartDataArray.push({Id:id,quantity:quant})}),this.$(".checkout-loader").show(),$.ajax({url:ApiDomain+"/api/carts/save",data:JSON.stringify(cartDataArray),type:"POST",contentType:"application/json"}).done(function(successObj){var redirectGuid=successObj.cartGuid;_this.$(".checkout-loader").hide(),window.location.href="/register/?cart="+redirectGuid}).fail(function(error){_this.$(".checkout-loader").hide(),_this.$(".btn-wrapper").prepend('<p class="checkout-err-msg">An error occurred. Please try again later.</p>')})},clearCart:function(){var success=!0;Backbone.trigger("clearCart",this,success),this.$(".checkout-loader").hide()}}),app.cartNotifyView=new app.CartNotifyView({el:"#cart"}),window.app=window.app||{},app.ClassView=Backbone.View.extend({initialize:function(){this.render()},render:function(){this.collection.each(function(model){this.renderSeminars(model)},this)},renderSeminars:function(seminarModel){this.$el.append(new app.SingleSeminarView({model:seminarModel}).render().el).hide().slideDown(500).fadeIn(600)}}),window.app=window.app||{},app.LocationView=Backbone.View.extend({tagName:"div",className:"one-location",events:{"click .location-icon":"showClassLocationMsg"},initialize:function(){this.template=_.template($("#locationTemplate").html()),this.render(),this.fetchCounter=this.collection.length},render:function(){var _this=this;this.locationIdArr=[],this.collection.each(function(model){var locationIdToGet=model.get("locationId");this.locationIdArr.push(locationIdToGet),_this.$el.append(_this.template(model.toJSON())).hide().slideDown(200).fadeIn(200),setTimeout(function(){_this.renderSchedules(model)},1)},this)},renderSchedules:function(theModel){var _this=this,courseIdToGet=theModel.get("courseId"),cityIdToGet=theModel.get("cityId"),searchIdToGet=theModel.get("searchId"),locationIdToGet=theModel.get("locationId"),elemToAppendSchedules=this.$(".schedule-items-wrap"),$locationLoader=this.$el.prev().find(".location-loader");$locationLoader.css("display","block"),app.scheduleCollection.fetch({remove:!1,data:JSON.stringify({courseId:courseIdToGet,cityId:cityIdToGet,searchId:searchIdToGet,locationId:locationIdToGet}),type:"POST",contentType:"application/json",success:function(data){_this.fetchCounter-=1,0===_this.fetchCounter&&($locationLoader.css("display","none"),app.scheduleView=new app.ScheduleView({collection:app.scheduleCollection,el:elemToAppendSchedules,locationLocId:_this.locationIdArr}))},error:function(err){console.log(err)}})},showClassLocationMsg:function(e){e.preventDefault();var target=$(e.currentTarget);target.parent().parent().find(".location-msg").toggleClass("showing")}}),window.app=window.app||{},app.ScheduleView=Backbone.View.extend({tagName:"li",className:"seminar",events:{"click .add-to-cart":function(e){this.addToCart(e)}},initialize:function(options){this.template=_.template($("#scheduleTemplate").html()),this.options=options||{},this.locLocIdArr=options.locationLocId,this.render()},render:function(){var _this=this;$.each(_this.collection.toJSON(),function(index,value){$.each(_this.locLocIdArr,function(index2,id){value.locationId===id&&$(_this.$el[index2]).append(_this.template(value)).hide().fadeIn(300)})})},addToCart:function(e){e.preventDefault();var target=$(e.currentTarget),_this=this;this.$classQty=target.parent().parent().find(".class-qty");var updateTheQuantity=function(){var changedQty=modelData.get("quant"),id=modelData.get("id"),matchingItem=app.cartCollection.where({theId:id}),matchingItemIdAttr=matchingItem[0].get("theId"),newQty=changedQty+matchingItem[0].get("quantity");matchingItem[0].set("quantity",newQty),matchingItem[0].save(),scrollTopAfterAdd(),id==matchingItemIdAttr&&($("#cart-item-list").find("[data-theid="+matchingItemIdAttr+"]").find(".class-qty").val(changedQty),Backbone.trigger("calculateSubtotal",changedQty)),_this.$classQty.val("")},scrollTopAfterAdd=function(){setTimeout(function(){$("html, body").animate({scrollTop:$("#cart").offset().top-80},200),$(".cart-visible").hasClass("down")||$(".cart-tab").trigger("click")},300)};if(""==this.$classQty.val()||0==this.$classQty.val())return target.parent().prev().find(".attendee-msg").addClass("showing"),this.$classQty.css("border-color","red"),this.$el.find(".btn-blue-hollow:focus").blur(),setTimeout(function(){target.parent().prev().find(".attendee-msg").removeClass("showing"),_this.$classQty.css("border-color","#d7d7d7")},3e3),!1;$(".cart-empty-msg").hide(),target.blur().text("Added!").addClass("added"),scrollTopAfterAdd(),setTimeout(function(){target.text("Add to cart").removeClass("added")},2e3);var id=target.data("id"),modelData=this.collection.get(id),courseIdNum=modelData.get("courseId"),classDate=(modelData.get("price"),modelData.get("date")),thequantity=parseInt(this.$classQty.val()),inCart=modelData.get("inCart"),theId=modelData.get("id");modelData.set("quant",thequantity),modelData.set("theId",theId);modelData.get("quant");if(inCart)updateTheQuantity();else{modelData.set("inCart",!0);var relatedClassModel=app.globalCollection.findWhere({courseId:courseIdNum}),relatedLocationModel=app.locationCollection.findWhere({courseId:courseIdNum}),titleOfClass=relatedClassModel.get("title"),cityOfClass=relatedLocationModel.get("cityState"),loadedFromLocalStore=[],isItemInCollection=!1,uniqueIdentifierOfModelAdded=(this.$(".qty").val(),theId);app.cartCollection.each(function(cartItemModel){loadedFromLocalStore.push(cartItemModel.get("theId"))});for(var i=0;i<loadedFromLocalStore.length;i++)uniqueIdentifierOfModelAdded==loadedFromLocalStore[i]&&(isItemInCollection=!0);isItemInCollection?updateTheQuantity():(app.cartItemModel=new app.CartItemModel({title:titleOfClass,city:cityOfClass,date:classDate,quantity:thequantity,theId:theId,price:modelData.get("price")}),this.listenTo(app.cartCollection,"add",this.renderCartItem),app.cartCollection.create(app.cartItemModel.toJSON()),this.$classQty.val(""),this.stopListening())}},renderCartItem:function(cartItem){var itemQuantity=cartItem.get("quantity"),itemPrice=cartItem.get("price");app.cartItemView=new app.CartItemView({model:cartItem,quantity:itemQuantity,price:itemPrice}).render(),this.addQtyToCart(itemQuantity,cartItem),Backbone.trigger("calculateSubtotal",itemQuantity)},addQtyToCart:function(theQuantity,cartItem){var oldVal=$(".items-total").text(),newNum=parseInt(oldVal)+parseInt(theQuantity);$(".items-total").text(newNum+" Items")}}),window.app=window.app||{},app.SingleSeminarView=Backbone.View.extend({tagName:"li",className:"seminar-item col-xs-12",events:{"click .view-opts":"showClassOptions"},initialize:function(){this.template=_.template($("#classTemplate").html()),this.firstClick=!1},render:function(){return this.$el.append(this.template(this.model.toJSON())),this},showClassOptions:function(e){e.preventDefault();var _this=this,open=this.model.get("open"),$schedItemWrap=this.$(".schedule-item-wrap"),viewText=$(e.target);open?$schedItemWrap.slideUp(400,function(){viewText.removeClass("red").html('<span class="plus">+</span>View Upcoming Seminars'),_this.model.set("open",!1)}):$schedItemWrap.slideDown(400,function(){viewText.addClass("red").html('<span class="plus turn">+</span>View Less'),_this.model.set("open",!0)}),$schedItemWrap.css({"border-top":"1px solid #D7D7D7"});var courseIdToGet=this.model.get("courseId"),searchIdToGet=this.model.get("searchId"),elemToRender=$($(e.currentTarget).closest(".result-description").next(".schedule-item-wrap"));this.firstClick||app.locationCollection.fetch({data:JSON.stringify({courseId:courseIdToGet,searchId:searchIdToGet}),type:"POST",contentType:"application/json",success:function(data){app.locationView=new app.LocationView({collection:app.locationCollection,el:elemToRender}),_this.model.set("open",!0),_this.firstClick=!0}})},updateOriginalModelQuantity:function(updatedQuantity){this.model.set("quantity",updatedQuantity),this.$(".qty").val(updatedQuantity),Backbone.off("updateOriginalModelQuantity")},showTotalPrice:function(e){e.preventDefault();var price=this.model.get("price");Backbone.trigger("displayTotalPrice",this.quantity,price)}}),app.singleSeminarView=new app.SingleSeminarView;