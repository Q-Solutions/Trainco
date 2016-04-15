(function() {
  'use strict';

  angular
    .module('train.seminar')
    .factory('courseSearch', courseSearch);

  courseSearch.$inject = ['$log', '$http', 'CONSTANTS'];
  /* @ngInject */
  function courseSearch($log, $http, CONSTANTS) {
    var apiHost = CONSTANTS.API_URL;
    var apiSemDetails =CONSTANTS.API_URL + 'schedules2/details';

    var service = {
      getSeminars: getSeminars,
      getSeminarDetails: getSeminarDetails,
      getSeminarsBox: getSeminarsBox
    };

    return service;
    function getSeminars(classId) {
      return $http.get(apiHost + '/' + classId + '?date-start=02/1/2016&date-end=12/31/2016')
        .then(function(response) {
          return response.data;
        })
        .catch(function(error) {
          $log.error('XHR Failed for getResults.\n' + angular.toJson(error.data, true));
        });
    }

    function getSeminarDetails(semId) {
      return $http.get(apiSemDetails + '/' + semId)
        .then(function(response) {
          return response.data;
        })
        .catch(function(error) {
          $log.error('XHR Failed for getResults.\n' + angular.toJson(error.data, true));
        });
    }

    function getSeminarsBox() {

      var location = localStorage.getItem('location');
      var topicParam1 = localStorage.getItem('topicParam1');
      var topicParam2 = localStorage.getItem('topicParam2');
      var topicParam3 = localStorage.getItem('topicParam3');
      var topicParam4 = localStorage.getItem('topicParam4');
      var minDateRange = localStorage.getItem('minDateRange');
      var maxDateRange = localStorage.getItem('maxDateRange');
      return $http.get(CONSTANTS.API_URL + '?location=' + location +
          '&topics=' + topicParam1 + ',' + topicParam2 + ',' + topicParam3 + ',' + topicParam4 +
          '&date-start=' + minDateRange + '-01-2016' +
          '&date-end=' + maxDateRange + '-01-2016')
        .then(function(response) {
          return response.data;
        })
        .catch(function(error) {
          $log.error('XHR Failed for getResults.\n' + angular.toJson(error.data, true));
        });
    }
  }
})();
