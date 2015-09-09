﻿'use strict';

window.app = window.app || {};
var ApiDomain = 'http://trainco-dev.imulus-client.com';
function TPCApp() {
	var _this = this;
	this.$win = $(window);
	this.$aHref = $('a[href^=#]');
	this.$page = $('html, body');

	$('.carousel').carousel();

	if ($('.catalog-top').length) {
		this.catalog = new Catalog();
	}

	this.homePage = new HomePage();

	if ($('#main-search').length) {
		if (app.mainSearchSelect == undefined)
			app.mainSearchSelect = new MainSearchSelect();
	}

	if ($('#date-range-slider').length) {
		this.datePicker = new DatePicker();
	}

	if ($('#count').length) {
		this.countUp = new CountUp(this.$win);
	}

	// share popover init
	if ($('.share-btn-wrap').length) {
		$('[data-toggle="popover"]').popover({
			animation: true,
			// container: '.btn-share',
			trigger: 'click',
			html: true
		});
	}

	// umbraco forms
	if($('.form-standard').length) {
		this.formStyles = new FormStyles();
	}

	// checkout
	if ($('.register-top').length) {
		this.Checkout = new Checkout();

		$('#reg-submit').on('click', function (e) {
			e.preventDefault();

			var formData = CreateFormPostString();

			CheckoutPost(formData);
		});
	}

	if ($('.register-two').length) {
		this.CheckoutCustomer = new CheckoutCustomer();
	}

	this.bindScroll();

	// CHECK IF USER IS ON A RETINA DEVICE
	var isRetina = false;
	var mediaQuery = "(-webkit-min-device-pixel-ratio: 1.5),\
	        (min--moz-device-pixel-ratio: 1.5),\
	        (-o-min-device-pixel-ratio: 3/2),\
	        (min-resolution: 1.5dppx)";
	if (window.devicePixelRatio > 1) {
		isRetina = true;
	}
	if (window.matchMedia && window.matchMedia(mediaQuery).matches) {
		isRetina = true;
	}

	this.animateCart(isRetina);
	this.retinaLogos(isRetina);

	_this.clickScrollTo();

	var hash = window.location.hash;

	// if one clicks "browse courses", go to that page and scroll to hash
	if($('.course-section').length) {
		_this.$page.animate({
			scrollTop: $(hash).offset().top - 140
		}, 300);
	}

	if (hash || $('.detail-page-app').length) {

		// this is all the search stuff.
		if (app.mainSearchSelect == undefined) {
			app.mainSearchSelect = new MainSearchSelect();
		}

		var searchParams;
		if (hash) {
			searchParams = app.mainSearchSelect.getHashSearchParams();
		} else {
			searchParams = app.mainSearchSelect.getSearchParams();
		}

		if(hash) {
			performSearch(searchParams);
		} else {
			$('.empty-location-msg').hide();
		}
	}


	// empty the cart on the success page
    if($('.success').length) {
		app.cartNotifyView.clearCart();
    }

    // change body bg color on search page
    if($('#search-results').length) {
    	$('body').css('background-color', '#F9F9F9 !important');
    }

    if($('.form-standard').length) {
    	this.addClassToFormBtn();
    }

    // if we are coming from a home page search, scroll down a bit on the next page
    if(window.location.search == "?homeref=1") {
    	_this.scrollToResults();
    }

    // if IE, trigger click to get rid of select2 "you can only select one" message
    // if (navigator.userAgent.indexOf('MSIE') !== -1 || navigator.appVersion.indexOf('Trident/') > 0) {
    //    $('.select2-search--inline').trigger('click');
    // }

    this.$win.on('resize', function(){
        _this.handleWindowResize();
    });

}

TPCApp.prototype.bindScroll = function () {
	var _this = this;
	this.$win.on('scroll', function () {
		_this.handleWindowScroll();
	});
};

TPCApp.prototype.handleWindowScroll = function () {
	this.currentScrollTop = this.$win.scrollTop();

	// only run this on certain pages.
	if ($('#count').length) {
		this.countUp.handleWindowScroll(this.currentScrollTop);
	}
};

TPCApp.prototype.handleWindowResize = function() {
	if ($('#date-range-slider').length) {
		this.datePicker = new DatePicker();
	}
};

// cart functionality
TPCApp.prototype.animateCart = function (retinaScreen) {
	var _this = this;
	this.$carttab = $('.cart-tab');
	this.$cartvis = $('.cart-visible');
	this.$cartTopImg = $('.cart-top').find('img');
	if (retinaScreen) {
		this.$carttab.find('img').attr('src', '/images/icon-cart-retina.png').css({
			width: 32 + 'px',
			top: -15 + 'px'
		});
	}

	this.$carttab.on('click', function () {
		$('.cart').slideToggle(300, function () {
			$(this).toggleClass('down');

			// change out cart icon, and if user is on retina, account for that.
			if (retinaScreen) {
				_this.$cartvis.toggleClass('down').find('img').attr('src', '/images/icon-cart-close-arrow-2x.png');
				_this.$cartTopImg.attr('src', '/images/icon-cart-retina.png').css({
					width: 32 + 'px'
				});
				if (!$(this).hasClass('down')) {
					_this.$carttab.find('img').attr('src', '/images/icon-cart-retina.png');
				}
			} else {
				_this.$cartvis.toggleClass('down').find('img').attr('src', '/images/icon-cart-close-arrow.png');
				if (!$(this).hasClass('down')) {
					_this.$cartTopImg.attr('src', '/images/icon-cart-tab.png');
					_this.$carttab.find('img').attr('src', '/images/icon-cart-tab.png');
				}
			}
		});
	});
};

TPCApp.prototype.clickScrollTo = function () {
	var _this = this;
	var offsetAmount = 140;
	this.$aHref.on('click', function (e) {
		e.preventDefault();
		_this.$page.animate({
			scrollTop: $($.attr(this, 'href')).offset().top - offsetAmount
		}, 300);
	});
};

TPCApp.prototype.retinaLogos = function(retinaScreen) {
	if(retinaScreen) {
		$('#logo').attr('src', '/images/logo-trainco-2x.png').css('width', 220 + 'px');
	} else {
		$('#logo').attr('src', '/images/logo-trainco-1x.png');
	}
};

TPCApp.prototype.addClassToFormBtn = function() {
	$('.form-standard').find('.btn').addClass('btn-reg').addClass('btn-blue-solid');
};

// if you do a search from the home page, scroll down on the next page.
TPCApp.prototype.scrollToResults = function() {
	var _this = this;
	setTimeout(function() {
		_this.$page.animate({
			scrollTop: $('#search-btn').offset().top - 80
		}, 300);
	}, 500);
};
