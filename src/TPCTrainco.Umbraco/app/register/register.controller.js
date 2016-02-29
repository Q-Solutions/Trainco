(function() {
  'use strict';
  /** @ngInject */
  angular
    .module('train.register')
    .controller('RegisterController', RegisterController);

  function RegisterController($rootScope, $scope, $log, Search, $localStorage, cartService, MonthSvc, $loading, $timeout, $document, $window) {
    var vm = this;
    vm.kwFilter = {};
    vm.mileRange = {};
    var searchAPI = 'http://trainco.axial-client.com/api/seminars2/search/?';
    var searchObj = {
      keywordParam: '',
      locParam: '',
      topicParam1: '',
      topicParam2: '',
      topicParam3: '',
      topicParam4: '',
      defStart: '',
      defEnd: ''
    }
    vm.dateRange = {};
    vm.$storage = $localStorage;
    vm.searchFired = false; // determines whether or not we will show an error message.
    vm.sbIsCollapsed = true; // mobile sidebar converted into menu.

    activate();

    /**
     * calculates the price for all the items in the shopping cart.
     * @method calculateTotalPrice
     * @param  {array}            itemList the items in the cart
     * @return {int}                     the price.
     */
    function calculateTotalPrice(itemList) {
      var totalPrice = itemList ? itemList.reduce(function(acc, item) {
        return acc + item.quantity * parseFloat(item.price);
      }, 0) : 0;
      return parseFloat(totalPrice.toFixed(2));
    }

    function receiveSeminarData(seminarsData) {
      vm.initialDirections = false;
      var seminarLocations = [];
      vm.seminarLocations = seminarsData;
      // below is to calculate the pagination
      vm.semLocLength = vm.seminarLocations.length / 4;
    }

    /**
     * selectively clears localStorage, leaving the cart
     * and the SearchTopic5 (all) intact.
     * @method emptyLocalStorage
     */
    function emptyLocalStorage() {
      delete vm.$storage.SearchLocation;
      delete vm.$storage.SearchTopic1;
      delete vm.$storage.SearchTopic2 ;
      delete vm.$storage.SearchTopic3;
      delete vm.$storage.SearchTopic4;
      delete vm.$storage.SearchDRmin;
      delete vm.$storage.SearchDRmax;
    }

    /**
     * pulls data from localStorage in order to run the
     * search from the off page search component as soon as the page loads.
     * @method activate
     */
    function activate() {
      if (vm.$storage.SearchLocation) {
        var keywordParam = '';
        vm.initialDirections = true;
        var searchObj = {
          keywordParam: null,
          locParam: vm.$storage.SearchLocation,
          topicParam1: vm.$storage.SearchTopic1,
          topicParam2: vm.$storage.SearchTopic2,
          topicParam3: vm.$storage.SearchTopic3,
          topicParam4: vm.$storage.SearchTopic4,
          defStart: vm.$storage.SearchDRmin,
          defEnd: vm.$storage.SearchDRmax
        }

        doParamSearch();
        localStorage.removeItem('ngStorage-SearchDRmin');
        localStorage.removeItem('ngStorage-SearchDRmax');
      } else {
        // showDirections displays the default blank state message
        vm.showDirections = true;
      }
      vm.cartItemList = cartService.getCartItems() || [];
      vm.cartTotalPrice = calculateTotalPrice(vm.cartItemList);
    }

    /**
     * adds item to the cart or updates the quantity
     * @method function
     * @param  {object} item the item and its properties
     * @param  {int} qty  how many attendees
     * @return {array}      returns the updated cartItemList
     *
     * @desc
     * The most important thing here is to understand that
     * qty DOES NOT exist on the $scope. it is only an input value
     * that gets passed to the cart service. It does not exist on the location
     *
     */
    vm.addItemToCart = function(item, qty, $event) {
      $log.debug(item, qty, $event)
      cartService.addItem(item, qty)

      vm.cartItemList = cartService.getCartItems() || [];
      vm.cartTotalPrice = calculateTotalPrice(vm.cartItemList);
      $log.debug(vm.cartItemList)
      $rootScope.$broadcast('cartUpdated', vm.cartItemList);
      // dirty, but gets the job done. Changes the button text to
      // display added on click.
      $($event.target).val('Added!');

      $timeout(function() {
        // restore the button to add to cart
        $($event.target).val('Add to cart');
      }, 3500);
      // clear the item qty in order to hide button again.
      item.qty = '';
    };

    /**
     * Handles the blur and enter for text box inputs.
     * @name manualTextBoxHandler
     * @param  {object} e the event
     * @return {method}   doParamSearch()
     */
    vm.manualTextBoxHandler = function(e) {
      if (e.keyCode === 13 || e.type === 'blur') {
        doParamSearch();
      }
    }

    vm.typingTimeout = '';
    /**
     * Handles the text input as a user types.
     * @name typingTextBoxHandler
     * @param  {object} e the event
     * @return {method}   doParamSearch()
     */
    vm.typingTextBoxHandler = function(e, field) {
      if (field === 'loc') {
        vm.locSearchFilter.locationAll = false;
      }
      if (e.keyCode != 13) {
        if (vm.typingTimeout) {
          clearTimeout(vm.typingTimeout)
        }

        vm.typingTimeout = $timeout(function(e) {
          $log.debug('typingTextBoxHandler running');
          doParamSearch();
        }, 1000)
      }
    }

    vm.locWatcher = function() {
      if (vm.locSearchFilter.locationAll === true) {
        vm.locSearchFilter.location = '';
        doParamSearch();
      }
    }
    vm.fromDateWatcher = function() {
      if (vm.dateRange.end) {
        doParamSearch();
      }
    }
    vm.dateWatcher = function() {
      doParamSearch();
    }

    /**
     * Settings for the mileage slider.
     * @type {Object}
     * translate adds the label to the value.
     */
    vm.mileRange = {
      value: 500, // default miles
      options: {
        min: 50, // minimum range.
        floor: 50,
        ceil: 1000, // maximum
        step: 50, // amount of miles to go up each step.
        /**
         * displays X mile radius above the slider handle thing.
         * @method function
         * @param  {int} value the number of miles aka where the handle lies.
         * @return {string}       the value plus text.
         */
        translate: function(value) {
          return value + ' mile radius';
        },
        /**
         * when the month range slider stops
         * @method function
         * @param  {int} modelValue the value of the slider 01, 02, 03...
         * @return {method}            do the search.
         */
        onEnd: function(modelValue) {
          doParamSearch();
        }
      }
    }

    vm.categories = {
        hvac: false,
        electrical: false,
        mechanical: false,
        management: false,
        all: true
      }
      /**
       * Watches the locationAll checkbox and runs on checked.
       * @method function
       * @return {array} returns the array seminarsData containing all locations.
       */
    vm.stateChanged = function() {
      $rootScope.$broadcast('topic', vm.categories);
    }

    function anyAreTrue(obj) {
      var labels = ['hvac', 'electrical', 'mechanical', 'management'];
      var output = false;
      labels.every(function (label) {
        if (obj[label]) {
          output = true;
          return false;
        }
        return true;
      });
      return output;
    }

    vm.$storage.SearchTopic5 = true;

    // Listens for a broadcast saying topic and then
    // runs a search with the updated topics.
    $scope.$on('topic', function(event, data) {
      var labelsArray = ['hvac', 'electrical', 'mechanical', 'management', 'all'];
      var previouslyAll = vm.$storage.SearchTopic5;

      $log.debug(previouslyAll, anyAreTrue(data));
      // if all was previously false but now it's true, set the others
      // to false and end.
      if (!previouslyAll && data.all === true) {
         vm.categories.hvac = false;
         vm.categories.electrical = false;
         vm.categories.management = false;
         vm.categories.mechanical = false;
         vm.categories.all = true;
      // if all was previously true and any are true now,
      // set all to false and end.
      } else if (previouslyAll && anyAreTrue(data)) {
        vm.categories.all = false;
      }

      vm.$storage.SearchTopic1 = data.hvac ? 'hvac' : undefined;
      vm.$storage.SearchTopic2 = data.electrical ? 'electrical' : undefined;
      vm.$storage.SearchTopic3 = data.mechanical ? 'mechanical' : undefined;
      vm.$storage.SearchTopic4 = data.management ? 'management' : undefined;
      vm.$storage.SearchTopic5 = vm.categories.all;

      doParamSearch();
    });


    var today = new Date();
    var thisMonth = today.getMonth();
    var thisYear = today.getFullYear();
    var monthNames = MonthSvc.getMonths();

    vm.startingMonthArray = monthNames.slice(thisMonth);

    vm.yearOfMonths = vm.startingMonthArray
    .concat(monthNames
    .slice(0, thisMonth)
    .map(function addYear(month) {
      if (parseInt(month.value) === 1) {
        return {
          name: month.name + ' ' + (thisYear + 1),
          value: month.value
        }
      } else {
        return month
      }
    }));
    var defaultStart = vm.startingMonthArray[0].value;
    var defaultEnd = vm.startingMonthArray[3].value;

    function checkYear() {
      var today = new Date();
      if (vm.dateRange.start >= vm.dateRange.end) {
        return today.getFullYear() + 1;
      } else {
        return today.getFullYear();
      }
    }

    function doParamSearch() {
      $loading.start('courses');

      var searchObj = {
        keywordParam: vm.$storage.kword || vm.kwFilter.word,
        locParam: vm.mileRange.value || '250',
        radiusParam: vm.$storage.SearchLocation || vm.locSearchFilter.location,
        topicParam1: vm.$storage.SearchTopic1,
        topicParam2: vm.$storage.SearchTopic2,
        topicParam3: vm.$storage.SearchTopic3,
        topicParam4: vm.$storage.SearchTopic4,
        defStart: vm.$storage.SearchDRmin || vm.dateRange.start || defaultStart,
        defEnd: vm.$storage.SearchDRmax || vm.dateRange.end || defaultEnd,
        endYear: checkYear()
      }

      Search.performSearch(searchObj).then(function(data) {
        /**
         * Clears localStorage search params because we executed a search
         * @method emptyLocalStorage
         * @return {any}          its going to happen regardless of whether or not theres values in LS.
         */
        emptyLocalStorage();
        if (data.seminars.length) {
          // Set showDirections to false because a search has now been executed
          // and that search returns results.
          vm.showDirections = false;
        } else {
          // Set showDirections to true if a search was made, but doesnt have results because
          // we will be displaying and setting searchFired to true in order to display
          // the error message.
          vm.showDirections = true;
        }
        var seminarsData = data.seminars;
        receiveSeminarData(seminarsData);
        // Set true to show error.
        vm.searchFired = true;
        return seminarsData;
      });
    }

    function resetFields() {
      vm.seminarLocations = [];
      vm.locSearchFilter.location = '';
      vm.kwFilter.word = '';
      vm.mileRange.value = '';
      vm.dateRange.start = '';
      vm.dateRange.end = '';
      vm.categories.hvac = false;
      vm.categories.electrical = false;
      vm.categories.management = false;
      vm.categories.mechanical = false;
      vm.categories.all = false;
    }

    vm.clearFilters = function() {
      emptyLocalStorage();
      resetFields();
      $document[0].body.scrollTop = $document[0].documentElement.scrollTop = 0;
      vm.showDirections = true;
      vm.searchFired = false;
    }

    vm.numLimit = 10;
    vm.mainCurrentPage = 0;
    vm.mainPageSize = 4;
    vm.goNextPage = function() {
        vm.mainCurrentPage = vm.mainCurrentPage + 1;
        $document[0].body.scrollTop = $document[0].documentElement.scrollTop = 0;
      }
      // $loading spinner options
    vm.options = {
      text: 'Loading...',
      overlay: true, // Display overlay
      spinner: true, // Display spinner
      spinnerOptions: {
        lines: 12, // The number of lines to draw
        length: 7, // The length of each line
        width: 4, // The line thickness
        radius: 10, // The radius of the inner circle
        rotate: 0, // Rotation offset
        corners: 1, // Roundness (0..1)
        color: '#000', // #rgb or #rrggbb
        direction: 1, // 1: clockwise, -1: counterclockwise
        speed: 2, // Rounds per second
        trail: 100, // Afterglow percentage
        opacity: 1 / 4, // Opacity of the lines
        fps: 20, // Frames per second when using setTimeout()
        zIndex: 2e9, // Use a high z-index by default
        className: 'dw-spinner', // CSS class to assign to the element
        top: 'auto', // Center vertically
        left: 'auto', // Center horizontally
        position: 'relative' // Element position
      }
    }
  }
})();
