(function() {
  'use strict';

  angular
      .module('train')
      .service('months', months);

  /** @ngInject */
  function months() {
    var data = [{
      'val': '01',
      'name': 'January'
    }, {
      'val': '02',
      'name': 'February'
    }, {
      'val': '03',
      'name': 'March'
    }, {
      'val': '04',
      'name': 'April'
    }, {
      'val': '05',
      'name': 'May'
    }, {
      'val': '06',
      'name': 'June'
    }, {
      'val': '07',
      'name': 'July'
    }, {
      'val': '08',
      'name': 'August'
    }, {
      'val': '09',
      'name': 'September'
    }, {
      'val': '10',
      'name': 'October'
    }, {
      'val': '11',
      'name': 'November'
    }, {
      'val': '12',
      'name': 'December'
    }]


    this.getMonths = getMonths;

    function getMonths() {
      return data;
    }
  }

})();