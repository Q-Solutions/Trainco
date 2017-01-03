(function() {
  'use strict';

  angular
    .module('train.components')
    .controller('MainSearchController', MainSearchController);

  MainSearchController.$inject = ['$location', '$log', 'Cities', 'MonthSvc', 'UtilitySvc', '$localStorage', '$timeout', '$rootScope', '$scope', '_']
  /** @ngInject */
  function MainSearchController($location, $log, Cities, MonthSvc, UtilitySvc, $localStorage, $timeout, $rootScope, $scope, _) {
    var vm = this;
    vm.$storage = $localStorage;

    var cities = Cities.getCities();
    if (cities[0].label == "")
        cities.splice(0, 1);
    vm.cities = cities;
    vm.createFunction = function(input) {
      // format the option and return it
      return {
        value: vm.cities.length,
        label: input
      };

    };
    vm.classTopics = {};
    var courseObjs = {
      hvac: vm.classTopics.hvac,
      electrical: vm.classTopics.electrical,
      management: vm.classTopics.management,
      mechanical:vm.classTopics.mechanical
    }

    var topic5all;

    if (vm.classTopics.all && UtilitySvc.anyAreTrue(courseObjs)) {
      topic5all = false;
    } else if (vm.classTopics.all) {
      vm.classTopics.hvac = false;
      vm.classTopics.electrical = false;
      vm.classTopics.management = false;
      vm.classTopics.mechanical = false;

      topic5all = true;
    }
    var today = new Date();
    var thisMonth = today.getMonth();
    var monthNames = MonthSvc.getAbrvMonths() || [];
      // Starts the array at the current month through December
    var startingMonthArray = monthNames.slice(thisMonth);

    function fixEndingArray(endingMonthArray) {
      var first = endingMonthArray[0];
      var num = parseInt(first.name.slice(3))
      $log.debug(first.name);
      var trunc = first.name.slice(0, 3)
      first.name = trunc + ' ' + (num + 1);
      return endingMonthArray;
    }
    var endingMonthArray = thisMonth > 0 ? fixEndingArray(monthNames.slice(0, thisMonth)) : [];

    var combinedMonthsArray = startingMonthArray.concat(endingMonthArray)
    var lengthValue = startingMonthArray.length

    var combinedMonthNames = _.map(combinedMonthsArray, _.property('name'));
    var combinedMonthValues = _.map(combinedMonthsArray, _.property('value'));

    vm.sliderValues = {
      minValue: 0,
      maxValue: 5,
      options: {
        floor: 0,
        ceil: 11,
        showTicks: true,
        showSelectionBarEnd: true,
        showTicksValues: true,
        stepsArray: combinedMonthNames
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
    }, 300);

    vm.doParamSearch = function() {
      if (vm.classTopics.hvac === true) {
        vm.topicParam1 = 'hvac'
      }
      if (vm.classTopics.electrical === true) {
        vm.topicParam2 = 'electrical'
      }
      if (vm.classTopics.mechanical === true) {
        vm.topicParam3 = 'mechanical'
      }
      if (vm.classTopics.management === true) {
        vm.topicParam4 = 'management'
      }
      if (vm.classTopics.all === true) {
        vm.topicParam5 = true;
      }
      var defStart = vm.sliderValues.minValue;
      var defEnd = vm.sliderValues.maxValue
      var theloc = vm.courseSearch.location.trim();
      vm.$storage.SearchLocation = theloc || ' ';

      function checkYear(bound) {
        var today = new Date();
        var curMonth = today.getMonth();
        var toCheck = parseInt(vm.sliderValues[bound]) - 1;
          if (toCheck < curMonth) {
            return today.getFullYear() + 1;
          } else {
            return today.getFullYear();
          }
      }


      vm.$storage.SearchTopic1 = vm.topicParam1;
      vm.$storage.SearchTopic2 = vm.topicParam2;
      vm.$storage.SearchTopic3 = vm.topicParam3;
      vm.$storage.SearchTopic4 = vm.topicParam4;
      vm.$storage.SearchTopic5 = topic5all;
      vm.$storage.SearchDRmin = combinedMonthValues[defStart];
      vm.$storage.SearchDRmax = combinedMonthValues[defEnd];
      vm.$storage.SearchDRyear = checkYear('max');
      vm.$storage.SearchDRstartyear = checkYear('min');

      window.location.href = '/search-seminars';
    }
  }
})();
