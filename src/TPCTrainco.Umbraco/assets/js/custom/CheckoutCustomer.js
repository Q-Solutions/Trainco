﻿'use strict';

function CheckoutCustomer() {
	this.$differentInfo = $('#supervisor-diff');
	this.$differentInfoFields = $('.hidden-different-check');
	this.$billingOptsSelect = $('#PaymentType');
	this.$ccInfo = $('.cc-info');
	this.$invoiceInfo = $('.invoice-info');
	this.$billingDifferent = $('#BillingDifferent');
	this.$promoWrap = $('.promo-wrap');
	this.$hearAbout = $('#HearAbout');
	this.$promoCode = $('#PromoCode');
	this.$hearAboutOther = $('#HearAboutOther');

	this.DisableSelectDropdowns();
	this.showPromoField();
	this.showOtherInfo();
	this.billingOptions();

	if ($("#BillFirstName").val()) {
		this.$differentInfoFields.slideDown('fast');
		this.$billingDifferent.val('true');
	}

	var selectedOption = this.$billingOptsSelect.val();
	if (selectedOption === 'credit') {
		this.$ccInfo.show();
		this.$invoiceInfo.hide();
	}
	else if (selectedOption === 'invoice') {
		this.$ccInfo.hide();
		this.$invoiceInfo.show();
	}

	this.$promoWrap.hide();
	this.$promoCode.hide();
	this.$hearAboutOther.hide();
	var selectedOption2 = this.$hearAbout.val();
	if (selectedOption2 == 'Direct Mail' || selectedOption2 === 'Print Ad' || selectedOption2 === 'Email') {
		this.$promoWrap.show();
		this.$promoCode.slideDown().addClass('showing');
	}
	else if (selectedOption2 == 'Other' || selectedOption2 == 'Referral') {
		this.$promoWrap.show();
		this.$hearAboutOther.slideDown().addClass('showing');
	}
};


CheckoutCustomer.prototype.DisableSelectDropdowns = function () {
	$('select option:first-child').attr('disabled', 'disabled');
};

CheckoutCustomer.prototype.showOtherInfo = function () {
	var _this = this;
	_this.$differentInfo.on('change', function () {
		if ($(this).is(':checked')) {
			_this.$differentInfoFields.slideDown('fast');
			_this.$billingDifferent.val('true');

		} else {
			_this.$differentInfoFields.slideUp('fast');
			_this.$billingDifferent.val('false');
		}
	});
};

CheckoutCustomer.prototype.showPromoField = function () {
	var _this = this;
	_this.$hearAbout.on('change', function () {
		var selectedOption = _this.$hearAbout.val();

		_this.$promoWrap.hide();
		_this.$promoCode.hide();
		_this.$hearAboutOther.hide();

		if (selectedOption == 'Direct Mail' || selectedOption === 'Print Ad' || selectedOption === 'Email') {
			_this.$promoWrap.show();
			_this.$promoCode.slideDown().addClass('showing');
		}
		else if (selectedOption == 'Other' || selectedOption == 'Referral') {
			_this.$promoWrap.show();
			_this.$hearAboutOther.slideDown().addClass('showing');
		}
	});
};

CheckoutCustomer.prototype.billingOptions = function () {
	var _this = this;

	_this.$billingOptsSelect.on('change', function () {
		var selectedOption = $('#PaymentType').val();
		if (selectedOption === 'credit') {
			_this.$invoiceInfo.hide();
			_this.$ccInfo.slideDown('fast');
		}
		else if (selectedOption === 'invoice') {
			_this.$ccInfo.hide();
			_this.$invoiceInfo.slideDown('fast');
		}
		else {
			// enable next button if invoice is chosen
			_this.$ccInfo.slideUp('fast');
			_this.$invoiceInfo.slideUp('fast');
		}
	});


	// show/hide cvv text info
	$('.cvv-text').on('click', function () {
		$(this).find('span').toggleClass('showing');
	});
};
