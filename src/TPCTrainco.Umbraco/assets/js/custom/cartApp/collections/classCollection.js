'use strict';

window.app = window.app || {};

app.ClassCollection = Backbone.Collection.extend({
	model: app.ClassModel,

	url: 'http://trainco-dev.imulus-client.com/api/seminars/search'

});

app.globalCollection = new app.ClassCollection;
	// var minDate = new Date();
	// this.minMonth = minDate.getMonth() + 2;

	// 	var maxDate = new Date();
	// this.maxMonth = minDate.getMonth() + 5;
	// app.datePicker = new DatePicker();
	  	// $('#date-range-slider').dateRangeSlider({
			    
		  //   defaultValues: {
		  //   	min: new Date(minDate), 
		  //   	max: new Date(maxRangeSelect)
		  //   }
		  
// check the hash to see if there is data there. (only on page load)
$(document).ready(function () {
	if (window.location.hash) {
		if (app.mainSearchSelect == undefined)
			app.mainSearchSelect = new MainSearchSelect();

		var searchParams = app.mainSearchSelect.getHashSearchParams();
		performSearch(searchParams);
	}

	// check if we are on a seminar detail page
	var pathArray = window.location.pathname.split('/');
	var pubSeminarPathItem = pathArray[1];
	if(pubSeminarPathItem === 'public-seminars') {
	// if(pubSeminarPathItem == 'detail.html') {
		app.mainSearchSelect = new MainSearchSelect();
		var seminarDetailSearchParams = JSON.stringify(app.mainSearchSelect.detailPageSearch());
		performSearch(seminarDetailSearchParams)
	}

});

// search button click
$('#search-btn').on('click', function () {
	var searchParams = app.mainSearchSelect.getSearchParams();
	performSearch(searchParams);
});

$('#search-btn-home').on('click', function () {
	var searchParams = app.mainSearchSelect.getSearchParams();
	
	var location = $('#main-search').select2('val').toString();

	window.location.href = '/search-seminars/?loc=' + encodeURIComponent(location) + window.location.hash;
});

// perform the search using the API and the search parameters
function performSearch(searchParams) {

	// parse the search data to show the search results message
	var dataReFormat = $.parseJSON(searchParams);

	// if no data, return
	if (dataReFormat == undefined || dataReFormat == false) {
		return;

		// if no classTopics property, return, which means you are on the detail page
	} else if(dataReFormat.classTopics == undefined || dataReFormat.classTopics == false) {
		return false;
	} else {

		// you are on the search page
		if(dataReFormat.classTopics.length >= 4) {
			var topics = ['all'];
		} else {
			var topics = dataReFormat.classTopics.filter(function (item, pos) {
				var length = dataReFormat.classTopics.length;
				return dataReFormat.classTopics.indexOf(item) == pos;
			});
		}

		// if more than two items selected, add and
		if(topics.length == 2) {
			var length = topics.length;
			topics.splice(length - 1, 0, 'and');
			var topicsList = topics.join(' ');
			var topicsListTwo = topicsList.replace('and,','and');
			var topics = topicsListTwo;
		} else if(topics.length > 2) {

			// if two or fewer, remove commas
			topics.splice(length - 1, 0, 'and');
			var topicsList = topics.join(', ');
			var topicsListTwo = topicsList.replace('and,','and');
			var topics = topicsListTwo;
		}
	}


	var $emptyMsg = $('.empty-message'),
		$classLoader = $('.class-loader');

	app.globalCollection.fetch({
		data: searchParams,
		type: "POST",
		contentType: "application/json",

		success: function (data) {
			$('.results').empty();
			$emptyMsg.fadeOut(100, function () {
				$classLoader.fadeIn(90);

				if (data.length === 0) {
					$classLoader.fadeOut(150, function () {
						$emptyMsg.fadeIn(150).text('We were unable to find classes that fit your preferences. Please change your search terms and try again.');
					});
				} else {
					$classLoader.fadeOut(150, function () {
						if($('.search-page').length) {
							$emptyMsg.fadeIn(150).text('Displaying results for ' + topics + ' seminars in ' + dataReFormat.location + '.');
						}
					});

					app.classView = new app.ClassView({
						collection: app.globalCollection,
						el: '.results'
					});
				}
			});
		}
	});
};


