(function() {
  'use strict';
  /** @ngInject */
  angular
    .module('train.register')
    .controller('RegisterController', RegisterController);

  RegisterController.$inject = ['$rootScope', '$scope', '$log', 'Search', '$localStorage', 'cartService', 'UtilitySvc', 'courseSearch', 'MonthSvc', '$loading', '$timeout', '$document', '$window', 'SeminarsSvc', 'CONSTANTS'];
  function RegisterController($rootScope, $scope, $log, Search, $localStorage, cartService, UtilitySvc, courseSearch, MonthSvc, $loading, $timeout, $document, $window, SeminarsSvc, CONSTANTS) {
    var vm = this;

    vm.kwFilter = {
        word: ''
    };
    vm.mileRange = {};
    vm.locSearchFilter = {};
    vm.categories = {
        hvac: false,
        electrical: false,
        mechanical: false,
        management: false,
        all: false
      }
    vm.locSearchFilter.locationAll = false;
    vm.locSearchFilter.simulcast = false;
    var searchAPI = CONSTANTS.API_URL;
    vm.dateRange = {};
    vm.$storage = $localStorage;
    vm.searchFired = false; // determines whether or not we will show an error message.
    vm.sbIsCollapsed = true; // mobile sidebar converted into menu.

    // For the course title dropdown
    vm.seminarListingData = SeminarsSvc.getSeminarList().success(function(res) {
      vm.listingData = res.seminars;
    });
    vm.selectedSeminar = {};
    /**
     * THIS IS DISABLED DONT DELETE
     * -- Seminar Dropdown search feature.
     */
    // vm.seminarDropDownWatcher = function() {
    //   var classId = vm.selectedSeminar.id;
    //   requestSeminarData(classId);
    //   emptyLocalStorage();
    //   resetFields();
    //   $document[0].body.scrollTop = $document[0].documentElement.scrollTop = 0;
    //   vm.showDirections = false;
    //   vm.searchFired = true;
    // }
    function requestSeminarData(classId) {
      var classId = vm.selectedSeminar.id;

      return courseSearch.getSeminars(classId).then(function(data) {
        var seminarsData = data.seminars;
        receiveSeminarData(seminarsData)
      });
    }

    vm.additionalClick = function (id,e) {
        var obj = vm.limits[id];
        obj.limit = obj.total;
    }

    vm.lessClick = function (id, e) {
        var obj = vm.limits[id];
        obj.limit = vm.numLimit;
        angular.element($window).scrollTop(angular.element(e.target).parents('section').offset().top);
    }

    vm.showAdditional = function (id) {
        return vm.limits[id].limit < vm.limits[id].total;
    }

    vm.showLess = function (id) {
        return vm.limits[id].total == vm.limits[id].limit;
    }

    vm.setLimit = function (id, length) {
        vm.limits[id] = {
            'limit': vm.numLimit,
            'total': length
        };
    }

    activate();

    function receiveSeminarData(seminarsData) {
          vm.initialDirections = false;
          var seminarLocations = [];
          vm.seminarLocations = seminarsData;
        // below is to calculate the pagination
          var length = vm.seminarLocations.length / 4
          if(length < 1 && vm.seminarLocations.length > 0)
              length = 1;
          vm.semLocLength = length;
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
      delete vm.$storage.SearchDRyear;
      delete vm.$storage.SearchDRstartyear;
    }

    /**
     * pulls data from localStorage in order to run the
     * search from the off page search component as soon as the page loads.
     * @method activate
     */
    function activate() {
      var keywordParam = '';
      vm.initialDirections = true;
      vm.locSearchFilter.location = vm.$storage.SearchLocation;
      vm.categories.hvac = !!vm.$storage.SearchTopic1;
      vm.categories.electrical = !!vm.$storage.SearchTopic2;
      vm.categories.mechanical = !!vm.$storage.SearchTopic3;
      vm.categories.management = !!vm.$storage.SearchTopic4;
      vm.categories.all = !!vm.$storage.SearchTopic5;


      if (!vm.$storage.SearchLocation) {
        // showDirections displays the default blank state message
        vm.showDirections = true;
      } else {
        vm.topicParam1 = vm.$storage.SearchTopic1;
        vm.topicParam2 = vm.$storage.SearchTopic2;
        vm.topicParam3 = vm.$storage.SearchTopic3;
        vm.topicParam4 = vm.$storage.SearchTopic4;
        vm.dateRange.end = vm.$storage.SearchDRmax;
        vm.dateRange.start = vm.$storage.SearchDRmin;
        doParamSearch();
      }

      vm.cartItemList = cartService.getCartItems() || [];
      vm.cartTotalPrice = UtilitySvc.calculateTotalPrice(vm.cartItemList);
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
      cartService.addItem(item, qty)

      vm.cartItemList = cartService.getCartItems();
      vm.cartTotalPrice = UtilitySvc.calculateTotalPrice(vm.cartItemList);

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
      if (e.keyCode === 13) {
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
    vm.typingTextBoxHandler = function (e, field) {
        if (e.keyCode == 9 || e.keyCode == 13)
            return;
        if (field === 'loc')
            vm.locSearchFilter.locationAll = false;
        if (field === 'loc' && vm.locSearchFilter.location.length < 4)
            return;
        if (vm.typingTimeout)
            clearTimeout(vm.typingTimeout);
        vm.typingTimeout = $timeout(function(e) {
            doParamSearch();
        }, 1000)
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
    vm.simulcastWatcher = function () {
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
      /**
       * Watches the locationAll checkbox and runs on checked.
       * @method function
       * @return {array} returns the array seminarsData containing all locations.
       */
    vm.stateChanged = function() {
      $rootScope.$broadcast('topic', vm.categories);
    }
    // if (vm.$storage.SearchLocation) {
    // vm.$storage.SearchTopic5 = true;
    // }
    // Listens for a broadcast saying topic and then
    // runs a search with the updated topics.
    $scope.$on('topic', function(event, data) {
      var labelsArray = ['hvac', 'electrical', 'mechanical', 'management', 'all'];
      var previouslyAll = vm.$storage.SearchTopic5;

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
    } else if (previouslyAll && UtilitySvc.anyAreTrue(data)) {
        vm.categories.all = false;
      }

      vm.$storage.SearchTopic5 = vm.categories.all;

      vm.topicParam1 = data.hvac ? 'hvac' : undefined;
      vm.topicParam2 = data.electrical ? 'electrical' : undefined;
      vm.topicParam3 = data.mechanical ? 'mechanical' : undefined;
      vm.topicParam4 = data.management ? 'management' : undefined;

      doParamSearch();
    });


    var today = new Date();
    var thisMonth = today.getMonth();
    var thisYear = today.getFullYear();
    var monthNames = MonthSvc.getMonths();

    var curYearArray = monthNames.slice(thisMonth).map(function addYear(month) {
      return {
        name: month.name + ' ' + (thisYear),
        value: month.value
      }
    });

    vm.startingMonthArray =
      curYearArray.concat(monthNames.slice(0, thisMonth).map(
        function addYear(month) {
          if (parseInt(month.value) <= 12) {
            return {
              name: month.name + ' ' + (thisYear + 1),
              value: month.value
            }
          } else {
            return month
          }
      }));
    vm.yearOfMonths =
      curYearArray.concat(monthNames.slice(0, thisMonth).map(function addYear(month) {
        if (parseInt(month.value) <= 12) {
          return {
            name: month.name + ' ' + (thisYear + 1),
            value: month.value
          }
        } else {
          return month
        }
      }));

    function checkYear(bound) {
      var today = new Date();
      var curMonth = today.getMonth();
      var toCheck = parseInt(vm.dateRange[bound]) - 1;
        if (toCheck < curMonth) {
          return today.getFullYear() + 1;
        } else {
          return today.getFullYear();
        }
    }

    function doParamSearch() {
      // $log.debug('the mid one', vm.topicParam2)

      $loading.start('courses');
      var today = new Date();
      var thisMonth = today.getMonth() + 1;
      var searchObj = {
        keywordParam: vm.kwFilter.word,
        locParam: vm.locSearchFilter.location,
        simulcast: vm.locSearchFilter.simulcast ? '1' : '0',
        radiusParam: vm.mileRange.value || '500',
        topicParam1: vm.topicParam1,
        topicParam2: vm.topicParam2,
        topicParam3: vm.topicParam3,
        topicParam4: vm.topicParam4,
        defStart: vm.dateRange.start || thisMonth,
        startYear: checkYear('start'),
        defEnd: vm.dateRange.end || (thisMonth + 11),
        endYear: checkYear('end')
      }

      // $log.debug(searchObj, vm.topicParam2)

      Search.performSearch(searchObj).then(function(data) {
        /**
         * Clears localStorage search params because we executed a search
         * @method emptyLocalStorage
         * @return {any}          its going to happen regardless of whether or not theres values in LS.
         */
        //emptyLocalStorage();
        if (data.seminars.length) {
          // Set showDirections to false because a search has now been executed
          // and that search returns results.
          vm.showDirections = false;
        //  delete vm.$storage.SearchDRyear;
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
      vm.topicParam1 = vm.topicParam2 = vm.topicParam3 = vm.topicParam4 = '';
      vm.locSearchFilter.locationAll = false;
    }

    vm.clearFilters = function() {
      emptyLocalStorage();
      resetFields();
      $document[0].body.scrollTop = $document[0].documentElement.scrollTop = 0;
      vm.showDirections = true;
      vm.searchFired = false;
      doParamSearch();
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
    vm.limits = {};
  }
})();
