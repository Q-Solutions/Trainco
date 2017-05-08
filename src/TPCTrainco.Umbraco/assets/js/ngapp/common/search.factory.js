(function() {
  'use strict';

  angular
    .module('train.common')
    .factory('Search', Search);

  Search.$inject = ['$log', '$http', 'CONSTANTS'];
  /** @ngInject */
  function Search($log, $http, CONSTANTS) {
    var apiHost = CONSTANTS.API_URL;
    var today = new Date();

    var service = {
      apiHost: apiHost,
      performSearch: performSearch,
      seminarSearch: seminarSearch
    };

    return service;

    function performSearch(searchObj) {
        var theTopics = [searchObj.topicParam1, searchObj.topicParam2, searchObj.topicParam3, searchObj.topicParam4];
        var endDate = new Date(searchObj.endYear, searchObj.defEnd, 0);
        var startDate = new Date(searchObj.startYear, searchObj.defStart, 0);
      return $http.get(apiHost + '?' +
          'keyword=' + searchObj.keywordParam +
          '&location=' + searchObj.locParam +
          '&radius=' + searchObj.radiusParam +
          '&topics=' + theTopics +
          '&date-start=' + startDate.getFullYear() + '-' + (startDate.getMonth() + 1) + '-01' +
          '&date-end=' + endDate.getFullYear() + '-' + (endDate.getMonth() + 1) + '-' + endDate.getDate() +
          '&simulcast=' + searchObj.simulcast + 
          '&locationPage=' + searchObj.locationPage)
        .then(getSearchComplete)
        .catch(getSearchFailed);

      function getSearchComplete(collection) {
        return collection.data;
      }

      function getSearchFailed(error) {
        $log.error('XHR Failed for getContributors.\n' + angular.toJson(error.data, true));
      }
    }

    function seminarSearch(classId) {
      return $http.get(apiHost + '/' + classId + '?date-start=02/1/'+ thisYear + '&date-end=12/31/' + (thisYear+1))
        .then(function(response) {
          return response.data;
        })
        .catch(function(error) {
          $log.error('XHR Failed for getResults.\n' + angular.toJson(error.data, true));
        });
    }
  }
})();
