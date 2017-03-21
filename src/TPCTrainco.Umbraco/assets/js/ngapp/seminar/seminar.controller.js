(function() {
  'use strict';

  angular
    .module('train.seminar')
    .controller('SeminarController', SeminarController);

  SeminarController.$inject = ['$log', 'courseSearch', 'cartService', 'UtilitySvc', '$timeout', '$document', '$window', '$rootScope', '$scope', 'MonthSvc', '_'];
  /* @ngInject */
  function SeminarController($log, courseSearch, cartService, UtilitySvc, $timeout, $document, $window, $rootScope, $scope, MonthSvc, _) {
    var vm = this;

    vm.courseId = {};

    requestSeminarData(courseSearch);

    var POP_TPL = '<div class=popover-content><ul class=list-inline><li><h3><strong>{{event.city}}</strong></h3><li><strong>{{ event.date}}</strong>' +
      '</ul><p ng-bind=event.locationDetails><div class=result-popover><div class="row result-table-head"><div class=col-xs-5>Training Days</div>' +
      '<div class=col-xs-3>Price</div><div class="col-xs-4 attendees-txt">Attendees</div></div><div class="row result-table-body">' +
      '<div class=col-xs-5><em>{{event.daysTitle}}</em></div><div class=col-xs-3>${{event.price}}</div><div class="col-xs-4 attendees">' +
      '<input class=attendee-input type=tel min=1 ng-model=qty string-to-number></div></div>' +
      '<input class="btn btn-blue-solid btn-reg"type=button ng-click="seminar.addItemToCart(event, qty, $event)"value="Add to Cart"></div></div>';

    vm.detailPop = {
      templateUrl: '/assets/js/ngapp/seminar/seminarPop.html'
    };
    vm.registerSem = function() {
      vm.popoverIsOpen = true;
      popoverIsOpen: true;
    }
    vm.popoverClose = function() {
      vm.popoverIsOpen = false;
    }
    vm.location = {};
    vm.seminarLocations = [];
    vm.seminarSimulcast = [];

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

      vm.cartItemList = cartService.getCartItems() || [];
      vm.cartTotalPrice = UtilitySvc.calculateTotalPrice(vm.cartItemList);

      $rootScope.$broadcast('cartUpdated', vm.cartItemList);
      // dirty, but gets the job done. Changes the button text to
      // display added on click.
      $($event.target).val('Added!');

      $timeout = setTimeout(function() {
        // restore the button to add to cart
        $($event.target).val('Add to cart');
      }, 3500);
      // clear the item qty in order to hide button again.
      item.qty = '';
    };

    var today = new Date();
    var thisMonth = today.getMonth();
    var monthNames = MonthSvc.getAbrvMonths();

    // Starts the array at the current month through December
    var startingMonthArray = monthNames.slice(thisMonth);

    function fixEndingArray(endingMonthArray) {
      var first = endingMonthArray[0];
      var num = parseInt(first.name.slice(3))
      var trunc = first.name.slice(0, 3)
      first.name = trunc + (num + 1);

      return endingMonthArray;
    }
    var endingMonthArray = thisMonth > 0 ? fixEndingArray(monthNames.slice(0, thisMonth)) : [];

    var combinedMonthsArray = startingMonthArray.concat(endingMonthArray)

    var combinedMonthNames = _.map(combinedMonthsArray, _.property('name'));
    var combinedMonthValues = _.map(combinedMonthsArray, _.property('value'));
    vm.monthsSlider = {
      minValue: 0,
      maxValue: 5,
      options: {
        floor: 0,
        ceil: 11,
        showTicks: true,
        showSelectionBarEnd: true,
        showTicksValues: true,
        stepsArray: combinedMonthNames,
        onEnd: function(modelValue) {
          watchHandles()
        }
      }
    };
    $timeout(function() {
      $scope.$broadcast('rzSliderForceRender');
      $('.tick > span').each(function(index, item) {
        var $item = $(item);
        var html = $item.html();
        var numTest = /^([^\d]+)(\d+)$/;
        var hasYear = numTest.test(html);

        if (hasYear) {
          $item.html(html.replace(numTest, '<span class="monthblock">$1</span><span class="yearblock">$2</span>'));
        }
      })
    }, 3000);

    function watchHandles() {
      var mapNum = parseInt(combinedMonthValues[0]);
      var thisYear = (new Date()).getFullYear();
      var thisYearCopy = thisYear;
      var minValue = (parseInt(vm.monthsSlider.minValue) + mapNum);
      var maxValue = (parseInt(vm.monthsSlider.maxValue) + mapNum);

      if (minValue > 12) {
        thisYear = thisYear + 1;
        minValue = minValue - 12;
        vm.filterMin = minValue + '-' + thisYear;
      } else {
        vm.filterMin = minValue + '-' + thisYear;
      }


      if (maxValue > 12) {
        thisYearCopy = thisYearCopy + 1;
        maxValue = maxValue - 12;
        vm.filterMax = maxValue + '-' + thisYearCopy;
      } else {
        vm.filterMax = maxValue + '-' + thisYearCopy;
      }

      function fixFormat(badDate) {
        // 10-2016 -> 10-1-2016 -> 10/1/2016
        var x = badDate.replace(/^(\d+\-)/, '$11-').replace(/\-/g, '/');
        return x;
      }

      vm.isBetween = function() {
        return function(item) {

          var testDate = item.dateMonthYear;
          var beginDate = vm.filterMin;
          var endDate = vm.filterMax;

          var fixTest = new Date(fixFormat(testDate));
          var fixBegin = new Date(fixFormat(beginDate));
          var fixEnd = new Date(fixFormat(endDate));
          var testMonth = fixTest.getMonth();
          var beginMonth = fixBegin.getMonth();
          var endMonth = fixEnd.getMonth();
          var testYear = fixTest.getFullYear();
          var beginYear = fixBegin.getFullYear();
          var endYear = fixEnd.getFullYear();

          // To be between dates, the test year has to be bigger than or equal to
          // the begin year and also it has to be smaller than or equal to the end year.
          if (testYear >= beginYear && testYear <= endYear) {

            // If the test year is smaller than the end year, then it is between
            // dates if the test month is >= the begin month.
            if (testYear < endYear) {
              if (testMonth >= beginMonth) {
                return true;
              }

            // If we hit the else case, then the test year MUST BE equal to the end year.
            // but the begin year MIGHT BE less than or equal to the end year.
            } else {
              // When all dates are in the same year, the test month has to be
              // between the begin month and the end month.
              if (testYear === beginYear) {
                if (testMonth >= beginMonth && testMonth <= endMonth) {
                  return true;
                }
              // In the else case, the begin year MUST BE less than the test year.
              // In a case where both years are the same, we only need to know that
              // the test month is less than or equal to the end month.
              } else {
                if (testMonth <= endMonth) {
                  return true;
                }
              }


            }
          }
        }
      }
    }
    watchHandles()
    function activate() {
      var classId = localStorage.getItem('classId');
    }

    function requestSeminarData(courseSearch) {
      var classId = localStorage.getItem('classId');

      return courseSearch.getSeminars(classId).then(function(data) {
        var seminarsData = data.seminars[0];
        receiveSeminarData(seminarsData);
        return seminarsData;
      });
    }

    function receiveSeminarData(seminarsData) {
        console.log(seminarsData);
        vm.seminarLocations = seminarsData.locationSchedules;
        vm.seminarSimulcast = seminarsData.simulcastSchedules;
        console.log(seminarsData);
      //var seminarLocationsArray = vm.seminarLocations;
      //seminarLocationsArray.forEach(function(location, index) {
      //  var dateF = location.dateFilter;
      //  return dateF;
      //});
    }

    vm.toggleSimulcastDescription = function ($e) {
        var obj = angular.element($e.target);
        var parent = obj.parents('li.simulcast');
        var ele = angular.element('div.text-learnmore', parent);
        ele.toggleClass('hide');
        if (ele.is(':visible'))
            obj.text('(Hide Description)');
        else
            obj.text('(Learn more)');
    };
  }
})();
