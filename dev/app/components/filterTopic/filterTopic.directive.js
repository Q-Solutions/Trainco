export function FilterTopicDirective() {
  'ngInject';

  let directive = {
    restrict: 'E',
    templateUrl: 'app/components/filterTopic/filterTopic.html',
    scope: {
    },
    controller: FilterTopicController,
    controllerAs: 'vm',
    bindToController: true
  };

  return directive;
}

class FilterTopicController {
  constructor (searchService, $http, $log, $rootScope) {
    'ngInject';
      this.$http = $http;
      this.$log = $log;
      this.$rootScope = $rootScope;

      this.courseSearch = searchService;
      this.categories = {
        hvac: true, electrical: true, mechanical: true, plant: true
      }
      this.courseTopics = {};
      this.courseTopics.categories = [];

      /**
       * Watches the locationAll checkbox and runs on checked.
       * @method function
       * @return {array} returns the array seminarsData containing all locations.
       */
      this.stateChanged = function() {

        if (this.courseTopics.categories) {

          $rootScope.$broadcast('topic', this.courseTopics.categories)

          // this.$http.get(searchAPI + 'location=all').
          // then((data) => {
          //   this.$state.go('results')
          //   let seminarsData = data.data.seminars;
          //   this.receiveSeminarData(seminarsData);
          //   return seminarsData;
          // });
        }
      }
  }
}
