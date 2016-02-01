export class RegisterController {
  constructor($log, searchService, $http, $state, $rootScope, $scope) {
    'ngInject';
    this.$state = $state;
    this.$log = $log;
    this.$scope = $scope;
    this.$rootScope = $rootScope;
    this.$http = $http;

    const searchAPI = 'http://trainco.axial-client.com/api/seminars2/search/?';
    // this.mainSearch(searchService);
    this.courseId = {};

    /**
     * Handle key input
     * @param  {object} e the event
     * ng-keydown="searchInput.handleInput($event)"
     */
    this.handleLocInput = (e) => {
      if (e.keyCode === 13 && this.locSearchFilter.location) {
        $rootScope.$broadcast('location', this.locSearchFilter.location);
        this.doParamSearch();
      }
    }
   this.$scope.$on('location', (event, data) => {
     this.locationParam = data;
   });

    /**
     * Watches the locationAll checkbox and runs on checked.
     * @method function
     * @return {array} returns the array seminarsData containing all locations.
     */
    this.stateChanged = function() {
      if (this.locSearchFilter.locationAll) {
        $rootScope.$broadcast('location', this.locSearchFilter.locationAll)
        this.$http.get(searchAPI + 'location=all').
        then((data) => {
          this.$state.go('results')
          let seminarsData = data.data.seminars;
          this.receiveSeminarData(seminarsData);
          return seminarsData;
        });
      }
    }
    this.minDate = new Date();
    this.$log.debug(this.minDate)
    /**
     * Settings for the mileage slider.
     * @type {Object}
     * this.mileRange.value = ng-model.
     */
      this.mileRange = {
        options: {
          min: 50,
          floor: 50,
          ceil: 1000,
          step: 50
        }
      }
   this.$scope.$on('topic', (event, data) => {
    if (data.hvac === true) {
      this.topicParam1 = 'hvac'
    }
    if (data.electrical === true) {
    this.topicParam2 = 'electrical'
    }
    if (data.mechanical === true) {
      this.topicParam3 = 'mechanical'
    }
    if (data.plant === true) {
      this.topicParam4 = 'management'
    }
    this.doParamSearch();
   });

    this.doParamSearch = () => {
      let radiusParam = this.mileRange.value || '250';
//'keyword=' + keywordParam
      this.$http.get(searchAPI +
        'location=' + this.locationParam +
        '&radius=' + radiusParam +
        '&topics=' + this.topicParam1 + ',' + this.topicParam2 + ',' + this.topicParam3 + ',' + this.topicParam4)
        .then((data) => {
          this.$state.go('results')
          let seminarsData = data.data.seminars;
          this.receiveSeminarData(seminarsData);
          return seminarsData;
        });
    }

  }
  receiveSeminarData(seminarsData) {
    let seminarLocations = [];
    this.seminarLocations = seminarsData;
  }
}
