﻿'use strict';

function Checkout() {

	
}


function CreateFormPostString() {
	var $cart = $('.form-container');
	var cartGuid = $cart.data('cart');

	var attendeeList = [];
	$('.form-item-wrapper').each(function () {
		var seminarId = $(this).data('seminar');
		$(this).find('.reg-form').each(function () {
			var attendeeNum = $(this).find('input[name="attendee"]').val();
			var attendeeInc = $(this).find('input[name="attendeeInc"]').val();
			var firstName = $(this).find('input[name="firstname"]').val();
			var lastName = $(this).find('input[name="lastname"]').val();
			var title = $(this).find('input[name="title"]').val();
			var email = $(this).find('input[name="email"]').val();

			var attendeeItem = {
				seminarId: seminarId,
				attendeeNum: attendeeNum,
				attendeeInc: attendeeInc,
				firstName: firstName,
				lastName: lastName,
				title: title,
				email: email
			};

			attendeeList.push(attendeeItem);
		})
	});

	var postData = {
		cartGuid: cartGuid,
		checkoutItems: attendeeList
	}

	return postData;
};

function CheckoutPost(checkoutData) {
	$('#reg-submit').css('opacity', 0);
	$('.checkout-loader').show();

	$('input').next('span').remove();
	$('input').css('border-color', '#d7d7d7');

	$.ajax({
		url: ApiDomain + '/api/checkout/submit',
		data: JSON.stringify(checkoutData),
		type: "POST",
		contentType: "application/json"
	}).done(function (successObj) {

		var success = successObj.success;
		var message = successObj.message;

		$('#reg-submit').css('opacity', 1);
		$('.checkout-loader').hide();

		if (success) {
			window.location.href = '/register/info/';
		}
		else {
			// There was a problem with the form.
			$('.checkout-err-msg').html(message);
			$('.checkout-err-msg').show();

			if (successObj.invalidItems.length > 0) {
				var formElArray = successObj.invalidItems;

				for (var i = 0, l = formElArray.length; i < l; i++) {
					var formEl = formElArray[i];

					$('#' + formEl.elementId).after('<span>' + formEl.message + '</span>');
					$('#' + formEl.elementId).css('border-color', 'red');
				}
			}

		}
	}).fail(function (error) {
		$('#reg-submit').css('opacity', 1);
		$('.checkout-loader').hide();
		$('#reg-submit').prepend('<p class="checkout-err-msg">An error occurred. Please try again later.</p>');
	});
};